using LegacyCodeTransformer.Core.Diagnostics;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I semantic analysis sonucunu temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser syntax tree üretir; ancak semantic kontroller ayrı bir sonuç modeliyle
    /// raporlanmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PL/I semantic analyzer tarafından üretilen diagnostic listesini, başarı
    /// durumunu ve semantic symbol table bilgisini taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// Bu input için SemanticResult.SymbolTable içinde iki global declaration sembolü
    /// taşınır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer çıktısında ve ConversionService pipeline içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Duplicate declaration, undefined identifier ve reference analysis sonuçlarına
    /// temel olur.
    /// </summary>
    public sealed class SemanticResult
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public SymbolTable SymbolTable { get; }

        public bool Success => !Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        public SemanticResult(
            IEnumerable<Diagnostic>? diagnostics = null,
            SymbolTable? symbolTable = null)
        {
            Diagnostics = diagnostics?.ToList() ?? new List<Diagnostic>();
            SymbolTable = symbolTable ?? new SymbolTable();
        }
    }
}