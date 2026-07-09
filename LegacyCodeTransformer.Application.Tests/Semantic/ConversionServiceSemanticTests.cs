using LegacyCodeTransformer.Application.Services;

namespace LegacyCodeTransformer.Application.Tests.Semantic
{
    public sealed class ConversionServiceSemanticTests
    {
        [Fact]
        public void ConvertPl1ToEgl_WithDuplicateDeclaration_ShouldFailBeforeTranspiler()
        {
            var source = """
             DCL MUST_NO FIXED DECIMAL(8);
             DCL MUST_NO CHAR(8);
            """;

            var service = new ConversionService();

            var result = service.ConvertPl1ToEgl(source);

            Assert.False(result.Success);
            Assert.Null(result.Output);
            Assert.Contains(
                result.Diagnostics,
                diagnostic => diagnostic.Message.Contains("Duplicate declaration bulundu: MUST_NO"));
        }
    }
}