using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Semantic;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.PL1.Tests.Semantic
{
    /// <summary>
    /// Procedure header parameter adları ile procedure body declaration
    /// modellerinin semantic olarak eşleştirilmesini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL function parameter üretiminden önce PL/I parameter adı ve
    /// veri tipi declaration modeli güvenli biçimde bağlanmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Tekli, çoklu, case-insensitive ve unresolved procedure parameter
    /// binding davranışlarını gerçekçi PL/I örnekleriyle sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P10.3 procedure semantic foundation testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL parameter type mapping ve parameter direction testlerine
    /// temel olur.
    /// </summary>
    public sealed class Pl1ProcedureParameterBindingTests
    {
        [Fact]
        public void Analyze_WithDeclaredProcedureParameter_ShouldCreateResolvedBinding()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
                     DCL PROCESS_TEXT CHAR(50);

                     ERROR_TEXT = PROCESS_TEXT;
                 END CUSTOMER_PROCESS;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);

            var binding = Assert.Single(
                result.ProcedureParameterBindings);

            Assert.Equal(
                "CUSTOMER_PROCESS",
                binding.ProcedureName);

            Assert.Equal(
                "PROCESS_TEXT",
                binding.ParameterName);

            Assert.True(binding.IsResolved);
            Assert.NotNull(binding.Declaration);
            Assert.Equal(
                "PROCESS_TEXT",
                binding.Declaration!.Name);

            var characterType =
                Assert.IsType<Pl1CharacterType>(
                    binding.Declaration.DataType);

            Assert.Equal(
                50,
                characterType.Length);
        }

        [Fact]
        public void Analyze_WithMultipleParameters_ShouldPreserveHeaderOrder()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 PROCESS_DATA: PROCEDURE(INPUT_TEXT, OUTPUT_TEXT);
                     DCL INPUT_TEXT CHAR(30);
                     DCL OUTPUT_TEXT CHAR(50);

                     OUTPUT_TEXT = INPUT_TEXT;
                 END PROCESS_DATA;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Equal(
                2,
                result.ProcedureParameterBindings.Count);

            Assert.Equal(
                "INPUT_TEXT",
                result.ProcedureParameterBindings[0]
                    .ParameterName);

            Assert.Equal(
                "OUTPUT_TEXT",
                result.ProcedureParameterBindings[1]
                    .ParameterName);

            Assert.All(
                result.ProcedureParameterBindings,
                binding =>
                    Assert.True(binding.IsResolved));
        }

        [Fact]
        public void Analyze_WithDifferentParameterDeclarationCasing_ShouldResolveBinding()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
                     DCL process_text CHAR(50);

                     ERROR_TEXT = PROCESS_TEXT;
                 END CUSTOMER_PROCESS;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            var binding = Assert.Single(
                result.ProcedureParameterBindings);

            Assert.True(binding.IsResolved);
            Assert.NotNull(binding.Declaration);
            Assert.Equal(
                "process_text",
                binding.Declaration!.Name);
        }

        [Fact]
        public void Analyze_WithMissingParameterDeclaration_ShouldKeepUnresolvedBindingWithoutDiagnostic()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
                     ERROR_TEXT = PROCESS_TEXT;
                 END CUSTOMER_PROCESS;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);

            var binding = Assert.Single(
                result.ProcedureParameterBindings);

            Assert.Equal(
                "PROCESS_TEXT",
                binding.ParameterName);

            Assert.False(binding.IsResolved);
            Assert.Null(binding.Declaration);
        }

        [Fact]
        public void Analyze_WithParameterlessProcedure_ShouldNotCreateBindings()
        {
            var syntaxTree = ParseSyntaxTree(
                """
                 PROCESS_CURSOR: PROCEDURE;
                     CALL FETCH_CURSOR;
                 END PROCESS_CURSOR;
                """);

            var analyzer = new Pl1SemanticAnalyzer();

            var result = analyzer.Analyze(syntaxTree);

            Assert.True(result.Success);
            Assert.Empty(
                result.ProcedureParameterBindings);
        }

        private static Pl1SyntaxTree ParseSyntaxTree(
            string source)
        {
            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);
            ParseResult<Pl1SyntaxTree> result =
                parser.Parse();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);

            return result.SyntaxTree!;
        }
    }
}