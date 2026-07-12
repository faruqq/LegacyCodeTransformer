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

            Assert.Equal(
                2,
                function.Statements.Count);

            Assert.IsType<EglAssignmentStatement>(
                function.Statements[0]);

            Assert.IsType<EglCallStatement>(
                function.Statements[1]);
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