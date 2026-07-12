using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Statements;

namespace LegacyCodeTransformer.Egl.Functions
{
    /// <summary>
    /// EGL function syntax modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I procedure modellerinin EGL hedef dilinde function olarak
    /// temsil edilmesi gerekir. Function bilgisi top-level executable
    /// statement gibi taşınmamalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// İlk kapsamda parametresiz EGL function adını ve function body
    /// statement listesini güçlü tipli bir syntax modelinde taşır.
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
    /// Pl1ToEglTranspiler tarafından Pl1Procedure dönüşüm çıktısı olarak
    /// oluşturulur ve EglSyntaxTree.Functions koleksiyonunda taşınır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL function parameter modelleri, local declaration'lar, return
    /// type bilgisi ve function-level metadata gerektiğinde gerçek
    /// ihtiyaçlara göre bu model kontrollü biçimde genişletilecektir.
    /// </summary>
    public sealed class EglFunction : SyntaxNode
    {
        public string Name { get; }

        public IReadOnlyList<EglStatement> Statements { get; }

        public EglFunction(
            string name,
            IEnumerable<EglStatement>? statements,
            SourceLocation location)
            : base(location)
        {
            Name = name;
            Statements = statements?.ToList() ??
                new List<EglStatement>();
        }
    }
}