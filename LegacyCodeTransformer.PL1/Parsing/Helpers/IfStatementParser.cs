using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I IF THEN ELSE statement parse davranışını yönetir.
///
/// Neden var?
/// ----------------------
/// StatementParser orchestration sınıfı IF grammar detaylarını doğrudan bilmemelidir.
/// IF condition, THEN statement ve optional ELSE statement parse davranışı ayrı bir
/// concrete parser içinde tutulmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// IF keyword ile başlayan PL/I karar yapılarını Pl1IfStatement modeline dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     IF SQLCODE = 0 THEN CALL FETCH_CURSOR;
///     IF A = B THEN PARAM = 'ABC';
///     IF A = B THEN CALL PROC1; ELSE CALL PROC2;
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser içinde StatementParserKind.If seçildiğinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// THEN DO / ELSE DO block parsing, nested IF parsing, condition semantic analysis
/// ve EGL if/else generation çalışmalarına temel olur.
/// </summary>
internal sealed class IfStatementParser : ParserBase
{
    public IfStatementParser(ParseContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Mevcut token pozisyonundan IF statement parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// IF statement PL/I control-flow desteğinin ilk temel yapısıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// IF keyword'ünü, THEN keyword'üne kadar condition tokenlarını, THEN sonrasındaki
    /// executable statement'ı ve varsa ELSE sonrasındaki executable statement'ı okuyarak
    /// Pl1IfStatement modeli üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     IF SQLCODE = 0 THEN CALL FETCH_CURSOR;
    ///     IF A = B THEN CALL PROC1; ELSE CALL PROC2;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementParser.ParseStatement içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// P05.4 içinde DO block ve nested block parser eklendiğinde THEN / ELSE tarafları
    /// aynı Pl1Statement modeli üzerinden genişletilecektir.
    /// </summary>
    public HelperParseResult<Pl1Statement> ParseIfStatement()
    {
        var statementStart = Current.Location;

        if (Current.Kind != Pl1TokenKind.IfKeyword)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "IF bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

        var tokenReader = new DelimitedTokenReader(Context);
        var conditionTokens = tokenReader.ReadUntil(Pl1TokenKind.ThenKeyword);

        if (Current.Kind != Pl1TokenKind.ThenKeyword)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "THEN bekleniyordu.",
                    Current));

            var recoveryHelper = new StatementRecoveryHelper(Context);
            recoveryHelper.SkipCurrentStatement();

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        if (conditionTokens.Count == 0)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "IF condition bekleniyordu.",
                    Current));

            var recoveryHelper = new StatementRecoveryHelper(Context);
            recoveryHelper.SkipCurrentStatement();

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

        var thenStatement = ParseChildStatement();

        if (thenStatement is null)
        {
            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Pl1Statement? elseStatement = null;

        if (Current.Kind == Pl1TokenKind.ElseKeyword)
        {
            Advance();

            elseStatement = ParseChildStatement();

            if (elseStatement is null)
            {
                return new HelperParseResult<Pl1Statement>(
                    null,
                    Position);
            }
        }

        var conditionExpression = ExpressionFactory.Create(
            conditionTokens,
            statementStart);

        var statement = new Pl1IfStatement(
            conditionExpression,
            thenStatement,
            elseStatement,
            statementStart);

        return new HelperParseResult<Pl1Statement>(
            statement,
            Position);
    }

    /// <summary>
    /// IF THEN veya ELSE sonrasındaki child statement'ı parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// IF statement'ın THEN ve ELSE kolları herhangi bir executable statement olabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Child statement parse sorumluluğunu tekrar StatementParser'a devreder ve nested
    /// parser orchestration standardını korur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     THEN CALL FETCH_CURSOR;
    ///     THEN PARAM = 'ABC';
    ///     ELSE CALL PROC2;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseIfStatement içinde THEN ve ELSE kollarını parse etmek için kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested IF, DO block ve SELECT gibi child statement türleri StatementParser
    /// genişledikçe otomatik olarak bu noktadan desteklenecektir.
    /// </summary>
    private Pl1Statement? ParseChildStatement()
    {
        var parser = new StatementParser(Context);
        var result = parser.ParseStatement();

        Position = result.Position;

        if (result.Value is null)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "Executable statement bekleniyordu.",
                    Current));
        }

        return result.Value;
    }
}