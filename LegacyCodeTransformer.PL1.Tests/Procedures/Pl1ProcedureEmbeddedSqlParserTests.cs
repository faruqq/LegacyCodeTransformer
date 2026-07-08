using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    public sealed class Pl1ProcedureEmbeddedSqlParserTests
    {
        [Fact]
        public void Parse_ShouldParseEmbeddedSqlStatementInsideProcedure()
        {
            var source = """
             PROCESS_CUSTOMER: PROCEDURE;
                 EXEC SQL INCLUDE SQLCA;
                 CALL FETCH_CUSTOMER;
             END PROCESS_CUSTOMER;
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

            Assert.Equal("PROCESS_CUSTOMER", procedure.Name);
            Assert.Equal(2, procedure.Statements.Count);

            var sqlStatement = Assert.IsType<Pl1EmbeddedSqlStatement>(
                procedure.Statements[0]);

            Assert.Equal("EXEC SQL INCLUDE SQLCA", sqlStatement.RawSqlText);

            Assert.IsType<Pl1CallStatement>(procedure.Statements[1]);
        }
    }
}