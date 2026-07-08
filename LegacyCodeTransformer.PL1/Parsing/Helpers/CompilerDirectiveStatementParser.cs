using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I compiler directive statement parse eden concrete parser sınıfıdır.
///
/// Neden var?
/// ----------------------
/// %INCLUDE, %PAGE, %EJECT gibi compiler directive satırları executable PL/I
/// statement değildir; ancak kaynak yapının parçasıdır ve kaybedilmemelidir.
///
/// Ne çözüyor?
/// ----------------------
/// '%' ile başlayan directive statement'ı semicolon'a kadar okuyarak
/// Pl1CompilerDirectiveStatement modeline dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// %INCLUDE COPYLIB;
/// %PAGE;
/// %EJECT;
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser Percent token gördüğünde bu parser'ı çağırır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// INCLUDE resolution, listing directive filtering ve compiler directive semantic
/// analysis davranışları için temel olur.
/// </summary>
internal sealed class CompilerDirectiveStatementParser : ParserBase
{
    public CompilerDirectiveStatementParser(ParseContext context)
        : base(context)
    {
    }

    public HelperParseResult<Pl1Statement> ParseCompilerDirectiveStatement()
    {
        var startLocation = Current.Location;

        if (Consume(Pl1TokenKind.Percent, "'%' bekleniyordu.") is null)
        {
            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        if (Current.Kind == Pl1TokenKind.Semicolon || IsAtEnd())
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "Compiler directive adı bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        var directiveName = Current.Text;
        var directiveTokens = new List<Pl1Token>();

        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.Semicolon)
        {
            directiveTokens.Add(Advance());
        }

        Consume(Pl1TokenKind.Semicolon, "';' bekleniyordu.");

        var rawDirectiveText = "%" + BuildRawText(directiveTokens);

        var statement = new Pl1CompilerDirectiveStatement(
            directiveName,
            rawDirectiveText,
            startLocation);

        return new HelperParseResult<Pl1Statement>(
            statement,
            Position);
    }

    private static string BuildRawText(IReadOnlyList<Pl1Token> tokens)
    {
        return string.Join(
            " ",
            tokens.Select(GetTokenText));
    }

    private static string GetTokenText(Pl1Token token)
    {
        return token.Kind == Pl1TokenKind.StringLiteral
            ? $"'{token.Text}'"
            : token.Text;
    }
}