using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I executable statement parse sürecinin orchestration sınıfıdır.
///
/// Neden var?
/// ----------------------
/// Pl1Parser ana parser olarak kalmalı ve declaration dışındaki executable
/// statement detaylarını doğrudan parse etmemelidir. Statement parsing kendi
/// orchestration katmanına sahip olmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// Statement başlangıç token'larını merkezi dispatcher üzerinden tanır ve ilgili
/// concrete statement parser'a yönlendirir.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     PARAM = 'ABC';
///     CALL FETCH_CURSOR;
///     IF SQLCODE = 0 THEN DO;
///     DO WHILE(SQLCODE = 0);
///
/// P05.3 içinde assignment parser gerçek model üretir. CALL, IF ve DO parser
/// davranışları sonraki adımlarda eklenecektir.
///
/// Nerede kullanılır?
/// ----------------------
/// Pl1Parser declaration dışındaki tokenları StatementParser'a yönlendirdiğinde
/// bu sınıf kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// CallStatementParser, IfStatementParser ve DoStatementParser bu orchestration
/// sınıfı üzerinden devreye alınacaktır.
/// </summary>
internal sealed class StatementParser : ParserBase
{
    private readonly StatementDispatcher _dispatcher = new();

    public StatementParser(ParseContext context)
        : base(context)
    {
    }

    public StatementParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
        : this(new ParseContext(tokens, position, diagnostics))
    {
    }

    /// <summary>
    /// Mevcut token pozisyonundan tek bir executable statement parse etmeye çalışır.
    ///
    /// Neden var?
    /// ----------------------
    /// Statement parser'ın dış dünyaya tek statement parse eden standart bir entrypoint
    /// sunması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Mevcut token'ın statement başlangıcı olup olmadığını kontrol eder. Statement
    /// başlangıcı değilse null döndürür. Assignment başlangıcıysa
    /// AssignmentStatementParser'a yönlendirir. Henüz desteklenmeyen statement türleri
    /// için diagnostic ve recovery davranışını korur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     PARAM = 'ABC';
    ///
    /// Bu input P05.3 ile Pl1AssignmentStatement üretir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1Parser ana orchestration akışında kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// CALL, IF ve DO parser'ları eklendiğinde bu method aynı dispatcher standardı
    /// üzerinden ilgili concrete parser'lara yönlenecektir.
    /// </summary>
    public HelperParseResult<Pl1Statement> ParseStatement()
    {
        var parserKind = _dispatcher.GetParserKind(Current.Kind);

        return parserKind switch
        {
            StatementParserKind.Unknown => new HelperParseResult<Pl1Statement>(
                null,
                Position),

            StatementParserKind.Assignment => ParseAssignmentStatement(),

            _ => ParseUnsupportedStatement(parserKind)
        };
    }

    private HelperParseResult<Pl1Statement> ParseAssignmentStatement()
    {
        var parser = new AssignmentStatementParser(Context);
        var result = parser.ParseAssignmentStatement();

        Position = result.Position;

        return result;
    }

    private HelperParseResult<Pl1Statement> ParseUnsupportedStatement(StatementParserKind parserKind)
    {
        var statementFamilyName = GetStatementFamilyName(parserKind);

        Diagnostics.Add(
            ParserDiagnosticFactory.UnexpectedToken(
                Current,
                $"{statementFamilyName} parser henüz eklenmedi"));

        SkipCurrentStatement();

        return new HelperParseResult<Pl1Statement>(
            null,
            Position);
    }

    private static string GetStatementFamilyName(StatementParserKind parserKind)
    {
        return parserKind switch
        {
            StatementParserKind.Assignment => "Assignment",
            StatementParserKind.Call => "CALL",
            StatementParserKind.If => "IF",
            StatementParserKind.Do => "DO",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Mevcut statement'ı noktalı virgüle veya EOF'a kadar güvenli şekilde atlar.
    ///
    /// Neden var?
    /// ----------------------
    /// Concrete statement parser'lar eklenmeden önce statement başlangıçları tanınsa
    /// bile parser'ın sonsuz döngüye girmemesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Desteklenmeyen veya henüz parse edilmeyen statement içeriğini semicolon'a kadar
    /// tüketir. Semicolon bulunursa onu da tüketerek parser'ı sonraki statement veya
    /// declaration için hazır hale getirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     CALL FETCH_CURSOR;
    ///
    /// CALL parser eklenene kadar bu statement model üretilmeden tamamen tüketilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseUnsupportedStatement içinde recovery davranışı olarak kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Unsupported syntax recovery ve diagnostic sonrası toparlama davranışı için
    /// ortak temel oluşturur.
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