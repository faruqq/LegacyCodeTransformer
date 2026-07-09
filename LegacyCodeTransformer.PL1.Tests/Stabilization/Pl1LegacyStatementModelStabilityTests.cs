using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Stabilization
{
    public sealed class Pl1LegacyStatementModelStabilityTests
    {
        [Fact]
        public void Pl1EmbeddedSqlStatement_ShouldPreserveRawSqlText()
        {
            var statement = new Pl1EmbeddedSqlStatement(
                "EXEC SQL INCLUDE SQLCA",
                SourceLocation.Unknown);

            Assert.Equal("EXEC SQL INCLUDE SQLCA", statement.RawSqlText);
        }

        [Fact]
        public void Pl1CompilerDirectiveStatement_ShouldPreserveNameArgumentsAndRawText()
        {
            var arguments = new List<string> { "COPYLIB" };

            var statement = new Pl1CompilerDirectiveStatement(
                "INCLUDE",
                arguments,
                "%INCLUDE COPYLIB",
                SourceLocation.Unknown);

            arguments.Add("OTHER");

            Assert.Equal("INCLUDE", statement.DirectiveName);
            Assert.Single(statement.Arguments);
            Assert.Equal("COPYLIB", statement.Arguments[0]);
            Assert.Equal("%INCLUDE COPYLIB", statement.RawDirectiveText);
        }
    }
}