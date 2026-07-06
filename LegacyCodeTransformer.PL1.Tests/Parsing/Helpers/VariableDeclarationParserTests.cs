using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class VariableDeclarationParserTests : ParserHelperTestBase
{
    /// <summary>
    /// Basic variable declaration bilgisinin Pl1VariableDeclaration olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// VariableDeclarationParser'ın DCL PARAM CHAR(08); syntax bilgisini değişken adı ve data type ile modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı PARAM, data type Pl1CharacterType(8) olmalıdır.
    /// </summary>
    [Fact]
    public void ParseVariableDeclaration_WithBasicCharDeclaration_ShouldCreateVariableDeclaration()
    {
        var tokens = new Pl1Lexer("DCL PARAM CHAR(08);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new VariableDeclarationParser(tokens, 0, diagnostics);

        var result = parser.ParseVariableDeclaration();

        Assert.NotNull(result.Declaration);
        Assert.Equal("PARAM", result.Declaration!.Name);
        Assert.Null(result.Declaration.ArraySize);
        Assert.Null(result.Declaration.InitialValue);

        var dataType = Assert.IsType<Pl1CharacterType>(result.Declaration.DataType);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Variable declaration içindeki INIT değerinin Pl1VariableDeclaration üzerinde korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// VariableDeclarationParser'ın INIT(' ') bilgisini InitialValueParser üzerinden okuyup declaration modeline taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08) INIT(' ');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// InitialValue Value boşluk olmalıdır.
    /// </summary>
    [Fact]
    public void ParseVariableDeclaration_WithInitialValue_ShouldSetInitialValue()
    {
        var tokens = new Pl1Lexer("DCL PARAM CHAR(08) INIT(' ');").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new VariableDeclarationParser(tokens, 0, diagnostics);

        var result = parser.ParseVariableDeclaration();

        Assert.NotNull(result.Declaration);
        Assert.NotNull(result.Declaration!.InitialValue);
        Assert.Equal(" ", result.Declaration.InitialValue!.Value);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Name-based array declaration bilgisinin ArraySize olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// VariableDeclarationParser'ın PARAM(2) syntax bilgisini declaration ArraySize alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM(2) CHAR(10);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ArraySize 2 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseVariableDeclaration_WithNameArraySize_ShouldSetArraySize()
    {
        var tokens = new Pl1Lexer("DCL PARAM(2) CHAR(10);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new VariableDeclarationParser(tokens, 0, diagnostics);

        var result = parser.ParseVariableDeclaration();

        Assert.NotNull(result.Declaration);
        Assert.Equal(2, result.Declaration!.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(result.Declaration.DataType);
        Assert.Equal(10, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DIM attribute bilgisinin ArraySize olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// VariableDeclarationParser'ın data type sonrasında gelen DIM(2) bilgisini declaration ArraySize alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(10) DIM(2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ArraySize 2 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseVariableDeclaration_WithDimensionAttribute_ShouldSetArraySize()
    {
        var tokens = new Pl1Lexer("DCL PARAM CHAR(10) DIM(2);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new VariableDeclarationParser(tokens, 0, diagnostics);

        var result = parser.ParseVariableDeclaration();

        Assert.NotNull(result.Declaration);
        Assert.Equal(2, result.Declaration!.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(result.Declaration.DataType);
        Assert.Equal(10, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }
}