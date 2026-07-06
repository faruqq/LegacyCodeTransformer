using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class BitTypeParserTests
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
        var tokens = new Pl1Lexer("BIT(1);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new BitTypeParser(
            tokens,
            0,
            diagnostics);

        var result = parser.Parse();

        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value!.Length);
        Assert.Empty(diagnostics.Diagnostics);
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
        var tokens = new Pl1Lexer("BIT(8);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new BitTypeParser(
            tokens,
            0,
            diagnostics);

        var result = parser.Parse();

        Assert.NotNull(result.Value);
        Assert.Equal(8, result.Value!.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }
}