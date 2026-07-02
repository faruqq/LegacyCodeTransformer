namespace LegacyCodeTransformer.Transpilers.Naming
{
    /// <summary>
    /// Identifier isim dönüşümünde kullanılacak naming style değerlerini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodunda değişken adları çoğunlukla büyük harfli ve alt çizgili
    /// formatta yazılır.
    ///
    /// Örnek PL/I:
    ///
    /// MUST_NO
    /// CUSTOMER_NO
    /// PROCESS_CODE
    ///
    /// EGL tarafında ise proje veya firma standardına göre farklı isimlendirme
    /// stilleri kullanılabilir.
    ///
    /// Bu enum, isim dönüşüm kuralını hardcoded olmaktan çıkarıp seçilebilir
    /// hale getirmek için oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - IdentifierNamingOptions içerisinde
    /// - IdentifierNameTransformer içerisinde
    /// - PL/I → EGL Transpiler declaration dönüşümünde
    /// - Application pipeline içerisinde opsiyonel dönüşüm ayarı olarak
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Farklı kurum standartları veya hedef dil kuralları desteklendiğinde
    /// yeni naming style değerleri bu enum'a eklenebilir.
    /// </summary>
    public enum IdentifierNamingStyle
    {
        /// <summary>
        /// Kaynak identifier adını değiştirmeden korur.
        ///
        /// Örnek:
        ///
        /// MUST_NO -> MUST_NO
        /// </summary>
        Preserve = 0,

        /// <summary>
        /// Identifier adını camelCase formatına dönüştürür.
        ///
        /// Örnek:
        ///
        /// MUST_NO -> mustNo
        /// CUSTOMER_NO -> customerNo
        /// </summary>
        CamelCase = 1,

        /// <summary>
        /// Identifier adını PascalCase formatına dönüştürür.
        ///
        /// Örnek:
        ///
        /// MUST_NO -> MustNo
        /// CUSTOMER_NO -> CustomerNo
        /// </summary>
        PascalCase = 2
    }
}