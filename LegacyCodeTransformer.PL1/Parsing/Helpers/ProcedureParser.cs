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
/// Pl1Parser ana orchestration sınıfı procedure grammar detaylarını doğrudan
/// bilmemelidir. Procedure header, option listesi, body sınırı ve END doğrulama
/// davranışı ayrı parser içinde tutulmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// PROCEDURE_NAME: PROCEDURE; ile başlayan ve END PROCEDURE_NAME; ile biten
/// PL/I procedure bloklarını Pl1Procedure modeline dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// PROCEDURE_NAME: PROCEDURE;
///     CALL FETCH_CURSOR;
/// END PROCEDURE_NAME;
///
/// PROGRAM_NAME: PROCEDURE OPTIONS(MAIN);
///     CALL INIT_PROCESS;
/// END PROGRAM_NAME;
///
/// Nerede kullanılır?
/// ----------------------
/// Pl1Parser içinde procedure başlangıcı tespit edildiğinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Procedure-level semantic analysis, call graph extraction, main procedure
/// tespiti ve procedure transpiler entegrasyonu bu parser çıktısı üzerinden
/// ilerleyecektir.
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
    /// P06 ile birlikte root syntax tree yalnızca global statement listesi değil,
    /// procedure bloklarını da taşımalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Procedure header bilgisini, options listesini ve procedure içindeki executable
    /// statement listesini okuyarak Pl1Procedure üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PROCESS_CURSOR: PROCEDURE;
    ///     CALL FETCH_CURSOR;
    /// END PROCESS_CURSOR;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1Parser.Parse içinde procedure başlangıcı görüldüğünde çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Procedure body içindeki statement çeşitleri arttıkça bu method aynı statement
    /// parser pipeline üzerinden genişlemeye devam edecektir.
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

        if (Consume(Pl1TokenKind.Colon, "':' bekleniyordu.") is null)
        {
            return new HelperParseResult<Pl1Procedure>(
                null,
                Position);
        }

        if (Consume(Pl1TokenKind.ProcedureKeyword, "PROCEDURE bekleniyordu.") is null)
        {
            return new HelperParseResult<Pl1Procedure>(
                null,
                Position);
        }

        var options = Current.Kind == Pl1TokenKind.OptionsKeyword
            ? ParseOptions()
            : new List<string>();

        if (Consume(Pl1TokenKind.Semicolon, "';' bekleniyordu.") is null)
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
            procedureStart);

        return new HelperParseResult<Pl1Procedure>(
            procedure,
            Position);
    }

    private List<string> ParseOptions()
    {
        var options = new List<string>();

        Advance();

        if (Consume(Pl1TokenKind.OpenParenthesis, "'(' bekleniyordu.") is null)
        {
            return options;
        }

        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.CloseParenthesis)
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

        Consume(Pl1TokenKind.CloseParenthesis, "')' bekleniyordu.");

        return options;
    }

    /// <summary>
    /// Procedure body içindeki executable statement listesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ProcedureParser yalnızca procedure sınırını yönetmelidir. CALL, IF, DO,
    /// assignment gibi statement türlerini tanımak ProcedureParser sorumluluğu
    /// değildir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// END PROCEDURE_NAME; sınırına kadar olan executable statement'ları mevcut
    /// StatementParser pipeline üzerinden parse eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PROCESS_CURSOR: PROCEDURE;
    ///     PARAM = 'ABC';
    ///     IF SQLCODE = 0 THEN CALL FETCH_CURSOR;
    ///     DO WHILE(SQLCODE = 0);
    ///         CALL FETCH_CURSOR;
    ///     END;
    /// END PROCESS_CURSOR;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParseProcedure içinde header parse edildikten sonra kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// StatementParser SELECT, READ, WRITE, RETURN, STOP, LEAVE veya EXEC SQL
    /// desteği kazandığında ProcedureParser değişmeden bu statement'ları
    /// procedure body içinde destekleyebilir.
    /// </summary>
    private List<Pl1Statement> ParseProcedureStatements()
    {
        var statements = new List<Pl1Statement>();

        while (!IsAtEnd() && !IsProcedureEnd())
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

                var recoveryHelper = new StatementRecoveryHelper(Context);
                recoveryHelper.SkipCurrentStatement();
            }
        }

        return statements;
    }

    private void ParseProcedureEnd(string procedureName)
    {
        Consume(Pl1TokenKind.EndKeyword, "END bekleniyordu.");

        if (Current.Kind != Pl1TokenKind.Identifier)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    $"END sonrası procedure adı bekleniyordu: {procedureName}.",
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

        Consume(Pl1TokenKind.Semicolon, "';' bekleniyordu.");
    }

    private bool IsProcedureEnd()
    {
        return Current.Kind == Pl1TokenKind.EndKeyword
            && Peek(1).Kind == Pl1TokenKind.Identifier;
    }
}