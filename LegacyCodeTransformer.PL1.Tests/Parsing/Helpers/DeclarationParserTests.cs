using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class DeclarationParserTests : ParserHelperTestBase
{
    /// <summary>
    /// DCL sonrasında Identifier geldiğinde variable declaration parser'a yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DeclarationParser'ın DCL PARAM CHAR(08); syntax bilgisini Pl1VariableDeclaration olarak parse ettiğini doğrular.
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
        var parser = CreateDeclarationParser(
            "DCL PARAM CHAR(08);",
            out var context);

        var result = parser.ParseDeclaration();

        var declaration = Assert.IsType<Pl1VariableDeclaration>(result.Value);
        Assert.Equal("PARAM", declaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(declaration.DataType);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// DCL sonrasında Number geldiğinde structure declaration parser'a yönlendirildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DeclarationParser'ın DCL 1 REC, 5 PARAM CHAR(08); syntax bilgisini Pl1StructureDeclaration olarak parse ettiğini doğrular.
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
        var parser = CreateDeclarationParser(
            "DCL 1 REC, 5 PARAM CHAR(08);",
            out var context);

        var result = parser.ParseDeclaration();

        var declaration = Assert.IsType<Pl1StructureDeclaration>(result.Value);
        Assert.Equal("REC", declaration.Name);
        Assert.Equal(1, declaration.Level);
        Assert.Single(declaration.Members);
        Assert.Empty(GetDiagnostics(context));
    }

    /// <summary>
    /// DCL sonrasında geçersiz token geldiğinde diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DeclarationParser'ın DCL sonrasında variable name veya structure level gelmediğinde diagnostic ürettiğini doğrular.
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
        var parser = CreateDeclarationParser(
            "DCL ;",
            out var context);

        var result = parser.ParseDeclaration();
        var diagnostics = GetDiagnostics(context);

        Assert.Null(result.Value);
        Assert.Single(diagnostics);
        Assert.Contains(
            "DCL sonrasında değişken adı veya structure seviye numarası bekleniyordu.",
            diagnostics[0].Message);
    }
}