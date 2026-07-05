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

        /// <summary>
        /// PL/I PIC 'XXX' kullanımının uçtan uca EGL char(3) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// Parser + Transpiler + Generator pipeline'ının alphanumeric PIC pattern'i char(n) olarak ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL PARAM6 PIC 'XXX';
        ///
        /// Beklenen temel çıktı:
        /// Param6 char(3);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithAlphanumericPicDeclaration_ShouldGenerateCharDeclaration()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL PARAM6 PIC 'XXX';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Param6 char(3);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I PIC '(20)X' kullanımının repeat count ile EGL char(20) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// Parser'ın alphanumeric repeat syntax length bilgisini hesapladığını ve generator'ın char(n) çıktısı ürettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL PARAM7 PIC '(20)X';
        ///
        /// Beklenen temel çıktı:
        /// Param7 char(20);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithRepeatedAlphanumericPicDeclaration_ShouldGenerateCharDeclarationWithLength()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL PARAM7 PIC '(20)X';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Param7 char(20);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I PIC 'AAA' kullanımının EGL char(3) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// A karakterlerinden oluşan alphanumeric PIC pattern'in karakter alanı olarak dönüştürüldüğünü doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL PARAM8 PIC 'AAA';
        ///
        /// Beklenen temel çıktı:
        /// Param8 char(3);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithAlphabeticPicDeclaration_ShouldGenerateCharDeclaration()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL PARAM8 PIC 'AAA';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Param8 char(3);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// PL/I PIC 'AXXAA' mixed alphanumeric pattern kullanımının EGL char(5) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// A ve X karakterlerinden oluşan mixed alphanumeric PIC pattern'in toplam length ile üretildiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL PARAM9 PIC 'AXXAA';
        ///
        /// Beklenen temel çıktı:
        /// Param9 char(5);
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithMixedAlphanumericPicDeclaration_ShouldGenerateCharDeclaration()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL PARAM9 PIC 'AXXAA';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Param9 char(5);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// Structure array içindeki alphanumeric PIC alanlarının parent array length hesabına dahil edildiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// Parser + Transpiler + Generator pipeline'ında PIC 'XXX' ve PIC '(20)X' alanlarının char length olarak hesaplandığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL 1 DIZI(2), 3 KOD PIC 'XXX', 3 AD PIC '(20)X';
        ///
        /// Beklenen temel çıktı:
        /// Parent array field length 3 + 20 = 23 olmalıdır.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithStructureArrayHavingAlphanumericPicFields_ShouldCalculateParentLength()
        {
            // Arrange
            var service = new ConversionService();
            var source =
                "DCL 1 DIZI(2), " +
                "3 KOD PIC 'XXX', " +
                "3 AD PIC '(20)X';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record Dizi type basicRecord" + Environment.NewLine +
                "    5 Dizi char(23)[2];" + Environment.NewLine +
                "        10 Kod char(3);" + Environment.NewLine +
                "        10 Ad char(20);" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// Nested group içindeki alphanumeric PIC alanlarının group field length hesabına dahil edildiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// Veri tipi olmayan nested group altında bulunan PIC 'AAA' ve PIC 'AXXAA' alanlarının toplam group char length değerine dahil edildiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL 1 MUSTERI, 3 ADRES, 5 IL PIC 'AAA', 5 KOD PIC 'AXXAA';
        ///
        /// Beklenen temel çıktı:
        /// ADRES group field length 3 + 5 = 8 olmalıdır.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithNestedGroupHavingAlphanumericPicFields_ShouldCalculateGroupLength()
        {
            // Arrange
            var service = new ConversionService();
            var source =
                "DCL 1 MUSTERI, " +
                "3 ADRES, " +
                "5 IL PIC 'AAA', " +
                "5 KOD PIC 'AXXAA';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "record Musteri type basicRecord" + Environment.NewLine +
                "    5 Adres char(8);" + Environment.NewLine +
                "        10 Il char(3);" + Environment.NewLine +
                "        10 Kod char(5);" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// Signed numeric PIC declaration bilgisinin EGL num(p) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// Parser + Transpiler + Generator pipeline'ının PIC 'S999' pattern'ını numeric alan olarak kabul ettiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL TUTAR PIC 'S999';
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Tutar num(3); çıktısı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithSignedNumericPicDeclaration_ShouldGenerateNumDeclaration()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL TUTAR PIC 'S999';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Tutar num(3);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// Signed numeric PIC declaration içindeki implied decimal bilgisinin EGL num(p,s) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// PIC 'S999V99' pattern'ında S bilgisinin metadata olarak korunduğunu fakat EGL output tarafında num(5,2) üretildiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL TUTAR PIC 'S999V99';
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Tutar num(5,2); çıktısı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithSignedNumericPicDeclarationHavingScale_ShouldGenerateNumDeclarationWithScale()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL TUTAR PIC 'S999V99';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Tutar num(5,2);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// Signed numeric PIC repeat declaration bilgisinin EGL num(p) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// PIC 'S(8)9' pattern'ındaki repeat syntax bilgisinin precision 8 olarak hesaplanıp EGL num(8) çıktısına taşındığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL SAYAC PIC 'S(8)9';
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Sayac num(8); çıktısı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithSignedRepeatedNumericPicDeclaration_ShouldGenerateNumDeclaration()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL SAYAC PIC 'S(8)9';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Sayac num(8);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// Signed numeric PIC repeat declaration içindeki scale bilgisinin EGL num(p,s) çıktısına dönüştüğünü doğrular.
        ///
        /// Bu test neyi doğrular?
        /// PIC 'S(10)9V99' pattern'ındaki repeat ve scale bilgisinin birlikte hesaplanarak EGL num(12,2) çıktısına dönüştüğünü doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL TUTAR PIC 'S(10)9V99';
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Tutar num(12,2); çıktısı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithSignedRepeatedNumericPicDeclarationHavingScale_ShouldGenerateNumDeclarationWithScale()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL TUTAR PIC 'S(10)9V99';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            var expected =
                "Tutar num(12,2);" + Environment.NewLine;

            Assert.True(result.Success);
            Assert.Equal(expected, result.Output);
            Assert.Empty(result.Diagnostics);
        }

        /// <summary>
        /// Zero suppression içeren formatted PIC declaration bilgisinin diagnostic ürettiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// Parser'ın PIC 'ZZ9' pattern'ını okuyabildiğini, fakat Transpiler'ın semantic kayıp riski nedeniyle EGL output üretmediğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL SAYI PIC 'ZZ9';
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Conversion başarısız olmalı, Output null olmalı ve desteklenmeyen PIC pattern diagnostic mesajı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithZeroSuppressFormattedPicDeclaration_ShouldReturnDiagnostic()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL SAYI PIC 'ZZ9';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Output);
            Assert.Single(result.Diagnostics);
            Assert.Contains(
                "Desteklenmeyen PIC pattern: ZZ9",
                result.Diagnostics[0].Message);
        }

        /// <summary>
        /// Thousands separator ve display decimal point içeren formatted PIC declaration bilgisinin diagnostic ürettiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// PIC 'Z,ZZ9V.99' pattern'ının formatted kabul edildiğini ve doğrudan EGL num mapping yapılmadığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL TUTAR PIC 'Z,ZZ9V.99';
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Conversion başarısız olmalı, Output null olmalı ve desteklenmeyen PIC pattern diagnostic mesajı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithSeparatorFormattedPicDeclaration_ShouldReturnDiagnostic()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL TUTAR PIC 'Z,ZZ9V.99';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Output);
            Assert.Single(result.Diagnostics);
            Assert.Contains(
                "Desteklenmeyen PIC pattern: Z,ZZ9V.99",
                result.Diagnostics[0].Message);
        }

        /// <summary>
        /// Leading plus edit mask içeren formatted PIC declaration bilgisinin diagnostic ürettiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// PIC '+999' pattern'ındaki + karakterinin signed numeric metadata değil, formatted edit mask olarak ele alındığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL SAYI PIC '+999';
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Conversion başarısız olmalı, Output null olmalı ve desteklenmeyen PIC pattern diagnostic mesajı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithLeadingPlusFormattedPicDeclaration_ShouldReturnDiagnostic()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL SAYI PIC '+999';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Output);
            Assert.Single(result.Diagnostics);
            Assert.Contains(
                "Desteklenmeyen PIC pattern: +999",
                result.Diagnostics[0].Message);
        }

        /// <summary>
        /// Leading minus edit mask içeren formatted PIC declaration bilgisinin diagnostic ürettiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// PIC '-999' pattern'ındaki - karakterinin signed numeric metadata değil, formatted edit mask olarak ele alındığını doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL SAYI PIC '-999';
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Conversion başarısız olmalı, Output null olmalı ve desteklenmeyen PIC pattern diagnostic mesajı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithLeadingMinusFormattedPicDeclaration_ShouldReturnDiagnostic()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL SAYI PIC '-999';";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Output);
            Assert.Single(result.Diagnostics);
            Assert.Contains(
                "Desteklenmeyen PIC pattern: -999",
                result.Diagnostics[0].Message);
        }

        /// <summary>
        /// BIT(1) declaration bilgisinin parser tarafından okunup transpiler aşamasında diagnostic ürettiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// BIT veri tipinin parse edildiğini fakat henüz EGL mapping yapılmadığı için conversion sonucunun başarısız olduğunu doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL FLAG BIT(1);
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Conversion başarısız olmalı, Output null olmalı ve BIT mapping desteklenmiyor diagnostic mesajı üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithBitDeclaration_ShouldReturnDiagnostic()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL FLAG BIT(1);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Output);
            Assert.Single(result.Diagnostics);
            Assert.Contains(
                "BIT veri tipi için EGL mapping henüz desteklenmiyor. Length: 1",
                result.Diagnostics[0].Message);
        }

        /// <summary>
        /// Structure member içinde BIT(8) kullanıldığında conversion diagnostic üretildiğini doğrular.
        ///
        /// Bu test neyi doğrular?
        /// BIT veri tipinin structure member olarak parse edildiğini fakat structure length / EGL mapping henüz desteklenmediği için diagnostic üretildiğini doğrular.
        ///
        /// Hangi input'u test eder?
        /// DCL 1 FLAGS, 5 MASK BIT(8);
        ///
        /// Beklenen temel model/çıktı nedir?
        /// Conversion başarısız olmalı, Output null olmalı ve BIT member length hesaplanamadığı için diagnostic üretilmelidir.
        /// </summary>
        [Fact]
        public void ConvertPl1ToEgl_WithBitStructureMember_ShouldReturnDiagnostic()
        {
            // Arrange
            var service = new ConversionService();
            var source = "DCL 1 FLAGS, 5 MASK BIT(8);";

            // Act
            var result = service.ConvertPl1ToEgl(source);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Output);
            Assert.Single(result.Diagnostics);
            Assert.Contains(
                "Structure member uzunluğu hesaplanamayan PL/I member veri tipi: Pl1BitType",
                result.Diagnostics[0].Message);
        }
    }
}
