using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
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
/// listesi, body declaration'ları, executable statement'lar ve END
/// doğrulama davranışı ayrı parser içinde tutulmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// Parametresiz veya parametreli PROCEDURE header bilgisini, options
/// listesini, procedure içindeki declaration modellerini ve executable
/// statement listesini Pl1Procedure modeline dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
///     DCL PROCESS_TEXT CHAR(50);
///
///     ERROR_TEXT = PROCESS_TEXT;
///     CALL WRITE_ERROR(ERROR_TEXT);
/// END CUSTOMER_PROCESS;
///
/// Nerede kullanılır?
/// ----------------------
/// Pl1Parser içinde procedure başlangıcı tespit edildiğinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Procedure parameter declaration eşleştirmesi, local scope,
/// semantic analysis ve EGL function transpiler entegrasyonu bu parser
/// çıktısı üzerinden ilerleyecektir.
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
    /// Root syntax tree procedure bloklarını top-level declaration ve
    /// statement listelerinden ayrı biçimde taşımalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Procedure adını, ordered parameter listesini, options listesini,
    /// local declaration modellerini ve executable body statement'larını
    /// okuyarak Pl1Procedure üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1Parser.Parse içinde procedure başlangıcı görüldüğünde çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parameter declaration resolution ve procedure-level semantic
    /// analysis davranışları bu methodun ürettiği model üzerinden
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

        var parameters = Current.Kind ==
            Pl1TokenKind.OpenParenthesis
                ? ParseParameters()
                : new List<string>();

        var options = Current.Kind ==
            Pl1TokenKind.OptionsKeyword
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

        var declarations = new List<Pl1Declaration>();
        var statements = new List<Pl1Statement>();

        ParseProcedureBody(
            declarations,
            statements);

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
            parameters,
            declarations);

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
    /// Procedure body içindeki declaration ve executable statement
    /// modellerini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Gerçek PL/I procedure'lerinde header parameter'larının veri tipi
    /// body içindeki DCL statement ile tanımlanabilir. Procedure body
    /// yalnızca executable statement'lardan oluşmak zorunda değildir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DCL token'ı görüldüğünde mevcut DeclarationParser'ı, diğer
    /// statement başlangıçlarında mevcut StatementParser'ı kullanır.
    /// Böylece declaration grammar'ı tekrar implemente edilmez.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseProcedure içinde header tamamlandıktan sonra kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Procedure local symbol table, parameter declaration binding ve
    /// scope analysis bu ayrılmış koleksiyonlar üzerinden ilerler.
    /// </summary>
    private void ParseProcedureBody(
        ICollection<Pl1Declaration> declarations,
        ICollection<Pl1Statement> statements)
    {
        while (!IsAtEnd() &&
               !IsProcedureEnd())
        {
            var bodyItemStartPosition = Position;

            if (Current.Kind == Pl1TokenKind.DclKeyword)
            {
                var declarationParser =
                    new DeclarationParser(Context);

                var declarationResult =
                    declarationParser.ParseDeclaration();

                Position = declarationResult.Position;

                if (declarationResult.Value is not null)
                {
                    declarations.Add(
                        declarationResult.Value);

                    continue;
                }
            }
            else
            {
                var statementParser =
                    new StatementParser(Context);

                var statementResult =
                    statementParser.ParseStatement();

                Position = statementResult.Position;

                if (statementResult.Value is not null)
                {
                    statements.Add(
                        statementResult.Value);

                    continue;
                }
            }

            if (Position == bodyItemStartPosition)
            {
                Diagnostics.Add(
                    ParserDiagnosticFactory.UnexpectedToken(
                        Current,
                        "procedure declaration, executable " +
                        "statement veya END procedure"));

                var recoveryHelper =
                    new StatementRecoveryHelper(Context);

                recoveryHelper.SkipCurrentStatement();
            }
        }
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
                    "END sonrası procedure adı bekleniyordu: " +
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