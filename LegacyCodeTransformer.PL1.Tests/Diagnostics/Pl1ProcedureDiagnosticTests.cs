using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Diagnostics
{
    public sealed class Pl1ProcedureDiagnosticTests
    {
        [Fact]
        public void Parse_WithMissingProcedureEnd_ShouldReturnExpectedEndDiagnosticAndKeepProcedure()
        {
            var result = ParseSource(
                """
                 PROCESS_CURSOR: PROCEDURE;
                     CALL FETCH_CURSOR;
                """);

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_CURSOR", procedure.Name);
            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[0]);

            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("END PROCESS_CURSOR"));
        }

        [Fact]
        public void Parse_WithMismatchedProcedureEndName_ShouldReturnEndNameDiagnosticAndKeepBody()
        {
            var result = ParseSource(
                """
                 PROCESS_CURSOR: PROCEDURE;
                     CALL FETCH_CURSOR;
                 END OTHER_CURSOR;
                """);

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_CURSOR", procedure.Name);
            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[0]);

            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("END procedure adı 'PROCESS_CURSOR' olmalıydı"));
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