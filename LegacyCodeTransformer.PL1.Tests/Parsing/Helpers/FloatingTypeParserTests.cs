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
        var parser = CreateFloatingTypeParser(
            "FLOAT;",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FloatingType>(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Float, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, dataType.Base);
        Assert.Null(dataType.Precision);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateFloatingTypeParser(
            "FLOAT DECIMAL(16);",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FloatingType>(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Float, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Decimal, dataType.Base);
        Assert.Equal(16, dataType.Precision);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateFloatingTypeParser(
            "FLOAT BIN(53);",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FloatingType>(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Float, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Binary, dataType.Base);
        Assert.Equal(53, dataType.Precision);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateFloatingTypeParser(
            "REAL;",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FloatingType>(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Real, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, dataType.Base);
        Assert.Null(dataType.Precision);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateFloatingTypeParser(
            "DOUBLE PRECISION;",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FloatingType>(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.DoublePrecision, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, dataType.Base);
        Assert.Null(dataType.Precision);
        Assert.Empty(GetDiagnostics(context));
    }
}