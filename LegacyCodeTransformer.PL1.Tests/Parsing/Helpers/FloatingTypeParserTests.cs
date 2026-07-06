using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class FloatingTypeParserTests : ParserHelperTestBase
{
    /// <summary>
    /// FLOAT token akışından Pl1FloatingType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// FloatingTypeParser'ın FLOAT keyword'ünü Kind Float, Base Unspecified ve Precision null olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// FLOAT;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DataType Kind Float, Base Unspecified, Precision null olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFloatKeyword_ShouldCreateFloatType()
    {
        // Arrange
        var tokens = new Pl1Lexer("FLOAT;").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new FloatingTypeParser(
            tokens,
            0,
            diagnostics);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Float, result.Value!.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, result.Value.Base);
        Assert.Null(result.Value.Precision);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// FLOAT DECIMAL precision bilgisinin Pl1FloatingType üzerinde korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// FloatingTypeParser'ın DECIMAL base bilgisini ve parantez içi precision değerini doğru okuduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// FLOAT DECIMAL(16);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DataType Kind Float, Base Decimal, Precision 16 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFloatDecimalPrecision_ShouldCreateDecimalFloatType()
    {
        // Arrange
        var tokens = new Pl1Lexer("FLOAT DECIMAL(16);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new FloatingTypeParser(
            tokens,
            0,
            diagnostics);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Float, result.Value!.Kind);
        Assert.Equal(Pl1FloatingBase.Decimal, result.Value.Base);
        Assert.Equal(16, result.Value.Precision);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// FLOAT BIN precision bilgisinin Pl1FloatingType üzerinde korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// FloatingTypeParser'ın BIN base bilgisini ve parantez içi precision değerini doğru okuduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// FLOAT BIN(53);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DataType Kind Float, Base Binary, Precision 53 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFloatBinaryPrecision_ShouldCreateBinaryFloatType()
    {
        // Arrange
        var tokens = new Pl1Lexer("FLOAT BIN(53);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new FloatingTypeParser(
            tokens,
            0,
            diagnostics);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Float, result.Value!.Kind);
        Assert.Equal(Pl1FloatingBase.Binary, result.Value.Base);
        Assert.Equal(53, result.Value.Precision);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// REAL token akışından Pl1FloatingType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// FloatingTypeParser'ın REAL keyword'ünü Kind Real olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// REAL;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DataType Kind Real, Base Unspecified, Precision null olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithRealKeyword_ShouldCreateRealType()
    {
        // Arrange
        var tokens = new Pl1Lexer("REAL;").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new FloatingTypeParser(
            tokens,
            0,
            diagnostics);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Real, result.Value!.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, result.Value.Base);
        Assert.Null(result.Value.Precision);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DOUBLE PRECISION token akışından Pl1FloatingType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// FloatingTypeParser'ın DOUBLE PRECISION keyword ikilisini DoublePrecision kind olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DOUBLE PRECISION;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DataType Kind DoublePrecision, Base Unspecified, Precision null olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithDoublePrecisionKeyword_ShouldCreateDoublePrecisionType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DOUBLE PRECISION;").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new FloatingTypeParser(
            tokens,
            0,
            diagnostics);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.DoublePrecision, result.Value!.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, result.Value.Base);
        Assert.Null(result.Value.Precision);
        Assert.Empty(diagnostics.Diagnostics);
    }
}