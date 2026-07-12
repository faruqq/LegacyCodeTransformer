using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I PROCEDURE bloğunu parse eden concrete parser sınıfıdır.
///
/// Neden var?
/// ----------------------
/// Pl1Parser ana orchestration sınıfı procedure grammar detaylarını
/// doğrudan bilmemelidir. Procedure header, parameter listesi, option
/// listesi, body sınırı ve END doğrulama davranışı ayrı parser içinde
/// tutulmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// Parametresiz veya parametreli PROCEDURE header bilgisini, options
/// listesini ve procedure içindeki executable statement listesini
/// Pl1Procedure modeline dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// PROCEDURE_NAME: PROCEDURE;
///     CALL FETCH_CURSOR;
/// END PROCEDURE_NAME;
///
/// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
///     ERROR_TEXT = PROCESS_TEXT;
/// END CUSTOMER_PROCESS;
///
/// PROGRAM_NAME: PROCEDURE(ARG1, ARG2) OPTIONS(MAIN);
///     CALL INIT_PROCESS;
/// END PROGRAM_NAME;
///
/// Nerede kullanılır?
/// ----------------------
/// Pl1Parser içinde procedure başlangıcı tespit edildiğinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Procedure parameter declaration eşleştirmesi, local declaration
/// parsing, parameter scope, semantic analysis ve EGL function
/// transpiler entegrasyonu bu parser çıktısı üzerinden ilerleyecektir.
/// </summary>
internal sealed class ProcedureParser : ParserBase
{
    public ProcedureParser(ParseContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Mevcut token pozisyonundan PL/I procedure bloğunu parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Root syntax tree procedure bloklarını declaration ve top-level
    /// statement listelerinden ayrı biçimde taşımalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Procedure adını, ordered parameter listesini, options listesini
    /// ve executable body statement'larını okuyarak Pl1Procedure üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PROCESS_CURSOR: PROCEDURE;
    ///     CALL FETCH_CURSOR;
    /// END PROCESS_CURSOR;
    ///
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1Parser.Parse içinde procedure başlangıcı görüldüğünde çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Procedure body declaration parsing ve parameter semantic
    /// resolution davranışları bu methodun ürettiği model üzerinden
    /// geliştirilecektir.
    /// </summary>
    public HelperParseResult<Pl1Procedure> ParseProcedure()
    {
        var procedureStart = Current.Location;

        if (Current.Kind != Pl1TokenKind.Identifier)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "Procedure adı bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Procedure>(
                null,
                Position);
        }

        var procedureName = Current.Text;

        Advance();

        if (Consume(
                Pl1TokenKind.Colon,
                "':' bekleniyordu.") is null)
        {
            return new HelperParseResult<Pl1Procedure>(
                null,
                Position);
        }

        if (Consume(
                Pl1TokenKind.ProcedureKeyword,
                "PROCEDURE bekleniyordu.") is null)
        {
            return new HelperParseResult<Pl1Procedure>(
                null,
                Position);
        }

        var parameters = Current.Kind == Pl1TokenKind.OpenParenthesis
            ? ParseParameters()
            : new List<string>();

        var options = Current.Kind == Pl1TokenKind.OptionsKeyword
            ? ParseOptions()
            : new List<string>();

        if (Consume(
                Pl1TokenKind.Semicolon,
                "';' bekleniyordu.") is null)
        {
            return new HelperParseResult<Pl1Procedure>(
                null,
                Position);
        }

        var statements = ParseProcedureStatements();

