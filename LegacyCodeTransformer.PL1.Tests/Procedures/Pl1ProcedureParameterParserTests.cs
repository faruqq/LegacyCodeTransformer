using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    /// <summary>
    /// PL/I procedure header parameter parsing davranışını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Gerçek PL/I kaynaklarında procedure header bir veya birden fazla
    /// parameter adı taşıyabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Tek parameter, çoklu parameter, parameter + OPTIONS ve eski
    /// parametresiz procedure davranışlarını regression testleriyle
    /// sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///
    /// PROCESS_DATA: PROCEDURE(CUSTOMER_NO, CUSTOMER_NAME);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P10 gerçek case procedure coverage testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parameter declaration eşleştirmesi, scope, type ve EGL direction
    /// analizlerinin parser sözleşmesini korur.
    /// </summary>
    public sealed class Pl1ProcedureParameterParserTests
    {
        [Fact]
        public void Parse_WithSingleProcedureParameter_ShouldPreserveParameterName()
        {
            var result = ParseSource(
                """
                 CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
                     ERROR_TEXT = PROCESS_TEXT;
                 END CUSTOMER_PROCESS;
                """);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal("CUSTOMER_PROCESS", procedure.Name);
            Assert.Single(procedure.Parameters);
            Assert.Equal("PROCESS_TEXT", procedure.Parameters[0]);
            Assert.Empty(procedure.Options);
            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1AssignmentStatement>(
                procedure.Statements[0]);
        }

        [Fact]
        public void Parse_WithMultipleProcedureParameters_ShouldPreserveParameterOrder()
        {
            var result = ParseSource(
                """
                 PROCESS_DATA: PROCEDURE(CUSTOMER_NO, CUSTOMER_NAME);
                     CALL WRITE_CUSTOMER(CUSTOMER_NO, CUSTOMER_NAME);
                 END PROCESS_DATA;
                """);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal(
                new[]
                {
                    "CUSTOMER_NO",
                    "CUSTOMER_NAME"
                },
                procedure.Parameters);

            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1CallStatement>(
                procedure.Statements[0]);
        }

        [Fact]
        public void Parse_WithParametersAndOptions_ShouldPreserveBothCollections()
        {
            var result = ParseSource(
                """
                 PROGRAM_NAME: PROCEDURE(ARG1, ARG2) OPTIONS(MAIN);
                     CALL INIT_PROCESS;
                 END PROGRAM_NAME;
                """);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal(
                new[]
                {
                    "ARG1",
                    "ARG2"
                },
                procedure.Parameters);

            Assert.Single(procedure.Options);
            Assert.Equal("MAIN", procedure.Options[0]);
        }

        [Fact]
        public void Parse_WithParameterlessProcedure_ShouldKeepParametersEmpty()
        {
            var result = ParseSource(
                """
                 PROCESS_CURSOR: PROCEDURE;
                     CALL FETCH_CURSOR;
                 END PROCESS_CURSOR;
                """);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);
            Assert.Empty(
                result.SyntaxTree.Procedures[0].Parameters);
        }

        [Fact]
        public void Parse_WithEmptyProcedureParameterList_ShouldReturnParameterDiagnostic()
        {
            var result = ParseSource(
                """
                 PROCESS_CURSOR: PROCEDURE();
                     CALL FETCH_CURSOR;
                 END PROCESS_CURSOR;
                """);

            Assert.False(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "Procedure parameter adı bekleniyordu"));
        }

        private static ParseResult<Pl1SyntaxTree> ParseSource(
            string source)
        {
            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);

            return parser.Parse();
        }
    }
}