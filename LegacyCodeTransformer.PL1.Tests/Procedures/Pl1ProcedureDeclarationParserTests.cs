using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    /// <summary>
    /// PL/I procedure body declaration parsing davranışını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Gerçek PL/I procedure header parameter'larının veri tipi body
    /// içindeki DCL statement ile tanımlanabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Procedure içindeki variable declaration modellerinin ayrı
    /// declaration koleksiyonunda, executable statement'ların ise
    /// mevcut statement koleksiyonunda korunduğunu doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P10 gerçek case procedure declaration coverage testlerinde
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parameter binding, procedure scope ve EGL function parameter
    /// type/direction testlerine temel olur.
    /// </summary>
    public sealed class Pl1ProcedureDeclarationParserTests
    {
        [Fact]
        public void Parse_WithProcedureParameterDeclaration_ShouldPreserveDeclaration()
        {
            var result = ParseSource(
                """
                 CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
                     DCL PROCESS_TEXT CHAR(50);

                     ERROR_TEXT = PROCESS_TEXT;
                 END CUSTOMER_PROCESS;
                """);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Single(procedure.Parameters);
            Assert.Equal(
                "PROCESS_TEXT",
                procedure.Parameters[0]);

            var declaration =
                Assert.IsType<Pl1VariableDeclaration>(
                    Assert.Single(procedure.Declarations));

            Assert.Equal(
                "PROCESS_TEXT",
                declaration.Name);

            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1AssignmentStatement>(
                procedure.Statements[0]);
        }

        [Fact]
        public void Parse_WithMultipleProcedureDeclarations_ShouldPreserveOrder()
        {
            var result = ParseSource(
                """
         PROCESS_DATA: PROCEDURE(INPUT_TEXT, OUTPUT_TEXT);
             DCL INPUT_TEXT CHAR(50);
             DCL OUTPUT_TEXT CHAR(50);

             OUTPUT_TEXT = INPUT_TEXT;
         END PROCESS_DATA;
        """);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Equal(
                2,
                procedure.Declarations.Count);

            var inputTextDeclaration =
                Assert.IsType<Pl1VariableDeclaration>(
                    procedure.Declarations[0]);

            var outputTextDeclaration =
                Assert.IsType<Pl1VariableDeclaration>(
                    procedure.Declarations[1]);

            Assert.Equal(
                "INPUT_TEXT",
                inputTextDeclaration.Name);

            Assert.Equal(
                "OUTPUT_TEXT",
                outputTextDeclaration.Name);

            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1AssignmentStatement>(
                procedure.Statements[0]);
        }

        [Fact]
        public void Parse_WithDeclarationAndCallInsideProcedure_ShouldKeepCollectionsSeparated()
        {
            var result = ParseSource(
                """
                 CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
                     DCL PROCESS_TEXT CHAR(50);

                     ERROR_TEXT = PROCESS_TEXT;
                     CALL WRITE_ERROR(ERROR_TEXT);
                 END CUSTOMER_PROCESS;
                """);

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Single(result.SyntaxTree!.Procedures);

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Single(procedure.Declarations);
            Assert.Equal(
                2,
                procedure.Statements.Count);

            Assert.IsType<Pl1AssignmentStatement>(
                procedure.Statements[0]);

            Assert.IsType<Pl1CallStatement>(
                procedure.Statements[1]);
        }

        [Fact]
        public void Parse_WithParameterlessProcedureWithoutDeclarations_ShouldPreserveExistingBehavior()
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

            var procedure = result.SyntaxTree.Procedures[0];

            Assert.Empty(procedure.Parameters);
            Assert.Empty(procedure.Declarations);
            Assert.Single(procedure.Statements);
            Assert.IsType<Pl1CallStatement>(
                procedure.Statements[0]);
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