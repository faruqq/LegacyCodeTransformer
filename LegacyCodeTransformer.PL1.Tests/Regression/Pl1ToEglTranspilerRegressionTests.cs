using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Transpilers.Pl1ToEgl;

namespace LegacyCodeTransformer.Transpilers.Tests.Regression
{
    public sealed class Pl1ToEglTranspilerRegressionTests
    {
        [Fact]
        public void Transpile_WithAssignmentCallIfAndDo_ShouldPreserveStatementCount()
        {
            var syntaxTree = new Pl1SyntaxTree(
                null,
                null,
                new Pl1Statement[]
                {
                    new Pl1AssignmentStatement(
                        new[] { new Pl1RawExpression("CUSTOMER_NO", SourceLocation.Unknown) },
                        new Pl1RawExpression("12345", SourceLocation.Unknown),
                        SourceLocation.Unknown),
                    new Pl1CallStatement(
                        "FETCH_CUSTOMER",
                        null,
                        SourceLocation.Unknown),
                    new Pl1IfStatement(
                        new Pl1RawExpression("SQLCODE = 0", SourceLocation.Unknown),
                        new Pl1CallStatement(
                            "FETCH_CURSOR",
                            null,
                            SourceLocation.Unknown),
                        null,
                        SourceLocation.Unknown),
                    new Pl1DoStatement(
                        Pl1DoStatementKind.While,
                        new Pl1RawExpression("SQLCODE = 0", SourceLocation.Unknown),
                        new Pl1BlockStatement(
                            new Pl1Statement[]
                            {
                                new Pl1CallStatement(
                                    "FETCH_CURSOR",
                                    null,
                                    SourceLocation.Unknown)
                            },
                            SourceLocation.Unknown),
                        SourceLocation.Unknown)
                },
                SourceLocation.Unknown);

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(syntaxTree);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Equal(4, result.SyntaxTree!.Statements.Count);
        }
    }
}