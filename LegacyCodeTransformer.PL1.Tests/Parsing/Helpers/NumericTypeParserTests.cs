using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
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
        var tokens = new Pl1Lexer("FIXED DECIMAL(15);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new NumericTypeParser(tokens, 0, diagnostics);

        var result = parser.ParseFixedBasedType();

        var dataType = Assert.IsType<Pl1FixedDecimalType>(result.Value);
        Assert.Equal(15, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("FIXED DEC(17,2);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new NumericTypeParser(tokens, 0, diagnostics);

        var result = parser.ParseFixedBasedType();

        var dataType = Assert.IsType<Pl1FixedDecimalType>(result.Value);
        Assert.Equal(17, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("DECIMAL FIXED(8);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new NumericTypeParser(tokens, 0, diagnostics);

        var result = parser.ParseDecimalBasedType();

        var dataType = Assert.IsType<Pl1FixedDecimalType>(result.Value);
        Assert.Equal(8, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("FIXED BIN(31);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new NumericTypeParser(tokens, 0, diagnostics);

        var result = parser.ParseFixedBasedType();

        var dataType = Assert.IsType<Pl1FixedBinaryType>(result.Value);
        Assert.Equal(31, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("BIN FIXED(15,0);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new NumericTypeParser(tokens, 0, diagnostics);

        var result = parser.ParseBinaryBasedType();

        var dataType = Assert.IsType<Pl1FixedBinaryType>(result.Value);
        Assert.Equal(15, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
        Assert.Empty(diagnostics.Diagnostics);
    }
}