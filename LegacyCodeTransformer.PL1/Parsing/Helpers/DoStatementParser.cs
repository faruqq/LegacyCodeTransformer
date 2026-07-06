using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I DO, DO WHILE ve DO UNTIL statement parse davranışını yönetir.
///
/// Neden var?
/// ----------------------
/// StatementParser orchestration sınıfı DO grammar detaylarını doğrudan bilmemelidir.
/// Block DO, DO WHILE ve DO UNTIL parsing davranışı ayrı bir concrete parser içinde
/// tutulmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// DO keyword ile başlayan PL/I block ve loop yapılarını Pl1DoStatement modeline
/// dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     DO;
///         CALL PROC1;
///     END;
///
///     DO WHILE(SQLCODE = 0);
///         CALL FETCH_CURSOR;
///     END;
///
///     DO UNTIL(EOF);
///         PARAM = 'ABC';
///     END;
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser içinde StatementParserKind.Do seçildiğinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Nested block parsing, IF THEN DO / ELSE DO parsing, loop semantic analysis ve
/// EGL block / while generation çalışmalarına temel olur.
/// </summary>
internal sealed class DoStatementParser : ParserBase
{
    public DoStatementParser(ParseContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Mevcut token pozisyonundan DO statement parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// DO statement PL/I block ve loop parsing desteğinin temel yapısıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DO keyword'ünü, varsa WHILE / UNTIL condition bilgisini, block body içindeki
    /// executable statement listesini ve END; sonlandırıcısını okuyarak Pl1DoStatement
    /// modeli üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     DO;
    ///         CALL PROC1;
    ///     END;
    ///
    ///     DO WHILE(SQLCODE = 0);
    ///         CALL FETCH_CURSOR;
    ///     END;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementParser.ParseStatement içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested DO, IF THEN DO, ELSE DO ve ileride sayaçlı DO parsing desteği bu method
    /// üzerinden genişletilecektir.
    /// </summary>
    public HelperParseResult<Pl1Statement> ParseDoStatement()
    {
        var statementStart = Current.Location;

        if (Current.Kind != Pl1TokenKind.DoKeyword)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "DO bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

        var kind = Pl1DoStatementKind.Block;
        Pl1Expression? condition = null;

        if (Current.Kind == Pl1TokenKind.WhileKeyword || Current.Kind == Pl1TokenKind.UntilKeyword)
        {
            kind = Current.Kind == Pl1TokenKind.WhileKeyword
                ? Pl1DoStatementKind.While
                : Pl1DoStatementKind.Until;

            Advance();

            condition = ParseDoCondition(statementStart);

            if (condition is null)
            {
                return new HelperParseResult<Pl1Statement>(
                    null,
                    Position);
            }
        }

        if (Current.Kind != Pl1TokenKind.Semicolon)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "';' bekleniyordu.",
                    Current));

            var recoveryHelper = new StatementRecoveryHelper(Context);
            recoveryHelper.SkipCurrentStatement();

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

        var body = ParseBody(statementStart);

        if (Current.Kind != Pl1TokenKind.EndKeyword)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "END bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

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

        var statement = new Pl1DoStatement(
            kind,
            condition,
            body,
            statementStart);

        return new HelperParseResult<Pl1Statement>(
            statement,
            Position);
    }

    /// <summary>
    /// DO WHILE / DO UNTIL condition alanını parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Koşullu DO statement'larda WHILE veya UNTIL keyword'ünden sonra parantez içinde
    /// condition expression bulunur.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Açılış parantezini, kapanış parantezine kadar condition tokenlarını ve kapanış
    /// parantezini okuyarak ExpressionFactory üzerinden Pl1Expression üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     DO WHILE(SQLCODE = 0);
    ///     DO UNTIL(EOF);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseDoStatement içinde koşullu DO türlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Gerçek condition expression parser eklendiğinde bu method ExpressionFactory
    /// üzerinden detaylı expression model üretimine geçebilir.
    /// </summary>
    private Pl1Expression? ParseDoCondition(SourceLocation location)
    {
        if (Current.Kind != Pl1TokenKind.OpenParenthesis)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "'(' bekleniyordu.",
                    Current));

            var recoveryHelper = new StatementRecoveryHelper(Context);
            recoveryHelper.SkipCurrentStatement();

            return null;
        }

        Advance();

        var tokenReader = new DelimitedTokenReader(Context);
        var conditionTokens = tokenReader.ReadUntil(Pl1TokenKind.CloseParenthesis);

        if (conditionTokens.Count == 0)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "DO condition bekleniyordu.",
                    Current));

            var recoveryHelper = new StatementRecoveryHelper(Context);
            recoveryHelper.SkipCurrentStatement();

            return null;
        }

        if (Current.Kind != Pl1TokenKind.CloseParenthesis)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "')' bekleniyordu.",
                    Current));

            return null;
        }

        Advance();

        return ExpressionFactory.Create(
            conditionTokens,
            location);
    }

    /// <summary>
    /// DO block body içindeki statement listesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// DO ... END arasında bir veya daha fazla executable statement bulunabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// END keyword'üne kadar child statement'ları StatementParser üzerinden recursive
    /// olarak parse eder ve Pl1BlockStatement modeli üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     DO;
    ///         PARAM = 'ABC';
    ///         CALL PROC1;
    ///     END;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseDoStatement içinde body üretiminde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested block parser, procedure body parser ve SELECT branch body parser için
    /// ortak yaklaşım oluşturur.
    /// </summary>
    private Pl1BlockStatement ParseBody(SourceLocation location)
    {
        var statements = new List<Pl1Statement>();

        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.EndKeyword)
        {
            var parser = new StatementParser(Context);
            var result = parser.ParseStatement();

            Position = result.Position;

            if (result.Value is not null)
            {
                statements.Add(result.Value);
                continue;
            }

            if (!IsAtEnd() && Current.Kind != Pl1TokenKind.EndKeyword)
            {
                Diagnostics.Add(
                    ParserDiagnosticFactory.ExpectedToken(
                        "Executable statement bekleniyordu.",
                        Current));

                var recoveryHelper = new StatementRecoveryHelper(Context);
                recoveryHelper.SkipCurrentStatement();
            }
        }

        return new Pl1BlockStatement(
            statements,
            location);
    }
}