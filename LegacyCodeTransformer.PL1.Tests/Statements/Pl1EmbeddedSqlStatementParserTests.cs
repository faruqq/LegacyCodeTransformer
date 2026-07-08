using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Statements
{
    public sealed class Pl1EmbeddedSqlStatementParserTests
    {
        [Fact]
        public void Parse_ShouldParseExecSqlIncludeStatement()
        {
            var source = """
             EXEC SQL INCLUDE SQLCA;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Declarations);
            Assert.Empty(result.SyntaxTree.Procedures);
            Assert.Single(result.SyntaxTree.Statements);

            var statement = Assert.IsType<Pl1EmbeddedSqlStatement>(
                result.SyntaxTree.Statements[0]);

            Assert.Equal("EXEC SQL INCLUDE SQLCA", statement.RawSqlText);
        }

        [Fact]
        public void Parse_ShouldParseExecSqlSelectStatementAsRawSqlText()
        {
            var source = """
             EXEC SQL SELECT CUSTOMER_NO INTO :CUSTOMER_NO FROM CUSTOMER_TABLE;
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Statements);

            var statement = Assert.IsType<Pl1EmbeddedSqlStatement>(
                result.SyntaxTree.Statements[0]);

            Assert.Equal(
                "EXEC SQL SELECT CUSTOMER_NO INTO : CUSTOMER_NO FROM CUSTOMER_TABLE",
                statement.RawSqlText);
        }
    }
}