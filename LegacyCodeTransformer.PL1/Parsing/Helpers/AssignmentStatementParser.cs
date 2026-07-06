using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I assignment statement parse davranışını yönetir.
///
/// Neden var?
/// ----------------------
/// StatementParser orchestration sınıfı concrete assignment grammar detaylarını
/// doğrudan bilmemelidir. Assignment parse davranışı ayrı bir helper parser içinde
/// tutulmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// Identifier ile başlayan ve '=' operatörü içeren PL/I assignment statement
/// yapılarını Pl1AssignmentStatement modeline dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     A = B;
///     SQLCODE = 0;
///     FLAG = 'Y';
///     DCLGLAU.BRM_KOD = 888;
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser içinde StatementParserKind.Assignment seçildiğinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Çoklu assignment, array access, function call expression ve gerçek expression
/// parser desteği eklendiğinde bu parser aynı entrypoint üzerinden geliştirilecektir.
/// </summary>
internal sealed class AssignmentStatementParser : ParserBase
{
    public AssignmentStatementParser(ParseContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Mevcut token pozisyonundan assignment statement parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Identifier ile başlayan statement'lar ilk aşamada assignment olarak ele alınır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Sol taraf target tokenlarını '=' operatörüne kadar, sağ taraf value tokenlarını
    /// semicolon'a kadar toplar ve Pl1RawExpression tabanlı Pl1AssignmentStatement
    /// modeli üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     PARAM = 'ABC';
    ///     TOTAL = PRICE;
    ///     SQLCODE = 0;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementParser.ParseStatement içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// P05.3 ilerledikçe çoklu target assignment ve daha gelişmiş expression parser
    /// entegrasyonu bu method üzerinden yapılacaktır.
    /// </summary>
    public HelperParseResult<Pl1Statement> ParseAssignmentStatement()
    {
        var statementStart = Current.Location;
        var targetTokens = ReadTokensUntil(Pl1TokenKind.Equals);

        if (Current.Kind != Pl1TokenKind.Equals)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "'=' bekleniyordu.",
                    Current));

            SkipCurrentStatement();

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

        var valueTokens = ReadTokensUntil(Pl1TokenKind.Semicolon);

        if (Current.Kind != Pl1TokenKind.Semicolon)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "';' bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

        if (targetTokens.Count == 0)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "Assignment hedefi bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        if (valueTokens.Count == 0)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "Assignment değeri bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        var targetExpressionText = AssignmentRawExpressionBuilder.Build(targetTokens);
        var valueExpressionText = AssignmentRawExpressionBuilder.Build(valueTokens);

        var statement = new Pl1AssignmentStatement(
            targets: new[]
            {
                new Pl1RawExpression(targetExpressionText, statementStart)
            },
            value: new Pl1RawExpression(valueExpressionText, statementStart),
            location: statementStart);

        return new HelperParseResult<Pl1Statement>(
            statement,
            Position);
    }

    /// <summary>
    /// Belirtilen token türüne veya EOF'a kadar tokenları okur.
    ///
    /// Neden var?
    /// ----------------------
    /// Assignment grammar içinde target ve value taraflarını ayırmak için
    /// delimiter tabanlı token toplama gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// '=' veya ';' gibi sınır tokenlarına kadar olan tokenları expression adayı
    /// olarak toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// A = B; inputunda:
    ///
    /// - '=' öncesinde A tokenı
    /// - ';' öncesinde B tokenı
    ///
    /// toplanır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseAssignmentStatement içinde targetTokens ve valueTokens üretiminde.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Expression parser eklendiğinde delimiter tabanlı okuma davranışı statement
    /// boundary tespiti için kullanılmaya devam edebilir.
    /// </summary>
    private List<Pl1Token> ReadTokensUntil(Pl1TokenKind delimiterKind)
    {
        var tokens = new List<Pl1Token>();

        while (!IsAtEnd() && Current.Kind != delimiterKind)
        {
            tokens.Add(Advance());
        }

        return tokens;
    }

    /// <summary>
    /// Hatalı assignment statement'ı semicolon'a veya EOF'a kadar atlar.
    ///
    /// Neden var?
    /// ----------------------
    /// Assignment parse sırasında '=' eksikse parser'ın aynı token üzerinde takılıp
    /// sonsuz döngüye girmemesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Hatalı statement içeriğini tüketir ve parser'ı sonraki statement için güvenli
    /// pozisyona taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     PARAM 'ABC';
    ///
    /// Bu inputta '=' olmadığı için statement semicolon'a kadar atlanır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseAssignmentStatement içinde recovery davranışı olarak.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Unsupported assignment syntax recovery davranışı ileride bu method üzerinden
    /// genişletilebilir.
    /// </summary>
    private void SkipCurrentStatement()
    {
        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.Semicolon)
        {
            Advance();
        }

        if (Current.Kind == Pl1TokenKind.Semicolon)
        {
            Advance();
        }
    }
}