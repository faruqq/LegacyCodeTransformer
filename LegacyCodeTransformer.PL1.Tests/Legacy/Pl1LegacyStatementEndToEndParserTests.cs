using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Legacy
{
    public sealed class Pl1LegacyStatementEndToEndParserTests
    {
        [Fact]
        public void Parse_ShouldParseEmbeddedSqlAndCompilerDirectiveAtTopLevel()
        {
            var source = """
             %INCLUDE COPYLIB;
             EXEC SQL INCLUDE SQLCA;
             CALL INIT_PROCESS;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Declarations);
            Assert.Empty(result.SyntaxTree.Procedures);
            Assert.Equal(3, result.SyntaxTree.Statements.Count);

            var directive = Assert.IsType<Pl1CompilerDirectiveStatement>(
                result.SyntaxTree.Statements[0]);

            Assert.Equal("INCLUDE", directive.DirectiveName);
            Assert.Single(directive.Arguments);
            Assert.Equal("COPYLIB", directive.Arguments[0]);
            Assert.Equal("%INCLUDE COPYLIB", directive.RawDirectiveText);

            var embeddedSql = Assert.IsType<Pl1EmbeddedSqlStatement>(
                result.SyntaxTree.Statements[1]);

            Assert.Equal("EXEC SQL INCLUDE SQLCA", embeddedSql.RawSqlText);

            var call = Assert.IsType<Pl1CallStatement>(
                result.SyntaxTree.Statements[2]);

            Assert.Equal("INIT_PROCESS", call.ProcedureName);
        }

        [Fact]
        public void Parse_ShouldPreserveLegacyStatementOrderInsideProcedure()
        {
            var source = """
             PROCESS_MAIN: PROCEDURE;
                 %PAGE;
                 EXEC SQL INCLUDE SQLCA;
                 CALL FETCH_CUSTOMER;
             END PROCESS_MAIN;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Statements);
            Assert.Single(result.SyntaxTree.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("PROCESS_MAIN", procedure.Name);
            Assert.Equal(3, procedure.Statements.Count);

            Assert.IsType<Pl1CompilerDirectiveStatement>(procedure.Statements[0]);
            Assert.IsType<Pl1EmbeddedSqlStatement>(procedure.Statements[1]);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[2]);
        }

        [Fact]
        public void Parse_ShouldKeepDeclarationsProceduresAndLegacyStatementsSeparated()
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

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);

            Assert.Single(result.SyntaxTree!.Declarations);
            Assert.Single(result.SyntaxTree.Statements);
            Assert.Single(result.SyntaxTree.Procedures);

            Assert.IsType<Pl1CompilerDirectiveStatement>(
                result.SyntaxTree.Statements[0]);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal(3, procedure.Statements.Count);
            Assert.IsType<Pl1EmbeddedSqlStatement>(procedure.Statements[0]);
            Assert.IsType<Pl1AssignmentStatement>(procedure.Statements[1]);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[2]);
        }
    }
}