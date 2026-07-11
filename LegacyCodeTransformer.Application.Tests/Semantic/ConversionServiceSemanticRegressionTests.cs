using LegacyCodeTransformer.Application.Services;

namespace LegacyCodeTransformer.Application.Tests.Semantic
{
    /// <summary>
    /// Semantic analysis katmanının application conversion pipeline içindeki
    /// davranışını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Semantic analyzer yalnızca bağımsız unit testlerde değil, gerçek
    /// PL/I → EGL dönüşüm pipeline içinde de doğru aşamada çalışmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Geçerli semantic input'un EGL output ürettiğini, duplicate declaration
    /// durumunda ise transpiler öncesinde dönüşümün durduğunu sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// CUSTOMER_NO = MUST_NO;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P09.5 application semantic regression testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Undefined identifier, type mismatch ve scope diagnostic'leri application
    /// pipeline'a eklendiğinde dönüşüm durdurma davranışını korur.
    /// </summary>
    public sealed class ConversionServiceSemanticRegressionTests
    {
        [Fact]
        public void ConvertPl1ToEgl_WithValidSemanticReferences_ShouldProduceEglOutput()
        {
            var source = """
             DCL MUST_NO FIXED DECIMAL(8);
             DCL CUSTOMER_NO FIXED DECIMAL(8);

             CUSTOMER_NO = MUST_NO;
            """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(source);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.NotNull(result.Output);

            var expected =
                "MustNo decimal(8);" + Environment.NewLine +
                "CustomerNo decimal(8);" + Environment.NewLine +
                "CustomerNo = MustNo;" + Environment.NewLine;

            Assert.Equal(expected, result.Output);
        }

        [Fact]
        public void ConvertPl1ToEgl_WithDuplicateDeclaration_ShouldStopBeforeEglGeneration()
        {
            var source = """
             DCL MUST_NO FIXED DECIMAL(8);
             DCL MUST_NO CHAR(8);

             CALL FETCH_CUSTOMER(MUST_NO);
            """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(source);

            Assert.False(result.Success);
            Assert.Null(result.Output);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "Duplicate declaration bulundu: MUST_NO"));
        }

        [Fact]
        public void ConvertPl1ToEgl_WithUnresolvedReference_ShouldContinueUntilPolicyIsDefined()
        {
            var source = """
             DCL CUSTOMER_NO FIXED DECIMAL(8);

             CUSTOMER_NO = GUNCEL_MUST_NO;
            """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(source);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.NotNull(result.Output);

            var expected =
                "CustomerNo decimal(8);" + Environment.NewLine +
                "CustomerNo = GuncelMustNo;" + Environment.NewLine;

            Assert.Equal(expected, result.Output);
        }
    }
}