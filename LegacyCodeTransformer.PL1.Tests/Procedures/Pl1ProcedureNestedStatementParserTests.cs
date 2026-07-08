using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    public sealed class Pl1ProcedureNestedStatementParserTests
    {
        [Fact]
        public void Parse_ShouldParseIfStatementInsideProcedure()
        {
            var source = """
             CHECK_SQL: PROCEDURE;
                 IF SQLCODE = 0 THEN CALL FETCH_CURSOR;
             END CHECK_SQL;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1IfStatement>(procedure.Statements[0]);
        }

        [Fact]
        public void Parse_ShouldParseDoWhileStatementInsideProcedureWithoutClosingProcedureEarly()
        {
            var source = """
             FETCH_ALL: PROCEDURE;
                 DO WHILE(SQLCODE = 0);
                     CALL FETCH_CURSOR;
                 END;
                 CALL CLOSE_CURSOR;
             END FETCH_ALL;
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

            var doStatement = Assert.IsType<Pl1DoStatement>(
                procedure.Statements[0]);

            Assert.Equal(Pl1DoStatementKind.While, doStatement.Kind);
            Assert.Single(doStatement.Body.Statements);
            Assert.IsType<Pl1CallStatement>(doStatement.Body.Statements[0]);

            Assert.IsType<Pl1CallStatement>(procedure.Statements[1]);
        }

        [Fact]
        public void Parse_ShouldParseDoUntilStatementInsideProcedure()
        {
            var source = """
             PROCESS_UNTIL_DONE: PROCEDURE;
                 DO UNTIL(DONE = 1);
                     DONE = 1;
                 END;
             END PROCESS_UNTIL_DONE;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Single(procedure.Statements);

            var doStatement = Assert.IsType<Pl1DoStatement>(
                procedure.Statements[0]);

            Assert.Equal(Pl1DoStatementKind.Until, doStatement.Kind);
            Assert.Single(doStatement.Body.Statements);
            Assert.IsType<Pl1AssignmentStatement>(doStatement.Body.Statements[0]);
        }
    }
}