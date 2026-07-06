using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class BitTypeParserTests : ParserHelperTestBase
{
    /// <summary>
    /// BIT token akışından Pl1BitType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// BitTypeParser'ın BIT(1) söz dizimini Length 1 olan Pl1BitType modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// BIT(1);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1BitType Length 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithBitOne_ShouldCreateBitType()
    {
        var parser = CreateBitTypeParser(
            "BIT(1);",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1BitType>(result.Value);
        Assert.Equal(1, dataType.Length);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// BIT(8) token akışından Pl1BitType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// BitTypeParser'ın BIT length bilgisini doğru taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// BIT(8);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1BitType Length 8 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithBitEight_ShouldCreateBitType()
    {
        var parser = CreateBitTypeParser(
            "BIT(8);",
            out var context);

        var result = parser.Parse();

        var dataType = Assert.IsType<Pl1BitType>(result.Value);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(GetDiagnostics(context));
    }
}