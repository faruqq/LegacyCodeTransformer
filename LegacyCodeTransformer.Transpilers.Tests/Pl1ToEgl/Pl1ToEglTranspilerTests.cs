using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
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
        Assert.Equal("BasicRecord", recordDeclaration.RecordType);
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
}