using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    public sealed class Pl1ProcedureDiagnosticParserTests
    {
        [Fact]
        public void Parse_ShouldAddDiagnosticWhenEndProcedureNameDoesNotMatch()
        {
            var source = """
             PROCESS_CURSOR: PROCEDURE;
                 CALL FETCH_CURSOR;
             END OTHER_CURSOR;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.False(result.Success);
            Assert.NotEmpty(result.Diagnostics);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_CURSOR", procedure.Name);
            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[0]);
        }

        [Fact]
        public void Parse_ShouldAddDiagnosticWhenProcedureEndIsMissing()
        {
            var source = """
             PROCESS_CURSOR: PROCEDURE;
                 CALL FETCH_CURSOR;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.False(result.Success);
            Assert.NotEmpty(result.Diagnostics);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_CURSOR", procedure.Name);
            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[0]);
        }
    }
}