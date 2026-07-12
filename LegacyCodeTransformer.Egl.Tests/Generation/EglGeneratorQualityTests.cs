using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Generation;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Tests.Generation;

/// <summary>
/// EglCodeGenerator output kalite standardını doğrulayan regression testlerini içerir.
///
/// Neden var?
/// ----------------------
/// P08 kapsamında generator output'unun okunabilir, deterministik ve indentation
/// standardına uygun kalması gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Declaration / statement sırası, boş syntax tree davranışı ve statement output
/// satır sonu standardını sabitler.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// Param char(8);
/// Param = "ABC";
/// FetchCustomer();
///
/// Nerede kullanılır?
/// ----------------------
/// EGL generator regression testlerinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Procedure, SQL ve daha gelişmiş EGL output formatları eklendiğinde mevcut
/// temel output standardının bozulmadığını garanti eder.
/// </summary>
public sealed class EglGeneratorQualityTests
{
    [Fact]
    public void Generate_WithNullSyntaxTree_ShouldReturnEmptyString()
    {
        var generator = new EglCodeGenerator();

        var result = generator.Generate(null!);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Generate_WithEmptySyntaxTree_ShouldReturnEmptyString()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: null,
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Generate_WithDeclarationAssignmentAndCall_ShouldPreserveOutputOrder()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: new EglDeclaration[]
            {
                new EglVariableDeclaration(
                    "Param",
                    new EglCharacterType(8, SourceLocation.Unknown),
                    SourceLocation.Unknown)
            },
            statements: new EglStatement[]
            {
                new EglAssignmentStatement(
                    "Param",
                    "\"ABC\"",
                    SourceLocation.Unknown),
                new EglCallStatement(
                    "FetchCustomer",
                    null,
                    SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
             "Param char(8);" + Environment.NewLine +
             "Param = \"ABC\";" + Environment.NewLine +
             "FetchCustomer();" + Environment.NewLine;

        Assert.Equal(expected, result);
    }
}