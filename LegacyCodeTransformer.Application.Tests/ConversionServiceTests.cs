using LegacyCodeTransformer.Application.Services;
using LegacyCodeTransformer.Transpilers.Naming;

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
            Assert.Equal("MustNo decimal(8);" + Environment.NewLine, result.Output);
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
            Assert.Equal("CustomerNo decimal(10,2);" + Environment.NewLine, result.Output);
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
        /// Param char(25);
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
            Assert.Equal("Param char(25);" + Environment.NewLine, result.Output);
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
        /// Param char(8);
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
            Assert.Equal("Param char(8);" + Environment.NewLine, result.Output);
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
        /// Param char(8);
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
            Assert.Equal("Param char(8);" + Environment.NewLine, result.Output);
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
        /// Param char(8);
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
            Assert.Equal("Param char(8);" + Environment.NewLine, result.Output);
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
        /// Param char(8);
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
            Assert.Equal("Param char(8);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// ConversionService üzerinden CamelCase naming strategy verildiğinde
        /// EGL çıktısının camelCase üretildiğini doğrular.
        ///
        /// Neden var?
        /// ----------------------
        /// Varsayılan naming strategy PascalCase olsa da, dönüşüm pipeline'ı
        /// farklı casing kurallarını destekleyebilmelidir.
        ///
        /// Test edilen PL/I:
        ///
        /// DCL MUST_NO FIXED DECIMAL(8);
        ///
        /// Beklenen EGL:
        ///
        /// mustNo decimal(8);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application uçtan uca dönüşüm testlerinde
        /// - Naming options bilgisinin Application katmanından Transpiler'a
        ///   taşındığını doğrulamada
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// CLI veya UI üzerinden naming style seçimi geldiğinde bu uçtan uca
        /// davranış korunacaktır.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithCamelCaseNamingOptions_ShouldGenerateCamelCaseIdentifier()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL MUST_NO FIXED DECIMAL(8);";
            var namingOptions = new IdentifierNamingOptions(
                IdentifierNamingStyle.CamelCase);

            // Act
            var result = service.ConvertPl1ToEgl(
                source,
                namingOptions);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("mustNo decimal(8);" + Environment.NewLine, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I structure declaration ifadesinin uçtan uca EGL record çıktısına dönüştüğünü doğrular.
        ///
        /// Neden var?
        /// ----------------------
        /// Structure desteği yalnızca parser veya transpiler seviyesinde değil,
        /// tüm dönüşüm pipeline'ında çalışmalıdır.
        ///
        /// Test edilen PL/I:
        ///
        /// DCL 1 PARAME_LIST,
        ///     5 PARAM CHAR(08) INIT(' '),
        ///     5 PARAM2 CHAR(01) INIT(';');
        ///
        /// Beklenen EGL:
        ///
        /// record ParameList type basicRecord
        ///     10 Param char(8);
        ///     10 Param2 char(1);
        /// end
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application uçtan uca dönüşüm testlerinde
        /// - Parser + Transpiler + Generator entegrasyonunu doğrulamada
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithStructureDeclaration_ShouldGenerateEglRecord()
        {
            // Arrange
            var service = new ConversionService();

            var source =
                "DCL 1 PARAME_LIST, " +
                "5 PARAM CHAR(08) INIT(' '), " +
                "5 PARAM2 CHAR(01) INIT(';');";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record ParameList type basicRecord" + Environment.NewLine +
                "        10 Param char(8);" + Environment.NewLine +
                "        10 Param2 char(1);" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);   
        }

        /// <summary>
        /// PL/I structure array tanımının uçtan uca EGL basicRecord array layout
        /// çıktısına dönüştüğünü doğrular.
        ///
        /// Test edilen PL/I:
        /// - DCL 1 DIZI(6)
        /// - Child field toplam uzunluğu: 1 + 2 + 2 + 2 + 8 = 15
        ///
        /// Beklenen EGL:
        /// - 5 Dizi char(15)[6];
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithStructureArrayDeclaration_ShouldGenerateEglRecordArrayField()
        {
            // Arrange
            var service = new ConversionService();

            var source =
                "DCL 1 DIZI(6), " +
                    "3 DIZI_PARAM1 CHAR(01) INIT((*)' '), " +
                    "3 DIZI_PARAM2 CHAR(02) INIT((*)' '), " +
                    "3 DIZI_PARAM3 CHAR(02) INIT((*)' '), " +
                    "3 DIZI_PARAM4 CHAR(02) INIT((*)' '), " +
                    "3 DIZI_PARAM5 CHAR(08) INIT((*)' ');";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record Dizi type basicRecord" + Environment.NewLine +
                "    5 Dizi char(15)[6];" + Environment.NewLine +
                "        10 DiziParam1 char(1);" + Environment.NewLine +
                "        10 DiziParam2 char(2);" + Environment.NewLine +
                "        10 DiziParam3 char(2);" + Environment.NewLine +
                "        10 DiziParam4 char(2);" + Environment.NewLine +
                "        10 DiziParam5 char(8);" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I structure member array tanımının uçtan uca EGL child field array
        /// çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Parser, Transpiler ve Generator pipeline'ının structure member üzerindeki
        /// dimension bilgisini kaybetmeden final EGL output'a taşıdığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL 1 PARAME_LIST,
        ///     5 PARAM_LIST(2) CHAR(10);
        ///
        /// Beklenen temel çıktı:
        /// - record ParameList type basicRecord
        /// - 10 ParamList char(10)[2];
        /// - level 10 için 8 boşluk indentation
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithStructureMemberArrayDeclaration_ShouldGenerateEglChildArrayField()
        {
            // Arrange
            var service = new ConversionService();

            var source =
                "DCL 1 PARAME_LIST, " +
                "5 PARAM_LIST(2) CHAR(10);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record ParameList type basicRecord" + Environment.NewLine +
                "        10 ParamList char(10)[2];" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I structure array içinde bulunan member array alanlarının uçtan uca
        /// EGL çıktısına doğru taşındığını ve parent length hesabına dahil edildiğini
        /// doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Structure array parent field length hesabında member array çarpanının
        /// kullanıldığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL 1 DIZI(6),
        ///     3 DIZI_PARAM1(2) CHAR(10),
        ///     3 DIZI_PARAM2 CHAR(5);
        ///
        /// Beklenen temel çıktı:
        /// - Parent field: 5 Dizi char(25)[6];
        /// - Çünkü hesap: (10 * 2) + 5 = 25
        /// - Child field: 10 DiziParam1 char(10)[2];
        /// - Child field: 10 DiziParam2 char(5);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithStructureArrayContainingMemberArray_ShouldCalculateParentLengthWithArrayMultiplier()
        {
            // Arrange
            var service = new ConversionService();

            var source =
                "DCL 1 DIZI(6), " +
                "3 DIZI_PARAM1(2) CHAR(10), " +
                "3 DIZI_PARAM2 CHAR(5);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record Dizi type basicRecord" + Environment.NewLine +
                "    5 Dizi char(25)[6];" + Environment.NewLine +
                "        10 DiziParam1 char(10)[2];" + Environment.NewLine +
                "        10 DiziParam2 char(5);" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I VARCHAR(n) tekil değişken tanımının uçtan uca EGL char(n)
        /// çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Lexer + Parser + Transpiler + Generator pipeline'ının VARCHAR keyword'ünü
        /// tanıdığını, length bilgisini koruduğunu ve EGL tarafında char(n)
        /// olarak ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL CUSTOMER_NAME VARCHAR(50);
        ///
        /// Beklenen temel çıktı:
        /// - CustomerName char(50);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithVarcharDeclaration_ShouldGenerateEglCharDeclaration()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL CUSTOMER_NAME VARCHAR(50);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "CustomerName char(50);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I structure member içinde kullanılan VARCHAR(n) veri tipinin uçtan uca
        /// EGL record field üzerinde char(n) olarak üretildiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Structure member veri tipi VARCHAR olduğunda parser'ın Pl1VarcharType
        /// ürettiğini, transpiler'ın bunu EglCharacterType'a çevirdiğini ve
        /// generator'ın char(n) output ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL 1 CUSTOMER_INFO,
        ///     5 CUSTOMER_NAME VARCHAR(50);
        ///
        /// Beklenen temel çıktı:
        /// - record CustomerInfo type basicRecord
        /// - 10 CustomerName char(50);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithStructureVarcharMember_ShouldGenerateEglCharField()
        {
            // Arrange
            var service = new ConversionService();

            var source =
                "DCL 1 CUSTOMER_INFO, " +
                "5 CUSTOMER_NAME VARCHAR(50);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record CustomerInfo type basicRecord" + Environment.NewLine +
                "        10 CustomerName char(50);" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I structure array içinde VARCHAR(n) alanı bulunduğunda parent field
        /// length hesabının VARCHAR uzunluğunu dikkate aldığını doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// VARCHAR(n) => char(n) mapping'inin yalnızca output üretiminde değil,
        /// structure array parent field length hesabında da kullanıldığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL 1 DIZI(6),
        ///     3 CUSTOMER_NAME VARCHAR(50),
        ///     3 CUSTOMER_CODE CHAR(10);
        ///
        /// Beklenen temel çıktı:
        /// - Parent field: 5 Dizi char(60)[6];
        /// - Çünkü hesap: VARCHAR(50) + CHAR(10) = 60
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithStructureArrayContainingVarchar_ShouldCalculateParentLengthWithVarcharLength()
        {
            // Arrange
            var service = new ConversionService();

            var source =
                "DCL 1 DIZI(6), " +
                "3 CUSTOMER_NAME VARCHAR(50), " +
                "3 CUSTOMER_CODE CHAR(10);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record Dizi type basicRecord" + Environment.NewLine +
                "    5 Dizi char(60)[6];" + Environment.NewLine +
                "        10 CustomerName char(50);" + Environment.NewLine +
                "        10 CustomerCode char(10);" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I FIXED DEC(p,s) synonym kullanımının uçtan uca EGL decimal(p,s)
        /// çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Lexer + Parser + Transpiler + Generator pipeline'ının DEC kısaltmasını
        /// DECIMAL ile aynı semantic modele taşıdığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL AMOUNT FIXED DEC(17,2);
        ///
        /// Beklenen temel çıktı:
        /// - Amount decimal(17,2);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithFixedDecDeclaration_ShouldGenerateDecimalDeclaration()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL AMOUNT FIXED DEC(17,2);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Amount decimal(17,2);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I DEC FIXED(p,s) ters keyword sırasının uçtan uca EGL decimal(p,s)
        /// çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Parser'ın DEC FIXED söz dizimini FIXED DEC ile aynı semantic modele
        /// taşıdığını ve generator'ın aynı EGL decimal çıktısını ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL AMOUNT DEC FIXED(17,2);
        ///
        /// Beklenen temel çıktı:
        /// - Amount decimal(17,2);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithDecFixedDeclaration_ShouldGenerateDecimalDeclaration()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL AMOUNT DEC FIXED(17,2);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Amount decimal(17,2);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I DECIMAL FIXED(p) ters keyword sırasının scale olmadan uçtan uca
        /// EGL decimal(p) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Parser'ın scale verilmemiş decimal tipte Scale bilgisini null koruduğunu
        /// ve generator'ın decimal(p,0) değil decimal(p) ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL COUNT DECIMAL FIXED(15);
        ///
        /// Beklenen temel çıktı:
        /// - Count decimal(15);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithDecimalFixedDeclarationWithoutScale_ShouldGenerateDecimalWithoutScale()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL COUNT DECIMAL FIXED(15);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Count decimal(15);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I FIXED DEC(p,0) kullanımında açıkça verilen zero scale bilgisinin
        /// EGL decimal(p,0) çıktısına taşındığını doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Parser'ın `,0` bilgisini kaybetmediğini ve generator'ın decimal(p)
        /// yerine decimal(p,0) ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL COUNT FIXED DEC(15,0);
        ///
        /// Beklenen temel çıktı:
        /// - Count decimal(15,0);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithFixedDecExplicitZeroScale_ShouldGenerateDecimalWithZeroScale()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL COUNT FIXED DEC(15,0);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Count decimal(15,0);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I FIXED BIN(15) kullanımının uçtan uca EGL smallint çıktısına
        /// dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Lexer + Parser + Transpiler + Generator pipeline'ının FIXED BIN(15)
        /// kullanımını smallint olarak ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL COUNT FIXED BIN(15);
        ///
        /// Beklenen temel çıktı:
        /// - Count smallint;
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithFixedBin15Declaration_ShouldGenerateSmallintDeclaration()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL COUNT FIXED BIN(15);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Count smallint;" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I BIN FIXED(31) ters keyword sırasının uçtan uca EGL int çıktısına
        /// dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Parser'ın BIN FIXED kullanımını FIXED BIN ile aynı semantic modele
        /// taşıdığını ve generator'ın int çıktısı ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL COUNT BIN FIXED(31);
        ///
        /// Beklenen temel çıktı:
        /// - Count int;
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithBinFixed31Declaration_ShouldGenerateIntDeclaration()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL COUNT BIN FIXED(31);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Count int;" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I structure içinde FIXED BIN alanlarının EGL record field olarak
        /// smallint ve int çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Structure member alanlarında binary fixed mapping'in doğru çalıştığını
        /// doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL 1 CUSTOMER_INFO,
        ///     5 ITEM_COUNT FIXED BIN(15),
        ///     5 TOTAL_COUNT FIXED BIN(31);
        ///
        /// Beklenen temel çıktı:
        /// - 10 ItemCount smallint;
        /// - 10 TotalCount int;
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithStructureFixedBinaryMembers_ShouldGenerateIntegerFields()
        {
            // Arrange
            var service = new ConversionService();

            var source =
                "DCL 1 CUSTOMER_INFO, " +
                "5 ITEM_COUNT FIXED BIN(15), " +
                "5 TOTAL_COUNT FIXED BIN(31);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record CustomerInfo type basicRecord" + Environment.NewLine +
                "        10 ItemCount smallint;" + Environment.NewLine +
                "        10 TotalCount int;" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I PIC '999' kullanımının uçtan uca EGL num(3) çıktısına dönüştüğünü
        /// doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Parser + Transpiler + Generator pipeline'ının numeric PIC pattern'i
        /// num(p) olarak ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// ----------------------
        /// DCL PARAM1 PIC '999';
        ///
        /// Beklenen temel çıktı:
        /// - Param1 num(3);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithNumericPicDeclaration_ShouldGenerateNumDeclaration()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL PARAM1 PIC '999';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Param1 num(3);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I PIC '999V99' kullanımının uçtan uca EGL num(5,2) çıktısına
        /// dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Parser'ın V implied decimal bilgisini scale olarak hesapladığını ve
        /// generator'ın num(p,s) çıktısı ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL PARAM2 PIC '999V99';
        ///
        /// Beklenen temel çıktı:
        /// - Param2 num(5,2);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithNumericPicHavingScale_ShouldGenerateNumDeclarationWithScale()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL PARAM2 PIC '999V99';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Param2 num(5,2);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I PIC '(13)9V99' kullanımının repeat count ve implied decimal bilgisiyle
        /// EGL num(15,2) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// ----------------------
        /// Parser'ın `(n)9` repeat count bilgisini precision hesabına dahil ettiğini
        /// ve V sonrasındaki digit sayısını scale olarak taşıdığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL PARAM3 PIC '(13)9V99';
        ///
        /// Beklenen temel çıktı:
        /// - Param3 num(15,2);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithRepeatedNumericPicHavingScale_ShouldGenerateNumDeclarationWithScale()
        {
            // Arrange
            var service = new ConversionService();

            var source = "DCL PARAM3 PIC '(13)9V99';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Param3 num(15,2);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }
    }
}
