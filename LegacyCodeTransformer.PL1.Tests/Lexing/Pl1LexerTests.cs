using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Tests.Lexing;

/// <summary>
/// Pl1Lexer için token üretim testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Parser'ın doğru çalışabilmesi için önce Lexer'ın kaynak kodu
/// doğru token'lara böldüğünü garanti altına almamız gerekir.
///
/// Bu testler PL/I → EGL pipeline'ın en alt seviyesindeki ilk doğrulamadır.
/// </summary>
public sealed class Pl1LexerTests
{
    [Fact]
    public void Tokenize_WithFixedDecimalDeclaration_ShouldReturnExpectedTokens()
    {
        // Arrange
        var source = "DCL MUST_NO FIXED DECIMAL(8);";
        var lexer = new Pl1Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.DclKeyword, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("MUST_NO", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.FixedKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.DecimalKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("8", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    [Fact]
    public void Tokenize_WithFixedDecimalScaleDeclaration_ShouldReturnCommaToken()
    {
        // Arrange
        var source = "DCL CUSTOMER_NO FIXED DECIMAL(10,2);";
        var lexer = new Pl1Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Contains(tokens, x => x.Kind == Pl1TokenKind.Comma);
        Assert.Contains(tokens, x => x.Kind == Pl1TokenKind.Number && x.Text == "10");
        Assert.Contains(tokens, x => x.Kind == Pl1TokenKind.Number && x.Text == "2");
    }
}