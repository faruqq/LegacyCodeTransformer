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

        /// <summary>
        /// PL/I CHAR(n) declaration ifadesinin uçtan uca EGL char(n) çıktısına
        /// dönüştüğünü doğrular.
        ///
        /// Neden var?
        /// ----------------------
        /// Bu test lexer, parser, normalizer, transpiler ve generator katmanlarının
        /// CHAR declaration için birlikte doğru çalıştığını garanti eder.
        ///
        /// Test edilen PL/I:
        ///
        /// DCL PARAM CHAR(25);
        ///
        /// Beklenen EGL:
        ///
        /// param char(25);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application uçtan uca dönüşüm testlerinde
        /// - CHAR desteğinin pipeline seviyesinde doğrulanmasında
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// INIT, structure ve array desteği eklendiğinde basit CHAR dönüşümünün
        /// bozulmadığını garanti eder.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithCharDeclaration_ShouldGenerateEglCharDeclaration()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL PARAM CHAR(25);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("param char(25);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I CHAR uzunluğunda baştaki sıfırların EGL çıktısında normalize
        /// edildiğini doğrular.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I kaynaklarında CHAR(08) yazımı geçerli ve yaygın olabilir.
        /// Model tarafında bu değer sayısal olarak tutulduğu için EGL çıktısında
        /// char(8) olarak üretilmelidir.
        ///
        /// Test edilen PL/I:
        ///
        /// DCL PARAM CHAR(08);
        ///
        /// Beklenen EGL:
        ///
        /// param char(8);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application uçtan uca dönüşüm testlerinde
        /// - Parser + Generator normalizasyon davranışını birlikte doğrulamada
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Numeric uzunluk, precision ve dimension değerlerinde ortak
        /// normalizasyon beklentisine referans olur.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithCharDeclarationHavingLeadingZeroLength_ShouldNormalizeLength()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL PARAM CHAR(08);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("param char(8);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I CHARACTER(n) declaration ifadesinin EGL char(n) çıktısına
        /// dönüştüğünü doğrular.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I dilinde CHARACTER uzun form, CHAR ise kısa form olarak kullanılır.
        /// Uçtan uca pipeline iki formu da aynı EGL char(n) çıktısına
        /// dönüştürmelidir.
        ///
        /// Test edilen PL/I:
        ///
        /// DECLARE PARAM CHARACTER(08);
        ///
        /// Beklenen EGL:
        ///
        /// param char(8);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application uçtan uca dönüşüm testlerinde
        /// - DECLARE alias ve CHARACTER keyword desteğini doğrulamada
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Farklı PL/I kaynak kod stillerinde CHAR / CHARACTER kullanımı değişse bile
        /// dönüşüm davranışının aynı kalmasını garanti eder.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithCharacterDeclaration_ShouldGenerateEglCharDeclaration()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DECLARE PARAM CHARACTER(08);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("param char(8);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I INIT başlangıç değeri parse edilse bile EGL çıktısına henüz
        /// yazdırılmadığını doğrular.
        ///
        /// Neden var?
        /// ----------------------
        /// Decision 042 kapsamında INIT / INITIAL bilgisinin ilk aşamada
        /// Syntax Tree üzerinde korunması, fakat EGL default value standardı
        /// netleşene kadar hedef kaynak koda yazdırılmaması kararlaştırılmıştır.
        ///
        /// Test edilen PL/I:
        ///
        /// DCL PARAM CHAR(08) INIT(' ');
        ///
        /// Beklenen EGL:
        ///
        /// param char(8);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application uçtan uca dönüşüm testlerinde
        /// - INIT parse desteğinin pipeline'ı bozmadığını doğrulamada
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// EGL default value üretimi eklendiğinde bu test güncellenecek veya
        /// yeni karar doğrultusunda ayrı testle değiştirilecektir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithCharInitValue_ShouldIgnoreInitInEglOutputForNow()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL PARAM CHAR(08) INIT(' ');";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("param char(8);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I repeat factor içeren INIT ifadesinin parse edilip EGL çıktısına
        /// henüz yazdırılmadığını doğrular.
        ///
        /// Neden var?
        /// ----------------------
        /// INIT((08)' ') söz dizimi parser tarafından kabul edilmelidir.
        /// Ancak EGL tarafındaki başlangıç değeri üretim standardı henüz
        /// netleşmediği için çıktı sadece veri tipi dönüşümünü içermelidir.
        ///
        /// Test edilen PL/I:
        ///
        /// DCL PARAM CHAR(8) INIT((08)' ');
        ///
        /// Beklenen EGL:
        ///
        /// param char(8);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application uçtan uca dönüşüm testlerinde
        /// - Repeat factor içeren INIT parse desteğini doğrulamada
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// INIT repeat factor mapping kararı alındığında bu test yeni beklenen
        /// EGL çıktısına göre revize edilecektir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithRepeatedCharInitValue_ShouldIgnoreInitInEglOutputForNow()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL PARAM CHAR(8) INIT((08)' ');";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("param char(8);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }
    }
}
