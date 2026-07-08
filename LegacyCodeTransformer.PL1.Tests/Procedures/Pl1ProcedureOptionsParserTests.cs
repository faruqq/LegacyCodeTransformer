using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    public sealed class Pl1ProcedureOptionsParserTests
    {
        [Fact]
        public void Parse_ShouldPreserveOptionsMain()
        {
            var source = """
             PROGRAM_NAME: PROCEDURE OPTIONS(MAIN);
                 CALL INIT_PROCESS;
             END PROGRAM_NAME;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROGRAM_NAME", procedure.Name);
            Assert.Single(procedure.Options);
            Assert.Equal("MAIN", procedure.Options[0]);
            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[0]);
        }

        [Fact]
        public void Parse_ShouldPreserveMultipleProcedureOptions()
        {
            var source = """
             PROGRAM_NAME: PROCEDURE OPTIONS(MAIN, REENTRANT);
                 CALL INIT_PROCESS;
             END PROGRAM_NAME;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal(2, procedure.Options.Count);
            Assert.Equal("MAIN", procedure.Options[0]);
            Assert.Equal("REENTRANT", procedure.Options[1]);
        }
    }
}