using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    public sealed class Pl1ProcedureSyntaxWalkerTests
    {
        [Fact]
        public void Visit_ShouldVisitProcedureAndProcedureStatements()
        {
            var callStatement = new Pl1CallStatement(
                "FETCH_CURSOR",
                null,
                SourceLocation.Unknown);

            var procedure = new Pl1Procedure(
                "PROCESS_CURSOR",
                null,
                new[] { callStatement },
                SourceLocation.Unknown);

            var syntaxTree = new Pl1SyntaxTree(
                null,
                new[] { procedure },
                null,
                SourceLocation.Unknown);

            var walker = new CountingWalker();

            walker.Visit(syntaxTree);

            Assert.Equal(1, walker.ProcedureCount);
            Assert.Equal(1, walker.CallStatementCount);
        }

        private sealed class CountingWalker : Pl1SyntaxWalker
        {
            public int ProcedureCount { get; private set; }

            public int CallStatementCount { get; private set; }

            protected override void VisitProcedure(Pl1Procedure procedure)
            {
                ProcedureCount++;

                base.VisitProcedure(procedure);
            }

            protected override void VisitCallStatement(Pl1CallStatement statement)
            {
                CallStatementCount++;

                base.VisitCallStatement(statement);
            }
        }
    }
}