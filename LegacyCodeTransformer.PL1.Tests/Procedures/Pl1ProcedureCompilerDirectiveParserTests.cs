using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    public sealed class Pl1ProcedureCompilerDirectiveParserTests
    {
        [Fact]
        public void Parse_ShouldParseCompilerDirectiveInsideProcedure()
        {
            var source = """
             PROCESS_MAIN: PROCEDURE;
                 %PAGE;
                 CALL FETCH_CUSTOMER;
             END PROCESS_MAIN;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal(2, procedure.Statements.Count);
            Assert.IsType<Pl1CompilerDirectiveStatement>(procedure.Statements[0]);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[1]);
        }
    }
}