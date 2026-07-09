using LegacyCodeTransformer.Application.Services;

namespace LegacyCodeTransformer.Application.Tests.Regression
{
    public sealed class Pl1ToEglRegressionTests
    {
        [Fact]
        public void Convert_WithDeclarationAssignmentAndCall_ShouldPreserveEndToEndOutput()
        {
            var source = """
             DCL CUSTOMER_NO FIXED DECIMAL(10);
             CUSTOMER_NO = 12345;
             CALL FETCH_CUSTOMER;
            """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(source);

            Assert.True(result.Success);

            var expected =
                "CustomerNo decimal(10);" + Environment.NewLine +
                "CustomerNo = 12345;" + Environment.NewLine +
                "call FetchCustomer();" + Environment.NewLine;

            Assert.Equal(expected, result.Output);
        }

        [Fact]
        public void Convert_WithIfAndCall_ShouldPreserveEndToEndOutput()
        {
            var source = """
             IF CUSTOMER_NO = MUST_NO THEN CALL FETCH_CUSTOMER;
            """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(source);

            Assert.True(result.Success);

            var expected =
                "if (CustomerNo = MustNo)" + Environment.NewLine +
                "    call FetchCustomer();" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.Equal(expected, result.Output);
        }

        [Fact]
        public void Convert_WithDoWhileAndCall_ShouldPreserveEndToEndOutput()
        {
            var source = """
             DO WHILE(SQLCODE = 0);
                 CALL FETCH_CURSOR;
             END;
            """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(source);

            Assert.True(result.Success);

            var expected =
                "while (Sqlcode = 0)" + Environment.NewLine +
                "    call FetchCursor();" + Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.Equal(expected, result.Output);
        }
    }
}