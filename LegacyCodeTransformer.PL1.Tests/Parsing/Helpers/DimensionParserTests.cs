using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class DimensionParserTests
{
    /// <summary>
    /// Name-based array size bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DimensionParser'ın (2) syntax bilgisini ArraySize 2 olarak parse ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// (2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ArraySize 2 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseOptionalArraySize_WithParenthesizedSize_ShouldReturnArraySize()
    {
        var tokens = new Pl1Lexer("(2);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DimensionParser(tokens, 0, diagnostics);

        var result = parser.ParseOptionalArraySize();

        Assert.Equal(2, result.ArraySize);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DIM attribute array size bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DimensionParser'ın DIM(2) syntax bilgisini ArraySize 2 olarak parse ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DIM(2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ArraySize 2 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseOptionalDimensionSize_WithDimKeyword_ShouldReturnArraySize()
    {
        var tokens = new Pl1Lexer("DIM(2);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DimensionParser(tokens, 0, diagnostics);

        var result = parser.ParseOptionalDimensionSize();

        Assert.Equal(2, result.ArraySize);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DIMENSION attribute array size bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DimensionParser'ın DIMENSION(3) syntax bilgisini ArraySize 3 olarak parse ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DIMENSION(3);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ArraySize 3 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseOptionalDimensionSize_WithDimensionKeyword_ShouldReturnArraySize()
    {
        var tokens = new Pl1Lexer("DIMENSION(3);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DimensionParser(tokens, 0, diagnostics);

        var result = parser.ParseOptionalDimensionSize();

        Assert.Equal(3, result.ArraySize);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Name-based array size ve DIMENSION array size conflict durumunda diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DimensionParser'ın iki farklı array size kaynağı dolu olduğunda diagnostic ürettiğini ve name-based değeri öncelikli kabul ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// nameArraySize 2, dimensionArraySize 3.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Sonuç 2 olmalı ve diagnostic üretilmelidir.
    /// </summary>
    [Fact]
    public void ResolveArraySize_WithBothSources_ShouldReturnNameArraySizeAndDiagnostic()
    {
        var tokens = new Pl1Lexer(";").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DimensionParser(tokens, 0, diagnostics);

        var result = parser.ResolveArraySize(
            2,
            3,
            SourceLocation.Unknown);

        Assert.Equal(2, result);
        Assert.Single(diagnostics.Diagnostics);
        Assert.Contains(
            "Array boyutu hem isim sonrasında hem de DIM / DIMENSION attribute ile verilemez.",
            diagnostics.Diagnostics[0].Message);
    }
}