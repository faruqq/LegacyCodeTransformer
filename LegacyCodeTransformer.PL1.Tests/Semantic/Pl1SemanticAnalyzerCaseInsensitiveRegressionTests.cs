using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Semantic;

namespace LegacyCodeTransformer.PL1.Tests.Semantic
{
    /// <summary>
    /// PL/I identifier semantic çözümlemesinin case-insensitive davranışını
    /// doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynaklarında declaration ve kullanım casing değerleri farklı
    /// yazılabilir. Semantic çözümleme yalnızca karakter casing farkı nedeniyle
    /// reference'ı unresolved kabul etmemelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// SymbolTable ve reference çözümleme davranışının aynı case-insensitive
    /// identifier standardını kullandığını sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// customer_no = CUSTOMER_NO;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P09.5 semantic regression testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Scope ve type resolution eklendiğinde PL/I identifier karşılaştırma
    /// standardının korunmasına temel olur.
    /// </summary>
    public sealed class Pl1SemanticAnalyzerCaseInsensitiveRegressionTests
    {
        [Fact]
        public void Analyze_WithDifferentReferenceCasing_ShouldResolveDeclaredSymbol()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL CUSTOMER_NO FIXED DECIMAL(8);

                 customer_no = CUSTOMER_NO;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Single(result.SymbolTable.Symbols);
            Assert.Equal(2, result.References.Count);

            Assert.All(
                result.References,
                reference => Assert.True(reference.IsResolved));

            Assert.All(
                result.References,
                reference => Assert.True(
                    string.Equals(
                        "CUSTOMER_NO",
                        reference.RootSymbolName,
                        StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void Analyze_WithQualifiedReferenceDifferentCasing_ShouldResolveRootStructure()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL 1 CUSTOMER_INFO,
                     5 MUST_NO CHAR(8),
                     5 CUSTOMER_NAME CHAR(30);

                 customer_info.must_no = '12345678';
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Single(result.References);

            var reference = result.References[0];

            Assert.Equal(
                "customer_info.must_no",
                reference.ReferenceText);

            Assert.Equal(
                "customer_info",
                reference.RootSymbolName);

            Assert.True(reference.IsResolved);
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