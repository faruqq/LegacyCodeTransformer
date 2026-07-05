using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class DeclarationParserTests
{
    /// <summary>
    /// DCL sonrasında Identifier geldiğinde variable declaration parser'a yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DeclarationParser'ın DCL PARAM CHAR(08); syntax bilgisini Pl1VariableDeclaration
    /// olarak parse ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1VariableDeclaration adı PARAM ve data type Pl1CharacterType(8) olmalıdır.
    /// </summary>
    [Fact]
    public void ParseDeclaration_WithIdentifierAfterDcl_ShouldCreateVariableDeclaration()
    {
        var tokens = new Pl1Lexer("DCL PARAM CHAR(08);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DeclarationParser(tokens, 0, diagnostics);

        var result = parser.ParseDeclaration();

        var declaration = Assert.IsType<Pl1VariableDeclaration>(result.Declaration);
        Assert.Equal("PARAM", declaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(declaration.DataType);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DCL sonrasında Number geldiğinde structure declaration parser'a yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DeclarationParser'ın DCL 1 REC, 5 PARAM CHAR(08); syntax bilgisini
    /// Pl1StructureDeclaration olarak parse ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 REC, 5 PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1StructureDeclaration adı REC olmalıdır.
    /// </summary>
    [Fact]
    public void ParseDeclaration_WithNumberAfterDcl_ShouldCreateStructureDeclaration()
    {
        var tokens = new Pl1Lexer("DCL 1 REC, 5 PARAM CHAR(08);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DeclarationParser(tokens, 0, diagnostics);

        var result = parser.ParseDeclaration();

        var declaration = Assert.IsType<Pl1StructureDeclaration>(result.Declaration);
        Assert.Equal("REC", declaration.Name);
        Assert.Equal(1, declaration.Level);
        Assert.Single(declaration.Members);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DCL sonrasında geçersiz token geldiğinde diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DeclarationParser'ın DCL sonrasında variable name veya structure level gelmediğinde
    /// diagnostic ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL ;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Declaration null olmalı ve diagnostic üretilmelidir.
    /// </summary>
    [Fact]
    public void ParseDeclaration_WithInvalidTokenAfterDcl_ShouldReturnDiagnostic()
    {
        var tokens = new Pl1Lexer("DCL ;").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new DeclarationParser(tokens, 0, diagnostics);

        var result = parser.ParseDeclaration();

        Assert.Null(result.Declaration);
        Assert.Single(diagnostics.Diagnostics);
        Assert.Contains(
            "DCL sonrasında değişken adı veya structure seviye numarası bekleniyordu.",
            diagnostics.Diagnostics[0].Message);
    }
}