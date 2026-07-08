using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.PL1.Tests.Statements
{
    public sealed class Pl1EmbeddedSqlSyntaxWalkerTests
    {
        [Fact]
        public void Visit_ShouldVisitEmbeddedSqlStatement()
        {
            var statement = new Pl1EmbeddedSqlStatement(
                "EXEC SQL INCLUDE SQLCA",
                SourceLocation.Unknown);

            var syntaxTree = new Pl1SyntaxTree(
                null,
                null,
                new[] { statement },
                SourceLocation.Unknown);

            var walker = new CountingWalker();

            walker.Visit(syntaxTree);

            Assert.Equal(1, walker.EmbeddedSqlStatementCount);
        }

        private sealed class CountingWalker : Pl1SyntaxWalker
        {
            public int EmbeddedSqlStatementCount { get; private set; }

            protected override void VisitEmbeddedSqlStatement(
                Pl1EmbeddedSqlStatement statement)
            {
                EmbeddedSqlStatementCount++;

                base.VisitEmbeddedSqlStatement(statement);
            }
        }
    }
}