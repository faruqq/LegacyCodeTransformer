using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Functions;
using LegacyCodeTransformer.Egl.Statements;

namespace LegacyCodeTransformer.Egl.Tests.Functions
{
    /// <summary>
    /// EGL function syntax modelinin temel davranışlarını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I procedure modelleri artık EGL tarafında güçlü tipli function
    /// modeliyle temsil edilmektedir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Function adının ve body statement listesinin model üzerinde
    /// kayıpsız taşındığını doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// function CustomerProcess()
    ///     CustomerNo = MustNo;
    ///     FetchCustomer(CustomerNo);
    /// end
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// EGL syntax model unit testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Function parameter, local declaration ve return type modelleri
    /// eklendiğinde temel function sözleşmesini korur.
    /// </summary>
    public sealed class EglFunctionTests
    {
        [Fact]
        public void Constructor_WithNameAndStatements_ShouldPreserveValues()
        {
            var statements = new EglStatement[]
            {
        new EglAssignmentStatement(
            "CustomerNo",
            "MustNo",
            SourceLocation.Unknown),

        new EglCallStatement(
            "FetchCustomer",
            new[]
            {
                "CustomerNo"
            },
            SourceLocation.Unknown)
            };

            var function = new EglFunction(
                "CustomerProcess",
                statements,
                SourceLocation.Unknown);

            Assert.Equal(
                "CustomerProcess",
                function.Name);

            Assert.Empty(
                function.Parameters);

            Assert.Equal(
                2,
                function.Statements.Count);

            Assert.IsType<EglAssignmentStatement>(
                function.Statements[0]);

            Assert.IsType<EglCallStatement>(
                function.Statements[1]);
        }

        [Fact]
        public void Constructor_WithParameters_ShouldPreserveParameterList()
        {
            var parameter = new EglFunctionParameter(
                "ProcessText",
                new LegacyCodeTransformer.Egl.Types.EglCharacterType(
                    50,
                    SourceLocation.Unknown),
                SourceLocation.Unknown);

            var function = new EglFunction(
                "CustomerProcess",
                statements: null,
                SourceLocation.Unknown,
                parameters: new[]
                {
            parameter
                });

            var result = Assert.Single(
                function.Parameters);

            Assert.Same(
                parameter,
                result);

            Assert.Empty(
                function.Statements);
        }

        [Fact]
        public void Constructor_WithMultipleParameters_ShouldPreserveParameterOrder()
        {
            var inputParameter = new EglFunctionParameter(
                "InputText",
                new LegacyCodeTransformer.Egl.Types.EglCharacterType(
                    30,
                    SourceLocation.Unknown),
                SourceLocation.Unknown);

            var outputParameter = new EglFunctionParameter(
                "OutputText",
                new LegacyCodeTransformer.Egl.Types.EglCharacterType(
                    50,
                    SourceLocation.Unknown),
                SourceLocation.Unknown);

            var function = new EglFunction(
                "ProcessData",
                statements: null,
                SourceLocation.Unknown,
                parameters: new[]
                {
            inputParameter,
            outputParameter
                });

            Assert.Equal(
                2,
                function.Parameters.Count);

            Assert.Same(
                inputParameter,
                function.Parameters[0]);

            Assert.Same(
                outputParameter,
                function.Parameters[1]);
        }

        [Fact]
        public void Constructor_WithoutStatements_ShouldCreateEmptyStatementCollection()
        {
            var function = new EglFunction(
                "InitializeProcess",
                statements: null,
                SourceLocation.Unknown);

            Assert.Equal(
                "InitializeProcess",
                function.Name);

            Assert.Empty(function.Statements);
        }
    }
}