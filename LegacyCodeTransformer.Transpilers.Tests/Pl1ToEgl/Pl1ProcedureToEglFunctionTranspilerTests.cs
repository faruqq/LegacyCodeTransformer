using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Transpilers.Pl1ToEgl;

namespace LegacyCodeTransformer.Transpilers.Tests.Pl1ToEgl
{
    /// <summary>
    /// PL/I procedure modellerinin EGL function modellerine
    /// dönüştürülmesini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Procedure modelleri parser ve semantic katmanda korunmasına rağmen
    /// transpiler tarafından işlenmezse business logic sessizce kaybolur.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Desteklenen parametresiz procedure dönüşümünü ve henüz
    /// desteklenmeyen parameter/local declaration durumlarındaki
    /// diagnostic davranışını sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE;
    ///     CUSTOMER_NO = MUST_NO;
    ///     CALL FETCH_CUSTOMER(CUSTOMER_NO);
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P10.3 procedure transpiler unit testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parameter taşıyan procedure ve local declaration dönüşüm
    /// testlerine temel olur.
    /// </summary>
    public sealed class Pl1ProcedureToEglFunctionTranspilerTests
    {
        [Fact]
        public void Transpile_WithParameterlessProcedure_ShouldCreateEglFunction()
        {
            var procedure = new Pl1Procedure(
                "CUSTOMER_PROCESS",
                options: null,
                statements: new Pl1Statement[]
                {
                    new Pl1AssignmentStatement(
                        new[]
                        {
                            new Pl1RawExpression(
                                "CUSTOMER_NO",
                                SourceLocation.Unknown)
                        },
                        new Pl1RawExpression(
                            "MUST_NO",
                            SourceLocation.Unknown),
                        SourceLocation.Unknown),

                    new Pl1CallStatement(
                        "FETCH_CUSTOMER",
                        new[]
                        {
                            new Pl1RawExpression(
                                "CUSTOMER_NO",
                                SourceLocation.Unknown)
                        },
                        SourceLocation.Unknown)
                },
                SourceLocation.Unknown);

            var syntaxTree = new Pl1SyntaxTree(
                declarations: null,
                procedures: new[]
                {
                    procedure
                },
                statements: null,
                SourceLocation.Unknown);

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.NotNull(result.SyntaxTree);

            var function = Assert.Single(
                result.SyntaxTree!.Functions);

            Assert.Equal(
                "CustomerProcess",
                function.Name);

            Assert.Equal(
                2,
                function.Statements.Count);

            Assert.IsType<EglAssignmentStatement>(
                function.Statements[0]);

            Assert.IsType<EglCallStatement>(
                function.Statements[1]);
        }

        [Fact]
        public void Transpile_WithParameterizedProcedure_ShouldReturnUnsupportedMappingDiagnostic()
        {
            var procedure = new Pl1Procedure(
                "CUSTOMER_PROCESS",
                options: null,
                statements: null,
                SourceLocation.Unknown,
                parameters: new[]
                {
                    "PROCESS_TEXT"
                });

            var syntaxTree = new Pl1SyntaxTree(
                declarations: null,
                procedures: new[]
                {
                    procedure
                },
                statements: null,
                SourceLocation.Unknown);

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(syntaxTree);

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Functions);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "Parameter içeren PL/I procedure için EGL " +
                        "function mapping henüz desteklenmiyor: " +
                        "CUSTOMER_PROCESS"));
        }

        [Fact]
        public void Transpile_WithProcedureBodyDeclaration_ShouldReturnUnsupportedMappingDiagnostic()
        {
            var procedure = new Pl1Procedure(
                "CUSTOMER_PROCESS",
                options: null,
                statements: null,
                SourceLocation.Unknown,
                parameters: null,
                declarations: new[]
                {
                    new Pl1.Declarations.Pl1VariableDeclaration(
                        "PROCESS_TEXT",
                        new Pl1.Types.Pl1CharacterType(
                            50,
                            SourceLocation.Unknown),
                        SourceLocation.Unknown)
                });

            var syntaxTree = new Pl1SyntaxTree(
                declarations: null,
                procedures: new[]
                {
                    procedure
                },
                statements: null,
                SourceLocation.Unknown);

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(syntaxTree);

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Functions);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "Body declaration içeren PL/I procedure için " +
                        "EGL function mapping henüz desteklenmiyor: " +
                        "CUSTOMER_PROCESS"));
        }
    }
}