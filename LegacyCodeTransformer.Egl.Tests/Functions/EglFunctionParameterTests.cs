using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Functions;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Tests.Functions
{
    /// <summary>
    /// EGL function parameter syntax modelinin temel davranışlarını
    /// doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I procedure parametreleri EGL function modeline
    /// dönüştürülürken parameter adı ve çözümlenen EGL veri tipi
    /// birlikte korunmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EglFunctionParameter modelinin parameter adını ve güçlü tipli
    /// EGL veri tipini kayıpsız taşıdığını doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P10.3 EGL function parameter model testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parameter direction, generator output ve procedure-to-function
    /// transpiler testlerine temel olur.
    /// </summary>
    public sealed class EglFunctionParameterTests
    {
        [Fact]
        public void Constructor_WithNameAndDataType_ShouldPreserveValuesAndUseUnknownDirection()
        {
            var dataType = new EglCharacterType(
                50,
                SourceLocation.Unknown);

            var parameter = new EglFunctionParameter(
                "ProcessText",
                dataType,
                SourceLocation.Unknown);

            Assert.Equal(
                "ProcessText",
                parameter.Name);

            Assert.Same(
                dataType,
                parameter.DataType);

            Assert.Equal(
                EglFunctionParameterDirection.Unknown,
                parameter.Direction);
        }

        [Fact]
        public void Constructor_WithCharacterType_ShouldPreserveLength()
        {
            var parameter = new EglFunctionParameter(
                "ProcessText",
                new EglCharacterType(
                    50,
                    SourceLocation.Unknown),
                SourceLocation.Unknown);

            var dataType =
                Assert.IsType<EglCharacterType>(
                    parameter.DataType);

            Assert.Equal(
                50,
                dataType.Length);
        }

        [Fact]
        public void Constructor_WithExplicitDirection_ShouldPreserveDirection()
        {
            var parameter = new EglFunctionParameter(
                "ProcessText",
                new EglCharacterType(
                    50,
                    SourceLocation.Unknown),
                SourceLocation.Unknown,
                EglFunctionParameterDirection.In);

            Assert.Equal(
                EglFunctionParameterDirection.In,
                parameter.Direction);
        }
    }
}