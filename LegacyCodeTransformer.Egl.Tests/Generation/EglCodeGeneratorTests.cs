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
    /// record ParameList type BasicRecord
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
                "BasicRecord",
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
            "record ParameList type BasicRecord" + Environment.NewLine +
            "    10 Param char(8);" + Environment.NewLine +
            "    10 Param2 char(1);" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

}
