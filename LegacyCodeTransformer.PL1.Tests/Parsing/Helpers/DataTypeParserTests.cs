using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class DataTypeParserTests : ParserHelperTestBase
{
    /// <summary>
    /// CHAR token akışının Pl1CharacterType modeline yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DataTypeParser'ın CharKeyword gördüğünde CharacterTypeParser.ParseCharacterType akışını kullandığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1CharacterType Length 8 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithCharKeyword_ShouldCreateCharacterType()
    {
        var parser = CreateDataTypeParser(
            "CHAR(08);",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1CharacterType>(result.Value);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// FIXED DECIMAL token akışının Pl1FixedDecimalType modeline yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DataTypeParser'ın FixedKeyword gördüğünde NumericTypeParser.ParseFixedBasedType akışını kullandığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// FIXED DECIMAL(15,2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1FixedDecimalType Precision 15, Scale 2 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFixedDecimalKeyword_ShouldCreateFixedDecimalType()
    {
        var parser = CreateDataTypeParser(
            "FIXED DECIMAL(15,2);",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FixedDecimalType>(result.Value);
        Assert.Equal(15, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// PIC token akışının Pl1PictureType modeline yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DataTypeParser'ın PIC keyword ve pattern string literal bilgisini okuyup PictureTypeParser üzerinden Pl1PictureType ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// PIC '999V99';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1PictureType Category Numeric, Precision 5, Scale 2 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithPictureKeyword_ShouldCreatePictureType()
    {
        var parser = CreateDataTypeParser(
            "PIC '999V99';",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1PictureType>(result.Value);
        Assert.Equal(Pl1PictureCategory.Numeric, dataType.Category);
        Assert.Equal(5, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// BIT token akışının Pl1BitType modeline yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DataTypeParser'ın BIT keyword gördüğünde BitTypeParser.Parse akışını kullandığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// BIT(8);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1BitType Length 8 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithBitKeyword_ShouldCreateBitType()
    {
        var parser = CreateDataTypeParser(
            "BIT(8);",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1BitType>(result.Value);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// FLOAT token akışının Pl1FloatingType modeline yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DataTypeParser'ın FloatKeyword gördüğünde FloatingTypeParser.Parse akışını kullandığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// FLOAT BIN(53);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1FloatingType Kind Float, Base Binary, Precision 53 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFloatKeyword_ShouldCreateFloatingType()
    {
        var parser = CreateDataTypeParser(
            "FLOAT BIN(53);",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FloatingType>(result.Value);
        Assert.Equal(Pl1FloatingTypeKind.Float, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Binary, dataType.Base);
        Assert.Equal(53, dataType.Precision);
        Assert.Empty(GetDiagnostics(context));
    }
}