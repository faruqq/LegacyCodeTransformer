using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Semantic;

namespace LegacyCodeTransformer.PL1.Tests.Semantic
{
    public sealed class Pl1SemanticAnalyzerSymbolTableTests
    {
        [Fact]
        public void Analyze_WithGlobalVariableDeclarations_ShouldCreateGlobalSymbols()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL MUST_NO FIXED DECIMAL(8);
                 DCL CUSTOMER_NO FIXED DECIMAL(8);
                 DCL CUSTOMER_NAME CHAR(30);
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Equal(3, result.SymbolTable.Symbols.Count);
            Assert.True(result.SymbolTable.Symbols.ContainsKey("MUST_NO"));
            Assert.True(result.SymbolTable.Symbols.ContainsKey("CUSTOMER_NO"));
            Assert.True(result.SymbolTable.Symbols.ContainsKey("CUSTOMER_NAME"));
        }

        [Fact]
        public void Analyze_WithStructureDeclaration_ShouldCreateOnlyTopLevelStructureSymbol()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 DCL 1 CUSTOMER_INFO,
                     5 MUST_NO CHAR(8),
                     5 CUSTOMER_NAME CHAR(30);
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.Single(result.SymbolTable.Symbols);
            Assert.True(result.SymbolTable.Symbols.ContainsKey("CUSTOMER_INFO"));
            Assert.False(result.SymbolTable.Symbols.ContainsKey("MUST_NO"));
            Assert.False(result.SymbolTable.Symbols.ContainsKey("CUSTOMER_NAME"));
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