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

        /// <summary>
        /// Declaration, procedure ve top-level statement modellerinin aynı EGL
        /// syntax tree içinde doğru root koleksiyonlara aktarıldığını doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Procedure desteği eklendikten sonra mevcut declaration ve top-level
        /// statement dönüşüm davranışları kaybolmamalıdır.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// Bir global declaration, bir parametresiz procedure ve bir top-level
        /// function çağrısı.
        ///
        /// Beklenen temel model/çıktı nedir?
        /// ----------------------
        /// EGL syntax tree bir declaration, bir function ve bir statement
        /// taşımalıdır.
        /// </summary>
        [Fact]
        public void Transpile_WithDeclarationProcedureAndTopLevelStatement_ShouldPreserveRootCollections()
        {
            var syntaxTree = new Pl1.Syntax.Pl1SyntaxTree(
                declarations: new Pl1.Declarations.Pl1Declaration[]
                {
            new Pl1.Declarations.Pl1VariableDeclaration(
                "CUSTOMER_NO",
                new Pl1.Types.Pl1FixedDecimalType(
                    8,
                    null,
                    Core.Syntax.SourceLocation.Unknown),
                Core.Syntax.SourceLocation.Unknown)
                },
                procedures: new[]
                {
            new Pl1.Procedures.Pl1Procedure(
                "CUSTOMER_PROCESS",
                options: null,
                statements: new Pl1.Statements.Pl1Statement[]
                {
                    new Pl1.Statements.Pl1CallStatement(
                        "FETCH_CUSTOMER",
                        null,
                        Core.Syntax.SourceLocation.Unknown)
                },
                Core.Syntax.SourceLocation.Unknown)
                },
                statements: new Pl1.Statements.Pl1Statement[]
                {
            new Pl1.Statements.Pl1CallStatement(
                "CUSTOMER_PROCESS",
                null,
                Core.Syntax.SourceLocation.Unknown)
                },
                Core.Syntax.SourceLocation.Unknown);

            var transpiler =
                new Pl1ToEgl.Pl1ToEglTranspiler();

            var result = transpiler.Transpile(syntaxTree);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Declarations);
            Assert.Single(result.SyntaxTree.Functions);
            Assert.Single(result.SyntaxTree.Statements);
        }
    }
}