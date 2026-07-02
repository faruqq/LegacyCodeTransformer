namespace LegacyCodeTransformer.Core.Diagnostics
{
    /// <summary>
    /// Dönüştürme pipeline'ı sırasında oluşan Diagnostic mesajlarını toplar.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser, Normalizer, Transpiler ve Generator aşamalarında
    /// birden fazla hata veya uyarı oluşabilir.
    /// Bu mesajların tek bir yerde toplanması ve standart şekilde yönetilmesi
    /// için oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parser içerisinde hata toplamak için
    /// - Normalizer içerisinde uyarı toplamak için
    /// - Transpiler içerisinde desteklenmeyen yapıları raporlamak için
    /// - Application katmanında tüm pipeline çıktısını birleştirmek için
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İhtiyaç oluşursa diagnostic filtreleme, gruplama veya kategori bazlı
    /// raporlama gibi özellikler bu sınıf üzerinden eklenebilir.
    ///
    /// Şimdilik sade tutulmuştur.
    /// </summary>
    public sealed class DiagnosticBag
    {
        private readonly List<Diagnostic> _diagnostics = new();

        /// <summary>
        /// Toplanan Diagnostic mesajlarının salt okunur listesidir.
        /// </summary>
        public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

        /// <summary>
        /// Error seviyesinde en az bir Diagnostic olup olmadığını gösterir.
        /// </summary>
        public bool HasErrors => _diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        /// <summary>
        /// Warning seviyesinde en az bir Diagnostic olup olmadığını gösterir.
        /// </summary>
        public bool HasWarnings => _diagnostics.Any(x => x.Severity == DiagnosticSeverity.Warning);

        public void Add(Diagnostic diagnostic)
        {
            if (diagnostic is null)
            {
                return;
            }

            _diagnostics.Add(diagnostic);
        }

        public void AddError(string message)
        {
            Add(new Diagnostic(DiagnosticSeverity.Error, message));
        }

        public void AddWarning(string message)
        {
            Add(new Diagnostic(DiagnosticSeverity.Warning, message));
        }

        public void AddInfo(string message)
        {
            Add(new Diagnostic(DiagnosticSeverity.Info, message));
        }

        public void AddRange(IEnumerable<Diagnostic> diagnostics)
        {
            if (diagnostics is null)
            {
                return;
            }

            _diagnostics.AddRange(diagnostics.Where(x => x is not null));
        }
    }
}
