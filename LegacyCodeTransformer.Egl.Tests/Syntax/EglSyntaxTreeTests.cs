using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Egl.Syntax;

namespace LegacyCodeTransformer.Egl.Tests.Syntax;

/// <summary>
/// EglSyntaxTree için declaration ve statement root model davranışlarını test eder.
///
/// Neden var?
/// ----------------------
/// P05.7 ile EGL syntax tree yalnızca declaration listesi değil, executable statement
/// listesi de taşımaya başlamıştır.
///
/// Ne çözüyor?
/// ----------------------
/// Transpiler'ın üreteceği EGL statement modellerinin root syntax tree üzerinde güvenli
/// şekilde taşınabildiğini doğrular.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// EGL syntax tree seviyesinde executable statement listesi taşıma davranışı.
///
/// Nerede kullanılır?
/// ----------------------
/// P05 statement pipeline ve ileride EGL generator statement output testlerinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// EglAssignmentStatement, EglCallStatement, EglIfStatement ve EglDoStatement modelleri
/// eklendiğinde bu root statement listesi generator input'u olacaktır.
/// </summary>
public sealed class EglSyntaxTreeTests
{
    /// <summary>
    /// Null declaration ve statement listeleriyle boş syntax tree oluşturulabildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// EglSyntaxTree geriye dönük uyumlu şekilde null collection inputlarını boş listeye
    /// çevirmelidir.
    ///
    /// Hangi input'u test eder?
    /// declarations null, statements null.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Declarations ve Statements listeleri boş olmalıdır.
    /// </summary>
    [Fact]
    public void Constructor_WithNullCollections_ShouldCreateEmptyLists()
    {
        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: null,
            location: SourceLocation.Unknown);

        Assert.Empty(syntaxTree.Declarations);
        Assert.Empty(syntaxTree.Statements);
    }

    /// <summary>
    /// Statement listesinin EGL syntax tree üzerinde korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// EglSyntaxTree constructor, verilen EglStatement listesini Statements property’sine
    /// taşımalıdır.
    ///
    /// Hangi input'u test eder?
    /// Test amaçlı fake EglStatement instance'ı.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Statements listesi tek elemanlı olmalıdır.
    /// </summary>
    [Fact]
    public void Constructor_WithStatements_ShouldPreserveStatementList()
    {
        var statement = new FakeEglStatement(SourceLocation.Unknown);

        var syntaxTree = new EglSyntaxTree(
            declarations: null,
            statements: new[]
            {
                statement
            },
            location: SourceLocation.Unknown);

        var result = Assert.Single(syntaxTree.Statements);

        Assert.Same(statement, result);
    }

    private sealed class FakeEglStatement : EglStatement
    {
        public FakeEglStatement(SourceLocation location)
            : base(location)
        {
        }
    }
}