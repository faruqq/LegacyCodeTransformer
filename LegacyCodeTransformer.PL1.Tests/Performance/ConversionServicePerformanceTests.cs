using System.Diagnostics;
using System.Text;
using LegacyCodeTransformer.Application.Services;

namespace LegacyCodeTransformer.Application.Tests.Performance
{
    /// <summary>
    /// Application conversion pipeline için performans smoke testlerini içerir.
    ///
    /// Neden var?
    /// ----------------------
    /// ConversionService lexer, parser, normalizer, transpiler ve generator
    /// adımlarını tek pipeline olarak çalıştırır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Orta büyüklükte bir PL/I input'unun uçtan uca dönüşüm pipeline içinde temel
    /// performans beklentisini koruduğunu doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL + assignment + CALL statement yoğun kaynak metni.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P08 application performance smoke testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Batch conversion, klasör bazlı dönüşüm ve IDE entegrasyonu için uçtan uca
    /// performans baseline testlerine temel olur.
    /// </summary>
    public sealed class ConversionServicePerformanceTests
    {
        [Fact]
        public void ConvertPl1ToEgl_WithMediumSizedSource_ShouldCompleteWithinSmokeThreshold()
        {
            var sourceBuilder = new StringBuilder();

            for (var i = 0; i < 100; i++)
            {
                sourceBuilder.AppendLine($" DCL CUSTOMER_NO_{i} FIXED DECIMAL(10);");
            }

            for (var i = 0; i < 100; i++)
            {
                sourceBuilder.AppendLine($" CUSTOMER_NO_{i} = {i};");
                sourceBuilder.AppendLine($" CALL FETCH_CUSTOMER_{i};");
            }

            var service = new ConversionService();

            var stopwatch = Stopwatch.StartNew();

            var result = service.ConvertPl1ToEgl(sourceBuilder.ToString());

            stopwatch.Stop();

            Assert.True(result.Success);
            Assert.NotNull(result.Output);
            Assert.Contains("CustomerNo0 decimal(10);", result.Output);
            Assert.Contains("call FetchCustomer0();", result.Output);
            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromSeconds(10),
                $"ConversionService smoke threshold aşıldı. Süre: {stopwatch.Elapsed}");
        }
    }
}