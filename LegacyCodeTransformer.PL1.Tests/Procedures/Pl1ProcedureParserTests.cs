using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    public sealed class Pl1ProcedureParserTests
    {
        [Fact]
        public void Parse_ShouldParseProcedureWithCallStatement()
        {
            var source = """
             PROCESS_CURSOR: PROCEDURE;
                 CALL FETCH_CURSOR;
             END PROCESS_CURSOR;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Declarations);
            Assert.Empty(result.SyntaxTree.Statements);
            Assert.Single(result.SyntaxTree.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_CURSOR", procedure.Name);
            Assert.Empty(procedure.Options);
            Assert.Single(procedure.Statements);

            var callStatement = Assert.IsType<Pl1CallStatement>(
                procedure.Statements[0]);

            Assert.Equal("FETCH_CURSOR", callStatement.ProcedureName);
        }

        [Fact]
        public void Parse_ShouldParseProcedureOptionsMain()
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
        }

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
        public void Parse_ShouldParseAssignmentAndCallStatementsInsideProcedure()
        {
            var source = """
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
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_CUSTOMER", procedure.Name);
            Assert.Equal(2, procedure.Statements.Count);
            Assert.IsType<Pl1AssignmentStatement>(procedure.Statements[0]);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[1]);
        }

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
        public void Parse_ShouldParseDoWhileStatementInsideProcedure()
        {
            var source = """
             FETCH_ALL: PROCEDURE;
                 DO WHILE(SQLCODE = 0);
                     CALL FETCH_CURSOR;
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

            Assert.Single(procedure.Statements);

            var doStatement = Assert.IsType<Pl1DoStatement>(
                procedure.Statements[0]);

            Assert.Equal(Pl1DoStatementKind.While, doStatement.Kind);
            Assert.Single(doStatement.Body.Statements);
            Assert.IsType<Pl1CallStatement>(doStatement.Body.Statements[0]);
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