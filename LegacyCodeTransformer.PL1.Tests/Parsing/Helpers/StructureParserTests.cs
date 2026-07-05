using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public sealed class StructureParserTests
{
    /// <summary>
    /// Basic structure declaration bilgisinin Pl1StructureDeclaration olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// StructureParser'ın DCL 1 REC, 5 PARAM CHAR(08); syntax bilgisini root structure ve tek typed member olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 REC, 5 PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Structure adı REC, member adı PARAM, member data type Pl1CharacterType(8) olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStructureDeclaration_WithBasicStructure_ShouldCreateStructureDeclaration()
    {
        var tokens = new Pl1Lexer("DCL 1 REC, 5 PARAM CHAR(08);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new StructureParser(tokens, 0, diagnostics);

        var result = parser.ParseStructureDeclaration();

        Assert.NotNull(result.Declaration);
        Assert.Equal("REC", result.Declaration!.Name);
        Assert.Equal(1, result.Declaration.Level);
        Assert.Null(result.Declaration.ArraySize);

        var member = Assert.Single(result.Declaration.Members);
        Assert.Equal("PARAM", member.Name);
        Assert.Equal(5, member.Level);

        var dataType = Assert.IsType<Pl1CharacterType>(member.DataType);
        Assert.Equal(8, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Structure array declaration bilgisinin ArraySize ile parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// StructureParser'ın DCL 1 DIZI(6), syntax bilgisindeki root array size değerini koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 DIZI(6), 3 KOD CHAR(01);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Structure adı DIZI, ArraySize 6 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStructureDeclaration_WithStructureArray_ShouldSetArraySize()
    {
        var tokens = new Pl1Lexer("DCL 1 DIZI(6), 3 KOD CHAR(01);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new StructureParser(tokens, 0, diagnostics);

        var result = parser.ParseStructureDeclaration();

        Assert.NotNull(result.Declaration);
        Assert.Equal("DIZI", result.Declaration!.Name);
        Assert.Equal(6, result.Declaration.ArraySize);

        var member = Assert.Single(result.Declaration.Members);
        Assert.Equal("KOD", member.Name);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Structure member DIM attribute bilgisinin member ArraySize olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// StructureParser'ın member data type sonrasında gelen DIM(2) bilgisini member ArraySize alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 REC, 5 PARAM CHAR(10) DIM(2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Member adı PARAM, ArraySize 2 olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStructureDeclaration_WithMemberDimension_ShouldSetMemberArraySize()
    {
        var tokens = new Pl1Lexer("DCL 1 REC, 5 PARAM CHAR(10) DIM(2);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new StructureParser(tokens, 0, diagnostics);

        var result = parser.ParseStructureDeclaration();

        Assert.NotNull(result.Declaration);

        var member = Assert.Single(result.Declaration!.Members);
        Assert.Equal("PARAM", member.Name);
        Assert.Equal(2, member.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(member.DataType);
        Assert.Equal(10, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Nested group member bilgisinin child member listesiyle parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// StructureParser'ın veri tipi olmayan group member altında daha yüksek level değerli child member listesini recursive olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 MUSTERI, 5 ADRES, 10 IL CHAR(02);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ADRES group member olmalı ve altında IL child member bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStructureDeclaration_WithNestedGroup_ShouldCreateChildMembers()
    {
        var tokens = new Pl1Lexer("DCL 1 MUSTERI, 5 ADRES, 10 IL CHAR(02);").Tokenize();
        var diagnostics = new DiagnosticBag();
        var parser = new StructureParser(tokens, 0, diagnostics);

        var result = parser.ParseStructureDeclaration();

        Assert.NotNull(result.Declaration);

        var groupMember = Assert.Single(result.Declaration!.Members);
        Assert.Equal("ADRES", groupMember.Name);
        Assert.True(groupMember.IsGroup);

        var childMember = Assert.Single(groupMember.Members);
        Assert.Equal("IL", childMember.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(childMember.DataType);
        Assert.Equal(2, dataType.Length);
        Assert.Empty(diagnostics.Diagnostics);
    }
}