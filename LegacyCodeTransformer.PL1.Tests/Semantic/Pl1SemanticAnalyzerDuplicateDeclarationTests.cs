using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Semantic;

namespace LegacyCodeTransformer.PL1.Tests.Semantic
{
    public sealed class Pl1SemanticAnalyzerDuplicateDeclarationTests
    {
        [Fact]
        public void Analyze_WithDuplicateVariableDeclaration_ShouldReturnDuplicateDeclarationDiagnostic()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL MUST_NO FIXED DECIMAL(8);
                 DCL MUST_NO CHAR(8);
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.False(result.Success);
            Assert.Single(result.Diagnostics);
            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("Duplicate declaration bulundu: MUST_NO"));

            Assert.Single(result.SymbolTable.Symbols);
            Assert.True(result.SymbolTable.Symbols.ContainsKey("MUST_NO"));
        }

        [Fact]
        public void Analyze_WithDuplicateDeclarationDifferentCase_ShouldReturnDuplicateDeclarationDiagnostic()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL MUST_NO FIXED DECIMAL(8);
                 DCL must_no CHAR(8);
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.False(result.Success);
            Assert.Single(result.Diagnostics);
            Assert.Single(result.SymbolTable.Symbols);
            Assert.True(result.SymbolTable.Symbols.ContainsKey("MUST_NO"));
        }

        [Fact]
        public void Analyze_WithDuplicateStructureDeclaration_ShouldReturnDuplicateDeclarationDiagnostic()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL 1 CUSTOMER_INFO,
                     5 MUST_NO CHAR(8);

                 DCL 1 CUSTOMER_INFO,
                     5 CUSTOMER_NAME CHAR(30);
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.False(result.Success);
            Assert.Single(result.Diagnostics);
            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("Duplicate declaration bulundu: CUSTOMER_INFO"));

            Assert.Single(result.SymbolTable.Symbols);
            Assert.True(result.SymbolTable.Symbols.ContainsKey("CUSTOMER_INFO"));
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