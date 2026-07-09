using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Generation;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Tests.Generation;

/// <summary>
/// EGL record output kalite standardını doğrulayan testleri içerir.
///
/// Neden var?
/// ----------------------
/// PL/I structure dönüşümleri EGL record output üretir. Record field indentation,
/// level ve array suffix davranışı P08 kapsamında sabitlenmelidir.
///
/// Ne çözüyor?
/// ----------------------
/// Nested record field indentation ve array field output davranışını doğrular.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// record Dizi type basicRecord
///     5 Dizi char(15)[6];
///         10 DiziParam1 char(1);
/// end
///
/// Nerede kullanılır?
/// ----------------------
/// EGL generator record regression testlerinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// sqlRecord, nested structure ve DB2 record output iyileştirmelerinde record
/// formatting standardını korur.
/// </summary>
public sealed class EglGeneratorRecordQualityTests
{
    [Fact]
    public void Generate_WithNestedRecordFields_ShouldPreserveLevelBasedIndentation()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: new EglDeclaration[]
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
                            SourceLocation.Unknown),
                        new EglRecordFieldDeclaration(
                            10,
                            "DiziParam2",
                            new EglCharacterType(2, SourceLocation.Unknown),
                            SourceLocation.Unknown)
                    },
                    SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "record Dizi type basicRecord" + Environment.NewLine +
            "    5 Dizi char(15)[6];" + Environment.NewLine +
            "        10 DiziParam1 char(1);" + Environment.NewLine +
            "        10 DiziParam2 char(2);" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }
}