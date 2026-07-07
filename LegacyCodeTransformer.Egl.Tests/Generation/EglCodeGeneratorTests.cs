using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Generation;
using LegacyCodeTransformer.Egl.InitialValues;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Tests.Generation;

/// <summary>
/// EglCodeGenerator için EGL syntax modelinden kaynak kod üretim testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Transpiler tarafından üretilen EglSyntaxTree modelinin doğru EGL kaynak
/// koduna dönüştürüldüğünü garanti altına almak için oluşturulmuştur.
///
/// Bu testler PL/I tarafını bilmez.
/// Sadece EGL modeli → EGL source dönüşümünü doğrular.
/// </summary>
public sealed class EglCodeGeneratorTests
{
    [Fact]
    public void Generate_WithDecimalDeclaration_ShouldGenerateEglDecimalDeclaration()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
                new EglVariableDeclaration(
                    "mustNo",
                    new EglDecimalType(8, 0, SourceLocation.Unknown),
                    SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("mustNo decimal(8,0);" + Environment.NewLine, result);
    }

    [Fact]
    public void Generate_WithDecimalScaleDeclaration_ShouldGenerateEglDecimalDeclarationWithScale()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
                new EglVariableDeclaration(
                    "customerNo",
                    new EglDecimalType(10, 2, SourceLocation.Unknown),
                    SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("customerNo decimal(10,2);" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL karakter veri tipi modelinden char(n) kaynak kodu üretildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Transpiler tarafından üretilen EglCharacterType modelinin gerçek EGL
    /// kaynak kodunda char(n) olarak yazdırılması gerekir.
    ///
    /// Test edilen EGL modeli:
    ///
    /// EglVariableDeclaration
    /// - Name: processCode
    /// - DataType: EglCharacterType(6)
    ///
    /// Beklenen EGL:
    ///
    /// processCode char(6);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - EGL Code Generator testlerinde
    /// - Hedef dil kaynak kodu üretimini doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Record field ve metadata üretimi geldiğinde char veri tipi formatının
    /// bozulmadığını garanti eder.
    /// </summary>
    [Fact]
    public void Generate_WithCharacterDeclaration_ShouldGenerateEglCharDeclaration()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "processCode",
                new EglCharacterType(6, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("processCode char(6);" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL record declaration modelinden record kaynak kodu üretildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure dönüşümü sonucunda oluşan EglRecordDeclaration modeli
    /// gerçek EGL record syntax'ına yazdırılmalıdır.
    ///
    /// Beklenen EGL:
    ///
    /// record ParameList type basicRecord
    ///     10 Param char(8);
    ///     10 Param2 char(1);
    /// end
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - EGL Code Generator testlerinde
    /// - Record kaynak kodu üretimini doğrulamada
    /// </summary>
    [Fact]
    public void Generate_WithRecordDeclaration_ShouldGenerateEglRecord()
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
                    "Param",
                    new EglCharacterType(8, SourceLocation.Unknown),
                    SourceLocation.Unknown),

                new EglRecordFieldDeclaration(
                    10,
                    "Param2",
                    new EglCharacterType(1, SourceLocation.Unknown),
                    SourceLocation.Unknown)
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
            "        10 Param char(8);" + Environment.NewLine +
            "        10 Param2 char(1);" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// EGL record field üzerinde ArraySize varsa çıktının char(n)[m]
    /// formatında üretildiğini doğrular.
    ///
    /// Test edilen EGL model:
    /// - 5 Dizi char(15)[6]
    /// - 10 DiziParam1 char(1)
    ///
    /// Beklenen çıktı:
    /// - 5 Dizi char(15)[6];
    /// </summary>
    [Fact]
    public void Generate_WithRecordArrayFieldDeclaration_ShouldGenerateEglRecordArrayField()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new EglDeclaration[]
            {
            new EglRecordDeclaration(
                "Dizi",
                "basicRecord",
                new[]
                {
                    new EglRecordFieldDeclaration(
                        5,
                        "Dizi",
                        new EglCharacterType(15, SourceLocation.Unknown),
                        SourceLocation.Unknown,
                        6),

                    new EglRecordFieldDeclaration(
                        10,
                        "DiziParam1",
                        new EglCharacterType(1, SourceLocation.Unknown),
                        SourceLocation.Unknown)
                },
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        var expected =
            "record Dizi type basicRecord" + Environment.NewLine +
            "    5 Dizi char(15)[6];" + Environment.NewLine +
            "        10 DiziParam1 char(1);" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// EGL decimal type üzerinde Scale null ise decimal(p) formatında çıktı
    /// üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Generator'ın scale verilmemiş decimal modeli için decimal(p,0)
    /// üretmediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// EglDecimalType(15, null)
    ///
    /// Beklenen çıktı:
    /// - Count decimal(15);
    /// </summary>
    [Fact]
    public void Generate_WithDecimalDeclarationWithoutScale_ShouldGenerateDecimalWithoutScale()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "Count",
                new EglDecimalType(15, null, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("Count decimal(15);" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL decimal type üzerinde Scale 0 ise decimal(p,0) formatında çıktı
    /// üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Generator'ın açıkça verilen scale 0 bilgisini output'a taşıdığını
    /// doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// EglDecimalType(15, 0)
    ///
    /// Beklenen çıktı:
    /// - Count decimal(15,0);
    /// </summary>
    [Fact]
    public void Generate_WithDecimalDeclarationHavingExplicitZeroScale_ShouldGenerateDecimalWithZeroScale()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "Count",
                new EglDecimalType(15, 0, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("Count decimal(15,0);" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL smallint type modelinin smallint keyword'üyle üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Generator'ın küçük integer type için standart casing olan `smallint`
    /// çıktısını ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// EglSmallIntType
    ///
    /// Beklenen çıktı:
    /// - Count smallint;
    /// </summary>
    [Fact]
    public void Generate_WithSmallIntDeclaration_ShouldGenerateSmallintDeclaration()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "Count",
                new EglSmallIntType(SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("Count smallint;" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL int type modelinin int keyword'üyle üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Generator'ın integer type için standart casing olan `int` çıktısını
    /// ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// EglIntType
    ///
    /// Beklenen çıktı:
    /// - Count int;
    /// </summary>
    [Fact]
    public void Generate_WithIntDeclaration_ShouldGenerateIntDeclaration()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "Count",
                new EglIntType(SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("Count int;" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL num type üzerinde Scale null ise num(p) formatında çıktı üretildiğini
    /// doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Generator'ın scale içermeyen num modelini num(p) formatında yazdırdığını
    /// doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// EglNumType(3, null)
    ///
    /// Beklenen çıktı:
    /// - Param1 num(3);
    /// </summary>
    [Fact]
    public void Generate_WithNumDeclarationWithoutScale_ShouldGenerateNumWithoutScale()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "Param1",
                new EglNumType(3, null, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("Param1 num(3);" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL num type üzerinde Scale varsa num(p,s) formatında çıktı üretildiğini
    /// doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Generator'ın implied decimal içeren numeric PIC mapping sonucu oluşan
    /// num modelini num(p,s) formatında yazdırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// EglNumType(5, 2)
    ///
    /// Beklenen çıktı:
    /// - Param2 num(5,2);
    /// </summary>
    [Fact]
    public void Generate_WithNumDeclarationHavingScale_ShouldGenerateNumWithScale()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "Param2",
                new EglNumType(5, 2, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("Param2 num(5,2);" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL character type modelinden Param6 char(3) kaynak kodu üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Generator'ın EglCharacterType(3) modelini char(3) olarak yazdırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// EglVariableDeclaration adı Param6, veri tipi EglCharacterType(3).
    ///
    /// Beklenen temel çıktı nedir?
    /// Param6 char(3);
    /// </summary>
    [Fact]
    public void Generate_WithAlphanumericPicCharacterDeclaration_ShouldGenerateCharDeclaration()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "Param6",
                new EglCharacterType(3, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("Param6 char(3);" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL character type modelinden Param7 char(20) kaynak kodu üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Generator'ın parser/transpiler tarafından taşınan length değerini char length olarak koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// EglVariableDeclaration adı Param7, veri tipi EglCharacterType(20).
    ///
    /// Beklenen temel çıktı nedir?
    /// Param7 char(20);
    /// </summary>
    [Fact]
    public void Generate_WithRepeatedAlphanumericPicCharacterDeclaration_ShouldGenerateCharDeclarationWithLength()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new[]
            {
            new EglVariableDeclaration(
                "Param7",
                new EglCharacterType(20, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal("Param7 char(20);" + Environment.NewLine, result);
    }

    /// <summary>
    /// EGL variable declaration üzerindeki initial value bilgisinin kaynak koda yazıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Generator'ın EglInitialValue modelini = "value" syntax'ı ile ürettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// EglVariableDeclaration adı Param, tipi char(4), initial value ABCD.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Param char(4) = "ABCD"; çıktısı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithInitialValue_ShouldGenerateDefaultValue()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new EglDeclaration[]
            {
            new EglVariableDeclaration(
                "Param",
                new EglCharacterType(4, SourceLocation.Unknown),
                SourceLocation.Unknown,
                null,
                new EglInitialValue(
                    "ABCD",
                    SourceLocation.Unknown))
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal(
            "Param char(4) = \"ABCD\";" + Environment.NewLine,
            result);
    }

    /// <summary>
    /// EGL initial value içindeki çift tırnak karakterinin escape edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Generator'ın string literal içindeki çift tırnak karakterini output üretirken escape ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// Initial value A"B.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Param char(3) = "A\"B"; çıktısı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithInitialValueHavingQuote_ShouldEscapeQuote()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new EglDeclaration[]
            {
            new EglVariableDeclaration(
                "Param",
                new EglCharacterType(3, SourceLocation.Unknown),
                SourceLocation.Unknown,
                null,
                new EglInitialValue(
                    "A\"B",
                    SourceLocation.Unknown))
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal(
            "Param char(3) = \"A\\\"B\";" + Environment.NewLine,
            result);
    }

    /// <summary>
    /// EGL initial value içindeki backslash karakterinin escape edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Generator'ın string literal içindeki backslash karakterini output üretirken escape ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// Initial value A\B.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Param char(3) = "A\\B"; çıktısı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithInitialValueHavingBackslash_ShouldEscapeBackslash()
    {
        // Arrange
        var syntaxTree = new EglSyntaxTree(
            new EglDeclaration[]
            {
            new EglVariableDeclaration(
                "Param",
                new EglCharacterType(3, SourceLocation.Unknown),
                SourceLocation.Unknown,
                null,
                new EglInitialValue(
                    "A\\B",
                    SourceLocation.Unknown))
            },
            SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        // Act
        var result = generator.Generate(syntaxTree);

        // Assert
        Assert.Equal(
            "Param char(3) = \"A\\\\B\";" + Environment.NewLine,
            result);
    }

    /// <summary>
    /// EGL assignment statement modelinden kaynak kod üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// EglCodeGenerator, EglSyntaxTree.Statements listesinde bulunan EglAssignmentStatement
    /// modelini EGL assignment statement satırına dönüştürmelidir.
    ///
    /// Hangi input'u test eder?
    /// EglAssignmentStatement Target Param, Value "ABC".
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Param = "ABC"; çıktısı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithAssignmentStatement_ShouldGenerateAssignmentStatement()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
            new EglAssignmentStatement(
                target: "Param",
                value: "\"ABC\"",
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        Assert.Equal(
            "Param = \"ABC\";" + Environment.NewLine,
            result);
    }

    /// <summary>
    /// EGL declaration ve assignment statement modellerinin aynı çıktıda sırayla üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Generator, önce Declarations listesini, ardından Statements listesini output'a
    /// yazmalıdır.
    ///
    /// Hangi input'u test eder?
    /// Param char(8); declaration ve Param = "ABC"; assignment statement.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Declaration satırı önce, assignment satırı sonra gelmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithDeclarationAndAssignmentStatement_ShouldGenerateBothInOrder()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: new[]
            {
            new EglVariableDeclaration(
                "Param",
                new EglCharacterType(8, SourceLocation.Unknown),
                SourceLocation.Unknown)
            },
            statements: new[]
            {
            new EglAssignmentStatement(
                target: "Param",
                value: "\"ABC\"",
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "Param char(8);" + Environment.NewLine +
            "Param = \"ABC\";" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// EGL CALL statement modelinden parametresiz CALL çıktısı üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// EglCodeGenerator, EglCallStatement modelini EGL call satırına dönüştürmelidir.
    ///
    /// Hangi input'u test eder?
    /// EglCallStatement ProcedureName FetchCursor, Arguments boş.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// call FetchCursor(); çıktısı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithCallStatement_ShouldGenerateCallStatement()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
            new EglCallStatement(
                procedureName: "FetchCursor",
                arguments: null,
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        Assert.Equal(
            "call FetchCursor();" + Environment.NewLine,
            result);
    }

    /// <summary>
    /// EGL CALL statement modelinden argument listesiyle CALL çıktısı üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Generator, CALL argument listesini virgül ve boşluk standardıyla yazdırmalıdır.
    ///
    /// Hangi input'u test eder?
    /// EglCallStatement ProcedureName Proc1, Arguments CustomerNo ve "ABC".
    ///
    /// Beklenen temel model/çıktı nedir?
    /// call Proc1(CustomerNo, "ABC"); çıktısı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithCallArguments_ShouldGenerateCallStatementWithArguments()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
            new EglCallStatement(
                procedureName: "Proc1",
                arguments: new[]
                {
                    "CustomerNo",
                    "\"ABC\""
                },
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        Assert.Equal(
            "call Proc1(CustomerNo, \"ABC\");" + Environment.NewLine,
            result);
    }

    /// <summary>
    /// EGL IF statement modelinden THEN branch içeren kaynak kod üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// EglCodeGenerator, EglIfStatement modelini indentation standardına uygun EGL if bloğuna
    /// dönüştürmelidir.
    ///
    /// Hangi input'u test eder?
    /// IF condition CustomerNo = MustNo, THEN call FetchCursor().
    ///
    /// Beklenen temel model/çıktı nedir?
    /// if bloğu, 4 boşluk indentation ile child CALL statement ve end satırı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithIfThenCallStatement_ShouldGenerateIfBlock()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
            new EglIfStatement(
                condition: "CustomerNo = MustNo",
                thenStatement: new EglCallStatement(
                    procedureName: "FetchCursor",
                    arguments: null,
                    location: SourceLocation.Unknown),
                elseStatement: null,
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "if (CustomerNo = MustNo)" + Environment.NewLine +
            "    call FetchCursor();" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// EGL IF statement modelinden THEN ELSE içeren kaynak kod üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Generator, optional ElseStatement bulunduğunda else satırını ve else branch child
    /// statement'ını output'a eklemelidir.
    ///
    /// Hangi input'u test eder?
    /// IF A = B THEN call Proc1(); ELSE call Proc2();
    ///
    /// Beklenen temel model/çıktı nedir?
    /// then ve else branch statementları 4 boşluk indentation ile üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithIfThenElseCallStatements_ShouldGenerateIfElseBlock()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
            new EglIfStatement(
                condition: "A = B",
                thenStatement: new EglCallStatement(
                    procedureName: "Proc1",
                    arguments: null,
                    location: SourceLocation.Unknown),
                elseStatement: new EglCallStatement(
                    procedureName: "Proc2",
                    arguments: null,
                    location: SourceLocation.Unknown),
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "if (A = B)" + Environment.NewLine +
            "    call Proc1();" + Environment.NewLine +
            "else" + Environment.NewLine +
            "    call Proc2();" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// EGL block DO statement modelinden do/end bloğu üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// EglCodeGenerator, EglDoStatement Kind Block modelini do/end bloğu olarak yazdırmalıdır.
    ///
    /// Hangi input'u test eder?
    /// do içinde call Proc1().
    ///
    /// Beklenen temel model/çıktı nedir?
    /// do satırı, 4 boşluk indentation ile child CALL ve end satırı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithBlockDoStatement_ShouldGenerateDoBlock()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
            new EglDoStatement(
                kind: EglDoStatementKind.Block,
                condition: null,
                statements: new[]
                {
                    new EglCallStatement(
                        procedureName: "Proc1",
                        arguments: null,
                        location: SourceLocation.Unknown)
                },
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "do" + Environment.NewLine +
            "    call Proc1();" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// EGL DO WHILE statement modelinden while/end bloğu üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// EglCodeGenerator, EglDoStatement Kind While modelini while bloğu olarak yazdırmalıdır.
    ///
    /// Hangi input'u test eder?
    /// while (Sqlcode = 0) içinde call FetchCursor().
    ///
    /// Beklenen temel model/çıktı nedir?
    /// while satırı, 4 boşluk indentation ile child CALL ve end satırı üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithDoWhileStatement_ShouldGenerateWhileBlock()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
            new EglDoStatement(
                kind: EglDoStatementKind.While,
                condition: "Sqlcode = 0",
                statements: new[]
                {
                    new EglCallStatement(
                        procedureName: "FetchCursor",
                        arguments: null,
                        location: SourceLocation.Unknown)
                },
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "while (Sqlcode = 0)" + Environment.NewLine +
            "    call FetchCursor();" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// EGL DO UNTIL statement modelinden negated while bloğu üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// EglCodeGenerator, EglDoStatement Kind Until modelini while (!(condition)) formatında yazdırmalıdır.
    ///
    /// Hangi input'u test eder?
    /// DO UNTIL EOF karşılığı.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// while (!(Eof)) bloğu üretilmelidir.
    /// </summary>
    [Fact]
    public void Generate_WithDoUntilStatement_ShouldGenerateNegatedWhileBlock()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
            new EglDoStatement(
                kind: EglDoStatementKind.Until,
                condition: "Eof",
                statements: new[]
                {
                    new EglCallStatement(
                        procedureName: "CloseCursor",
                        arguments: null,
                        location: SourceLocation.Unknown)
                },
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "while (!(Eof))" + Environment.NewLine +
            "    call CloseCursor();" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }
}
