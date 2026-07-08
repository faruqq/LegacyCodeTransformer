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
/// PARAM = 'ABC';
/// CALL FETCH_CURSOR;
/// IF SQLCODE = 0 THEN CALL FETCH_CURSOR;
/// DO WHILE(SQLCODE = 0);
/// EXEC SQL INCLUDE SQLCA;
///
/// Nerede kullanılır?
/// ----------------------
/// Pl1Parser declaration dışındaki tokenları StatementParser'a yönlendirdiğinde
/// bu sınıf kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// SelectStatementParser, EmbeddedSqlStatementParser ve diğer executable statement
/// parser'lar bu orchestration sınıfı üzerinden devreye alınacaktır.
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
    /// Mevcut token'ın statement başlangıcı olup olmadığını kontrol eder. Assignment,
    /// CALL, IF, DO ve EXEC SQL başlangıçlarını concrete parser'lara yönlendirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PARAM = 'ABC';
    /// CALL FETCH_CURSOR;
    /// IF SQLCODE = 0 THEN CALL FETCH_CURSOR;
    /// DO WHILE(SQLCODE = 0);
    /// EXEC SQL INCLUDE SQLCA;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1Parser ana orchestration akışında ve ProcedureParser body parsing içinde
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Yeni statement parser'ları eklendiğinde bu method aynı dispatcher standardı
    /// üzerinden genişletilecektir.
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
            StatementParserKind.Call => ParseCallStatement(),
            StatementParserKind.If => ParseIfStatement(),
            StatementParserKind.Do => ParseDoStatement(),
            StatementParserKind.EmbeddedSql => ParseEmbeddedSqlStatement(),
            StatementParserKind.CompilerDirective => ParseCompilerDirectiveStatement(),

            _ => ParseUnsupportedStatement(parserKind)
        };
    }

    private HelperParseResult<Pl1Statement> ParseCompilerDirectiveStatement()
    {
        var parser = new CompilerDirectiveStatementParser(Context);
        var result = parser.ParseCompilerDirectiveStatement();

        Position = result.Position;

        return result;
    }

    private HelperParseResult<Pl1Statement> ParseAssignmentStatement()
    {
        var parser = new AssignmentStatementParser(Context);
        var result = parser.ParseAssignmentStatement();

        Position = result.Position;

        return result;
    }

    private HelperParseResult<Pl1Statement> ParseCallStatement()
    {
        var parser = new CallStatementParser(Context);
        var result = parser.ParseCallStatement();

        Position = result.Position;

        return result;
    }

    private HelperParseResult<Pl1Statement> ParseIfStatement()
    {
        var parser = new IfStatementParser(Context);
        var result = parser.ParseIfStatement();

        Position = result.Position;

        return result;
    }

    private HelperParseResult<Pl1Statement> ParseDoStatement()
    {
        var parser = new DoStatementParser(Context);
        var result = parser.ParseDoStatement();

        Position = result.Position;

        return result;
    }

    private HelperParseResult<Pl1Statement> ParseEmbeddedSqlStatement()
    {
        var parser = new EmbeddedSqlStatementParser(Context);
        var result = parser.ParseEmbeddedSqlStatement();

        Position = result.Position;

        return result;
    }

    private HelperParseResult<Pl1Statement> ParseUnsupportedStatement(
        StatementParserKind parserKind)
    {
        var statementFamilyName = GetStatementFamilyName(parserKind);

        Diagnostics.Add(
            ParserDiagnosticFactory.UnexpectedToken(
                Current,
                $"{statementFamilyName} parser henüz eklenmedi"));

        var recoveryHelper = new StatementRecoveryHelper(Context);
        recoveryHelper.SkipCurrentStatement();

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
            StatementParserKind.EmbeddedSql => "EXEC SQL",
            StatementParserKind.CompilerDirective => "Compiler Directive",
            _ => "Unknown"
        };
    }
}