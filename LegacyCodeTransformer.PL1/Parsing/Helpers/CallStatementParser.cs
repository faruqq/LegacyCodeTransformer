using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I CALL statement parse davranışını yönetir.
///
/// Neden var?
/// ----------------------
/// StatementParser orchestration sınıfı CALL grammar detaylarını doğrudan bilmemelidir.
/// Procedure çağrısı parse davranışı ayrı bir concrete parser içinde tutulmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// CALL keyword ile başlayan PL/I procedure çağrılarını Pl1CallStatement modeline
/// dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     CALL FETCH_CURSOR;
///     CALL SQL_HATA_OLUSTUR('SELECT GLAU_HISTORY');
///     CALL PROC1(A, 'ABC', B);
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser içinde StatementParserKind.Call seçildiğinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Procedure dependency çıkarımı, call graph üretimi, parameter semantic analysis
/// ve EGL function/service call generation çalışmalarına temel olur.
/// </summary>
internal sealed class CallStatementParser : ParserBase
{
    public CallStatementParser(ParseContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Mevcut token pozisyonundan CALL statement parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// CALL statement, PL/I executable statement ailesinin temel üyelerinden biridir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// CALL keyword'ünü, procedure adını, opsiyonel argument listesini ve statement
    /// sonlandırıcı semicolon'u okuyarak Pl1CallStatement modeli üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     CALL FETCH_CURSOR;
    ///     CALL PROC1(A, 'ABC', B);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementParser.ParseStatement içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Procedure resolution, parameter count validation ve EGL call statement üretimi
    /// bu model üzerinden ilerleyecektir.
    /// </summary>
    public HelperParseResult<Pl1Statement> ParseCallStatement()
    {
        var statementStart = Current.Location;

        if (Current.Kind != Pl1TokenKind.CallKeyword)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "CALL bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        Advance();

        if (Current.Kind != Pl1TokenKind.Identifier)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "CALL procedure adı bekleniyordu.",
                    Current));

            var recoveryHelper = new StatementRecoveryHelper(Context);
            recoveryHelper.SkipCurrentStatement();

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        var procedureName = Current.Text;

        Advance();

        var arguments = Current.Kind == Pl1TokenKind.OpenParenthesis
            ? ParseArgumentList(statementStart)
            : new List<Pl1Expression>();

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

        var statement = new Pl1CallStatement(
            procedureName,
            arguments,
            statementStart);

        return new HelperParseResult<Pl1Statement>(
            statement,
            Position);
    }

    private List<Pl1Expression> ParseArgumentList(SourceLocation location)
    {
        var arguments = new List<Pl1Expression>();
        var tokenReader = new DelimitedTokenReader(Context);
        var recoveryHelper = new StatementRecoveryHelper(Context);

        Advance();

        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.CloseParenthesis)
        {
            var argumentTokens = tokenReader.ReadUntilAny(
                Pl1TokenKind.Comma,
                Pl1TokenKind.CloseParenthesis);

            if (argumentTokens.Count > 0)
            {
                arguments.Add(
                    ExpressionFactory.Create(
                        argumentTokens,
                        location));
            }

            if (Current.Kind == Pl1TokenKind.Comma)
            {
                Advance();
                continue;
            }

            if (Current.Kind != Pl1TokenKind.CloseParenthesis)
            {
                Diagnostics.Add(
                    ParserDiagnosticFactory.ExpectedToken(
                        "',' veya ')' bekleniyordu.",
                        Current));

                recoveryHelper.SkipCurrentStatement();

                return arguments;
            }
        }

        if (Current.Kind != Pl1TokenKind.CloseParenthesis)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "')' bekleniyordu.",
                    Current));

            return arguments;
        }

        Advance();

        return arguments;
    }
}