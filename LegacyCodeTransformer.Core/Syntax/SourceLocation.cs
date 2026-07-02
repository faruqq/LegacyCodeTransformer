namespace LegacyCodeTransformer.Core.Syntax
{

    /// <summary>
    /// Kaynak kod içerisindeki bir öğenin konumunu temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser, normalizer, transpiler ve generator aşamalarında oluşabilecek
    /// hata veya uyarıların orijinal kaynak koddaki yerini gösterebilmek için
    /// oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - SyntaxNode üzerinde
    /// - Parser hata mesajlarında
    /// - Diagnostic sisteminde
    /// - Desteklenmeyen sözdizimi raporlarında
    /// - Gelecekte eklenecek source mapping altyapısında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I kodundaki bir satırın üretilen EGL kodunda hangi satıra karşılık
    /// geldiğini göstermek için kullanılabilir.
    /// </summary>
    public sealed class SourceLocation
    {
        /// <summary>
        /// Kaynak dosyadaki satır numarası.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Kaynak dosyadaki sütun numarası.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Dosyanın başından itibaren karakter pozisyonu.
        /// </summary>
        public int Position { get; }

        public SourceLocation(int line, int column, int position)
        {
            Line = line;
            Column = column;
            Position = position;
        }

        /// <summary>
        /// Konum bilgisinin bilinmediği durumlarda kullanılacak varsayılan değer.
        /// </summary>
        public static SourceLocation Unknown { get; } = new(0, 0, 0);

        public override string ToString()
        {
            return Line <= 0 || Column <= 0
                ? "Bilinmeyen konum"
                : $"Satır {Line}, Sütun {Column}";
        }
    }
}
