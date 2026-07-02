using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Types;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;
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

        Assert.Equal("mustNo", declaration.Name);

        var dataType = Assert.IsType<EglDecimalType>(declaration.DataType);

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

        Assert.Equal("customerNo", declaration.Name);

        var dataType = Assert.IsType<EglDecimalType>(declaration.DataType);

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
    /// - Name: processCode
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

        Assert.Equal("processCode", declaration.Name);

        var dataType = Assert.IsType<EglCharacterType>(declaration.DataType);

        Assert.Equal(6, dataType.Length);
    }
}