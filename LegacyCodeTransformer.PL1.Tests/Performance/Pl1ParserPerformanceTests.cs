using System.Diagnostics;
using System.Text;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;

namespace LegacyCodeTransformer.PL1.Tests.Performance
{
    /// <summary>
    /// PL/I parser için büyük input smoke testlerini içerir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser büyüdükçe declaration, statement, procedure ve legacy statement
    /// desteğinin büyük kaynaklarda temel performans davranışını koruması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Büyük ama kontrollü PL/I input'larında parser'ın aşırı yavaşlamadan ve syntax
    /// tree ayrımını bozmadan çalıştığını doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Çok sayıda DCL, CALL ve PROCEDURE içeren kaynak metinler.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P08 performans smoke testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Batch conversion, klasör bazlı dönüşüm ve büyük kurumsal kaynak dosyalar için
    /// performans baseline testlerine temel olur.
    /// </summary>
    public sealed class Pl1ParserPerformanceTests
    {
        [Fact]
        public void Parse_WithLargeDeclarationAndStatementInput_ShouldCompleteWithinSmokeThreshold()
        {
            var sourceBuilder = new StringBuilder();

            for (var i = 0; i < 250; i++)
            {
                sourceBuilder.AppendLine($" DCL CUSTOMER_NO_{i} FIXED DECIMAL(10);");
            }

            for (var i = 0; i < 250; i++)
            {
                sourceBuilder.AppendLine($" CUSTOMER_NO_{i} = {i};");
            }

            for (var i = 0; i < 100; i++)
            {
                sourceBuilder.AppendLine($" CALL FETCH_CUSTOMER_{i};");
            }

            var stopwatch = Stopwatch.StartNew();

            var result = ParseSource(sourceBuilder.ToString());

            stopwatch.Stop();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Equal(250, result.SyntaxTree!.Declarations.Count);
            Assert.Equal(350, result.SyntaxTree.Statements.Count);
            Assert.Empty(result.SyntaxTree.Procedures);
            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromSeconds(10),
                $"Parser smoke threshold aşıldı. Süre: {stopwatch.Elapsed}");
        }

        [Fact]
        public void Parse_WithManyProcedures_ShouldCompleteWithinSmokeThreshold()
        {
            var sourceBuilder = new StringBuilder();

            for (var i = 0; i < 100; i++)
            {
                sourceBuilder.AppendLine($" PROCESS_{i}: PROCEDURE;");
                sourceBuilder.AppendLine($"     CALL FETCH_{i};");
                sourceBuilder.AppendLine($" END PROCESS_{i};");
            }

            var stopwatch = Stopwatch.StartNew();

            var result = ParseSource(sourceBuilder.ToString());

            stopwatch.Stop();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Declarations);
            Assert.Empty(result.SyntaxTree.Statements);
            Assert.Equal(100, result.SyntaxTree.Procedures.Count);
            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromSeconds(10),
                $"Procedure parser smoke threshold aşıldı. Süre: {stopwatch.Elapsed}");
        }

        private static Core.Results.ParseResult<Pl1.Syntax.Pl1SyntaxTree> ParseSource(
            string source)
        {
            var lexer = new Pl1Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Pl1Parser(tokens);

            return parser.Parse();
        }
    }
}