namespace LegacyCodeTransformer.Core.Diagnostics
{
    /// <summary>
    /// Diagnostic mesajlarının önem seviyesini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser, normalizer, transpiler ve generator aşamalarında oluşan
    /// mesajların standart bir önem seviyesiyle ifade edilmesi için oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Diagnostic sınıfında
    /// - DiagnosticBag içerisinde
    /// - ParseResult ve ConversionResult başarı kontrolünde
    /// - CLI çıktılarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İleride daha detaylı raporlama yapılmak istenirse yeni seviyeler eklenebilir.
    ///
    /// Örneğin:
    /// - Critical
    /// - Debug
    /// - Trace
    ///
    /// Ancak ilk sürümde yalnızca Info, Warning ve Error yeterli görülmüştür.
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>
        /// Bilgilendirme amaçlı mesajdır.
        /// Dönüşüm sürecini durdurmaz.
        /// </summary>
        Info = 1,

        /// <summary>
        /// Dikkat edilmesi gereken fakat süreci durdurmayan durumdur.
        /// Örneğin desteklenmeyen ama yorum olarak taşınan bir ifade.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Süreci başarısız hale getiren hatadır.
        /// Parser hataları veya dönüştürülemeyen kritik yapılar bu seviyede olabilir.
        /// </summary>
        Error = 3
    }
}
