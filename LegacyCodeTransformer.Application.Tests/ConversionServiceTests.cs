using LegacyCodeTransformer.Application.Services;

namespace LegacyCodeTransformer.Application.Tests
{
    /// <summary>
    /// ConversionService için uçtan uca dönüşüm testlerini içerir.
    ///
    /// Neden var?
    /// ----------------------
    /// Lexer, Parser, Normalizer, Transpiler ve Generator katmanlarının
    /// birlikte doğru çalıştığını doğrulamak için oluşturulmuştur.
    ///
    /// Bu testler tek bir sınıfı değil, PL/I → EGL pipeline akışını test eder.
    /// </summary>
    public sealed class ConversionServiceTests
    {
        [Fact]
        public void ConvertPl1ToEgl_WithFixedDecimalDeclaration_ShouldGenerateEglDecimalDeclaration()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL MUST_NO FIXED DECIMAL(8);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("mustNo decimal(8,0);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        [Fact]
        public void ConvertPl1ToEgl_WithFixedDecimalScaleDeclaration_ShouldGenerateEglDecimalDeclarationWithScale()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL CUSTOMER_NO FIXED DECIMAL(10,2);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("customerNo decimal(10,2);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }
    }
}
