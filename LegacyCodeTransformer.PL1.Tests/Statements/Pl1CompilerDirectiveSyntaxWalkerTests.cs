using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.PL1.Tests.Statements
{
    public sealed class Pl1CompilerDirectiveSyntaxWalkerTests
    {
        [Fact]
        public void Visit_ShouldVisitCompilerDirectiveStatement()
        {
            var statement = new Pl1CompilerDirectiveStatement(
                "PAGE",
                null,
                "%PAGE",
                SourceLocation.Unknown);

            var syntaxTree = new Pl1SyntaxTree(
                null,
                null,
                new[] { statement },
                SourceLocation.Unknown);

            var walker = new CountingWalker();

            walker.Visit(syntaxTree);

            Assert.Equal(1, walker.CompilerDirectiveStatementCount);
        }

        private sealed class CountingWalker : Pl1SyntaxWalker
        {
            public int CompilerDirectiveStatementCount { get; private set; }

            protected override void VisitCompilerDirectiveStatement(
                Pl1CompilerDirectiveStatement statement)
            {
                CompilerDirectiveStatementCount++;

                base.VisitCompilerDirectiveStatement(statement);
            }
        }
    }
}