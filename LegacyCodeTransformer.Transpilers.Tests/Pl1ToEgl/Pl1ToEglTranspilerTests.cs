using LegacyCodeTransformer.Application.Services;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Generation;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;
using LegacyCodeTransformer.Transpilers.Naming;
using LegacyCodeTransformer.Transpilers.Pl1ToEgl;

namespace LegacyCodeTransformer.Transpilers.Tests.Pl1ToEgl;

/// <summary>
/// Pl1ToEglTranspiler için PL/I syntax modelinden EGL syntax modeline dönüşüm testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Parser tarafından üretilen PL/I modelinin, hedef EGL modeline doğru çevrildiğini
/// garanti altına almak için oluşturulmuştur.
///
/// Bu testler string çıktı üretimini değil, modelden modele dönüşümü doğrular.
/// </summary>
public sealed class Pl1ToEglTranspilerTests
{
    [Fact]
    public void Transpile_WithFixedDecimalDeclaration_ShouldCreateEglDecimalDeclaration()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "MUST_NO",
                new Pl1FixedDecimalType(8, 0, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("MustNo", variableDeclaration.Name);

        var dataType = Assert.IsType<EglDecimalType>(variableDeclaration.DataType);

        Assert.Equal(8, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
    }

    [Fact]
    public void Transpile_WithFixedDecimalScaleDeclaration_ShouldCreateEglDecimalDeclarationWithScale()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "CUSTOMER_NO",
                new Pl1FixedDecimalType(10, 2, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("CustomerNo", variableDeclaration.Name);

        var dataType = Assert.IsType<EglDecimalType>(variableDeclaration.DataType);

        Assert.Equal(10, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
    }

    /// <summary>
    /// PL/I karakter veri tipi modelinin EGL char veri tipi modeline dönüştüğünü doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser çıktısında CHAR / CHARACTER tanımları Pl1CharacterType olarak
    /// temsil edilir.
    /// EGL generator ise doğrudan PL/I tiplerini değil, EGL tip modellerini bilir.
    /// Bu nedenle Transpiler katmanının Pl1CharacterType modelini
    /// EglCharacterType modeline çevirmesi gerekir.
    ///
    /// Test edilen model:
    ///
    /// PL/I:
    /// - Pl1VariableDeclaration
    /// - Name: PROCESS_CODE
    /// - DataType: Pl1CharacterType(6)
    ///
    /// Beklenen EGL:
    /// - EglVariableDeclaration
    /// - Name: ProcessCode
    /// - DataType: EglCharacterType(6)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler testlerinde
    /// - Modelden modele dönüşüm doğrulamasında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Structure field, record field ve INIT bilgisi eklendiğinde karakter
    /// alanların hedef tipe doğru taşındığını garanti eder.
    /// </summary>
    [Fact]
    public void Transpile_WithCharacterDeclaration_ShouldCreateEglCharacterDeclaration()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "PROCESS_CODE",
                new Pl1CharacterType(6, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("ProcessCode", variableDeclaration.Name);

        var dataType = Assert.IsType<EglCharacterType>(variableDeclaration.DataType);

        Assert.Equal(6, dataType.Length);
    }

    /// <summary>
    /// PL/I identifier adının varsayılan olarak PascalCase üretildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Firma EGL kod standardında çoğunlukla PascalCase kullanıldığı için
    /// varsayılan naming strategy PascalCase olarak belirlenmiştir.
    ///
    /// Test edilen PL/I adı:
    ///
    /// MUST_NO
    ///
    /// Beklenen EGL adı:
    ///
    /// MustNo
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler testlerinde
    /// - Varsayılan naming strategy davranışını doğrulamada
    /// </summary>
    [Fact]
    public void Transpile_WithDefaultNamingOptions_ShouldCreatePascalCaseIdentifier()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "MUST_NO",
                new Pl1FixedDecimalType(8, 0, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("MustNo", variableDeclaration.Name);
    }

    /// <summary>
    /// PL/I identifier adının CamelCase strategy ile camelCase üretildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Önceki dönüşüm davranışı lower camel case idi.
    /// Strategy yapısına geçildikten sonra bu davranış opsiyonel olarak
    /// desteklenmeye devam etmelidir.
    ///
    /// Test edilen PL/I adı:
    ///
    /// MUST_NO
    ///
    /// Beklenen EGL adı:
    ///
    /// mustNo
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler testlerinde
    /// - Geriye dönük camelCase davranışını doğrulamada
    /// </summary>
    [Fact]
    public void Transpile_WithCamelCaseNamingOptions_ShouldCreateCamelCaseIdentifier()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "MUST_NO",
                new Pl1FixedDecimalType(8, 0, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler(
            new IdentifierNamingOptions(IdentifierNamingStyle.CamelCase));

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("mustNo", variableDeclaration.Name);
    }

    /// <summary>
    /// PL/I identifier adının Preserve strategy ile değiştirilmeden korunduğunu doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Bazı dönüşüm senaryolarında kaynak sistemdeki identifier adlarının aynen
    /// korunması gerekebilir.
    /// Strategy yapısı bu davranışı da desteklemelidir.
    ///
    /// Test edilen PL/I adı:
    ///
    /// MUST_NO
    ///
    /// Beklenen EGL adı:
    ///
    /// MUST_NO
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler testlerinde
    /// - Preserve naming strategy davranışını doğrulamada
    /// </summary>
    [Fact]
    public void Transpile_WithPreserveNamingOptions_ShouldPreserveIdentifier()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "MUST_NO",
                new Pl1FixedDecimalType(8, 0, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler(
            new IdentifierNamingOptions(IdentifierNamingStyle.Preserve));

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("MUST_NO", variableDeclaration.Name);
    }


    /// <summary>
    /// PL/I structure declaration modelinin EGL record declaration modeline dönüştüğünü doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser tarafından üretilen Pl1StructureDeclaration hedef dilde
    /// EglRecordDeclaration olarak temsil edilmelidir.
    ///
    /// Beklenen dönüşüm:
    ///
    /// PARAME_LIST -> ParameList
    /// PARAM -> Param
    /// PARAM2 -> Param2
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler testlerinde
    /// - Structure to record mapping doğrulamasında
    /// </summary>
    [Fact]
    public void Transpile_WithStructureDeclaration_ShouldCreateEglRecordDeclaration()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new Pl1Declaration[]
            {
            new Pl1StructureDeclaration(
                1,
                "PARAME_LIST",
                new[]
                {
                    new Pl1StructureMember(
                        5,
                        "PARAM",
                        new Pl1CharacterType(8, SourceLocation.Unknown),
                        SourceLocation.Unknown),

                    new Pl1StructureMember(
                        5,
                        "PARAM2",
                        new Pl1CharacterType(1, SourceLocation.Unknown),
                        SourceLocation.Unknown)
                },
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var recordDeclaration = Assert.IsType<EglRecordDeclaration>(declaration);

        Assert.Equal("ParameList", recordDeclaration.Name);
        Assert.Equal("basicRecord", recordDeclaration.RecordType);
        Assert.Equal(2, recordDeclaration.Fields.Count);

        Assert.Equal(10, recordDeclaration.Fields[0].Level);
        Assert.Equal("Param", recordDeclaration.Fields[0].Name);

        var firstFieldType = Assert.IsType<EglCharacterType>(recordDeclaration.Fields[0].DataType);
        Assert.Equal(8, firstFieldType.Length);

        Assert.Equal(10, recordDeclaration.Fields[1].Level);
        Assert.Equal("Param2", recordDeclaration.Fields[1].Name);

        var secondFieldType = Assert.IsType<EglCharacterType>(recordDeclaration.Fields[1].DataType);
        Assert.Equal(1, secondFieldType.Length);
    }

    /// <summary>
    /// PL/I structure array modelinden EGL record içinde parent array field
    /// üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın Pl1StructureDeclaration.ArraySize bilgisini okuyarak
    /// aynı basicRecord içinde parent array field ürettiğini doğrular.
    ///
    /// Test edilen input model:
    /// - DIZI(6)
    /// - 5 adet CHAR child member
    ///
    /// Beklenen temel EGL model:
    /// - Record name: Dizi
    /// - RecordType: basicRecord
    /// - İlk field: 5 Dizi char(15)[6]
    /// - Child field'lar: level 10
    /// </summary>
    [Fact]
    public void Transpile_WithStructureArrayDeclaration_ShouldCreateEglRecordArrayField()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new Pl1Declaration[]
            {
        new Pl1StructureDeclaration(
            1,
            "DIZI",
            new[]
            {
                new Pl1StructureMember(
                    3,
                    "DIZI_PARAM1",
                    new Pl1CharacterType(1, SourceLocation.Unknown),
                    SourceLocation.Unknown),

                new Pl1StructureMember(
                    3,
                    "DIZI_PARAM2",
                    new Pl1CharacterType(2, SourceLocation.Unknown),
                    SourceLocation.Unknown),

                new Pl1StructureMember(
                    3,
                    "DIZI_PARAM3",
                    new Pl1CharacterType(2, SourceLocation.Unknown),
                    SourceLocation.Unknown),

                new Pl1StructureMember(
                    3,
                    "DIZI_PARAM4",
                    new Pl1CharacterType(2, SourceLocation.Unknown),
                    SourceLocation.Unknown),

                new Pl1StructureMember(
                    3,
                    "DIZI_PARAM5",
                    new Pl1CharacterType(8, SourceLocation.Unknown),
                    SourceLocation.Unknown)
            },
            SourceLocation.Unknown,
            6)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var recordDeclaration = Assert.IsType<EglRecordDeclaration>(declaration);

        Assert.Equal("Dizi", recordDeclaration.Name);
        Assert.Equal("basicRecord", recordDeclaration.RecordType);
        Assert.Equal(6, recordDeclaration.Fields.Count);

        Assert.Equal(5, recordDeclaration.Fields[0].Level);
        Assert.Equal("Dizi", recordDeclaration.Fields[0].Name);
        Assert.Equal(6, recordDeclaration.Fields[0].ArraySize);

        var parentFieldType = Assert.IsType<EglCharacterType>(
            recordDeclaration.Fields[0].DataType);

        Assert.Equal(15, parentFieldType.Length);

        Assert.Equal(10, recordDeclaration.Fields[1].Level);
        Assert.Equal("DiziParam1", recordDeclaration.Fields[1].Name);

        Assert.Equal(10, recordDeclaration.Fields[5].Level);
        Assert.Equal("DiziParam5", recordDeclaration.Fields[5].Name);
    }

    /// <summary>
    /// PL/I structure member üzerinde bulunan array dimension bilgisinin
    /// EGL record field modeline taşındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın Pl1StructureMember.ArraySize bilgisini okuyup
    /// EglRecordFieldDeclaration.ArraySize alanına aktardığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - DCL 1 PARAME_LIST
    /// - 5 PARAM_LIST(2) CHAR(10)
    ///
    /// Beklenen temel EGL model:
    /// - Record name: ParameList
    /// - RecordType: basicRecord
    /// - Field name: ParamList
    /// - Field level: 10
    /// - Field ArraySize: 2
    /// - Field DataType: EglCharacterType(10)
    /// </summary>
    [Fact]
    public void Transpile_WithStructureMemberArrayDeclaration_ShouldCreateEglRecordArrayField()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
        new Pl1Declaration[]
        {
            new Pl1StructureDeclaration(
            1,
            "PARAME_LIST",
            new[]
            {
            new Pl1StructureMember(
            5,
            "PARAM_LIST",
            new Pl1CharacterType(10, SourceLocation.Unknown),
            SourceLocation.Unknown,
            null,
            2)
            },
            SourceLocation.Unknown)
        },
        SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var recordDeclaration = Assert.IsType<EglRecordDeclaration>(declaration);

        Assert.Equal("ParameList", recordDeclaration.Name);
        Assert.Equal("basicRecord", recordDeclaration.RecordType);

        var field = Assert.Single(recordDeclaration.Fields);

        Assert.Equal(10, field.Level);
        Assert.Equal("ParamList", field.Name);
        Assert.Equal(2, field.ArraySize);

        var fieldType = Assert.IsType<EglCharacterType>(field.DataType);

        Assert.Equal(10, fieldType.Length);
    }

    /// <summary>
    /// PL/I structure array içindeki member array alanlarının parent field
    /// length hesabına dahil edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın structure array parent field uzunluğunu hesaplarken
    /// Pl1StructureMember.ArraySize değerini çarpan olarak kullandığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - DIZI(6)
    /// - DIZI_PARAM1(2) CHAR(10)
    /// - DIZI_PARAM2 CHAR(5)
    ///
    /// Beklenen temel EGL model:
    /// - Parent field: 5 Dizi char(25)[6]
    /// - Çünkü hesap: (10 * 2) + 5 = 25
    /// - Child field: 10 DiziParam1 char(10)[2]
    /// - Child field: 10 DiziParam2 char(5)
    /// </summary>
    [Fact]
    public void Transpile_WithStructureArrayContainingMemberArray_ShouldCalculateParentLengthWithArrayMultiplier()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
        new Pl1Declaration[]
        {
            new Pl1StructureDeclaration(
            1,
            "DIZI",
            new[]
            {
                new Pl1StructureMember(
                3,
                "DIZI_PARAM1",
                new Pl1CharacterType(10, SourceLocation.Unknown),
                SourceLocation.Unknown,
                null,
                2),

                new Pl1StructureMember(
                    3,
                    "DIZI_PARAM2",
                    new Pl1CharacterType(5, SourceLocation.Unknown),
                    SourceLocation.Unknown)
            },
            SourceLocation.Unknown,
            6)
        },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var recordDeclaration = Assert.IsType<EglRecordDeclaration>(declaration);

        Assert.Equal("Dizi", recordDeclaration.Name);
        Assert.Equal("basicRecord", recordDeclaration.RecordType);
        Assert.Equal(3, recordDeclaration.Fields.Count);

        var parentField = recordDeclaration.Fields[0];

        Assert.Equal(5, parentField.Level);
        Assert.Equal("Dizi", parentField.Name);
        Assert.Equal(6, parentField.ArraySize);

        var parentFieldType = Assert.IsType<EglCharacterType>(parentField.DataType);

        Assert.Equal(25, parentFieldType.Length);

        var firstChildField = recordDeclaration.Fields[1];

        Assert.Equal(10, firstChildField.Level);
        Assert.Equal("DiziParam1", firstChildField.Name);
        Assert.Equal(2, firstChildField.ArraySize);

        var firstChildFieldType = Assert.IsType<EglCharacterType>(firstChildField.DataType);

        Assert.Equal(10, firstChildFieldType.Length);

        var secondChildField = recordDeclaration.Fields[2];

        Assert.Equal(10, secondChildField.Level);
        Assert.Equal("DiziParam2", secondChildField.Name);
        Assert.Null(secondChildField.ArraySize);

        var secondChildFieldType = Assert.IsType<EglCharacterType>(secondChildField.DataType);

        Assert.Equal(5, secondChildFieldType.Length);

    }

    /// <summary>
    /// EGL record child field üzerinde ArraySize varsa çıktının level 10
    /// indentation standardıyla ve char(n)[m] formatında üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Generator'ın field-level array bilgisini yalnızca parent field için değil,
    /// child record field için de doğru yazdırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// EGL model:
    /// - RecordType: basicRecord
    /// - Field: 10 ParamList char(10)[2]
    ///
    /// Beklenen temel çıktı:
    /// - level 10 için 8 boşluk indentation
    /// - char(10)[2] suffix formatı
    /// - basicRecord casing korunumu
    /// </summary>
    [Fact]
    public void Generate_WithChildRecordArrayFieldDeclaration_ShouldGenerateEglChildArrayField()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new EglDeclaration[]
            {
        new EglRecordDeclaration(
            "ParameList",
            "basicRecord",
            new[]
            {
                new EglRecordFieldDeclaration(
                    10,
                    "ParamList",
                    new EglCharacterType(10, SourceLocation.Unknown),
                    SourceLocation.Unknown,
                    2)
            },
            SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        var expected =
            "record ParameList type basicRecord" + Environment.NewLine +
            "        10 ParamList char(10)[2];" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// PL/I nested structure member modelinin EGL parent group field ve child
    /// field modellerine dönüştüğünü doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın veri tipi olmayan group member için parent EGL field
    /// ürettiğini ve group length değerini child field toplamından hesapladığını
    /// doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - ADRES_BILGI group member
    /// - IL_KOD CHAR(02)
    /// - ILCE_KOD CHAR(03)
    ///
    /// Beklenen temel EGL model:
    /// - 5 AdresBilgi char(5)
    /// - 10 IlKod char(2)
    /// - 10 IlceKod char(3)
    /// </summary>
    [Fact]
    public void Transpile_WithNestedStructureMember_ShouldCreateGroupFieldWithChildFields()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new Pl1Declaration[]
            {
        new Pl1StructureDeclaration(
            1,
            "PARAME_LIST",
            new[]
            {
                new Pl1StructureMember(
                    5,
                    "ADRES_BILGI",
                    null,
                    SourceLocation.Unknown,
                    null,
                    null,
                    new[]
                    {
                        new Pl1StructureMember(
                            10,
                            "IL_KOD",
                            new Pl1CharacterType(2, SourceLocation.Unknown),
                            SourceLocation.Unknown),

                        new Pl1StructureMember(
                            10,
                            "ILCE_KOD",
                            new Pl1CharacterType(3, SourceLocation.Unknown),
                            SourceLocation.Unknown)
                    })
            },
            SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var recordDeclaration = Assert.IsType<EglRecordDeclaration>(declaration);

        Assert.Equal("ParameList", recordDeclaration.Name);
        Assert.Equal("basicRecord", recordDeclaration.RecordType);
        Assert.Equal(3, recordDeclaration.Fields.Count);

        var groupField = recordDeclaration.Fields[0];

        Assert.Equal(5, groupField.Level);
        Assert.Equal("AdresBilgi", groupField.Name);
        Assert.Null(groupField.ArraySize);

        var groupFieldType = Assert.IsType<EglCharacterType>(groupField.DataType);

        Assert.Equal(5, groupFieldType.Length);

        var firstChildField = recordDeclaration.Fields[1];

        Assert.Equal(10, firstChildField.Level);
        Assert.Equal("IlKod", firstChildField.Name);

        var firstChildType = Assert.IsType<EglCharacterType>(firstChildField.DataType);

        Assert.Equal(2, firstChildType.Length);

        var secondChildField = recordDeclaration.Fields[2];

        Assert.Equal(10, secondChildField.Level);
        Assert.Equal("IlceKod", secondChildField.Name);

        var secondChildType = Assert.IsType<EglCharacterType>(secondChildField.DataType);

        Assert.Equal(3, secondChildType.Length);
    }

    /// <summary>
    /// PL/I çok seviyeli nested structure member modelinin EGL tarafında
    /// artan level değerleriyle üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın nested group seviyeleri derinleştikçe EGL level değerini
    /// 5 artırarak ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - GROUP1
    /// - GROUP2
    /// - FIELD1 CHAR(01)
    ///
    /// Beklenen temel EGL model:
    /// - 5 Group1 char(1)
    /// - 10 Group2 char(1)
    /// - 15 Field1 char(1)
    /// </summary>
    [Fact]
    public void Transpile_WithMultiLevelNestedStructureMember_ShouldCreateNestedGroupFieldsWithIncreasingLevels()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new Pl1Declaration[]
            {
        new Pl1StructureDeclaration(
            1,
            "PARAME_LIST",
            new[]
            {
                new Pl1StructureMember(
                    5,
                    "GROUP1",
                    null,
                    SourceLocation.Unknown,
                    null,
                    null,
                    new[]
                    {
                        new Pl1StructureMember(
                            10,
                            "GROUP2",
                            null,
                            SourceLocation.Unknown,
                            null,
                            null,
                            new[]
                            {
                                new Pl1StructureMember(
                                    15,
                                    "FIELD1",
                                    new Pl1CharacterType(1, SourceLocation.Unknown),
                                    SourceLocation.Unknown)
                            })
                    })
            },
            SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var recordDeclaration = Assert.IsType<EglRecordDeclaration>(declaration);

        Assert.Equal("ParameList", recordDeclaration.Name);
        Assert.Equal("basicRecord", recordDeclaration.RecordType);
        Assert.Equal(3, recordDeclaration.Fields.Count);

        var group1 = recordDeclaration.Fields[0];

        Assert.Equal(5, group1.Level);
        Assert.Equal("Group1", group1.Name);

        var group1Type = Assert.IsType<EglCharacterType>(group1.DataType);

        Assert.Equal(1, group1Type.Length);

        var group2 = recordDeclaration.Fields[1];

        Assert.Equal(10, group2.Level);
        Assert.Equal("Group2", group2.Name);

        var group2Type = Assert.IsType<EglCharacterType>(group2.DataType);

        Assert.Equal(1, group2Type.Length);

        var field1 = recordDeclaration.Fields[2];

        Assert.Equal(15, field1.Level);
        Assert.Equal("Field1", field1.Name);

        var field1Type = Assert.IsType<EglCharacterType>(field1.DataType);

        Assert.Equal(1, field1Type.Length);
    }

    /// <summary>
    /// PL/I nested structure tanımının uçtan uca EGL parent group field ve child
    /// field çıktısına dönüştüğünü doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser + Transpiler + Generator pipeline'ının nested group member
    /// hiyerarşisini koruyarak final EGL output ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL 1 PARAME_LIST,
    ///     5 ADRES_BILGI,
    ///         10 IL_KOD CHAR(02),
    ///         10 ILCE_KOD CHAR(03);
    ///
    /// Beklenen temel çıktı:
    /// - 5 AdresBilgi char(5);
    /// - 10 IlKod char(2);
    /// - 10 IlceKod char(3);
    /// </summary>
    [Fact]
    public void ConvertPl1ToEgl_WithNestedStructureMember_ShouldGenerateGroupFieldWithChildFields()
    {
        // Arrange
        var service = new ConversionService();

        var source =
            "DCL 1 PARAME_LIST, " +
            "5 ADRES_BILGI, " +
            "10 IL_KOD CHAR(02), " +
            "10 ILCE_KOD CHAR(03);";

        // Act
        var result = service.ConvertPl1ToEgl(source);

        // Assert
        var expected =
            "record ParameList type basicRecord" + Environment.NewLine +
            "    5 AdresBilgi char(5);" + Environment.NewLine +
            "        10 IlKod char(2);" + Environment.NewLine +
            "        10 IlceKod char(3);" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.True(result.Success);
        Assert.Equal(expected, result.Output);
        Assert.Empty(result.Diagnostics);
    }

    /// <summary>
    /// PL/I çok seviyeli nested structure tanımının uçtan uca EGL tarafında
    /// artan level ve indentation değerleriyle üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Nested structure mapping'in belirli bir kırılım sayısına hardcoded
    /// olmadığını ve recursive olarak derinleşebildiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL 1 PARAME_LIST,
    ///     5 GROUP1,
    ///         10 GROUP2,
    ///             15 FIELD1 CHAR(01);
    ///
    /// Beklenen temel çıktı:
    /// - 5 Group1 char(1);
    /// - 10 Group2 char(1);
    /// - 15 Field1 char(1);
    /// </summary>
    [Fact]
    public void ConvertPl1ToEgl_WithMultiLevelNestedStructureMember_ShouldGenerateIncreasingLevels()
    {
        // Arrange
        var service = new ConversionService();

        var source =
            "DCL 1 PARAME_LIST, " +
            "5 GROUP1, " +
            "10 GROUP2, " +
            "15 FIELD1 CHAR(01);";

        // Act
        var result = service.ConvertPl1ToEgl(source);

        // Assert
        var expected =
            "record ParameList type basicRecord" + Environment.NewLine +
            "    5 Group1 char(1);" + Environment.NewLine +
            "        10 Group2 char(1);" + Environment.NewLine +
            "            15 Field1 char(1);" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.True(result.Success);
        Assert.Equal(expected, result.Output);
        Assert.Empty(result.Diagnostics);
    }

    /// <summary>
    /// PL/I VARCHAR(n) veri tipinin EGL char(n) veri tipine dönüştüğünü doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın Pl1VarcharType modelini EglCharacterType modeline
    /// dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - DCL CUSTOMER_NAME VARCHAR(50)
    ///
    /// Beklenen temel EGL model:
    /// - Name: CustomerName
    /// - DataType: EglCharacterType
    /// - Length: 50
    /// </summary>
    [Fact]
    public void Transpile_WithVarcharDeclaration_ShouldCreateEglCharacterDeclaration()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "CUSTOMER_NAME",
                new Pl1VarcharType(50, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("CustomerName", variableDeclaration.Name);

        var dataType = Assert.IsType<EglCharacterType>(variableDeclaration.DataType);

        Assert.Equal(50, dataType.Length);
    }

    /// <summary>
    /// Structure array parent length hesabında VARCHAR(n) alanının n uzunluğu ile
    /// hesaba katıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// CalculateDataTypeLength davranışının Pl1VarcharType için length değerini
    /// döndürdüğünü dolaylı olarak doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - DIZI(6)
    /// - CUSTOMER_NAME VARCHAR(50)
    /// - CUSTOMER_CODE CHAR(10)
    ///
    /// Beklenen temel EGL model:
    /// - Parent field: 5 Dizi char(60)[6]
    /// - Çünkü hesap: VARCHAR(50) + CHAR(10) = 60
    /// </summary>
    [Fact]
    public void Transpile_WithStructureArrayContainingVarchar_ShouldCalculateParentLengthWithVarcharLength()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new Pl1Declaration[]
            {
            new Pl1StructureDeclaration(
                1,
                "DIZI",
                new[]
                {
                    new Pl1StructureMember(
                        3,
                        "CUSTOMER_NAME",
                        new Pl1VarcharType(50, SourceLocation.Unknown),
                        SourceLocation.Unknown),

                    new Pl1StructureMember(
                        3,
                        "CUSTOMER_CODE",
                        new Pl1CharacterType(10, SourceLocation.Unknown),
                        SourceLocation.Unknown)
                },
                SourceLocation.Unknown,
                6)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var recordDeclaration = Assert.IsType<EglRecordDeclaration>(declaration);

        Assert.Equal("Dizi", recordDeclaration.Name);
        Assert.Equal("basicRecord", recordDeclaration.RecordType);
        Assert.Equal(3, recordDeclaration.Fields.Count);

        var parentField = recordDeclaration.Fields[0];

        Assert.Equal(5, parentField.Level);
        Assert.Equal("Dizi", parentField.Name);
        Assert.Equal(6, parentField.ArraySize);

        var parentFieldType = Assert.IsType<EglCharacterType>(parentField.DataType);

        Assert.Equal(60, parentFieldType.Length);

        var firstChildField = recordDeclaration.Fields[1];

        Assert.Equal(10, firstChildField.Level);
        Assert.Equal("CustomerName", firstChildField.Name);

        var firstChildFieldType = Assert.IsType<EglCharacterType>(firstChildField.DataType);

        Assert.Equal(50, firstChildFieldType.Length);

        var secondChildField = recordDeclaration.Fields[2];

        Assert.Equal(10, secondChildField.Level);
        Assert.Equal("CustomerCode", secondChildField.Name);

        var secondChildFieldType = Assert.IsType<EglCharacterType>(secondChildField.DataType);

        Assert.Equal(10, secondChildFieldType.Length);
    }

    /// <summary>
    /// PL/I FIXED BIN(15) veri tipinin EGL smallint veri tipine dönüştüğünü
    /// doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın Pl1FixedBinaryType precision 15 kullanımını
    /// EglSmallIntType modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - DCL COUNT FIXED BIN(15)
    ///
    /// Beklenen temel EGL model:
    /// - Name: Count
    /// - DataType: EglSmallIntType
    /// </summary>
    [Fact]
    public void Transpile_WithFixedBinary15Declaration_ShouldCreateEglSmallIntDeclaration()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "COUNT",
                new Pl1FixedBinaryType(15, null, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("Count", variableDeclaration.Name);
        Assert.IsType<EglSmallIntType>(variableDeclaration.DataType);
    }

    /// <summary>
    /// PL/I FIXED BIN(31) veri tipinin EGL int veri tipine dönüştüğünü doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın Pl1FixedBinaryType precision 31 kullanımını EglIntType
    /// modeline dönüştürdüğünü doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - DCL COUNT FIXED BIN(31)
    ///
    /// Beklenen temel EGL model:
    /// - Name: Count
    /// - DataType: EglIntType
    /// </summary>
    [Fact]
    public void Transpile_WithFixedBinary31Declaration_ShouldCreateEglIntDeclaration()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "COUNT",
                new Pl1FixedBinaryType(31, null, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<EglVariableDeclaration>(declaration);

        Assert.Equal("Count", variableDeclaration.Name);
        Assert.IsType<EglIntType>(variableDeclaration.DataType);
    }

    /// <summary>
    /// Desteklenmeyen FIXED BINARY precision değerlerinde diagnostic üretildiğini
    /// doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın mapping kararı olmayan binary precision değerlerini sessizce
    /// yanlış bir EGL type'a çevirmediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - DCL COUNT FIXED BIN(13)
    ///
    /// Beklenen temel sonuç:
    /// - Result.Success false
    /// - Diagnostic var
    /// - SyntaxTree declaration listesi boş
    /// </summary>
    [Fact]
    public void Transpile_WithUnsupportedFixedBinaryPrecision_ShouldReturnDiagnostic()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "COUNT",
                new Pl1FixedBinaryType(13, null, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);
        Assert.Empty(result.SyntaxTree!.Declarations);
    }

    /// <summary>
    /// Desteklenmeyen FIXED BINARY scale değerlerinde diagnostic üretildiğini
    /// doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Transpiler'ın scale değeri 0 dışında olan binary fixed alanları ilk
    /// kapsamda desteklemediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// PL/I model:
    /// - DCL COUNT FIXED BIN(15,4)
    ///
    /// Beklenen temel sonuç:
    /// - Result.Success false
    /// - Diagnostic var
    /// - SyntaxTree declaration listesi boş
    /// </summary>
    [Fact]
    public void Transpile_WithUnsupportedFixedBinaryScale_ShouldReturnDiagnostic()
    {
        // Arrange
        var pl1SyntaxTree = new Pl1SyntaxTree(
            new[]
            {
            new Pl1VariableDeclaration(
                "COUNT",
                new Pl1FixedBinaryType(15, 4, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var transpiler = new Pl1ToEglTranspiler();

        // Act
        var result = transpiler.Transpile(pl1SyntaxTree);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);
        Assert.Empty(result.SyntaxTree!.Declarations);
    }
}