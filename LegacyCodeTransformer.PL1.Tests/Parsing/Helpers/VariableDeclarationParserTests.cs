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
        var parser = CreateVariableDeclarationParser(
            "DCL PARAM CHAR(08);",
            out var context);

        var result = parser.ParseVariableDeclaration();

        Assert.NotNull(result.Value);
        Assert.Equal("PARAM", result.Value!.Name);
        Assert.Null(result.Value.ArraySize);
        Assert.Null(result.Value.InitialValue);

        var dataType = Assert.IsType<Pl1CharacterType>(result.Value.DataType);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateVariableDeclarationParser(
            "DCL PARAM CHAR(08) INIT(' ');",
            out var context);

        var result = parser.ParseVariableDeclaration();

        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.InitialValue);
        Assert.Equal(" ", result.Value.InitialValue!.Value);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateVariableDeclarationParser(
            "DCL PARAM(2) CHAR(10);",
            out var context);

        var result = parser.ParseVariableDeclaration();

        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(result.Value.DataType);
        Assert.Equal(10, dataType.Length);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateVariableDeclarationParser(
            "DCL PARAM CHAR(10) DIM(2);",
            out var context);

        var result = parser.ParseVariableDeclaration();

        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(result.Value.DataType);
        Assert.Equal(10, dataType.Length);
        Assert.Empty(GetDiagnostics(context));
    }
}