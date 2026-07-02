using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Lexing
{
    /// <summary>
    /// PL/I kaynak kodundan lexer tarafından üretilen tek bir token'ı temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser'ın ham metin yerine anlamlı token'lar üzerinden çalışması için
    /// oluşturulmuştur.
    ///
    /// Örneğin:
    ///
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// ifadesi şu token'lara ayrılır:
    /// - DclKeyword
    /// - Identifier
    /// - FixedKeyword
    /// - DecimalKeyword
    /// - OpenParenthesis
    /// - Number
    /// - CloseParenthesis
    /// - Semicolon
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Lexer çıktısında
    /// - PL/I Parser girişinde
    /// - Parser hata mesajlarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Token seviyesi hata raporlama, syntax highlighting veya IDE entegrasyonu
    /// gibi alanlarda kullanılabilir.
    /// </summary>
    public sealed class Pl1Token
    {
        /// <summary>
        /// Token türüdür.
        /// </summary>
        public Pl1TokenKind Kind { get; }

        /// <summary>
        /// Token'ın kaynak kod içerisindeki orijinal metnidir.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Token'ın kaynak kod içerisindeki konumudur.
        /// </summary>
        public SourceLocation Location { get; }

        public Pl1Token(
            Pl1TokenKind kind,
            string text,
            SourceLocation location)
        {
            Kind = kind;
            Text = text;
            Location = location ?? SourceLocation.Unknown;
        }

        public override string ToString()
        {
            return $"{Kind}: {Text} ({Location})";
        }
    }
}
