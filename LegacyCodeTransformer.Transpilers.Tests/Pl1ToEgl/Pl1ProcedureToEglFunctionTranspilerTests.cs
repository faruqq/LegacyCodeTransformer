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
        public void Transpile_WithResolvedParameterizedProcedure_ShouldCreateEglFunctionParameter()
        {
            var parameterDeclaration =
                new Pl1.Declarations.Pl1VariableDeclaration(
                    "PROCESS_TEXT",
                    new Pl1.Types.Pl1CharacterType(
                        50,
                        SourceLocation.Unknown),
                    SourceLocation.Unknown);

            var procedure = new Pl1Procedure(
                "CUSTOMER_PROCESS",
                options: null,
                statements: new Pl1Statement[]
                {
            new Pl1AssignmentStatement(
                new[]
                {
                    new Pl1RawExpression(
                        "ERROR_TEXT",
                        SourceLocation.Unknown)
                },
                new Pl1RawExpression(
                    "PROCESS_TEXT",
                    SourceLocation.Unknown),
                SourceLocation.Unknown)
                },
                SourceLocation.Unknown,
                parameters: new[]
                {
            "PROCESS_TEXT"
                },
                declarations: new[]
                {
            parameterDeclaration
                });

            var syntaxTree = new Pl1SyntaxTree(
                declarations: null,
                procedures: new[]
                {
            procedure
                },
                statements: null,
                SourceLocation.Unknown);

            var semanticAnalyzer =
                new Pl1.Semantic.Pl1SemanticAnalyzer();

            var semanticResult =
                semanticAnalyzer.Analyze(syntaxTree);

            Assert.True(
                semanticResult.Success);

            var binding = Assert.Single(
                semanticResult.ProcedureParameterBindings);

            Assert.Equal(
                Pl1.Semantic.Pl1ProcedureParameterDirection.In,
                binding.Direction);

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(
                syntaxTree,
                semanticResult);

            Assert.True(
                result.Success);

            Assert.Empty(
                result.Diagnostics);

            Assert.NotNull(
                result.SyntaxTree);

            var function = Assert.Single(
                result.SyntaxTree!.Functions);

            Assert.Equal(
                "CustomerProcess",
                function.Name);

            var parameter = Assert.Single(
                function.Parameters);

            Assert.Equal(
                "ProcessText",
                parameter.Name);

            Assert.Equal(
                LegacyCodeTransformer.Egl.Functions
                    .EglFunctionParameterDirection.In,
                parameter.Direction);

            var parameterType =
                Assert.IsType<
                    LegacyCodeTransformer.Egl.Types.EglCharacterType>(
                        parameter.DataType);

            Assert.Equal(
                50,
                parameterType.Length);

            var statement =
                Assert.IsType<EglAssignmentStatement>(
                    Assert.Single(function.Statements));

            Assert.Equal(
                "ErrorText",
                statement.Target);

            Assert.Equal(
                "ProcessText",
                statement.Value);
        }

        [Fact]
        public void Transpile_WithParameterizedProcedureWithoutSemanticResult_ShouldReturnSemanticResultDiagnostic()
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

            var result = transpiler.Transpile(
                syntaxTree);

            Assert.False(
                result.Success);

            Assert.NotNull(
                result.SyntaxTree);

            Assert.Empty(
                result.SyntaxTree!.Functions);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "Parameter içeren PL/I procedure dönüşümü için " +
                        "semantic analysis sonucu bulunamadı: " +
                        "CUSTOMER_PROCESS"));
        }

        [Fact]
        public void Transpile_WithUnresolvedProcedureParameterBinding_ShouldReturnBindingDiagnostic()
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

            var semanticAnalyzer =
                new Pl1.Semantic.Pl1SemanticAnalyzer();

            var semanticResult =
                semanticAnalyzer.Analyze(syntaxTree);

            Assert.True(
                semanticResult.Success);

            var binding = Assert.Single(
                semanticResult.ProcedureParameterBindings);

            Assert.False(
                binding.IsResolved);

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(
                syntaxTree,
                semanticResult);

            Assert.False(
                result.Success);

            Assert.NotNull(
                result.SyntaxTree);

            Assert.Empty(
                result.SyntaxTree!.Functions);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "PL/I procedure parametresi için declaration " +
                        "binding çözümlenemedi: " +
                        "CUSTOMER_PROCESS.PROCESS_TEXT"));
        }

        [Fact]
        public void Transpile_WithMultipleResolvedProcedureParameters_ShouldPreserveParameterOrderTypesAndDirections()
        {
            var inputDeclaration =
                new Pl1.Declarations.Pl1VariableDeclaration(
                    "INPUT_TEXT",
                    new Pl1.Types.Pl1CharacterType(
                        30,
                        SourceLocation.Unknown),
                    SourceLocation.Unknown);

            var outputDeclaration =
                new Pl1.Declarations.Pl1VariableDeclaration(
                    "OUTPUT_TEXT",
                    new Pl1.Types.Pl1CharacterType(
                        50,
                        SourceLocation.Unknown),
                    SourceLocation.Unknown);

            var procedure = new Pl1Procedure(
                "PROCESS_DATA",
                options: null,
                statements: new Pl1Statement[]
                {
            new Pl1AssignmentStatement(
                new[]
                {
                    new Pl1RawExpression(
                        "TARGET_TEXT",
                        SourceLocation.Unknown)
                },
                new Pl1RawExpression(
                    "INPUT_TEXT",
                    SourceLocation.Unknown),
                SourceLocation.Unknown),

            new Pl1AssignmentStatement(
                new[]
                {
                    new Pl1RawExpression(
                        "RESULT_TEXT",
                        SourceLocation.Unknown)
                },
                new Pl1RawExpression(
                    "OUTPUT_TEXT",
                    SourceLocation.Unknown),
                SourceLocation.Unknown)
                },
                SourceLocation.Unknown,
                parameters: new[]
                {
            "INPUT_TEXT",
            "OUTPUT_TEXT"
                },
                declarations: new[]
                {
            inputDeclaration,
            outputDeclaration
                });

            var syntaxTree = new Pl1SyntaxTree(
                declarations: null,
                procedures: new[]
                {
            procedure
                },
                statements: null,
                SourceLocation.Unknown);

            var semanticAnalyzer =
                new Pl1.Semantic.Pl1SemanticAnalyzer();

            var semanticResult =
                semanticAnalyzer.Analyze(syntaxTree);

            Assert.True(
                semanticResult.Success);

            Assert.All(
                semanticResult.ProcedureParameterBindings,
                binding =>
                    Assert.Equal(
                        Pl1.Semantic.Pl1ProcedureParameterDirection.In,
                        binding.Direction));

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(
                syntaxTree,
                semanticResult);

            Assert.True(
                result.Success);

            Assert.Empty(
                result.Diagnostics);

            Assert.NotNull(
                result.SyntaxTree);

            var function = Assert.Single(
                result.SyntaxTree!.Functions);

            Assert.Equal(
                2,
                function.Parameters.Count);

            Assert.Equal(
                "InputText",
                function.Parameters[0].Name);

            Assert.Equal(
                "OutputText",
                function.Parameters[1].Name);

            Assert.All(
                function.Parameters,
                parameter =>
                    Assert.Equal(
                        LegacyCodeTransformer.Egl.Functions
                            .EglFunctionParameterDirection.In,
                        parameter.Direction));

            var inputType =
                Assert.IsType<
                    LegacyCodeTransformer.Egl.Types.EglCharacterType>(
                        function.Parameters[0].DataType);

            var outputType =
                Assert.IsType<
                    LegacyCodeTransformer.Egl.Types.EglCharacterType>(
                        function.Parameters[1].DataType);

            Assert.Equal(
                30,
                inputType.Length);

            Assert.Equal(
                50,
                outputType.Length);

            Assert.Equal(
                2,
                function.Statements.Count);
        }

        [Fact]
        public void Transpile_WithParameterDeclarationAndLocalDeclaration_ShouldReturnLocalDeclarationDiagnostic()
        {
            var parameterDeclaration =
                new Pl1.Declarations.Pl1VariableDeclaration(
                    "PROCESS_TEXT",
                    new Pl1.Types.Pl1CharacterType(
                        50,
                        SourceLocation.Unknown),
                    SourceLocation.Unknown);

            var localDeclaration =
                new Pl1.Declarations.Pl1VariableDeclaration(
                    "LOCAL_TEXT",
                    new Pl1.Types.Pl1CharacterType(
                        20,
                        SourceLocation.Unknown),
                    SourceLocation.Unknown);

            var procedure = new Pl1Procedure(
                "CUSTOMER_PROCESS",
                options: null,
                statements: new Pl1Statement[]
                {
            new Pl1AssignmentStatement(
                new[]
                {
                    new Pl1RawExpression(
                        "LOCAL_TEXT",
                        SourceLocation.Unknown)
                },
                new Pl1RawExpression(
                    "PROCESS_TEXT",
                    SourceLocation.Unknown),
                SourceLocation.Unknown)
                },
                SourceLocation.Unknown,
                parameters: new[]
                {
            "PROCESS_TEXT"
                },
                declarations: new[]
                {
            parameterDeclaration,
            localDeclaration
                });

            var syntaxTree = new Pl1SyntaxTree(
                declarations: null,
                procedures: new[]
                {
            procedure
                },
                statements: null,
                SourceLocation.Unknown);

            var semanticAnalyzer =
                new Pl1.Semantic.Pl1SemanticAnalyzer();

            var semanticResult =
                semanticAnalyzer.Analyze(syntaxTree);

            Assert.True(
                semanticResult.Success);

            var binding = Assert.Single(
                semanticResult.ProcedureParameterBindings);

            Assert.Equal(
                Pl1.Semantic.Pl1ProcedureParameterDirection.In,
                binding.Direction);

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(
                syntaxTree,
                semanticResult);

            Assert.False(
                result.Success);

            Assert.NotNull(
                result.SyntaxTree);

            Assert.Empty(
                result.SyntaxTree!.Functions);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "Body declaration içeren PL/I procedure için EGL " +
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

        [Fact]
        public void Transpile_WithUnknownProcedureParameterDirection_ShouldReturnDirectionDiagnostic()
        {
            var parameterDeclaration =
                new Pl1.Declarations.Pl1VariableDeclaration(
                    "PROCESS_TEXT",
                    new Pl1.Types.Pl1CharacterType(
                        50,
                        SourceLocation.Unknown),
                    SourceLocation.Unknown);

            var procedure = new Pl1Procedure(
                "CUSTOMER_PROCESS",
                options: null,
                statements: null,
                SourceLocation.Unknown,
                parameters: new[]
                {
            "PROCESS_TEXT"
                },
                declarations: new[]
                {
            parameterDeclaration
                });

            var syntaxTree = new Pl1SyntaxTree(
                declarations: null,
                procedures: new[]
                {
            procedure
                },
                statements: null,
                SourceLocation.Unknown);

            var semanticAnalyzer =
                new Pl1.Semantic.Pl1SemanticAnalyzer();

            var semanticResult =
                semanticAnalyzer.Analyze(syntaxTree);

            var binding = Assert.Single(
                semanticResult.ProcedureParameterBindings);

            Assert.True(
                binding.IsResolved);

            Assert.Equal(
                Pl1.Semantic.Pl1ProcedureParameterDirection.Unknown,
                binding.Direction);

            var transpiler = new Pl1ToEglTranspiler();

            var result = transpiler.Transpile(
                syntaxTree,
                semanticResult);

            Assert.False(
                result.Success);

            Assert.NotNull(
                result.SyntaxTree);

            Assert.Empty(
                result.SyntaxTree!.Functions);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "PL/I procedure parametresi için direction " +
                        "çözümlenemedi: " +
                        "CUSTOMER_PROCESS.PROCESS_TEXT"));
        }
    }
}