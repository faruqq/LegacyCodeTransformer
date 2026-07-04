using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types
{
    /// <summary>
    /// PL/I FIXED BINARY / FIXED BIN veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında binary integer alanlar FIXED BINARY, FIXED BIN,
    /// BINARY FIXED veya BIN FIXED söz dizimleriyle tanımlanabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Binary numeric alanın precision ve varsa scale bilgisini syntax tree
    /// üzerinde kaybetmeden taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED BIN(15)
    /// - BIN FIXED(15)
    /// - FIXED BINARY(31)
    /// - BINARY FIXED(31)
    /// - FIXED BIN(15,0)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser veri tipi parse işleminde
    /// - PL/I → EGL Transpiler binary integer mapping işleminde
    /// - Structure length hesabında
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Binary fractional alanlar, farklı precision değerleri ve diagnostic
    /// kuralları için temel modeldir.
    /// </summary>
    public sealed class Pl1FixedBinaryType : Pl1DataType
    {
        public int Precision { get; }

        public int? Scale { get; }

        public Pl1FixedBinaryType(
            int precision,
            int? scale,
            SourceLocation location)
            : base(location)
        {
            Precision = precision;
            Scale = scale;
        }
    }
}