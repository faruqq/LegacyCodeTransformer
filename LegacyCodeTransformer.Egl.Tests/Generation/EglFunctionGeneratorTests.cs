using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Functions;
using LegacyCodeTransformer.Egl.Generation;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Tests.Generation
{
    /// <summary>
    /// EGL function modellerinin kaynak koda dönüştürülmesini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// EglFunction modelinin yalnızca syntax tree üzerinde bulunması
    /// yeterli değildir. Generator geçerli function bloğu üretmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Function header, body indentation, doğrudan function invocation
    /// ve end satırı üretimini doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// function CustomerProcess()
    ///     CustomerNo = MustNo;
    ///     FetchCustomer(CustomerNo, CustomerName);
    /// end
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// EGL generator unit testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parameter, local declaration ve return statement generator
    /// testlerine temel olur.
    /// </summary>
    public sealed class EglFunctionGeneratorTests
    {
        [Fact]
        public void Generate_WithParameterlessFunction_ShouldGenerateFunctionBlock()
        {
            var function = new EglFunction(
                "CustomerProcess",
                new EglStatement[]
                {
                    new EglAssignmentStatement(
                        "CustomerNo",
                        "MustNo",
                        SourceLocation.Unknown),

                    new EglCallStatement(
                        "FetchCustomer",
                        new[]
                        {
                            "CustomerNo",
                            "CustomerName"
                        },
                        SourceLocation.Unknown)
                },
                SourceLocation.Unknown);

            var syntaxTree = new EglSyntaxTree(
                declarations: null,
                functions: new[]
                {
                    function
                },
                statements: null,
                location: SourceLocation.Unknown);

            var generator = new EglCodeGenerator();

            var result = generator.Generate(syntaxTree);

            var expected =
                "function CustomerProcess()" + Environment.NewLine +
                "    CustomerNo = MustNo;" + Environment.NewLine +
                "    FetchCustomer(CustomerNo, CustomerName);" +
                Environment.NewLine +
                "end" + Environment.NewLine;

            Assert.Equal(
                expected,
                result);
        }

        [Fact]
        public void Generate_WithDeclarationsFunctionAndTopLevelInvocation_ShouldPreserveRootOrder()
        {
            var syntaxTree = new EglSyntaxTree(
                declarations: new EglDeclaration[]
                {
                    new EglVariableDeclaration(
                        "CustomerNo",
                        new EglDecimalType(
                            8,
                            null,
                            SourceLocation.Unknown),
                        SourceLocation.Unknown)
                },
                functions: new[]
                {
                    new EglFunction(
                        "CustomerProcess",
                        new EglStatement[]
                        {
                            new EglAssignmentStatement(
                                "CustomerNo",
                                "MustNo",
                                SourceLocation.Unknown)
                        },
                        SourceLocation.Unknown)
                },
                statements: new EglStatement[]
                {
                    new EglCallStatement(
                        "CustomerProcess",
                        null,
                        SourceLocation.Unknown)
                },
                location: SourceLocation.Unknown);

            var generator = new EglCodeGenerator();

            var result = generator.Generate(syntaxTree);

            var expected =
                "CustomerNo decimal(8);" + Environment.NewLine +
                Environment.NewLine +
                "function CustomerProcess()" + Environment.NewLine +
                "    CustomerNo = MustNo;" + Environment.NewLine +
                "end" + Environment.NewLine +
                Environment.NewLine +
                "CustomerProcess();" + Environment.NewLine;

            Assert.Equal(
                expected,
                result);
        }

        [Fact]
        public void Generate_WithFunctionParameters_ShouldGenerateTypesDirectionsAndPreserveOrder()
        {
            var function = new EglFunction(
                "ProcessData",
                statements: null,
                SourceLocation.Unknown,
                parameters: new[]
                {
            new EglFunctionParameter(
                "InputText",
                new EglCharacterType(
                    30,
                    SourceLocation.Unknown),
                SourceLocation.Unknown,
                EglFunctionParameterDirection.In),

            new EglFunctionParameter(
                "OutputText",
                new EglCharacterType(
                    50,
                    SourceLocation.Unknown),
                SourceLocation.Unknown,
                EglFunctionParameterDirection.Out),

            new EglFunctionParameter(
                "SharedText",
                new EglCharacterType(
                    20,
                    SourceLocation.Unknown),
                SourceLocation.Unknown,
                EglFunctionParameterDirection.InOut)
                });

            var syntaxTree = new EglSyntaxTree(
                declarations: null,
                functions: new[]
                {
            function
                },
                statements: null,
                location: SourceLocation.Unknown);

            var generator = new EglCodeGenerator();

            var result = generator.Generate(
                syntaxTree);

            var expected =
                "function ProcessData(" +
                "InputText char(30) in, " +
                "OutputText char(50) out, " +
                "SharedText char(20) inOut)" +
                Environment.NewLine +
                "end" +
                Environment.NewLine;

            Assert.Equal(
                expected,
                result);
        }
    }
}