using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.PL1.Tests.Stabilization
{
    public sealed class Pl1SyntaxTreeModelStabilityTests
    {
        [Fact]
        public void Pl1SyntaxTree_ShouldKeepProceduresAndTopLevelStatementsSeparated()
        {
            var procedureCall = new Pl1CallStatement(
                "FETCH_CUSTOMER",
                null,
                SourceLocation.Unknown);

            var procedure = new Pl1Procedure(
                "PROCESS_CUSTOMER",
                null,
                new[] { procedureCall },
                SourceLocation.Unknown);

            var topLevelDirective = new Pl1CompilerDirectiveStatement(
                "INCLUDE",
                new[] { "COPYLIB" },
                "%INCLUDE COPYLIB",
                SourceLocation.Unknown);

            var syntaxTree = new Pl1SyntaxTree(
                null,
                new[] { procedure },
                new[] { topLevelDirective },
                SourceLocation.Unknown);

            Assert.Empty(syntaxTree.Declarations);
            Assert.Single(syntaxTree.Procedures);
            Assert.Single(syntaxTree.Statements);

            Assert.Equal("PROCESS_CUSTOMER", syntaxTree.Procedures[0].Name);
            Assert.IsType<Pl1CompilerDirectiveStatement>(syntaxTree.Statements[0]);
        }

        [Fact]
        public void Pl1Procedure_ShouldCopyOptionsAndStatements()
        {
            var options = new List<string> { "MAIN" };
            var statements = new List<Pl1Statement>
            {
                new Pl1CallStatement(
                    "INIT_PROCESS",
                    null,
                    SourceLocation.Unknown)
            };

            var procedure = new Pl1Procedure(
                "PROGRAM_NAME",
                options,
                statements,
                SourceLocation.Unknown);

            options.Add("REENTRANT");
            statements.Add(new Pl1CallStatement(
                "SECOND_PROCESS",
                null,
                SourceLocation.Unknown));

            Assert.Single(procedure.Options);
            Assert.Equal("MAIN", procedure.Options[0]);

            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1CallStatement>(procedure.Statements[0]);
        }
    }
}