using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Parsing;

/// <summary>
/// PL/I token listesini Pl1SyntaxTree modeline dönüştüren ana parser sınıfıdır.
///
/// Neden var?
/// Lexer yalnızca kaynak PL/I metnini token listesine ayırır. Bu token listesinin
/// declaration, procedure ve executable statement modellerine dönüştürülmesi gerekir.
///
/// Ne çözüyor?
/// Parser ana orchestration katmanı olarak çalışır. Declaration detaylarını
/// DeclarationParser'a, procedure detaylarını ProcedureParser'a ve executable
/// statement detaylarını StatementParser'a yönlendirir.
///
/// Hangi örneği destekliyor?
/// DCL PARAM CHAR(08);
/// PROCESS_CURSOR: PROCEDURE;
///     CALL FETCH_CURSOR;
/// END PROCESS_CURSOR;
///
/// Nerede kullanılır?
/// ConversionService pipeline içinde, parser unit testlerinde ve transpiler öncesinde
/// PL/I syntax tree oluşturmak için kullanılır.
///
/// Gelecekte neye temel olur?
/// Procedure parser statement integration, statement pipeline, semantic analysis ve
/// transpiler giriş modeli bu orchestration üzerinden ilerleyecektir.
/// </summary>
public sealed class Pl1Parser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics = new();
    private int _position;

    public Pl1Parser(IReadOnlyList<Pl1Token>? tokens)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
    }

    /// <summary>
    /// Token listesini okuyarak Pl1SyntaxTree üretir.
    ///
    /// Neden var?
    /// PL/I kaynak kodundan gelen token listesi dönüşüm pipeline'ında kullanılabilecek
    /// güçlü tipli syntax tree modeline çevrilmelidir.
    ///
    /// Ne çözüyor?
    /// DCL / DECLARE gördüğünde declaration parser'a yönlendirir.
    /// PROCEDURE_NAME: PROCEDURE; gördüğünde procedure parser'a yönlendirir.
    /// Statement başlangıcı gördüğünde statement parser'a yönlendirir.
    /// Desteklenmeyen token gördüğünde diagnostic üretir ve recovery için ilerler.
    ///
    /// Hangi örneği destekliyor?
    /// DCL PARAM CHAR(08);
    /// PROCESS_CURSOR: PROCEDURE;
    ///     CALL FETCH_CURSOR;
    /// END PROCESS_CURSOR;
    ///
    /// Nerede kullanılır?
    /// ConversionService içinde ve parser unit testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// Procedure body parsing ve statement pipeline entegrasyonu bu orchestration
    /// üzerinden ilerleyecektir.
    /// </summary>
    public ParseResult<Pl1SyntaxTree> Parse()
    {
        var declarations = new List<Pl1Declaration>();
        var procedures = new List<Pl1Procedure>();
        var statements = new List<Pl1Statement>();

        while (!IsAtEnd())
        {
            if (Current.Kind == Pl1TokenKind.DclKeyword)
            {
                var declaration = ParseDeclaration();

                if (declaration is not null)
                {
                    declarations.Add(declaration);
                }

                continue;
            }

            if (IsProcedureStart())
            {
                var procedure = ParseProcedure();

                if (procedure is not null)
                {
                    procedures.Add(procedure);
                }

                continue;
            }

            if (IsStatementStart(Current.Kind))
            {
                var statement = ParseStatement();

                if (statement is not null)
                {
                    statements.Add(statement);
                }

                continue;
            }

            AddUnexpectedTokenDiagnostic(
                Current,
                "DCL, procedure veya executable statement");

            Advance();
        }

        var syntaxTree = new Pl1SyntaxTree(
            declarations,
            procedures,
            statements,
            SourceLocation.Unknown);

        return new ParseResult<Pl1SyntaxTree>(
            syntaxTree,
            _diagnostics.Diagnostics);
    }

    private Pl1Declaration? ParseDeclaration()
    {
        var parser = new DeclarationParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseDeclaration();

        _position = result.Position;

        return result.Value;
    }

    private Pl1Procedure? ParseProcedure()
    {
        var context = new ParseContext(
            _tokens,
            _position,
            _diagnostics);

        var parser = new ProcedureParser(context);
        var result = parser.ParseProcedure();

        _position = result.Position;

        return result.Value;
    }

    private Pl1Statement? ParseStatement()
    {
        var parser = new StatementParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseStatement();

        _position = result.Position;

        return result.Value;
    }

    private bool IsProcedureStart()
    {
        return Current.Kind == Pl1TokenKind.Identifier
            && Peek(1).Kind == Pl1TokenKind.Colon
            && Peek(2).Kind == Pl1TokenKind.ProcedureKeyword;
    }

    private static bool IsStatementStart(Pl1TokenKind tokenKind)
    {
        var dispatcher = new StatementDispatcher();

        return dispatcher.IsStatementStart(tokenKind);
    }

    private void AddUnexpectedTokenDiagnostic(
        Pl1Token token,
        string expectedText)
    {
        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenmeyen token: {token.Text}. Beklenen: {expectedText}.",
            token.Location));
    }

    private void Advance()
    {
        if (!IsAtEnd())
        {
            _position++;
        }
    }

    private Pl1Token Peek(int offset)
    {
        var index = _position + offset;

        if (index >= _tokens.Count)
        {
            return _tokens[^1];
        }

        return _tokens[index];
    }

    private bool IsAtEnd()
    {
        return Current.Kind == Pl1TokenKind.EndOfFile;
    }

    private Pl1Token Current
    {
        get
        {
            if (_position >= _tokens.Count)
            {
                return _tokens[^1];
            }

            return _tokens[_position];
        }
    }
}