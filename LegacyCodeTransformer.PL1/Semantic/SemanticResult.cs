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
    /// PL/I semantic analyzer tarafından üretilen diagnostic listesini ve başarı
    /// durumunu taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Duplicate declaration veya undefined identifier gibi semantic hatalar ileride bu
    /// model üzerinden raporlanacaktır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer çıktısında ve ConversionService pipeline içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Symbol table, duplicate declaration ve reference analysis sonuçlarına temel olur.
    /// </summary>
    public sealed class SemanticResult
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public bool Success => !Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        public SemanticResult(IEnumerable<Diagnostic>? diagnostics = null)
        {
            Diagnostics = diagnostics?.ToList() ?? new List<Diagnostic>();
        }
    }
}