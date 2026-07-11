using LegacyCodeTransformer.Core.Diagnostics;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I semantic analysis sonucunu temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser syntax tree üretir; ancak semantic kontroller ve çözümleme bilgileri
    /// ayrı bir sonuç modeliyle raporlanmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Semantic diagnostic listesini, başarı durumunu, symbol table bilgisini ve
    /// kaynak koddan toplanan symbol reference listesini taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// CUSTOMER_NO = MUST_NO;
    ///
    /// Bu input için SymbolTable iki symbol, References ise CUSTOMER_NO ve
    /// MUST_NO kullanımlarını taşır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer çıktısında ve ConversionService pipeline içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Undefined identifier politikası, reference navigation, structure member
    /// resolution ve type analysis sonuçlarına temel olur.
    /// </summary>
    public sealed class SemanticResult
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public SymbolTable SymbolTable { get; }

        public IReadOnlyList<Pl1SymbolReference> References { get; }

        public bool Success =>
            !Diagnostics.Any(
                diagnostic =>
                    diagnostic.Severity == DiagnosticSeverity.Error);

        public SemanticResult(
            IEnumerable<Diagnostic>? diagnostics = null,
            SymbolTable? symbolTable = null,
            IEnumerable<Pl1SymbolReference>? references = null)
        {
            Diagnostics = diagnostics?.ToList() ?? new List<Diagnostic>();
            SymbolTable = symbolTable ?? new SymbolTable();
            References = references?.ToList() ??
                new List<Pl1SymbolReference>();
        }
    }
}