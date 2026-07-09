using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;

namespace LegacyCodeTransformer.PL1.Tests.Diagnostics
{
    public sealed class Pl1TopLevelDiagnosticTests
    {
        [Fact]
        public void Parse_WithIncompleteCompilerDirective_ShouldReturnDirectiveNameDiagnostic()
        {
            var result = ParseSource(
                "%");

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Declarations);
            Assert.Empty(result.SyntaxTree.Procedures);
            Assert.Empty(result.SyntaxTree.Statements);

            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("Compiler directive adı bekleniyordu"));
        }

        [Fact]
        public void Parse_WithBadTokenAfterValidStatement_ShouldKeepParsedStatementAndReturnDiagnostic()
        {
            var result = ParseSource(
                """
                 CALL INIT_PROCESS;
                 @
                """);

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Declarations);
            Assert.Empty(result.SyntaxTree.Procedures);
            Assert.Single(result.SyntaxTree.Statements);

            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("DCL, procedure veya executable statement"));
        }

        private static Core.Results.ParseResult<Pl1.Syntax.Pl1SyntaxTree> ParseSource(
            string source)
        {
            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);

            return parser.Parse();
        }
    }
}