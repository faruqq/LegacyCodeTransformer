using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I kaynak kodunda kullanılan bir symbol reference bilgisini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// SymbolTable declaration bilgilerini taşır; ancak semantic analysis sırasında
    /// declaration'ların nerelerde kullanıldığının da korunması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Basit identifier ve qualified identifier kullanımlarını, çözümlemede
    /// kullanılacak root symbol adıyla birlikte taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_NO
    ///
    /// CUSTOMER_INFO.MUST_NO
    ///
    /// İlk örnekte ReferenceText ve RootSymbolName CUSTOMER_NO olur.
    /// İkinci örnekte ReferenceText CUSTOMER_INFO.MUST_NO,
    /// RootSymbolName CUSTOMER_INFO olur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1ReferenceCollector tarafından üretilir ve SemanticResult içinde taşınır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Undefined identifier politikası, structure member resolution, reference
    /// navigation ve rename analysis çalışmalarına temel olur.
    /// </summary>
    public sealed class Pl1SymbolReference
    {
        public string ReferenceText { get; }

        public string RootSymbolName { get; }

        public bool IsResolved { get; }

        public SourceLocation Location { get; }

        public Pl1SymbolReference(
            string referenceText,
            string rootSymbolName,
            bool isResolved,
            SourceLocation location)
        {
            ReferenceText = referenceText;
            RootSymbolName = rootSymbolName;
            IsResolved = isResolved;
            Location = location;
        }
    }
}