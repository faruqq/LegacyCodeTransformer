using LegacyCodeTransformer.Core.Diagnostics;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I semantic analysis sonucunu temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser syntax tree üretir; ancak semantic kontroller ve çözümleme
    /// bilgileri ayrı bir sonuç modeliyle raporlanmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Semantic diagnostic listesini, global symbol table bilgisini,
    /// kaynak koddan toplanan symbol reference listesini ve procedure
    /// parameter binding sonuçlarını taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL ERROR_TEXT CHAR(50);
    ///
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END CUSTOMER_PROCESS;
    ///
    /// Bu örnekte global ERROR_TEXT symbol table içinde, PROCESS_TEXT
    /// ise procedure parameter binding listesinde korunur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer çıktısında ve ConversionService pipeline
    /// içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL function parameter mapping, procedure scope, undefined
    /// parameter declaration diagnostic'i ve parameter direction
    /// analizine temel olur.
    /// </summary>
    public sealed class SemanticResult
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public SymbolTable SymbolTable { get; }

        public IReadOnlyList<Pl1SymbolReference> References { get; }

        public IReadOnlyList<Pl1ProcedureParameterBinding>
            ProcedureParameterBindings
        { get; }

        public bool Success =>
            !Diagnostics.Any(
                diagnostic =>
                    diagnostic.Severity == DiagnosticSeverity.Error);

        public SemanticResult(
            IEnumerable<Diagnostic>? diagnostics = null,
            SymbolTable? symbolTable = null,
            IEnumerable<Pl1SymbolReference>? references = null,
            IEnumerable<Pl1ProcedureParameterBinding>?
                procedureParameterBindings = null)
        {
            Diagnostics = diagnostics?.ToList() ??
                new List<Diagnostic>();

            SymbolTable = symbolTable ??
                new SymbolTable();

            References = references?.ToList() ??
                new List<Pl1SymbolReference>();

            ProcedureParameterBindings =
                procedureParameterBindings?.ToList() ??
                new List<Pl1ProcedureParameterBinding>();
        }
    }
}