using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class InitialValueParserTests : ParserHelperTestBase
{
    /// <summary>
    /// INIT string literal değerinin Pl1InitialValue olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// InitialValueParser'ın INIT('ABCD') syntax'ını repeat factor olmadan parse ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// INIT('ABCD');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value ABCD, RepeatCount null, AppliesToAllElements false olmalıdır.
    /// </summary>
    [Fact]
    public void ParseOptionalInitialValue_WithInitLiteral_ShouldCreateInitialValue()
    {
        var tokens = new Pl1Lexer("INIT('ABCD');").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new InitialValueParser(tokens, 0, diagnostics);

        var result = parser.ParseOptionalInitialValue();

        Assert.NotNull(result.InitialValue);
        Assert.Equal("ABCD", result.InitialValue!.Value);
        Assert.Null(result.InitialValue.RepeatCount);
        Assert.False(result.InitialValue.AppliesToAllElements);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// INITIAL keyword synonym bilgisinin Pl1InitialValue olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// InitialValueParser'ın INITIAL keyword'ünü INIT ile aynı semantic model olarak ele aldığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// INITIAL(';');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value ; olmalıdır.
    /// </summary>
    [Fact]
    public void ParseOptionalInitialValue_WithInitialKeyword_ShouldCreateInitialValue()
    {
        var tokens = new Pl1Lexer("INITIAL(';');").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new InitialValueParser(tokens, 0, diagnostics);

        var result = parser.ParseOptionalInitialValue();

        Assert.NotNull(result.InitialValue);
        Assert.Equal(";", result.InitialValue!.Value);
        Assert.Null(result.InitialValue.RepeatCount);
        Assert.False(result.InitialValue.AppliesToAllElements);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// INIT repeat factor bilgisinin Pl1InitialValue üzerinde korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// InitialValueParser'ın INIT((08)' ') syntax'ındaki repeat count bilgisini parse ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// INIT((08)' ');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value boşluk, RepeatCount 8 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseOptionalInitialValue_WithRepeatFactor_ShouldCreateInitialValueWithRepeatCount()
    {
        var tokens = new Pl1Lexer("INIT((08)' ');").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new InitialValueParser(tokens, 0, diagnostics);

        var result = parser.ParseOptionalInitialValue();

        Assert.NotNull(result.InitialValue);
        Assert.Equal(" ", result.InitialValue!.Value);
        Assert.Equal(8, result.InitialValue.RepeatCount);
        Assert.False(result.InitialValue.AppliesToAllElements);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// INIT all-elements bilgisinin Pl1InitialValue üzerinde korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// InitialValueParser'ın INIT((*)' ') syntax'ındaki all-elements bilgisini parse ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// INIT((*)' ');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value boşluk, AppliesToAllElements true olmalıdır.
    /// </summary>
    [Fact]
    public void ParseOptionalInitialValue_WithAllElementsFactor_ShouldCreateInitialValueWithAllElements()
    {
        var tokens = new Pl1Lexer("INIT((*)' ');").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new InitialValueParser(tokens, 0, diagnostics);

        var result = parser.ParseOptionalInitialValue();

        Assert.NotNull(result.InitialValue);
        Assert.Equal(" ", result.InitialValue!.Value);
        Assert.Null(result.InitialValue.RepeatCount);
        Assert.True(result.InitialValue.AppliesToAllElements);
        Assert.Empty(diagnostics.Diagnostics);
    }
}