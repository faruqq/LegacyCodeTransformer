using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Generation;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Egl.Syntax;

namespace LegacyCodeTransformer.Egl.Tests.Generation;

/// <summary>
/// Nested EGL statement output kalitesini doğrulayan generator testlerini içerir.
///
/// Neden var?
/// ----------------------
/// IF ve DO statement'ları nested output üretir. P08 kapsamında indentation
/// standardının bozulmaması gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// IF içindeki DO, DO içindeki CALL ve nested indentation davranışını sabitler.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// if (Sqlcode = 0)
///     do
///         call FetchCursor();
///     end
/// end
///
/// Nerede kullanılır?
/// ----------------------
/// EGL generator nested statement regression testlerinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// SELECT, procedure block ve SQL output eklendiğinde nested indentation standardı
/// bu testlerle korunur.
/// </summary>
public sealed class EglGeneratorNestedStatementQualityTests
{
    [Fact]
    public void Generate_WithIfContainingDoBlock_ShouldPreserveNestedIndentation()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new EglStatement[]
            {
                new EglIfStatement(
                    "Sqlcode = 0",
                    new EglDoStatement(
                        EglDoStatementKind.Block,
                        null,
                        new EglStatement[]
                        {
                            new EglCallStatement(
                                "FetchCursor",
                                null,
                                SourceLocation.Unknown)
                        },
                        SourceLocation.Unknown),
                    null,
                    SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "if (Sqlcode = 0)" + Environment.NewLine +
            "    do" + Environment.NewLine +
            "        call FetchCursor();" + Environment.NewLine +
            "    end" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_WithDoWhileContainingIfStatement_ShouldPreserveNestedIndentation()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new EglStatement[]
            {
                new EglDoStatement(
                    EglDoStatementKind.While,
                    "Sqlcode = 0",
                    new EglStatement[]
                    {
                        new EglIfStatement(
                            "CustomerNo = MustNo",
                            new EglCallStatement(
                                "ProcessCustomer",
                                null,
                                SourceLocation.Unknown),
                            null,
                            SourceLocation.Unknown)
                    },
                    SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var generator = new EglCodeGenerator();

        var result = generator.Generate(syntaxTree);

        var expected =
            "while (Sqlcode = 0)" + Environment.NewLine +
            "    if (CustomerNo = MustNo)" + Environment.NewLine +
            "        call ProcessCustomer();" + Environment.NewLine +
            "    end" + Environment.NewLine +
            "end" + Environment.NewLine;

        Assert.Equal(expected, result);
    }
}