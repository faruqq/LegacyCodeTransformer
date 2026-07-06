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
    /// semicolon'a kadar toplar ve ExpressionFactory üzerinden Pl1Expression üretir.
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
        var tokenReader = new DelimitedTokenReader(Context);
        var recoveryHelper = new StatementRecoveryHelper(Context);

        var targetTokens = tokenReader.ReadUntil(Pl1TokenKind.Equals);

        if (Current.Kind != Pl1TokenKind.Equals)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "'=' bekleniyordu.",
                    Current));

            recoveryHelper.SkipCurrentStatement();

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

        var valueTokens = tokenReader.ReadUntil(Pl1TokenKind.Semicolon);

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

        var targetExpression = ExpressionFactory.Create(
            targetTokens,
            statementStart);

        var valueExpression = ExpressionFactory.Create(
            valueTokens,
            statementStart);

        var statement = new Pl1AssignmentStatement(
            targets: new[]
            {
                targetExpression
            },
            value: valueExpression,
            location: statementStart);

        return new HelperParseResult<Pl1Statement>(
            statement,
            Position);
    }
}