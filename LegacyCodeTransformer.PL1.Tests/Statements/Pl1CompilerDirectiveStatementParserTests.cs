using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Statements
{
    public sealed class Pl1CompilerDirectiveStatementParserTests
    {
        [Fact]
        public void Parse_ShouldParseIncludeDirective()
        {
            var source = """
             %INCLUDE COPYLIB;
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

            Assert.Equal("INCLUDE", statement.DirectiveName);
            Assert.Equal("%INCLUDE COPYLIB", statement.RawDirectiveText);
        }

        [Fact]
        public void Parse_ShouldParsePageDirective()
        {
            var source = """
             %PAGE;
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

            Assert.Equal("PAGE", statement.DirectiveName);
            Assert.Equal("%PAGE", statement.RawDirectiveText);
        }
    }
}