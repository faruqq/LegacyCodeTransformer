using LegacyCodeTransformer.Core.Diagnostics;

namespace LegacyCodeTransformer.Core.Results
{
    /// <summary>
    /// Kaynak dilden hedef dile yapılan dönüşümün nihai sonucunu temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Dönüşüm pipeline'ı yalnızca hedef kod üretmez.
    /// Aynı zamanda hata, uyarı veya bilgilendirme mesajları da üretebilir.
    /// Bu sınıf, üretilen hedef kod ile Diagnostic listesini birlikte taşımak
    /// için oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application servislerinde
    /// - CLI çıktılarında
    /// - Gelecekte eklenecek GUI veya IDE entegrasyonlarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Dönüşüm sonucunun standart hale gelmesini sağlar.
    /// İleride farklı dönüşümler eklendiğinde aynı sonuç modeli kullanılabilir.
    ///
    /// Örneğin:
    /// - PL/I → EGL
    /// - PL/I → C#
    /// - EGL → C#
    /// </summary>
    public sealed class ConversionResult
    {
        /// <summary>
        /// Üretilen hedef kaynak koddur.
        /// Hata durumunda null olabilir.
        /// </summary>
        public string? Output { get; }

        /// <summary>
        /// Dönüşüm sırasında oluşan hata, uyarı veya bilgi mesajlarıdır.
        /// </summary>
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Dönüşümün başarılı olup olmadığını gösterir.
        /// Output varsa ve Error seviyesinde diagnostic yoksa true döner.
        /// </summary>
        public bool Success => Output is not null &&
                               !Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        public ConversionResult(
            string? output,
            IEnumerable<Diagnostic>? diagnostics = null)
        {
            Output = output;
            Diagnostics = diagnostics?.ToList() ?? new List<Diagnostic>();
        }
    }
}
