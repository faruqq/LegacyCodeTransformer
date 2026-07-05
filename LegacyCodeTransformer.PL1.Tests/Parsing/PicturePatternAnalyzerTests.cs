using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing;

public sealed class PicturePatternAnalyzerTests
{
    /// <summary>
    /// Numeric PIC pattern bilgisinin precision değeriyle analiz edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın 9 karakterlerinden oluşan pattern'ı numeric olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// 999
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, Precision 3, Length 3, Scale null.
    /// </summary>
    [Fact]
    public void Analyze_WithNumericPicturePattern_ShouldReturnNumericAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("999");

        // Assert
        Assert.Equal(Pl1PictureCategory.Numeric, analysis.Category);
        Assert.Equal(3, analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Equal(3, analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.True(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Numeric PIC pattern içindeki V bilgisinin scale olarak analiz edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın implied decimal point sonrası digit sayısını Scale değerine çevirdiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// 999V99
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, Precision 5, Scale 2, Length 5.
    /// </summary>
    [Fact]
    public void Analyze_WithNumericPicturePatternHavingScale_ShouldReturnScale()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("999V99");

        // Assert
        Assert.Equal(Pl1PictureCategory.Numeric, analysis.Category);
        Assert.Equal(5, analysis.Precision);
        Assert.Equal(2, analysis.Scale);
        Assert.Equal(5, analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.True(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Numeric PIC repeat pattern bilgisinin toplam precision olarak analiz edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın (n)9 tekrar söz dizimini toplam precision hesabına dahil ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// (13)9V99
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, Precision 15, Scale 2, Length 15.
    /// </summary>
    [Fact]
    public void Analyze_WithRepeatedNumericPicturePattern_ShouldReturnExpandedPrecision()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("(13)9V99");

        // Assert
        Assert.Equal(Pl1PictureCategory.Numeric, analysis.Category);
        Assert.Equal(15, analysis.Precision);
        Assert.Equal(2, analysis.Scale);
        Assert.Equal(15, analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.True(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Alphanumeric PIC pattern bilgisinin character length ile analiz edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın X karakterlerinden oluşan pattern'ı alphanumeric olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// XXX
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Alphanumeric, Length 3.
    /// </summary>
    [Fact]
    public void Analyze_WithAlphanumericPicturePattern_ShouldReturnAlphanumericAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("XXX");

        // Assert
        Assert.Equal(Pl1PictureCategory.Alphanumeric, analysis.Category);
        Assert.Null(analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Equal(3, analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.False(analysis.IsNumeric);
        Assert.True(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Alphanumeric PIC repeat pattern bilgisinin toplam length olarak analiz edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın (n)X tekrar söz dizimini character length hesabına dahil ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// (20)X
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Alphanumeric, Length 20.
    /// </summary>
    [Fact]
    public void Analyze_WithRepeatedAlphanumericPicturePattern_ShouldReturnExpandedLength()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("(20)X");

        // Assert
        Assert.Equal(Pl1PictureCategory.Alphanumeric, analysis.Category);
        Assert.Null(analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Equal(20, analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.False(analysis.IsNumeric);
        Assert.True(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Formatted PIC pattern bilgisinin doğrudan EGL mapping dışı bırakıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın Z, virgül veya nokta gibi edit mask karakterleri içeren pattern'ı formatted olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// Z,ZZ9V.99
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Formatted, IsFormatted true, SupportsDirectEglMapping false.
    /// </summary>
    [Fact]
    public void Analyze_WithFormattedPicturePattern_ShouldReturnFormattedAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("Z,ZZ9V.99");

        // Assert
        Assert.Equal(Pl1PictureCategory.Formatted, analysis.Category);
        Assert.Null(analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Null(analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.False(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.True(analysis.IsFormatted);
        Assert.False(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Signed numeric PIC pattern bilgisinin sign bilgisi korunarak analiz edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın baştaki S karakterini sign metadata olarak ayırdığını ve kalan pattern'ı numeric olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// S999
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, IsSigned true, Precision 3, Length 3.
    /// </summary>
    [Fact]
    public void Analyze_WithSignedNumericPicturePattern_ShouldReturnSignedNumericAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("S999");

        // Assert
        Assert.Equal(Pl1PictureCategory.Numeric, analysis.Category);
        Assert.Equal(3, analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Equal(3, analysis.Length);
        Assert.True(analysis.IsSigned);
        Assert.True(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Signed numeric PIC pattern içindeki implied decimal bilgisinin sign metadata ile birlikte analiz edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın baştaki S karakterini sign metadata olarak ayırdığını ve kalan 999V99 pattern'ını numeric precision / scale bilgisiyle analiz ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// S999V99
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, IsSigned true, Precision 5, Scale 2, Length 5 olmalıdır.
    /// </summary>
    [Fact]
    public void Analyze_WithSignedNumericPicturePatternHavingScale_ShouldReturnSignedNumericAnalysisWithScale()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("S999V99");

        // Assert
        Assert.Equal(Pl1PictureCategory.Numeric, analysis.Category);
        Assert.Equal(5, analysis.Precision);
        Assert.Equal(2, analysis.Scale);
        Assert.Equal(5, analysis.Length);
        Assert.True(analysis.IsSigned);
        Assert.True(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Signed numeric PIC repeat pattern bilgisinin sign metadata korunarak expanded precision ile analiz edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın S(8)9 pattern'ında S karakterini sign metadata olarak tuttuğunu ve (8)9 tekrar söz dizimini Precision 8 olarak genişlettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// S(8)9
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, IsSigned true, Precision 8, Scale null, Length 8 olmalıdır.
    /// </summary>
    [Fact]
    public void Analyze_WithSignedRepeatedNumericPicturePattern_ShouldReturnSignedNumericAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("S(8)9");

        // Assert
        Assert.Equal(Pl1PictureCategory.Numeric, analysis.Category);
        Assert.Equal(8, analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Equal(8, analysis.Length);
        Assert.True(analysis.IsSigned);
        Assert.True(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Signed numeric PIC repeat pattern içindeki scale bilgisinin doğru hesaplandığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın S(10)9V99 pattern'ında toplam precision değerini 12 ve scale değerini 2 olarak hesapladığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// S(10)9V99
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, IsSigned true, Precision 12, Scale 2, Length 12 olmalıdır.
    /// </summary>
    [Fact]
    public void Analyze_WithSignedRepeatedNumericPicturePatternHavingScale_ShouldReturnSignedNumericAnalysisWithScale()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("S(10)9V99");

        // Assert
        Assert.Equal(Pl1PictureCategory.Numeric, analysis.Category);
        Assert.Equal(12, analysis.Precision);
        Assert.Equal(2, analysis.Scale);
        Assert.Equal(12, analysis.Length);
        Assert.True(analysis.IsSigned);
        Assert.True(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.False(analysis.IsFormatted);
        Assert.True(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Z edit mask karakteri içeren PIC pattern bilgisinin formatted olarak sınıflandırıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın Z karakterini storage digit değil, display/edit mask karakteri olarak değerlendirdiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ZZ9
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Formatted, IsFormatted true, SupportsDirectEglMapping false olmalıdır.
    /// </summary>
    [Fact]
    public void Analyze_WithZeroSuppressFormattedPicturePattern_ShouldReturnFormattedAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("ZZ9");

        // Assert
        Assert.Equal(Pl1PictureCategory.Formatted, analysis.Category);
        Assert.Null(analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Null(analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.False(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.True(analysis.IsFormatted);
        Assert.False(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Thousands separator ve display decimal point içeren PIC pattern bilgisinin formatted olarak sınıflandırıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın virgül ve nokta karakterlerini doğrudan EGL num mapping yapılmayacak format maskesi olarak değerlendirdiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// Z,ZZ9V.99
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Formatted, IsFormatted true, SupportsDirectEglMapping false olmalıdır.
    /// </summary>
    [Fact]
    public void Analyze_WithSeparatorFormattedPicturePattern_ShouldReturnFormattedAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("Z,ZZ9V.99");

        // Assert
        Assert.Equal(Pl1PictureCategory.Formatted, analysis.Category);
        Assert.Null(analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Null(analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.False(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.True(analysis.IsFormatted);
        Assert.False(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Leading plus sign içeren PIC pattern bilgisinin formatted olarak sınıflandırıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın + karakterini numeric sign metadata değil, formatted edit mask olarak değerlendirdiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// +999
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Formatted, IsFormatted true, SupportsDirectEglMapping false olmalıdır.
    /// </summary>
    [Fact]
    public void Analyze_WithLeadingPlusFormattedPicturePattern_ShouldReturnFormattedAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("+999");

        // Assert
        Assert.Equal(Pl1PictureCategory.Formatted, analysis.Category);
        Assert.Null(analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Null(analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.False(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.True(analysis.IsFormatted);
        Assert.False(analysis.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Leading minus sign içeren PIC pattern bilgisinin formatted olarak sınıflandırıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Analyzer'ın - karakterini numeric sign metadata değil, formatted edit mask olarak değerlendirdiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// -999
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Formatted, IsFormatted true, SupportsDirectEglMapping false olmalıdır.
    /// </summary>
    [Fact]
    public void Analyze_WithLeadingMinusFormattedPicturePattern_ShouldReturnFormattedAnalysis()
    {
        // Act
        var analysis = PicturePatternAnalyzer.Analyze("-999");

        // Assert
        Assert.Equal(Pl1PictureCategory.Formatted, analysis.Category);
        Assert.Null(analysis.Precision);
        Assert.Null(analysis.Scale);
        Assert.Null(analysis.Length);
        Assert.False(analysis.IsSigned);
        Assert.False(analysis.IsNumeric);
        Assert.False(analysis.IsAlphanumeric);
        Assert.True(analysis.IsFormatted);
        Assert.False(analysis.SupportsDirectEglMapping);
    }
}