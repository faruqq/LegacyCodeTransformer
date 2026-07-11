using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Semantic;

namespace LegacyCodeTransformer.PL1.Tests.Semantic
{
    /// <summary>
    /// PL/I basic symbol reference analysis davranışını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// P09.4 kapsamında semantic analyzer gerçek PL/I statement'larında kullanılan
    /// basit identifier reference bilgilerini toplamalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Assignment hedefi/değeri, CALL argümanı ve qualified structure reference
    /// kullanımlarının symbol table karşısında çözümlenme durumunu doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// CUSTOMER_NO = MUST_NO;
    /// CALL FETCH_CUSTOMER(CUSTOMER_NO);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// PL/I semantic analyzer unit testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Undefined identifier politikası ve full expression reference analysis
    /// testlerine temel olur.
    /// </summary>
    public sealed class Pl1SemanticAnalyzerReferenceTests
    {
        [Fact]
        public void Analyze_WithDeclaredAssignmentAndCallReferences_ShouldResolveReferences()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL MUST_NO FIXED DECIMAL(8);
                 DCL CUSTOMER_NO FIXED DECIMAL(8);

                 CUSTOMER_NO = MUST_NO;
                 CALL FETCH_CUSTOMER(CUSTOMER_NO);
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Equal(3, result.References.Count);

            Assert.All(
                result.References,
                reference => Assert.True(reference.IsResolved));

            Assert.Equal(
                2,
                result.References.Count(
                    reference =>
                        reference.RootSymbolName == "CUSTOMER_NO"));

            Assert.Single(
                result.References,
                reference =>
                    reference.RootSymbolName == "MUST_NO");
        }

        [Fact]
        public void Analyze_WithQualifiedStructureReference_ShouldResolveRootStructureSymbol()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL 1 CUSTOMER_INFO,
                     5 MUST_NO CHAR(8),
                     5 CUSTOMER_NAME CHAR(30);

                 CUSTOMER_INFO.MUST_NO = '12345678';
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Single(result.References);

            var reference = result.References[0];

            Assert.Equal(
                "CUSTOMER_INFO.MUST_NO",
                reference.ReferenceText);

            Assert.Equal(
                "CUSTOMER_INFO",
                reference.RootSymbolName);

            Assert.True(reference.IsResolved);
        }

        [Fact]
        public void Analyze_WithReferenceWithoutExplicitDeclaration_ShouldPreserveUnresolvedReferenceWithoutDiagnostic()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL CUSTOMER_NO FIXED DECIMAL(8);

                 CUSTOMER_NO = GUNCEL_MUST_NO;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Equal(2, result.References.Count);

            var unresolvedReference = Assert.Single(
                result.References,
                reference =>
                    reference.RootSymbolName == "GUNCEL_MUST_NO");

            Assert.False(unresolvedReference.IsResolved);
        }

        [Fact]
        public void Analyze_WithComplexCondition_ShouldNotGuessReferencesFromRawExpression()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL MUST_NO FIXED DECIMAL(8);
                 DCL CUSTOMER_NO FIXED DECIMAL(8);

                 IF MUST_NO = CUSTOMER_NO THEN
                     CALL FETCH_CUSTOMER;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Empty(result.References);
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