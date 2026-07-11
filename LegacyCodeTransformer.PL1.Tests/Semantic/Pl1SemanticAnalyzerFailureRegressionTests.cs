using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Semantic;

namespace LegacyCodeTransformer.PL1.Tests.Semantic
{
    /// <summary>
    /// Semantic hata durumlarında symbol ve reference bilgilerinin kontrollü
    /// biçimde korunmasını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Semantic diagnostic üretilmesi analyzer'ın syntax tree üzerindeki diğer
    /// güvenli bilgileri tamamen kaybetmesine neden olmamalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Duplicate declaration durumunda ilk symbol'ün korunduğunu, sonraki
    /// declaration'ın eklenmediğini ve reference analizinin çalışmaya devam
    /// ettiğini sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL MUST_NO CHAR(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// CUSTOMER_NO = MUST_NO;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P09.5 semantic failure regression testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Birden fazla semantic diagnostic'in aynı analiz çalışmasında
    /// toplanabilmesine ve IDE diagnostic senaryolarına temel olur.
    /// </summary>
    public sealed class Pl1SemanticAnalyzerFailureRegressionTests
    {
        [Fact]
        public void Analyze_WithDuplicateDeclaration_ShouldKeepFirstSymbolAndResolvedReferences()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL MUST_NO FIXED DECIMAL(8);
                 DCL MUST_NO CHAR(8);
                 DCL CUSTOMER_NO FIXED DECIMAL(8);

                 CUSTOMER_NO = MUST_NO;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.False(result.Success);
            Assert.Single(result.Diagnostics);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "Duplicate declaration bulundu: MUST_NO"));

            Assert.Equal(2, result.SymbolTable.Symbols.Count);
            Assert.True(result.SymbolTable.Symbols.ContainsKey("MUST_NO"));
            Assert.True(result.SymbolTable.Symbols.ContainsKey("CUSTOMER_NO"));

            Assert.Equal(2, result.References.Count);
            Assert.All(
                result.References,
                reference => Assert.True(reference.IsResolved));
        }

        [Fact]
        public void Analyze_WithUnresolvedReference_ShouldPreserveReferenceWithoutFailingSemanticResult()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL CUSTOMER_NO FIXED DECIMAL(8);

                 CUSTOMER_NO = GUNCEL_MUST_NO;
                 CALL FETCH_CUSTOMER(GUNCEL_MUST_NO);
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Equal(3, result.References.Count);

            var unresolvedReferences = result.References
                .Where(
                    reference =>
                        reference.RootSymbolName == "GUNCEL_MUST_NO")
                .ToList();

            Assert.Equal(2, unresolvedReferences.Count);
            Assert.All(
                unresolvedReferences,
                reference => Assert.False(reference.IsResolved));
        }

        private static Pl1.Syntax.Pl1SyntaxTree ParseSyntaxTree(string source)
        {
            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);

            return result.SyntaxTree!;
        }
    }
}