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
        var parser = CreateInitialValueParser(
            "INIT('ABCD');",
            out var context);

        var result = parser.ParseOptionalInitialValue();

        Assert.NotNull(result.Value);
        Assert.Equal("ABCD", result.Value!.Value);
        Assert.Null(result.Value.RepeatCount);
        Assert.False(result.Value.AppliesToAllElements);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateInitialValueParser(
            "INITIAL(';');",
            out var context);

        var result = parser.ParseOptionalInitialValue();

        Assert.NotNull(result.Value);
        Assert.Equal(";", result.Value!.Value);
        Assert.Null(result.Value.RepeatCount);
        Assert.False(result.Value.AppliesToAllElements);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateInitialValueParser(
            "INIT((08)' ');",
            out var context);

        var result = parser.ParseOptionalInitialValue();

        Assert.NotNull(result.Value);
        Assert.Equal(" ", result.Value!.Value);
        Assert.Equal(8, result.Value.RepeatCount);
        Assert.False(result.Value.AppliesToAllElements);
        Assert.Empty(GetDiagnostics(context));
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
        var parser = CreateInitialValueParser(
            "INIT((*)' ');",
            out var context);

        var result = parser.ParseOptionalInitialValue();

        Assert.NotNull(result.Value);
        Assert.Equal(" ", result.Value!.Value);
        Assert.Null(result.Value.RepeatCount);
        Assert.True(result.Value.AppliesToAllElements);
        Assert.Empty(GetDiagnostics(context));
    }
}