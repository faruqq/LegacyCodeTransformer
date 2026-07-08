using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    public sealed class Pl1ProcedureEndToEndParserTests
    {
        [Fact]
        public void Parse_ShouldKeepGlobalDeclarationsAndProcedureStatementsSeparated()
        {
            var source = """
             DCL CUSTOMER_NO FIXED DECIMAL(10);
             DCL CUSTOMER_NAME CHAR(30);

             PROCESS_CUSTOMER: PROCEDURE;
                 CUSTOMER_NO = 12345;
                 CALL FETCH_CUSTOMER;
             END PROCESS_CUSTOMER;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);

            Assert.Equal(2, result.SyntaxTree!.Declarations.Count);
            Assert.Empty(result.SyntaxTree.Statements);
            Assert.Single(result.SyntaxTree.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_CUSTOMER", procedure.Name);
            Assert.Equal(2, procedure.Statements.Count);
            Assert.IsType<Pl1AssignmentStatement>(procedure.Statements[0]);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[1]);
        }

        [Fact]
        public void Parse_ShouldParseMultipleProceduresInSameSource()
        {
            var source = """
             INIT_PROCESS: PROCEDURE;
                 CALL OPEN_CURSOR;
             END INIT_PROCESS;

             FETCH_PROCESS: PROCEDURE;
                 CALL FETCH_CURSOR;
             END FETCH_PROCESS;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);

            Assert.Empty(result.SyntaxTree!.Declarations);
            Assert.Empty(result.SyntaxTree.Statements);
            Assert.Equal(2, result.SyntaxTree.Procedures.Count);

            Assert.Equal("INIT_PROCESS", result.SyntaxTree.Procedures[0].Name);
            Assert.Equal("FETCH_PROCESS", result.SyntaxTree.Procedures[1].Name);

            Assert.Single(result.SyntaxTree.Procedures[0].Statements);
            Assert.Single(result.SyntaxTree.Procedures[1].Statements);

            Assert.IsType<Pl1CallStatement>(result.SyntaxTree.Procedures[0].Statements[0]);
            Assert.IsType<Pl1CallStatement>(result.SyntaxTree.Procedures[1].Statements[0]);
        }
    }
}