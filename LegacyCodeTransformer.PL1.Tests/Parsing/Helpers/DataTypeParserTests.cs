using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class DataTypeParserTests
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
        var tokens = new Pl1Lexer("CHAR(08);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DataTypeParser(tokens, 0, diagnostics);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1CharacterType>(result.DataType);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("FIXED DECIMAL(15,2);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DataTypeParser(tokens, 0, diagnostics);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FixedDecimalType>(result.DataType);
        Assert.Equal(15, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("PIC '999V99';").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DataTypeParser(tokens, 0, diagnostics);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1PictureType>(result.DataType);
        Assert.Equal(Pl1PictureCategory.Numeric, dataType.Category);
        Assert.Equal(5, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("BIT(8);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DataTypeParser(tokens, 0, diagnostics);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1BitType>(result.DataType);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("FLOAT BIN(53);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DataTypeParser(tokens, 0, diagnostics);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1FloatingType>(result.DataType);
        Assert.Equal(Pl1FloatingTypeKind.Float, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Binary, dataType.Base);
        Assert.Equal(53, dataType.Precision);
        Assert.Empty(diagnostics.Diagnostics);
    }
}