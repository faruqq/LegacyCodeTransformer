using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.PL1.Tests.Stabilization
{
    public sealed class Pl1SyntaxWalkerModelStabilityTests
    {
        [Fact]
        public void Visit_ShouldTraverseProcedureAndLegacyStatements()
        {
            var procedure = new Pl1Procedure(
                "PROCESS_MAIN",
                null,
                new Pl1Statement[]
                {
                    new Pl1CompilerDirectiveStatement(
                        "PAGE",
                        null,
                        "%PAGE",
                        SourceLocation.Unknown),
                    new Pl1EmbeddedSqlStatement(
                        "EXEC SQL INCLUDE SQLCA",
                        SourceLocation.Unknown),
                    new Pl1CallStatement(
                        "FETCH_CUSTOMER",
                        null,
                        SourceLocation.Unknown)
                },
                SourceLocation.Unknown);

            var syntaxTree = new Pl1SyntaxTree(
                null,
                new[] { procedure },
                null,
                SourceLocation.Unknown);

            var walker = new CountingWalker();

            walker.Visit(syntaxTree);

            Assert.Equal(1, walker.ProcedureCount);
            Assert.Equal(1, walker.CompilerDirectiveCount);
            Assert.Equal(1, walker.EmbeddedSqlCount);
            Assert.Equal(1, walker.CallCount);
        }

        private sealed class CountingWalker : Pl1SyntaxWalker
        {
            public int ProcedureCount { get; private set; }

            public int CompilerDirectiveCount { get; private set; }

            public int EmbeddedSqlCount { get; private set; }

            public int CallCount { get; private set; }

            protected override void VisitProcedure(Pl1Procedure procedure)
            {
                ProcedureCount++;

                base.VisitProcedure(procedure);
            }

            protected override void VisitCompilerDirectiveStatement(
                Pl1CompilerDirectiveStatement statement)
            {
                CompilerDirectiveCount++;

                base.VisitCompilerDirectiveStatement(statement);
            }

            protected override void VisitEmbeddedSqlStatement(
                Pl1EmbeddedSqlStatement statement)
            {
                EmbeddedSqlCount++;

                base.VisitEmbeddedSqlStatement(statement);
            }

            protected override void VisitCallStatement(Pl1CallStatement statement)
            {
                CallCount++;

                base.VisitCallStatement(statement);
            }
        }
    }
}