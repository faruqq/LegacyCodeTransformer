using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Normalization
{
    /// <summary>
    /// PL/I normalizasyon işleminin sonucunu temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Normalizer yalnızca Pl1SyntaxTree döndürmez.
    /// Aynı zamanda uyarı veya hata mesajları da üretebilir.
    /// Bu nedenle normalize edilmiş tree ile Diagnostic listesini birlikte taşımak için
    /// oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Normalizer çıktısında
    /// - Application pipeline içerisinde
    /// - Transpiler öncesi kontrol aşamasında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Normalizer daha aktif hale geldiğinde, yaptığı sadeleştirmeleri veya
    /// desteklenmeyen normalizasyon durumlarını Diagnostic olarak raporlayabilir.
    /// </summary>
    public sealed class Pl1NormalizationResult
    {
        /// <summary>
        /// Normalize edilmiş PL/I syntax tree modelidir.
        /// Hata durumunda null olabilir.
        /// </summary>
        public Pl1SyntaxTree? SyntaxTree { get; }

        /// <summary>
        /// Normalizasyon sırasında oluşan hata, uyarı veya bilgi mesajlarıdır.
        /// </summary>
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Normalizasyon işleminin başarılı olup olmadığını gösterir.
        /// </summary>
        public bool Success => SyntaxTree is not null &&
                               !Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        public Pl1NormalizationResult(
            Pl1SyntaxTree? syntaxTree,
            IEnumerable<Diagnostic>? diagnostics = null)
        {
            SyntaxTree = syntaxTree;
            Diagnostics = diagnostics?.ToList() ?? new List<Diagnostic>();
        }
    }
}
