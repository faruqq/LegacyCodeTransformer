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
}