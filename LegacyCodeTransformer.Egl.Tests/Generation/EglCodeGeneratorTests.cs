using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Generation;
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
}
