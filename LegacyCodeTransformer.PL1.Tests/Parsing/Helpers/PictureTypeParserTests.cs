using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class PictureTypeParserTests
{
    /// <summary>
    /// Numeric PIC pattern bilgisinden Pl1PictureType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// PictureTypeParser'ın PicturePatternAnalyzer sonucunu Pl1PictureType modeline doğru taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// 999V99
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, Precision 5, Scale 2, Length 5 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithNumericPicturePattern_ShouldCreateNumericPictureType()
    {
        // Act
        var result = PictureTypeParser.Parse(
            "999V99",
            SourceLocation.Unknown);

        // Assert
        Assert.Equal("999V99", result.RawPattern);
        Assert.Equal(Pl1PictureCategory.Numeric, result.Category);
        Assert.Equal(5, result.Precision);
        Assert.Equal(2, result.Scale);
        Assert.Equal(5, result.Length);
        Assert.False(result.IsSigned);
        Assert.True(result.IsNumeric);
        Assert.False(result.IsAlphanumeric);
        Assert.False(result.IsFormatted);
        Assert.True(result.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Alphanumeric PIC pattern bilgisinden Pl1PictureType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// PictureTypeParser'ın alphanumeric classification sonucunu Pl1PictureType üzerinde koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// (20)X
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Alphanumeric, Length 20 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithAlphanumericPicturePattern_ShouldCreateAlphanumericPictureType()
    {
        // Act
        var result = PictureTypeParser.Parse(
            "(20)X",
            SourceLocation.Unknown);

        // Assert
        Assert.Equal("(20)X", result.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, result.Category);
        Assert.Null(result.Precision);
        Assert.Null(result.Scale);
        Assert.Equal(20, result.Length);
        Assert.False(result.IsSigned);
        Assert.False(result.IsNumeric);
        Assert.True(result.IsAlphanumeric);
        Assert.False(result.IsFormatted);
        Assert.True(result.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Signed numeric PIC pattern bilgisinden Pl1PictureType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// PictureTypeParser'ın IsSigned metadata bilgisini Pl1PictureType üzerinde koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// S999
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, IsSigned true, Precision 3 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithSignedNumericPicturePattern_ShouldCreateSignedPictureType()
    {
        // Act
        var result = PictureTypeParser.Parse(
            "S999",
            SourceLocation.Unknown);

        // Assert
        Assert.Equal("S999", result.RawPattern);
        Assert.Equal(Pl1PictureCategory.Numeric, result.Category);
        Assert.Equal(3, result.Precision);
        Assert.Null(result.Scale);
        Assert.Equal(3, result.Length);
        Assert.True(result.IsSigned);
        Assert.True(result.IsNumeric);
        Assert.False(result.IsAlphanumeric);
        Assert.False(result.IsFormatted);
        Assert.True(result.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Formatted PIC pattern bilgisinden Pl1PictureType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// PictureTypeParser'ın formatted pattern için direct EGL mapping desteğini kapalı tuttuğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// ZZ9
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Formatted, IsFormatted true, SupportsDirectEglMapping false olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFormattedPicturePattern_ShouldCreateFormattedPictureType()
    {
        // Act
        var result = PictureTypeParser.Parse(
            "ZZ9",
            SourceLocation.Unknown);

        // Assert
        Assert.Equal("ZZ9", result.RawPattern);
        Assert.Equal(Pl1PictureCategory.Formatted, result.Category);
        Assert.Null(result.Precision);
        Assert.Null(result.Scale);
        Assert.Null(result.Length);
        Assert.False(result.IsSigned);
        Assert.False(result.IsNumeric);
        Assert.False(result.IsAlphanumeric);
        Assert.True(result.IsFormatted);
        Assert.False(result.SupportsDirectEglMapping);
    }
}