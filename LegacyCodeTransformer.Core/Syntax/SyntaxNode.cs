namespace LegacyCodeTransformer.Core.Syntax
{
    /// <summary>
    /// Syntax ağacındaki bütün node'lar için temel sınıftır.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I, EGL veya gelecekte desteklenecek diğer dillerdeki syntax öğelerinin
    /// ortak bir temel tip üzerinden temsil edilebilmesi için oluşturulmuştur.
    ///
    /// Örneğin:
    /// - Pl1VariableDeclaration
    /// - Pl1IfStatement
    /// - EglVariableDeclaration
    /// - EglIfStatement
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I syntax modellerinde
    /// - EGL syntax modellerinde
    /// - Transpiler katmanında
    /// - Diagnostic ve hata raporlama süreçlerinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İhtiyaç doğarsa bütün syntax node'lara ortak davranışlar buradan eklenebilir.
    /// Örneğin:
    /// - Child node dolaşımı
    /// - Annotation desteği
    /// - Source mapping desteği
    /// - Tree comparison/diff desteği
    ///
    /// Şimdilik yalnızca SourceLocation taşır.
    /// Bu bilinçli bir karardır; over-engineering'den kaçınmak için ekstra
    /// özellikler ihtiyaç doğmadan eklenmemiştir.
    /// </summary>
    public abstract class SyntaxNode
    {
        /// <summary>
        /// Bu node'un kaynak kod içerisindeki konum bilgisidir.
        /// </summary>
        public SourceLocation Location { get; }

        protected SyntaxNode(SourceLocation location)
        {
            Location = location ?? SourceLocation.Unknown;
        }
    }
}
