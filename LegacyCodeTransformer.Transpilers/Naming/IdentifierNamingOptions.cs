namespace LegacyCodeTransformer.Transpilers.Naming
{
    /// <summary>
    /// Identifier isim dönüşümü için kullanılacak ayarları temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I → EGL dönüşümünde identifier isimleri her projede aynı casing
    /// kuralıyla üretilmeyebilir.
    ///
    /// Bazı projelerde PascalCase, bazı projelerde camelCase, bazı durumlarda
    /// ise kaynak adın aynen korunması istenebilir.
    ///
    /// Bu sınıf, Transpiler katmanına isim dönüşüm kuralını dışarıdan verme
    /// imkânı sağlamak için oluşturulmuştur.
    ///
    /// Varsayılan davranış:
    ///
    /// PascalCase
    ///
    /// olarak belirlenmiştir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1ToEglTranspiler constructor'ında
    /// - ConversionService overload'larında
    /// - Unit testlerde farklı naming style davranışlarını doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// CLI veya UI tarafında kullanıcıya naming style seçtirme ihtiyacı
    /// doğduğunda bu options modeli doğrudan kullanılabilecektir.
    /// </summary>
    public sealed class IdentifierNamingOptions
    {
        /// <summary>
        /// Varsayılan identifier naming ayarını temsil eder.
        ///
        /// Firma EGL kod standardında çoğunlukla PascalCase kullanıldığı için
        /// varsayılan değer PascalCase olarak belirlenmiştir.
        /// </summary>
        public static IdentifierNamingOptions Default { get; } = new(
            IdentifierNamingStyle.PascalCase);

        /// <summary>
        /// Uygulanacak identifier naming style değeridir.
        /// </summary>
        public IdentifierNamingStyle Style { get; }

        /// <summary>
        /// Identifier naming options modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Transpiler içinde hardcoded isim dönüşümü yapmak yerine, dönüşüm
        /// davranışını dışarıdan yönetilebilir hale getirmek için kullanılır.
        ///
        /// Örnek:
        ///
        /// new IdentifierNamingOptions(IdentifierNamingStyle.PascalCase)
        /// new IdentifierNamingOptions(IdentifierNamingStyle.CamelCase)
        /// new IdentifierNamingOptions(IdentifierNamingStyle.Preserve)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application pipeline içerisinde
        /// - PL/I → EGL Transpiler oluşturulurken
        /// - Unit testlerde strategy davranışını doğrularken
        /// </summary>
        public IdentifierNamingOptions(IdentifierNamingStyle style)
        {
            Style = style;
        }
    }
}