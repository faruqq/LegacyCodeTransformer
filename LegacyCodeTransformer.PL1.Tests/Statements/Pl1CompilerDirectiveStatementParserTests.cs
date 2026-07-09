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
            Assert.Single(statement.Arguments);
            Assert.Equal("COPYLIB", statement.Arguments[0]);
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
            Assert.Empty(statement.Arguments);
            Assert.Equal("%PAGE", statement.RawDirectiveText);
        }

        [Fact]
        public void Parse_ShouldParseProcessDirectiveArguments()
        {
            var source = """
             %PROCESS MACRO;
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
            Assert.Single(statement.Arguments);
            Assert.Equal("MACRO", statement.Arguments[0]);
            Assert.Equal("%PROCESS MACRO", statement.RawDirectiveText);
        }

        [Fact]
        public void Parse_ShouldPreserveDirectiveArgumentsWithoutGrammarParsing()
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