        if (Current.Kind == Pl1TokenKind.EndKeyword)
        {
            ParseProcedureEnd(procedureName);
        }
        else
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    $"END {procedureName}; bekleniyordu.",
                    Current));
        }

        var procedure = new Pl1Procedure(
            procedureName,
            options,
            statements,
            procedureStart,
            parameters);

        return new HelperParseResult<Pl1Procedure>(
            procedure,
            Position);
    }

    /// <summary>
    /// PROCEDURE keyword'ünden sonra gelen ordered parameter listesini
    /// parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Gerçek PL/I procedure tanımları bir veya birden fazla parameter
    /// adını header üzerinde taşıyabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Parantez içindeki identifier listesini sırasını koruyarak
    /// Pl1Procedure.Parameters koleksiyonuna aktarır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PROCEDURE(PROCESS_TEXT)
    ///
    /// PROCEDURE(CUSTOMER_NO, CUSTOMER_NAME)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseProcedure içinde PROCEDURE sonrasında açık parantez
    /// görüldüğünde çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Header parameter adı ile body declaration eşleştirmesi ve EGL
    /// function parameter type/direction analizi bu ordered liste
    /// üzerinden geliştirilecektir.
    /// </summary>
    private List<string> ParseParameters()
    {
        var parameters = new List<string>();

        if (Consume(
                Pl1TokenKind.OpenParenthesis,
                "'(' bekleniyordu.") is null)
        {
            return parameters;
        }

        if (Current.Kind == Pl1TokenKind.CloseParenthesis)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "Procedure parameter adı bekleniyordu.",
                    Current));

            Advance();

            return parameters;
        }

        while (!IsAtEnd() &&
               Current.Kind != Pl1TokenKind.CloseParenthesis)
        {
            if (Current.Kind != Pl1TokenKind.Identifier)
            {
                Diagnostics.Add(
                    ParserDiagnosticFactory.ExpectedToken(
                        "Procedure parameter adı bekleniyordu.",
                        Current));

                Advance();

                continue;
            }

            parameters.Add(Current.Text);
            Advance();

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

                break;
            }
        }

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        return parameters;
    }

    private List<string> ParseOptions()
    {
        var options = new List<string>();

        Advance();

        if (Consume(
                Pl1TokenKind.OpenParenthesis,
                "'(' bekleniyordu.") is null)
        {
            return options;
        }

        while (!IsAtEnd() &&
               Current.Kind != Pl1TokenKind.CloseParenthesis)
        {
            if (Current.Kind != Pl1TokenKind.Identifier)
            {
                Diagnostics.Add(
                    ParserDiagnosticFactory.ExpectedToken(
                        "Procedure option adı bekleniyordu.",
                        Current));

                Advance();

                continue;
            }

            options.Add(Current.Text);
            Advance();

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

                break;
            }
        }

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        return options;
    }

    /// <summary>
    /// Procedure body içindeki executable statement listesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ProcedureParser yalnızca procedure sınırını yönetmelidir. CALL,
    /// IF, DO ve assignment gibi statement grammar detayları mevcut
    /// StatementParser sorumluluğunda kalmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// END PROCEDURE_NAME sınırına kadar executable statement'ları
    /// mevcut statement parser pipeline üzerinden parse eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PROCESS_CURSOR: PROCEDURE;
    ///     PARAM = 'ABC';
    ///     CALL FETCH_CURSOR;
    /// END PROCESS_CURSOR;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseProcedure içinde header tamamlandıktan sonra kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Procedure local declaration desteği ayrı bir body item ayrımıyla
    /// eklendiğinde bu method kontrollü biçimde genişletilecektir.
    /// </summary>
    private List<Pl1Statement> ParseProcedureStatements()
    {
        var statements = new List<Pl1Statement>();

        while (!IsAtEnd() &&
               !IsProcedureEnd())
        {
            var statementStartPosition = Position;
            var statementParser = new StatementParser(Context);
            var result = statementParser.ParseStatement();

            Position = result.Position;

            if (result.Value is not null)
            {
                statements.Add(result.Value);

                continue;
            }

            if (Position == statementStartPosition)
            {
                Diagnostics.Add(
                    ParserDiagnosticFactory.UnexpectedToken(
                        Current,
                        "procedure executable statement veya END procedure"));

                var recoveryHelper =
                    new StatementRecoveryHelper(Context);

                recoveryHelper.SkipCurrentStatement();
            }
        }

        return statements;
    }

    private void ParseProcedureEnd(string procedureName)
    {
        Consume(
            Pl1TokenKind.EndKeyword,
            "END bekleniyordu.");

        if (Current.Kind != Pl1TokenKind.Identifier)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    $"END sonrası procedure adı bekleniyordu: " +
                    $"{procedureName}.",
                    Current));

            return;
        }

        var endName = Current.Text;

        Advance();

        if (!string.Equals(
                endName,
                procedureName,
                StringComparison.OrdinalIgnoreCase))
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    $"END procedure adı '{procedureName}' olmalıydı.",
                    Current));
        }

        Consume(
            Pl1TokenKind.Semicolon,
            "';' bekleniyordu.");
    }

    private bool IsProcedureEnd()
    {
        return Current.Kind == Pl1TokenKind.EndKeyword &&
               Peek(1).Kind == Pl1TokenKind.Identifier;
    }
}