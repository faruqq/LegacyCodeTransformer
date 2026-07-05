using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class CharacterTypeParserTests
{
    /// <summary>
    /// CHAR token akışından Pl1CharacterType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CharacterTypeParser'ın CHAR(08) söz dizimini Length 8 olan Pl1CharacterType modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1CharacterType Length 8 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseCharacterType_WithCharKeyword_ShouldCreateCharacterType()
    {
        var tokens = new Pl1Lexer("CHAR(08);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new CharacterTypeParser(
            tokens,
            0,
            diagnostics);

        var result = parser.ParseCharacterType();

        var dataType = Assert.IsType<Pl1CharacterType>(result.DataType);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// CHARACTER token akışından Pl1CharacterType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CharacterTypeParser'ın CHARACTER(25) söz dizimini CHAR ile aynı semantic model olarak ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// CHARACTER(25);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1CharacterType Length 25 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseCharacterType_WithCharacterKeyword_ShouldCreateCharacterType()
    {
        var tokens = new Pl1Lexer("CHARACTER(25);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new CharacterTypeParser(
            tokens,
            0,
            diagnostics);

        var result = parser.ParseCharacterType();

        var dataType = Assert.IsType<Pl1CharacterType>(result.DataType);
        Assert.Equal(25, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// VARCHAR token akışından Pl1VarcharType üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CharacterTypeParser'ın VARCHAR(50) söz dizimini Pl1VarcharType modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// VARCHAR(50);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1VarcharType Length 50 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseVarcharType_WithVarcharKeyword_ShouldCreateVarcharType()
    {
        var tokens = new Pl1Lexer("VARCHAR(50);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new CharacterTypeParser(
            tokens,
            0,
            diagnostics);

        var result = parser.ParseVarcharType();

        var dataType = Assert.IsType<Pl1VarcharType>(result.DataType);
        Assert.Equal(50, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }
}