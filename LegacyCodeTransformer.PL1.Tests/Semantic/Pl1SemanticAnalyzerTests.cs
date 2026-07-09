using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Semantic;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.PL1.Tests.Semantic
{
    /// <summary>
    /// PL/I semantic analyzer foundation testlerini içerir.
    ///
    /// Neden var?
    /// ----------------------
    /// P09.1 kapsamında semantic analyzer pipeline'a eklenmiştir fakat henüz semantic
    /// kural çalıştırmamaktadır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Analyzer'ın boş syntax tree üzerinde başarılı sonuç döndürdüğünü ve diagnostic
    /// üretmediğini sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Parser tarafından üretilmiş geçerli bir Pl1SyntaxTree.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Semantic analyzer unit testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Duplicate declaration ve reference analysis testleri aynı semantic analyzer
    /// giriş noktası üzerinden genişletilecektir.
    /// </summary>
    public sealed class Pl1SemanticAnalyzerTests
    {
        [Fact]
        public void Analyze_WithEmptySyntaxTree_ShouldReturnSuccess()
        {
            var syntaxTree = new Pl1SyntaxTree(
                null,
                null,
                null,
                SourceLocation.Unknown);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
        }
    }
}