using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types
{
    /// <summary>
    /// PL/I FIXED DECIMAL / FIXED DEC veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında decimal sayısal alanlar FIXED DECIMAL veya FIXED DEC
    /// söz dizimiyle tanımlanabilir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL AMOUNT FIXED DECIMAL(17,2);
    /// DCL COUNT FIXED DECIMAL(15);
    /// DCL RATE FIXED DEC(9,4);
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Decimal alanın precision ve varsa scale bilgisini syntax tree üzerinde
    /// kaybetmeden taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(15)
    /// - FIXED DECIMAL(15,0)
    /// - FIXED DECIMAL(17,2)
    /// - FIXED DEC(9,4)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser veri tipi parse işleminde
    /// - PL/I → EGL Transpiler decimal type mapping işleminde
    /// - Structure length hesabında
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DEC FIXED / DECIMAL FIXED synonym desteği ve numeric type mapping
    /// stratejisinin genişletilmesi için temel modeldir.
    /// </summary>
    public sealed class Pl1FixedDecimalType : Pl1DataType
    {
        /// <summary>
        /// Decimal alanın toplam digit sayısıdır.
        ///
        /// Örnek:
        ///
        /// FIXED DECIMAL(17,2)
        ///
        /// için Precision değeri 17 olur.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        /// Decimal alanın küsürat digit sayısıdır.
        ///
        /// Neden nullable?
        /// ----------------------
        /// PL/I tarafında FIXED DECIMAL(15) ile FIXED DECIMAL(15,0)
        /// aynı şey değildir. İlkinde scale hiç verilmemiştir, ikincisinde
        /// scale açıkça 0 verilmiştir.
        ///
        /// Bu ayrımı korumak için:
        /// - Scale null ise kaynakta scale yoktur.
        /// - Scale 0 ise kaynakta açıkça ,0 vardır.
        /// - Scale 2 ise kaynakta açıkça ,2 vardır.
        /// </summary>
        public int? Scale { get; }

        /// <summary>
        /// PL/I FIXED DECIMAL veri tipi modelini oluşturur.
        /// </summary>
        public Pl1FixedDecimalType(
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