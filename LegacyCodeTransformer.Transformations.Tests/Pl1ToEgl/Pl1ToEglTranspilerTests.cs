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
}