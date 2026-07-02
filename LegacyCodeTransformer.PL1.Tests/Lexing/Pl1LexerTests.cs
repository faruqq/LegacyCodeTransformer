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

    /// <summary>
    /// PL/I CHAR veri tipi keyword'ünün doğru token'a dönüştürüldüğünü doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// CHAR desteği parser'a gelmeden önce lexer seviyesinde doğru token
    /// üretilmelidir.
    /// Parser, CHAR söz dizimini ancak CharKeyword token'ı sayesinde
    /// karakter veri tipi olarak algılayabilir.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL PARAM CHAR(08);
    ///
    /// Beklenen token:
    /// - CharKeyword
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Lexer testlerinde
    /// - CHAR declaration parse sürecinin ilk doğrulama adımında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// INIT, structure ve array desteği geldiğinde CHAR keyword davranışının
    /// bozulmadığını garanti eder.
    /// </summary>
    [Fact]
    public void Tokenize_WithCharDeclaration_ShouldReturnCharKeywordToken()
    {
        // Arrange
        var source = "DCL PARAM CHAR(08);";
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
                Assert.Equal("PARAM", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CharKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("08", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// PL/I CHARACTER veri tipi keyword'ünün doğru token'a dönüştürüldüğünü doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I dilinde CHARACTER, CHAR veri tipinin uzun yazımıdır.
    /// Lexer bu keyword'ü normal identifier olarak bırakırsa parser
    /// CHARACTER(n) söz dizimini veri tipi olarak algılayamaz.
    ///
    /// Test edilen PL/I:
    ///
    /// DECLARE PARAM CHARACTER(08);
    ///
    /// Beklenen token'lar:
    /// - DclKeyword
    /// - CharacterKeyword
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Lexer testlerinde
    /// - DECLARE alias desteğini ve CHARACTER keyword ayrımını doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Farklı PL/I kod yazım standartlarında CHAR yerine CHARACTER kullanılsa
    /// bile dönüşüm hattının çalışmaya devam ettiğini garanti eder.
    /// </summary>
    [Fact]
    public void Tokenize_WithCharacterDeclaration_ShouldReturnCharacterKeywordToken()
    {
        // Arrange
        var source = "DECLARE PARAM CHARACTER(08);";
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
                Assert.Equal("PARAM", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CharacterKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("08", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }
}