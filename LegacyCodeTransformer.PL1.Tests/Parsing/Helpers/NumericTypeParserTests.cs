using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class NumericTypeParserTests : ParserHelperTestBase
{
    /// <summary>
    /// FIXED DECIMAL token akışından Pl1FixedDecimalType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// NumericTypeParser'ın FIXED DECIMAL(15) syntax'ını decimal fixed modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// FIXED DECIMAL(15);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1FixedDecimalType Precision 15, Scale null olmalıdır.
    /// </summary>
    [Fact]
    public void ParseFixedBasedType_WithFixedDecimal_ShouldCreateFixedDecimalType()
    {
        var parser = CreateNumericTypeParser(
            "FIXED DECIMAL(15);",
            out var context);

        var result = parser.ParseFixedBasedType();

        var dataType = Assert.IsType<Pl1FixedDecimalType>(result.Value);
        Assert.Equal(15, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// FIXED DEC token akışından scale bilgisiyle Pl1FixedDecimalType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// NumericTypeParser'ın DEC synonym ve `(p,s)` scale bilgisini desteklediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// FIXED DEC(17,2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1FixedDecimalType Precision 17, Scale 2 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseFixedBasedType_WithFixedDecHavingScale_ShouldCreateFixedDecimalType()
    {
        var parser = CreateNumericTypeParser(
            "FIXED DEC(17,2);",
            out var context);

        var result = parser.ParseFixedBasedType();

        var dataType = Assert.IsType<Pl1FixedDecimalType>(result.Value);
        Assert.Equal(17, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// DECIMAL FIXED token akışından Pl1FixedDecimalType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// NumericTypeParser'ın ters keyword sırasındaki DECIMAL FIXED syntax'ını desteklediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DECIMAL FIXED(8);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1FixedDecimalType Precision 8, Scale null olmalıdır.
    /// </summary>
    [Fact]
    public void ParseDecimalBasedType_WithDecimalFixed_ShouldCreateFixedDecimalType()
    {
        var parser = CreateNumericTypeParser(
            "DECIMAL FIXED(8);",
            out var context);

        var result = parser.ParseDecimalBasedType();

        var dataType = Assert.IsType<Pl1FixedDecimalType>(result.Value);
        Assert.Equal(8, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// FIXED BIN token akışından Pl1FixedBinaryType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// NumericTypeParser'ın FIXED BIN(31) syntax'ını binary fixed modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// FIXED BIN(31);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1FixedBinaryType Precision 31, Scale null olmalıdır.
    /// </summary>
    [Fact]
    public void ParseFixedBasedType_WithFixedBin_ShouldCreateFixedBinaryType()
    {
        var parser = CreateNumericTypeParser(
            "FIXED BIN(31);",
            out var context);

        var result = parser.ParseFixedBasedType();

        var dataType = Assert.IsType<Pl1FixedBinaryType>(result.Value);
        Assert.Equal(31, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// BIN FIXED token akışından Pl1FixedBinaryType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// NumericTypeParser'ın ters keyword sırasındaki BIN FIXED syntax'ını desteklediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// BIN FIXED(15,0);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1FixedBinaryType Precision 15, Scale 0 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseBinaryBasedType_WithBinFixedHavingScale_ShouldCreateFixedBinaryType()
    {
        var parser = CreateNumericTypeParser(
            "BIN FIXED(15,0);",
            out var context);

        var result = parser.ParseBinaryBasedType();

        var dataType = Assert.IsType<Pl1FixedBinaryType>(result.Value);
        Assert.Equal(15, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
        Assert.Empty(GetDiagnostics(context));
    }
}