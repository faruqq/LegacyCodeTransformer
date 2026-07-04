using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Types
{
    /// <summary>
    /// EGL decimal veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I decimal numeric alanlar EGL tarafında decimal type olarak
    /// üretilecektir.
    ///
    /// Örnek EGL:
    ///
    /// decimal(15)
    /// decimal(15,0)
    /// decimal(17,2)
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EGL decimal type için precision ve optional scale bilgisini taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - decimal(15)
    /// - decimal(15,0)
    /// - decimal(17,2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Transpiler PL/I decimal type mapping işleminde
    /// - EGL Code Generator data type üretiminde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// NUM, PIC ve farklı decimal formatting kararları geldiğinde decimal
    /// output davranışının merkezi modelidir.
    /// </summary>
    public sealed class EglDecimalType : EglDataType
    {
        /// <summary>
        /// Decimal alanın toplam digit sayısıdır.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        /// Decimal alanın optional scale bilgisidir.
        ///
        /// Scale null ise generator decimal(p) üretir.
        ///
        /// Scale null değilse generator decimal(p,s) üretir.
        /// </summary>
        public int? Scale { get; }

        /// <summary>
        /// EGL decimal veri tipi modelini oluşturur.
        /// </summary>
        public EglDecimalType(
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