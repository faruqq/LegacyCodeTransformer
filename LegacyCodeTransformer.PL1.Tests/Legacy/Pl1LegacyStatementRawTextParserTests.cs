using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Legacy
{
    public sealed class Pl1LegacyStatementRawTextParserTests
    {
        [Fact]
        public void Parse_ShouldPreserveEmbeddedSqlSelectAsRawText()
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

        [Fact]
        public void Parse_ShouldPreserveCompilerDirectiveArgumentsAsTokens()
        {
            var source = """
             %PROCESS FLAG(TEST);
            """;

            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            var result = parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Statements);

            var statement = Assert.IsType<Pl1CompilerDirectiveStatement>(
                result.SyntaxTree.Statements[0]);

            Assert.Equal("PROCESS", statement.DirectiveName);
            Assert.Equal(
                new[] { "FLAG", "(", "TEST", ")" },
                statement.Arguments);
            Assert.Equal("%PROCESS FLAG ( TEST )", statement.RawDirectiveText);
        }
    }
}