using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Semantic;

namespace LegacyCodeTransformer.PL1.Tests.Semantic
{
    /// <summary>
    /// PL/I semantic analyzer'ın birlikte çalışan temel davranışlarını
    /// gerçekçi kaynak örnekleri üzerinden doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Symbol table, duplicate declaration ve basic reference analysis ayrı
    /// testlerle doğrulanmıştır. Ancak bu davranışların aynı syntax tree üzerinde
    /// birlikte çalıştığı da sabitlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Global declaration sembollerinin oluşturulmasını, statement
    /// reference'larının çözülmesini ve semantic sonucun tutarlı kalmasını
    /// regression testleriyle güvence altına alır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NAME CHAR(30);
    ///
    /// CUSTOMER_NO = MUST_NO;
    /// CALL FETCH_CUSTOMER(CUSTOMER_NO, CUSTOMER_NAME);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P09.5 semantic analysis regression testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Type resolution, scope analysis ve daha gelişmiş reference çözümleme
    /// davranışları eklendiğinde mevcut semantic sözleşmenin bozulmadığını
    /// doğrulamaya devam eder.
    /// </summary>
    public sealed class Pl1SemanticAnalyzerRegressionTests
    {
        [Fact]
        public void Analyze_WithBankingDeclarationsAndReferences_ShouldCreateConsistentSemanticResult()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL MUST_NO FIXED DECIMAL(8);
                 DCL CUSTOMER_NO FIXED DECIMAL(8);
                 DCL CUSTOMER_NAME CHAR(30);

                 CUSTOMER_NO = MUST_NO;
                 CALL FETCH_CUSTOMER(CUSTOMER_NO, CUSTOMER_NAME);
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);

            Assert.Equal(3, result.SymbolTable.Symbols.Count);
            Assert.True(result.SymbolTable.Symbols.ContainsKey("MUST_NO"));
            Assert.True(result.SymbolTable.Symbols.ContainsKey("CUSTOMER_NO"));
            Assert.True(result.SymbolTable.Symbols.ContainsKey("CUSTOMER_NAME"));

            Assert.Equal(4, result.References.Count);
            Assert.All(
                result.References,
                reference => Assert.True(reference.IsResolved));

            Assert.Single(
                result.References,
                reference =>
                    reference.RootSymbolName == "MUST_NO");

            Assert.Equal(
                2,
                result.References.Count(
                    reference =>
                        reference.RootSymbolName == "CUSTOMER_NO"));

            Assert.Single(
                result.References,
                reference =>
                    reference.RootSymbolName == "CUSTOMER_NAME");
        }

        [Fact]
        public void Analyze_WithGlobalDeclarationsUsedInsideProcedure_ShouldResolveProcedureReferences()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL MUST_NO FIXED DECIMAL(8);
                 DCL CUSTOMER_NO FIXED DECIMAL(8);

                 CUSTOMER_FETCH: PROCEDURE;
                     CUSTOMER_NO = MUST_NO;
                     CALL FETCH_CUSTOMER(CUSTOMER_NO);
                 END CUSTOMER_FETCH;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Equal(2, result.SymbolTable.Symbols.Count);
            Assert.Equal(3, result.References.Count);

            Assert.All(
                result.References,
                reference => Assert.True(reference.IsResolved));
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