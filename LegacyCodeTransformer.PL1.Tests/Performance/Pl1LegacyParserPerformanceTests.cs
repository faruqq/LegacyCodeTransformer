using System.Diagnostics;
using System.Text;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.PL1.Tests.Performance
{
    /// <summary>
    /// Legacy statement parser için performans smoke testlerini içerir.
    ///
    /// Neden var?
    /// ----------------------
    /// EXEC SQL ve compiler directive statement desteği statement pipeline'a eklendi.
    /// Bu legacy statement'ların büyük inputlarda parser akışını bozmadığı
    /// doğrulanmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Çok sayıda EXEC SQL ve compiler directive statement içeren kaynaklarda parser
    /// davranışını sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// %INCLUDE COPYLIB;
    /// EXEC SQL INCLUDE SQLCA;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P08 legacy parser performans smoke testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DB2 erişimi yoğun gerçek PL/I dosyaları ve macro/directive yoğun kaynaklar
    /// için performans baseline testlerine temel olur.
    /// </summary>
    public sealed class Pl1LegacyParserPerformanceTests
    {
        [Fact]
        public void Parse_WithManyLegacyStatements_ShouldCompleteWithinSmokeThreshold()
        {
            var sourceBuilder = new StringBuilder();

            for (var i = 0; i < 150; i++)
            {
                sourceBuilder.AppendLine($" %INCLUDE COPYLIB_{i};");
                sourceBuilder.AppendLine(" EXEC SQL INCLUDE SQLCA;");
                sourceBuilder.AppendLine($" CALL LEGACY_PROCESS_{i};");
            }

            var stopwatch = Stopwatch.StartNew();

            var result = ParseSource(sourceBuilder.ToString());

            stopwatch.Stop();

            Assert.True(result.Success);
            Assert.NotNull(result.SyntaxTree);
            Assert.Empty(result.SyntaxTree!.Declarations);
            Assert.Empty(result.SyntaxTree.Procedures);
            Assert.Equal(450, result.SyntaxTree.Statements.Count);
            Assert.IsType<Pl1CompilerDirectiveStatement>(result.SyntaxTree.Statements[0]);
            Assert.IsType<Pl1EmbeddedSqlStatement>(result.SyntaxTree.Statements[1]);
            Assert.IsType<Pl1CallStatement>(result.SyntaxTree.Statements[2]);
            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromSeconds(10),
                $"Legacy parser smoke threshold aşıldı. Süre: {stopwatch.Elapsed}");
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