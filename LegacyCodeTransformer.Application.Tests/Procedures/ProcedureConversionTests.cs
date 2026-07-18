using LegacyCodeTransformer.Application.Services;

namespace LegacyCodeTransformer.Application.Tests.Procedures
{
    /// <summary>
    /// PL/I procedure → EGL function dönüşümünü application pipeline
    /// seviyesinde doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Model ve generator unit testleri tek başına lexer, parser,
    /// semantic analyzer, transpiler ve generator zincirinin birlikte
    /// çalıştığını garanti etmez.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Case001 benzeri gerçekçi PL/I kaynağının EGL function çıktısına
    /// dönüştürüldüğünü uçtan uca doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Global declaration'lar ile procedure içindeki assignment ve
    /// argument içeren CALL statement.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P10.3 application end-to-end testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parameter alan function, local declaration ve OPTIONS(MAIN)
    /// conversion testlerine temel olur.
    /// </summary>
    public sealed class ProcedureConversionTests
    {
        [Fact]
        public void ConvertPl1ToEgl_WithParameterlessProcedure_ShouldGenerateEglFunction()
        {
            var source = """
             DCL MUST_NO FIXED DECIMAL(8);
             DCL CUSTOMER_NO FIXED DECIMAL(8);
             DCL CUSTOMER_NAME CHAR(30);

             CUSTOMER_PROCESS: PROCEDURE;
                 CUSTOMER_NO = MUST_NO;
                 CALL FETCH_CUSTOMER(CUSTOMER_NO, CUSTOMER_NAME);
             END CUSTOMER_PROCESS;
            """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(source);

            Assert.True(result.Success);
            Assert.Empty(result.Diagnostics);
            Assert.NotNull(result.Output);

            var expected =
                "MustNo decimal(8);" + Environment.NewLine +
                "CustomerNo decimal(8);" + Environment.NewLine +
                "CustomerName char(30);" + Environment.NewLine +
                Environment.NewLine +
                "function CustomerProcess()" + Environment.NewLine +
                "    CustomerNo = MustNo;" + Environment.NewLine +
                "    FetchCustomer(CustomerNo, CustomerName);" +
                Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.Equal(
                expected,
                result.Output);
        }

        [Fact]
        public void ConvertPl1ToEgl_WithResolvedInputParameter_ShouldGenerateParameterizedEglFunction()
        {
            var source = """
     DCL ERROR_TEXT CHAR(50);

     CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
         DCL PROCESS_TEXT CHAR(50);

         ERROR_TEXT = PROCESS_TEXT;
         CALL WRITE_ERROR(ERROR_TEXT);
     END CUSTOMER_PROCESS;

     CALL CUSTOMER_PROCESS('CUSTOMER NOT FOUND');
    """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(
                source);

            Assert.True(
                result.Success);

            Assert.Empty(
                result.Diagnostics);

            Assert.NotNull(
                result.Output);

            var expected =
                "ErrorText char(50);" +
                Environment.NewLine +
                Environment.NewLine +
                "function CustomerProcess(" +
                "ProcessText char(50) in)" +
                Environment.NewLine +
                "    ErrorText = ProcessText;" +
                Environment.NewLine +
                "    WriteError(ErrorText);" +
                Environment.NewLine +
                "end" +
                Environment.NewLine +
                Environment.NewLine +
                "CustomerProcess(\"CUSTOMER NOT FOUND\");" +
                Environment.NewLine;

            Assert.Equal(
                expected,
                result.Output);
        }
    }
}