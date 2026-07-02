using LegacyCodeTransformer.Core.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyCodeTransformer.Pl1.Types
{
    /// <summary>
    /// PL/I FIXED DECIMAL veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kodunda numerik alanlar sıklıkla FIXED DECIMAL ile tanımlanır.
    /// İlk parser hedefimiz olan DCL MUST_NO FIXED DECIMAL(8); ifadesini
    /// modelleyebilmek için bu sınıf gereklidir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// Bu modelde:
    /// - Precision: 8
    /// - Scale: 0
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - PL/I Normalizer içerisinde varsayılan scale belirlemede
    /// - PL/I → EGL Transpiler içerisinde decimal tip dönüşümünde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// FIXED DECIMAL(8,2) gibi scale içeren tanımlar desteklendiğinde
    /// Scale property’si doğrudan kullanılacaktır.
    /// </summary>
    public sealed class Pl1FixedDecimalType : Pl1DataType
    {
        /// <summary>
        /// Decimal alanın toplam basamak sayısıdır.
        /// Örneğin FIXED DECIMAL(8) için 8.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        /// Decimal alanın virgülden sonraki basamak sayısıdır.
        /// FIXED DECIMAL(8) için varsayılan değer 0 kabul edilir.
        /// </summary>
        public int Scale { get; }

        public Pl1FixedDecimalType(
            int precision,
            int scale,
            SourceLocation location)
            : base(location)
        {
            Precision = precision;
            Scale = scale;
        }
    }
}
