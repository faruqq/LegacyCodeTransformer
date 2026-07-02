using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing;

/// <summary>
/// Pl1Parser için syntax tree üretim testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Lexer token üretimini doğruladık.
/// Şimdi bu token'ların doğru PL/I syntax modeline dönüştürüldüğünü
/// garanti altına almamız gerekir.
///
/// Bu testler özellikle DCL FIXED DECIMAL ifadelerinin
/// Pl1VariableDeclaration ve Pl1FixedDecimalType olarak parse edildiğini doğrular.
/// </summary>
public sealed class Pl1ParserTests
{
    [Fact]
    public void Parse_WithFixedDecimalDeclaration_ShouldCreateVariableDeclaration()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL MUST_NO FIXED DECIMAL(8);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("MUST_NO", declaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(declaration.DataType);

        Assert.Equal(8, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
    }

    [Fact]
    public void Parse_WithFixedDecimalScaleDeclaration_ShouldCreateVariableDeclarationWithScale()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL CUSTOMER_NO FIXED DECIMAL(10,2);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("CUSTOMER_NO", declaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(declaration.DataType);

        Assert.Equal(10, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
    }

    [Fact]
    public void Parse_WithMissingSemicolon_ShouldReturnDiagnosticError()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL MUST_NO FIXED DECIMAL(8)").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Diagnostics);
        Assert.Contains(result.Diagnostics, x => x.Message.Contains("';' bekleniyordu."));
    }
}