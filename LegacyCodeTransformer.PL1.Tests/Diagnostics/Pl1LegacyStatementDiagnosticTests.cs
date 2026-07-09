using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;

namespace LegacyCodeTransformer.PL1.Tests.Diagnostics
{
    public sealed class Pl1LegacyStatementDiagnosticTests
    {
        [Fact]
        public void Parse_WithCompilerDirectiveWithoutName_ShouldReturnDirectiveNameDiagnostic()
        {
            var result = ParseSource(
                "%;");

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);

            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("Compiler directive adı bekleniyordu"));
        }

        [Fact]
        public void Parse_WithExecWithoutSqlKeyword_ShouldReturnSqlExpectationDiagnostic()
        {
            var result = ParseSource(
                "EXEC INCLUDE SQLCA;");

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);

            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("SQL bekleniyordu"));
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