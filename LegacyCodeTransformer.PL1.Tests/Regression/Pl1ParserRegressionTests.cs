using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Regression
{
    public sealed class Pl1ParserRegressionTests
    {
        [Fact]
        public void Parse_WithMixedSource_ShouldPreserveRootSeparation()
        {
            var source = """
             DCL CUSTOMER_NO FIXED DECIMAL(10);
             %INCLUDE COPYLIB;

             PROCESS_CUSTOMER: PROCEDURE;
                 EXEC SQL INCLUDE SQLCA;
                 CUSTOMER_NO = 12345;
                 CALL FETCH_CUSTOMER;
             END PROCESS_CUSTOMER;
            """;

            var result = ParseSource(source);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);

            Assert.Single(result.SyntaxTree!.Declarations);
            Assert.Single(result.SyntaxTree.Statements);
            Assert.Single(result.SyntaxTree.Procedures);

            Assert.IsType<Pl1CompilerDirectiveStatement>(
                result.SyntaxTree.Statements[0]);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_CUSTOMER", procedure.Name);
            Assert.Equal(3, procedure.Statements.Count);
            Assert.IsType<Pl1EmbeddedSqlStatement>(procedure.Statements[0]);
            Assert.IsType<Pl1AssignmentStatement>(procedure.Statements[1]);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[2]);
        }

        [Fact]
        public void Parse_WithNestedControlFlow_ShouldPreserveHierarchy()
        {
            var source = """
             PROCESS_MAIN: PROCEDURE;
                 IF SQLCODE = 0 THEN DO;
                     DO WHILE(SQLCODE = 0);
                         CALL FETCH_CURSOR;
                     END;
                 END;
             END PROCESS_MAIN;
            """;

            var result = ParseSource(source);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            var ifStatement = Assert.IsType<Pl1IfStatement>(
                procedure.Statements[0]);

            var thenBlock = Assert.IsType<Pl1BlockStatement>(
                ifStatement.ThenStatement);

            var doStatement = Assert.IsType<Pl1DoStatement>(
                thenBlock.Statements[0]);

            Assert.Equal(Pl1DoStatementKind.While, doStatement.Kind);
            Assert.Single(doStatement.Body.Statements);
            Assert.IsType<Pl1CallStatement>(doStatement.Body.Statements[0]);
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