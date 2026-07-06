using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing;

public sealed class Pl1ParserTests
{
    private static ParseResult<Pl1SyntaxTree> ParseSource(string source)
    {
        var tokens = new Pl1Lexer(source).Tokenize();
        var parser = new Pl1Parser(tokens);

        return parser.Parse();
    }

    private static Pl1VariableDeclaration ParseSingleVariableDeclaration(string source)
    {
        var result = ParseSource(source);

        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        return Assert.IsType<Pl1VariableDeclaration>(declaration);
    }

    private static Pl1StructureDeclaration ParseSingleStructureDeclaration(string source)
    {
        var result = ParseSource(source);

        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        return Assert.IsType<Pl1StructureDeclaration>(declaration);
    }

    /// <summary>
    /// FIXED DECIMAL declaration bilgisinin variable declaration olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın DCL MUST_NO FIXED DECIMAL(8); ifadesini Pl1VariableDeclaration ve Pl1FixedDecimalType olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı MUST_NO, precision 8 ve scale null olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFixedDecimalDeclaration_ShouldCreateVariableDeclaration()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL MUST_NO FIXED DECIMAL(8);");

        Assert.Equal("MUST_NO", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);
        Assert.Equal(8, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// FIXED DECIMAL declaration içinde açıkça verilen zero scale bilgisinin korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın FIXED DECIMAL(8,0) içindeki scale 0 bilgisini null'a çevirmeden taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL MUST_NO FIXED DECIMAL(8,0);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı MUST_NO, precision 8 ve scale 0 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFixedDecimalZeroScaleDeclaration_ShouldCreateVariableDeclarationWithZeroScale()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL MUST_NO FIXED DECIMAL(8,0);");

        Assert.Equal("MUST_NO", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);
        Assert.Equal(8, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
    }

    /// <summary>
    /// FIXED DECIMAL declaration içinde scale bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın FIXED DECIMAL(10,2) içindeki precision ve scale bilgisini doğru taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL CUSTOMER_NO FIXED DECIMAL(10,2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı CUSTOMER_NO, precision 10 ve scale 2 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFixedDecimalScaleDeclaration_ShouldCreateVariableDeclarationWithScale()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL CUSTOMER_NO FIXED DECIMAL(10,2);");

        Assert.Equal("CUSTOMER_NO", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);
        Assert.Equal(10, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
    }

    /// <summary>
    /// Eksik semicolon durumunda parser diagnostic ürettiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın declaration sonunda semicolon beklediğini ve eksik olduğunda diagnostic ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL MUST_NO FIXED DECIMAL(8)
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Parse başarısız olmalı ve "';' bekleniyordu." diagnostic mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithMissingSemicolon_ShouldReturnDiagnosticError()
    {
        var result = ParseSource(
            "DCL MUST_NO FIXED DECIMAL(8)");

        Assert.False(result.Success);
        Assert.NotEmpty(result.Diagnostics);
        Assert.Contains(
            result.Diagnostics,
            x => x.Message.Contains("';' bekleniyordu."));
    }

    /// <summary>
    /// PL/I CHAR(n) tanımının Pl1CharacterType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın CHAR keyword'ünü karakter veri tipi modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(25);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı PARAM, veri tipi Pl1CharacterType ve Length 25 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithCharDeclaration_ShouldCreateCharacterTypeDeclaration()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM CHAR(25);");

        Assert.Equal("PARAM", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);
        Assert.Equal(25, dataType.Length);
    }

    /// <summary>
    /// PL/I CHAR uzunluğunda baştaki sıfırların modelde normalize edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın CHAR(08) değerini sayısal length 8 olarak taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı PARAM, veri tipi Pl1CharacterType ve Length 8 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithCharDeclarationHavingLeadingZeroLength_ShouldNormalizeLength()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM CHAR(08);");

