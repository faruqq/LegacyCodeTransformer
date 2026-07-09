using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Legacy
{
    public sealed class Pl1LegacyStatementProcedureParserTests
    {
        [Fact]
        public void Parse_ShouldParseLegacyStatementsInsideDoWhileProcedureBody()
        {
            var source = """
             FETCH_ALL: PROCEDURE;
                 DO WHILE(SQLCODE = 0);
                     EXEC SQL FETCH C1 INTO :CUSTOMER_NO;
                     CALL PROCESS_CUSTOMER;
                 END;
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

            var doStatement = Assert.IsType<Pl1DoStatement>(
                procedure.Statements[0]);

            Assert.Equal(Pl1DoStatementKind.While, doStatement.Kind);
            Assert.Equal(2, doStatement.Body.Statements.Count);
            Assert.IsType<Pl1EmbeddedSqlStatement>(doStatement.Body.Statements[0]);
            Assert.IsType<Pl1CallStatement>(doStatement.Body.Statements[1]);
        }

        [Fact]
        public void Parse_ShouldParseLegacyStatementsInsideIfThenDoProcedureBody()
        {
            var source = """
             CHECK_SQL: PROCEDURE;
                 IF SQLCODE = 0 THEN DO;
                     %PAGE;
                     EXEC SQL INCLUDE SQLCA;
                 END;
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

            var ifStatement = Assert.IsType<Pl1IfStatement>(
                procedure.Statements[0]);

            var thenBlock = Assert.IsType<Pl1BlockStatement>(
                ifStatement.ThenStatement);

            Assert.Equal(2, thenBlock.Statements.Count);
            Assert.IsType<Pl1CompilerDirectiveStatement>(thenBlock.Statements[0]);
            Assert.IsType<Pl1EmbeddedSqlStatement>(thenBlock.Statements[1]);
        }
    }
}