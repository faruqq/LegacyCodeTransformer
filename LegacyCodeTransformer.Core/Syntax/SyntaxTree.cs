namespace LegacyCodeTransformer.Core.Syntax
{
    /// <summary>
    /// Bir dile ait syntax tree yapıları için ortak temel sınıftır.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I, EGL veya gelecekte desteklenecek diğer dillerin tamamı
    /// kendi SyntaxTree modeline sahip olacaktır.
    ///
    /// Örneğin:
    /// - Pl1SyntaxTree
    /// - EglSyntaxTree
    /// - CSharpSyntaxTree
    ///
    /// Bu sınıf, bu farklı dil modelleri için ortak bir temel sağlar.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parser çıktılarında
    /// - Normalizer giriş ve çıkışlarında
    /// - Transpiler giriş ve çıkışlarında
    /// - Generator girişlerinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İhtiyaç doğarsa bütün syntax tree modellerine ortak özellikler
    /// buradan eklenebilir.
    ///
    /// Örneğin:
    /// - Dosya yolu bilgisi
    /// - Kaynak metin bilgisi
    /// - Dil bilgisi
    /// - Diagnostic bilgileri
    /// - Root node yapısı
    ///
    /// Şimdilik yalnızca SourceLocation taşır.
    /// Bu bilinçli bir karardır; over-engineering'den kaçınmak için
    /// ekstra özellikler ihtiyaç doğmadan eklenmemiştir.
    /// </summary>
    public abstract class SyntaxTree
    {
        /// <summary>
        /// Syntax tree'nin kaynak kod içerisindeki başlangıç konumudur.
        /// Genellikle dosya başlangıcını temsil eder.
        /// </summary>
        public SourceLocation Location { get; }

        protected SyntaxTree(SourceLocation location)
        {
            Location = location ?? SourceLocation.Unknown;
        }
    }
}