        Assert.Equal("PARAM", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);
        Assert.Equal(8, dataType.Length);
    }

    /// <summary>
    /// PL/I CHARACTER(n) tanımının Pl1CharacterType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın CHARACTER uzun formunu CHAR ile aynı semantic model olarak ele aldığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DECLARE PARAM CHARACTER(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı PARAM, veri tipi Pl1CharacterType ve Length 8 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithCharacterDeclaration_ShouldCreateCharacterTypeDeclaration()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DECLARE PARAM CHARACTER(08);");

        Assert.Equal("PARAM", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);
        Assert.Equal(8, dataType.Length);
    }

    /// <summary>
    /// PL/I INIT(' ') başlangıç değerinin declaration modeline taşındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın simple INIT string literal bilgisini Pl1VariableDeclaration.InitialValue üzerinde koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08) INIT(' ');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// InitialValue.Value boşluk, RepeatCount null ve AppliesToAllElements false olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithSimpleInitValue_ShouldSetInitialValue()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM CHAR(08) INIT(' ');");

        Assert.Equal("PARAM", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal(" ", variableDeclaration.InitialValue!.Value);
        Assert.Null(variableDeclaration.InitialValue.RepeatCount);
        Assert.False(variableDeclaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I INIT(';') başlangıç değerinin karakter sabiti olarak taşındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın özel karakter içeren string literal başlangıç değerini kaybetmeden taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM2 CHAR(01) INIT(';');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// InitialValue.Value noktalı virgül olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithSemicolonInitValue_ShouldSetInitialValue()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM2 CHAR(01) INIT(';');");

        Assert.Equal("PARAM2", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal(";", variableDeclaration.InitialValue!.Value);
        Assert.Null(variableDeclaration.InitialValue.RepeatCount);
        Assert.False(variableDeclaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I INIT((08)' ') tekrar faktörünün sayısal olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın repeat factor bilgisini InitialValue.RepeatCount alanında koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM3 CHAR(8) INIT((08)' ');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// InitialValue.Value boşluk, RepeatCount 8 ve AppliesToAllElements false olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithRepeatedInitValue_ShouldSetRepeatCount()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM3 CHAR(8) INIT((08)' ');");

        Assert.Equal("PARAM3", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal(" ", variableDeclaration.InitialValue!.Value);
        Assert.Equal(8, variableDeclaration.InitialValue.RepeatCount);
        Assert.False(variableDeclaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I INIT((*)' ') all-elements bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın (*) repeat factor bilgisini AppliesToAllElements true olarak taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(8) INIT((*)' ');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// InitialValue.Value boşluk, RepeatCount null ve AppliesToAllElements true olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithAllElementsInitValue_ShouldSetAppliesToAllElements()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM CHAR(8) INIT((*)' ');");

        Assert.Equal("PARAM", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal(" ", variableDeclaration.InitialValue!.Value);
        Assert.Null(variableDeclaration.InitialValue.RepeatCount);
        Assert.True(variableDeclaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I basic structure declaration bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın DCL 1 ile başlayan structure declaration ifadesini Pl1StructureDeclaration olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 PARAME_LIST, 5 PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Structure adı PARAME_LIST, level 1, tek member PARAM CHAR(8) olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithBasicStructureDeclaration_ShouldCreateStructureDeclaration()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 PARAME_LIST, 5 PARAM CHAR(08);");

        Assert.Equal("PARAME_LIST", structureDeclaration.Name);
        Assert.Equal(1, structureDeclaration.Level);
        Assert.Null(structureDeclaration.ArraySize);

        var member = Assert.Single(structureDeclaration.Members);
        Assert.Equal("PARAM", member.Name);
        Assert.Equal(5, member.Level);
        Assert.Null(member.InitialValue);
        Assert.Null(member.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(member.DataType);
        Assert.Equal(8, dataType.Length);
    }

    /// <summary>
    /// PL/I structure member INIT bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın structure member üzerindeki INIT(' ') bilgisini Pl1StructureMember.InitialValue alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 PARAME_LIST, 5 PARAM CHAR(08) INIT(' ');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// PARAM member InitialValue.Value boşluk olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithStructureMemberInitValue_ShouldSetMemberInitialValue()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 PARAME_LIST, 5 PARAM CHAR(08) INIT(' ');");

        var member = Assert.Single(structureDeclaration.Members);

        Assert.Equal("PARAM", member.Name);
        Assert.NotNull(member.InitialValue);
        Assert.Equal(" ", member.InitialValue!.Value);
        Assert.Null(member.InitialValue.RepeatCount);
        Assert.False(member.InitialValue.AppliesToAllElements);

        var dataType = Assert.IsType<Pl1CharacterType>(member.DataType);
        Assert.Equal(8, dataType.Length);
    }

    /// <summary>
    /// PL/I structure array declaration bilgisinin root ArraySize alanında korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın DCL 1 DIZI(6) syntax bilgisini structure declaration array size olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 DIZI(6), 3 DIZI_PARAM1 CHAR(01);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Structure adı DIZI, ArraySize 6 ve member DIZI_PARAM1 CHAR(1) olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithStructureArrayDeclaration_ShouldSetStructureArraySize()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 DIZI(6), 3 DIZI_PARAM1 CHAR(01);");

        Assert.Equal("DIZI", structureDeclaration.Name);
        Assert.Equal(1, structureDeclaration.Level);
        Assert.Equal(6, structureDeclaration.ArraySize);

        var member = Assert.Single(structureDeclaration.Members);
        Assert.Equal("DIZI_PARAM1", member.Name);
        Assert.Equal(3, member.Level);

        var dataType = Assert.IsType<Pl1CharacterType>(member.DataType);
        Assert.Equal(1, dataType.Length);
    }

    /// <summary>
    /// PL/I variable declaration name-based array bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PARAM(2) syntax bilgisini Pl1VariableDeclaration.ArraySize alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM(2) CHAR(10);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı PARAM, ArraySize 2 ve data type CHAR(10) olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithVariableArrayDeclaration_ShouldSetArraySize()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM(2) CHAR(10);");

        Assert.Equal("PARAM", variableDeclaration.Name);
        Assert.Equal(2, variableDeclaration.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);
        Assert.Equal(10, dataType.Length);
    }

    /// <summary>
    /// PL/I variable declaration DIM attribute bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın data type sonrasında gelen DIM(2) bilgisini Pl1VariableDeclaration.ArraySize alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(10) DIM(2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı PARAM, ArraySize 2 ve data type CHAR(10) olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithVariableDimensionAttribute_ShouldSetArraySize()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM CHAR(10) DIM(2);");

        Assert.Equal("PARAM", variableDeclaration.Name);
        Assert.Equal(2, variableDeclaration.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);
        Assert.Equal(10, dataType.Length);
    }

    /// <summary>
    /// PL/I variable declaration DIMENSION attribute bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın DIMENSION(3) bilgisini Pl1VariableDeclaration.ArraySize alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(10) DIMENSION(3);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı PARAM, ArraySize 3 ve data type CHAR(10) olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithVariableDimensionKeywordAttribute_ShouldSetArraySize()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM CHAR(10) DIMENSION(3);");

        Assert.Equal("PARAM", variableDeclaration.Name);
        Assert.Equal(3, variableDeclaration.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);
        Assert.Equal(10, dataType.Length);
    }

    /// <summary>
    /// PL/I structure member name-based array bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın structure member adından sonra gelen (2) bilgisini member ArraySize alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 REC, 5 PARAM_LIST(2) CHAR(10);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// PARAM_LIST member ArraySize 2 ve data type CHAR(10) olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithStructureMemberArrayDeclaration_ShouldSetMemberArraySize()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 REC, 5 PARAM_LIST(2) CHAR(10);");

        var member = Assert.Single(structureDeclaration.Members);

        Assert.Equal("PARAM_LIST", member.Name);
        Assert.Equal(2, member.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(member.DataType);
        Assert.Equal(10, dataType.Length);
    }

    /// <summary>
    /// PL/I structure member DIM attribute bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın structure member veri tipi sonrasında gelen DIM(2) bilgisini member ArraySize alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 REC, 5 PARAM_LIST CHAR(10) DIM(2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// PARAM_LIST member ArraySize 2 ve data type CHAR(10) olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithStructureMemberDimensionAttribute_ShouldSetMemberArraySize()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 REC, 5 PARAM_LIST CHAR(10) DIM(2);");

        var member = Assert.Single(structureDeclaration.Members);

        Assert.Equal("PARAM_LIST", member.Name);
        Assert.Equal(2, member.ArraySize);

        var dataType = Assert.IsType<Pl1CharacterType>(member.DataType);
        Assert.Equal(10, dataType.Length);
    }

    /// <summary>
    /// PL/I nested structure bilgisinin recursive member modeliyle parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın veri tipi olmayan group member alanını nested group olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 MUSTERI, 5 ADRES, 10 IL CHAR(02);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ADRES group member olmalı, altında IL child member bulunmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithNestedStructureDeclaration_ShouldCreateNestedMembers()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 MUSTERI, 5 ADRES, 10 IL CHAR(02);");

        var groupMember = Assert.Single(structureDeclaration.Members);

        Assert.Equal("ADRES", groupMember.Name);
        Assert.True(groupMember.IsGroup);
        Assert.Null(groupMember.DataType);

        var childMember = Assert.Single(groupMember.Members);

        Assert.Equal("IL", childMember.Name);
        Assert.Equal(10, childMember.Level);

        var dataType = Assert.IsType<Pl1CharacterType>(childMember.DataType);
        Assert.Equal(2, dataType.Length);
    }

    /// <summary>
    /// PL/I VARCHAR declaration bilgisinin Pl1VarcharType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın VARCHAR(n) syntax bilgisini Pl1VarcharType modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL CUSTOMER_NAME VARCHAR(50);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı CUSTOMER_NAME, veri tipi Pl1VarcharType ve Length 50 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithVarcharDeclaration_ShouldCreateVarcharType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL CUSTOMER_NAME VARCHAR(50);");

        Assert.Equal("CUSTOMER_NAME", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1VarcharType>(variableDeclaration.DataType);
        Assert.Equal(50, dataType.Length);
    }

    /// <summary>
    /// PL/I FIXED DECIMAL synonym bilgisinin parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın FIXED DEC(p,s) syntax bilgisini FIXED DECIMAL ile aynı semantic model olarak ele aldığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL AMOUNT FIXED DEC(15,2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı AMOUNT, precision 15 ve scale 2 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFixedDecSynonym_ShouldCreateFixedDecimalType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL AMOUNT FIXED DEC(15,2);");

        Assert.Equal("AMOUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);
        Assert.Equal(15, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
    }

    /// <summary>
    /// PL/I DEC FIXED ters keyword sırasının parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın DEC FIXED(p,s) syntax bilgisini fixed decimal modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL AMOUNT DEC FIXED(17,2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı AMOUNT, precision 17 ve scale 2 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithDecFixedSynonym_ShouldCreateFixedDecimalType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL AMOUNT DEC FIXED(17,2);");

        Assert.Equal("AMOUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);
        Assert.Equal(17, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
    }

    /// <summary>
    /// PL/I DECIMAL FIXED ters keyword sırasının parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın DECIMAL FIXED(p) syntax bilgisini fixed decimal modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL AMOUNT DECIMAL FIXED(12);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı AMOUNT, precision 12 ve scale null olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithDecimalFixedSynonym_ShouldCreateFixedDecimalType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL AMOUNT DECIMAL FIXED(12);");

        Assert.Equal("AMOUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);
        Assert.Equal(12, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// PL/I FIXED BIN(15) bilgisinin Pl1FixedBinaryType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın binary fixed veri tipini precision bilgisiyle modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL SHORT_CODE FIXED BIN(15);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı SHORT_CODE, precision 15 ve scale null olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFixedBin15Declaration_ShouldCreateFixedBinaryType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL SHORT_CODE FIXED BIN(15);");

        Assert.Equal("SHORT_CODE", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedBinaryType>(variableDeclaration.DataType);
        Assert.Equal(15, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// PL/I FIXED BIN(31) bilgisinin Pl1FixedBinaryType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın binary fixed precision 31 bilgisini koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL INT_CODE FIXED BIN(31);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı INT_CODE, precision 31 ve scale null olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFixedBin31Declaration_ShouldCreateFixedBinaryType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL INT_CODE FIXED BIN(31);");

        Assert.Equal("INT_CODE", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedBinaryType>(variableDeclaration.DataType);
        Assert.Equal(31, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// PL/I BIN FIXED ters keyword sırasının parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın BIN FIXED(p) syntax bilgisini fixed binary modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL INT_CODE BIN FIXED(31);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı INT_CODE, precision 31 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithBinFixedDeclaration_ShouldCreateFixedBinaryType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL INT_CODE BIN FIXED(31);");

        Assert.Equal("INT_CODE", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedBinaryType>(variableDeclaration.DataType);
        Assert.Equal(31, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// PL/I PIC numeric pattern bilgisinin Pl1PictureType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PIC '999' bilgisini numeric picture category olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL CODE PIC '999';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, Precision 3, Scale null ve Length 3 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithNumericPictureDeclaration_ShouldCreatePictureType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL CODE PIC '999';");

        Assert.Equal("CODE", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);
        Assert.Equal("999", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Numeric, dataType.Category);
        Assert.Equal(3, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Equal(3, dataType.Length);
        Assert.True(dataType.IsNumeric);
        Assert.False(dataType.IsAlphanumeric);
        Assert.False(dataType.IsFormatted);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I PIC implied decimal bilgisinin scale olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PIC '999V99' pattern içindeki V karakterini implied decimal separator olarak yorumladığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL AMOUNT PIC '999V99';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, Precision 5, Scale 2 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithNumericPictureHavingScale_ShouldCreatePictureTypeWithScale()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL AMOUNT PIC '999V99';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("999V99", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Numeric, dataType.Category);
        Assert.Equal(5, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.Equal(5, dataType.Length);
        Assert.True(dataType.IsNumeric);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I PIC repeat notation bilgisinin precision ve scale hesabında kullanıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PIC '(13)9V99' pattern bilgisini expanded numeric picture olarak analiz ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL AMOUNT PIC '(13)9V99';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, Precision 15, Scale 2 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithNumericPictureHavingRepeatNotation_ShouldCreatePictureTypeWithPrecisionAndScale()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL AMOUNT PIC '(13)9V99';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("(13)9V99", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Numeric, dataType.Category);
        Assert.Equal(15, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.Equal(15, dataType.Length);
        Assert.True(dataType.IsNumeric);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I PICTURE keyword synonym bilgisinin PIC ile aynı şekilde parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PICTURE keyword'ünü PIC ile aynı Pl1PictureType modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL CODE PICTURE '999';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// RawPattern 999, Category Numeric ve Precision 3 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithPictureKeyword_ShouldCreatePictureType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL CODE PICTURE '999';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("999", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Numeric, dataType.Category);
        Assert.Equal(3, dataType.Precision);
        Assert.True(dataType.IsNumeric);
    }

    /// <summary>
    /// PL/I alphanumeric PIC pattern bilgisinin Pl1PictureType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PIC 'XXX' pattern bilgisini alphanumeric picture category olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL NAME PIC 'XXX';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Alphanumeric, Length 3 ve SupportsDirectEglMapping true olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithAlphanumericPictureDeclaration_ShouldCreateAlphanumericPictureType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL NAME PIC 'XXX';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("XXX", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, dataType.Category);
        Assert.Null(dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Equal(3, dataType.Length);
        Assert.False(dataType.IsNumeric);
        Assert.True(dataType.IsAlphanumeric);
        Assert.False(dataType.IsFormatted);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I alphanumeric PIC repeat notation bilgisinin length hesabında kullanıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PIC '(20)X' pattern bilgisini length 20 olan alphanumeric picture olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL NAME PIC '(20)X';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Alphanumeric ve Length 20 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithAlphanumericPictureRepeatNotation_ShouldCreateAlphanumericPictureType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL NAME PIC '(20)X';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("(20)X", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, dataType.Category);
        Assert.Equal(20, dataType.Length);
        Assert.True(dataType.IsAlphanumeric);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I signed numeric PIC bilgisinin IsSigned metadata alanında korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PIC 'S999' pattern bilgisini signed numeric picture olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL BALANCE PIC 'S999';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Numeric, IsSigned true ve Precision 3 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithSignedNumericPictureDeclaration_ShouldCreateSignedPictureType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL BALANCE PIC 'S999';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("S999", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Numeric, dataType.Category);
        Assert.Equal(3, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Equal(3, dataType.Length);
        Assert.True(dataType.IsSigned);
        Assert.True(dataType.IsNumeric);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I formatted PIC pattern bilgisinin direct EGL mapping dışında bırakıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PIC 'ZZ9' pattern bilgisini formatted picture olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL AMOUNT PIC 'ZZ9';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Formatted, IsFormatted true ve SupportsDirectEglMapping false olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFormattedPictureDeclaration_ShouldCreateFormattedPictureType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL AMOUNT PIC 'ZZ9';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("ZZ9", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Formatted, dataType.Category);
        Assert.Null(dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Null(dataType.Length);
        Assert.False(dataType.IsNumeric);
        Assert.False(dataType.IsAlphanumeric);
        Assert.True(dataType.IsFormatted);
        Assert.False(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I BIT declaration bilgisinin Pl1BitType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın BIT(n) syntax bilgisini length değerini koruyan Pl1BitType modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL FLAG BIT(1);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı FLAG, veri tipi Pl1BitType ve Length 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithBitDeclaration_ShouldCreateBitType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL FLAG BIT(1);");

        Assert.Equal("FLAG", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1BitType>(variableDeclaration.DataType);
        Assert.Equal(1, dataType.Length);
    }

    /// <summary>
    /// PL/I BIT structure member bilgisinin Pl1BitType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın structure member üzerindeki BIT(8) bilgisini member data type olarak taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 FLAGS, 5 MASK BIT(8);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// MASK member veri tipi Pl1BitType ve Length 8 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithBitStructureMember_ShouldCreateBitType()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 FLAGS, 5 MASK BIT(8);");

        var member = Assert.Single(structureDeclaration.Members);

        Assert.Equal("MASK", member.Name);

        var dataType = Assert.IsType<Pl1BitType>(member.DataType);
        Assert.Equal(8, dataType.Length);
    }

    /// <summary>
    /// PL/I FLOAT declaration bilgisinin Pl1FloatingType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın FLOAT keyword bilgisini Kind Float, Base Unspecified ve Precision null olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL RATE FLOAT;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı RATE, floating kind Float olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFloatDeclaration_ShouldCreateFloatingType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL RATE FLOAT;");

        Assert.Equal("RATE", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FloatingType>(variableDeclaration.DataType);
        Assert.Equal(Pl1FloatingTypeKind.Float, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, dataType.Base);
        Assert.Null(dataType.Precision);
    }

    /// <summary>
    /// PL/I FLOAT BIN precision bilgisinin Pl1FloatingType üzerinde korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın FLOAT BIN(53) syntax bilgisini binary floating semantic ile modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL RATE FLOAT BIN(53);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Kind Float, Base Binary ve Precision 53 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFloatBinaryDeclaration_ShouldCreateFloatingType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL RATE FLOAT BIN(53);");

        var dataType = Assert.IsType<Pl1FloatingType>(variableDeclaration.DataType);

        Assert.Equal(Pl1FloatingTypeKind.Float, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Binary, dataType.Base);
        Assert.Equal(53, dataType.Precision);
    }

    /// <summary>
    /// PL/I REAL declaration bilgisinin Pl1FloatingType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın REAL keyword bilgisini Kind Real olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL RATE REAL;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı RATE, floating kind Real olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithRealDeclaration_ShouldCreateFloatingType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL RATE REAL;");

        var dataType = Assert.IsType<Pl1FloatingType>(variableDeclaration.DataType);

        Assert.Equal(Pl1FloatingTypeKind.Real, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, dataType.Base);
        Assert.Null(dataType.Precision);
    }

    /// <summary>
    /// PL/I DOUBLE PRECISION declaration bilgisinin Pl1FloatingType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın DOUBLE PRECISION keyword ikilisini Kind DoublePrecision olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL RATE DOUBLE PRECISION;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Değişken adı RATE, floating kind DoublePrecision olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithDoublePrecisionDeclaration_ShouldCreateFloatingType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL RATE DOUBLE PRECISION;");

        var dataType = Assert.IsType<Pl1FloatingType>(variableDeclaration.DataType);

        Assert.Equal(Pl1FloatingTypeKind.DoublePrecision, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Unspecified, dataType.Base);
        Assert.Null(dataType.Precision);
    }

    /// <summary>
    /// PL/I FLOAT DECIMAL declaration bilgisinin decimal base ile parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın FLOAT DECIMAL syntax bilgisini Base Decimal olarak koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL RATE FLOAT DECIMAL(16);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Kind Float, Base Decimal ve Precision 16 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFloatDecimalDeclaration_ShouldCreateFloatingType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL RATE FLOAT DECIMAL(16);");

        var dataType = Assert.IsType<Pl1FloatingType>(variableDeclaration.DataType);

        Assert.Equal(Pl1FloatingTypeKind.Float, dataType.Kind);
        Assert.Equal(Pl1FloatingBase.Decimal, dataType.Base);
        Assert.Equal(16, dataType.Precision);
    }

    /// <summary>
    /// Desteklenmeyen top-level token için diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın DCL dışında başlayan token akışını syntax tree'ye eklemeden diagnostic ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Parse başarısız olmalı ve beklenmeyen token diagnostic mesajı üretilmelidir.
    /// </summary>
    [Fact]
    public void Parse_WithUnexpectedTopLevelToken_ShouldReturnDiagnostic()
    {
        var result = ParseSource(
            "PARAM CHAR(08);");

        Assert.False(result.Success);
        Assert.NotEmpty(result.Diagnostics);
        Assert.Contains(
            result.Diagnostics,
            x => x.Message.Contains("Beklenmeyen token: PARAM. Beklenen: DCL."));
    }

    /// <summary>
    /// Aynı source içinde birden fazla declaration parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın ardışık DCL ifadelerini aynı syntax tree içinde declaration listesi olarak tuttuğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM1 CHAR(08); DCL PARAM2 FIXED DECIMAL(5);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// SyntaxTree içinde iki declaration olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithMultipleDeclarations_ShouldCreateMultipleDeclarations()
    {
        var result = ParseSource(
            "DCL PARAM1 CHAR(08); DCL PARAM2 FIXED DECIMAL(5);");

        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);
        Assert.Equal(2, result.SyntaxTree!.Declarations.Count);

        var firstDeclaration = Assert.IsType<Pl1VariableDeclaration>(result.SyntaxTree.Declarations[0]);
        Assert.Equal("PARAM1", firstDeclaration.Name);

        var secondDeclaration = Assert.IsType<Pl1VariableDeclaration>(result.SyntaxTree.Declarations[1]);
        Assert.Equal("PARAM2", secondDeclaration.Name);
    }

    /// <summary>
    /// Array size bilgisinin hem name-based hem DIM attribute ile verilmesi durumunda diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın PARAM(2) CHAR(10) DIM(3) conflict durumunu diagnostic olarak raporladığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM(2) CHAR(10) DIM(3);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Parse başarısız olmalı ve array boyutu conflict diagnostic mesajı üretilmelidir.
    /// </summary>
    [Fact]
    public void Parse_WithArraySizeConflict_ShouldReturnDiagnostic()
    {
        var result = ParseSource(
            "DCL PARAM(2) CHAR(10) DIM(3);");

        Assert.False(result.Success);
        Assert.NotEmpty(result.Diagnostics);
        Assert.Contains(
            result.Diagnostics,
            x => x.Message.Contains("Array boyutu hem isim sonrasında hem de DIM / DIMENSION attribute ile verilemez."));
    }

    /// <summary>
    /// PL/I INITIAL keyword'ünün INIT ile aynı başlangıç değeri davranışını ürettiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın INITIAL keyword bilgisini Pl1InitialValue modeline taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM5 CHAR(4) INITIAL('ABCD');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// InitialValue.Value ABCD olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithInitialKeyword_ShouldSetInitialValue()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM5 CHAR(4) INITIAL('ABCD');");

        Assert.Equal("PARAM5", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal("ABCD", variableDeclaration.InitialValue!.Value);
        Assert.Null(variableDeclaration.InitialValue.RepeatCount);
        Assert.False(variableDeclaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I structure member alanlarının veri tipi ve INIT bilgileriyle parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın aynı structure altında birden fazla member alanı doğru sırayla modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 PARAME_LIST, 5 PARAM CHAR(08) INIT(' '), 5 PARAM2 CHAR(01) INIT(';');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// İki member oluşmalı; PARAM boşluk, PARAM2 noktalı virgül INIT değeri taşımalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithStructureDeclaration_ShouldCreateStructureMembers()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 PARAME_LIST, 5 PARAM CHAR(08) INIT(' '), 5 PARAM2 CHAR(01) INIT(';');");

        Assert.Equal(2, structureDeclaration.Members.Count);

        var firstMember = structureDeclaration.Members[0];
        Assert.Equal("PARAM", firstMember.Name);
        Assert.Equal(" ", firstMember.InitialValue!.Value);

        var secondMember = structureDeclaration.Members[1];
        Assert.Equal("PARAM2", secondMember.Name);
        Assert.Equal(";", secondMember.InitialValue!.Value);
    }

    /// <summary>
    /// PL/I structure içinde çok seviyeli nested group yapısının parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın 5, 10 ve 15 level hiyerarşisini recursive member ağacı olarak kurduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 PARAME_LIST, 5 GROUP1, 10 GROUP2, 15 FIELD1 CHAR(01);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// GROUP1 altında GROUP2, GROUP2 altında FIELD1 bulunmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithMultiLevelNestedStructureMember_ShouldCreateNestedGroupTree()
    {
        var structureDeclaration = ParseSingleStructureDeclaration(
            "DCL 1 PARAME_LIST, 5 GROUP1, 10 GROUP2, 15 FIELD1 CHAR(01);");

        var group1 = Assert.Single(structureDeclaration.Members);
        Assert.True(group1.IsGroup);

        var group2 = Assert.Single(group1.Members);
        Assert.True(group2.IsGroup);

        var field1 = Assert.Single(group2.Members);
        Assert.Equal("FIELD1", field1.Name);

        var field1Type = Assert.IsType<Pl1CharacterType>(field1.DataType);
        Assert.Equal(1, field1Type.Length);
    }

    /// <summary>
    /// PL/I FIXED BINARY(15,0) kullanımında explicit zero scale bilgisinin korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın binary fixed kullanımında açıkça yazılan ,0 bilgisini kaybetmediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL COUNT FIXED BINARY(15,0);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Precision 15, Scale 0 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithFixedBinaryExplicitZeroScale_ShouldCreateFixedBinaryTypeWithZeroScale()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL COUNT FIXED BINARY(15,0);");

        var dataType = Assert.IsType<Pl1FixedBinaryType>(variableDeclaration.DataType);

        Assert.Equal(15, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
    }

    /// <summary>
    /// PL/I PIC 'AAA' alphabetic pattern bilgisinin alphanumeric picture olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın A karakterlerinden oluşan PIC pattern'ı karakter tabanlı alan olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM8 PIC 'AAA';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Alphanumeric, Length 3 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithAlphabeticPicDeclaration_ShouldCreateAlphanumericPictureType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM8 PIC 'AAA';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("AAA", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, dataType.Category);
        Assert.Equal(3, dataType.Length);
        Assert.True(dataType.IsAlphanumeric);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I PIC 'AXXAA' mixed alphanumeric pattern bilgisinin toplam length ile parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın A ve X karakterlerinden oluşan mixed alphanumeric PIC pattern'ı tek karakter alanı olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM9 PIC 'AXXAA';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Category Alphanumeric, Length 5 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithMixedAlphanumericPicDeclaration_ShouldCreateAlphanumericPictureType()
    {
        var variableDeclaration = ParseSingleVariableDeclaration(
            "DCL PARAM9 PIC 'AXXAA';");

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("AXXAA", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, dataType.Category);
        Assert.Equal(5, dataType.Length);
        Assert.True(dataType.IsAlphanumeric);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// Parser'ın desteklenmeyen token için declaration veya statement beklendiğini bildirdiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1Parser, statement başlangıcı olmayan unsupported token gördüğünde yeni top-level
    /// beklenti mesajını üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// %
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Diagnostic içinde DCL veya executable statement bekleniyordu anlamına gelen mesaj bulunmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithUnsupportedTopLevelToken_ShouldReturnDeclarationOrStatementDiagnostic()
    {
        var result = ParseSource(
            "%");

        Assert.False(result.Success);
        Assert.NotNull(result.SyntaxTree);
        Assert.Empty(result.SyntaxTree!.Declarations);
        Assert.Empty(result.SyntaxTree.Statements);

        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Message.Contains("DCL veya executable statement"));
    }

    /// <summary>
    /// Parser'ın top-level assignment statement modelini syntax tree üzerinde taşıdığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1Parser, Identifier ile başlayan assignment statement'ı StatementParser'a
    /// yönlendirmeli ve üretilen Pl1AssignmentStatement modelini SyntaxTree.Statements
    /// listesine eklemelidir.
    ///
    /// Hangi input'u test eder?
    /// PARAM = 'ABC';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Declarations boş, Statements tek elemanlı olmalı; target PARAM, value 'ABC'
    /// olarak taşınmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithAssignmentStatement_ShouldAddAssignmentStatementToSyntaxTree()
    {
        var result = ParseSource(
            "PARAM = 'ABC';");

        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);
        Assert.Empty(result.SyntaxTree!.Declarations);

        var statement = Assert.Single(result.SyntaxTree.Statements);
        var assignmentStatement = Assert.IsType<Pl1AssignmentStatement>(statement);

        var target = Assert.Single(assignmentStatement.Targets);
        var targetExpression = Assert.IsType<Pl1RawExpression>(target);
        var valueExpression = Assert.IsType<Pl1RawExpression>(assignmentStatement.Value);

        Assert.Equal("PARAM", targetExpression.Text);
        Assert.Equal("'ABC'", valueExpression.Text);
    }

    /// <summary>
    /// Parser'ın declaration ve assignment statement modellerini aynı syntax tree içinde taşıdığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1Parser, önce DCL declaration parse etmeli, ardından assignment statement'ı
    /// StatementParser üzerinden parse edip aynı syntax tree'ye eklemelidir.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08); PARAM = 'ABC';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Declarations tek elemanlı, Statements tek elemanlı olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithDeclarationFollowedByAssignmentStatement_ShouldAddBothModelsToSyntaxTree()
    {
        var result = ParseSource(
            "DCL PARAM CHAR(08); PARAM = 'ABC';");

        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM", variableDeclaration.Name);

        var statement = Assert.Single(result.SyntaxTree.Statements);
        var assignmentStatement = Assert.IsType<Pl1AssignmentStatement>(statement);

        var target = Assert.Single(assignmentStatement.Targets);
        var targetExpression = Assert.IsType<Pl1RawExpression>(target);
        var valueExpression = Assert.IsType<Pl1RawExpression>(assignmentStatement.Value);

        Assert.Equal("PARAM", targetExpression.Text);
        Assert.Equal("'ABC'", valueExpression.Text);
    }

    /// <summary>
    /// Parser'ın qualified member assignment statement modelini syntax tree üzerinde taşıdığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1Parser, structure member erişimi içeren assignment statement'ı raw target
    /// expression olarak doğru formatta taşımalıdır.
    ///
    /// Hangi input'u test eder?
    /// DCLGLAU.BRM_KOD = 888;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Target DCLGLAU.BRM_KOD, value 888 olmalıdır.
    /// </summary>
    [Fact]
    public void Parse_WithQualifiedMemberAssignmentStatement_ShouldPreserveQualifiedTargetExpression()
    {
        var result = ParseSource(
            "DCLGLAU.BRM_KOD = 888;");

        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var statement = Assert.Single(result.SyntaxTree!.Statements);
        var assignmentStatement = Assert.IsType<Pl1AssignmentStatement>(statement);

        var target = Assert.Single(assignmentStatement.Targets);
        var targetExpression = Assert.IsType<Pl1RawExpression>(target);
        var valueExpression = Assert.IsType<Pl1RawExpression>(assignmentStatement.Value);

        Assert.Equal("DCLGLAU.BRM_KOD", targetExpression.Text);
        Assert.Equal("888", valueExpression.Text);
    }
}