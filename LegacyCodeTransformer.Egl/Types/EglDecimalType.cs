
using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Types
{
    /// <summary>
    /// EGL decimal veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I FIXED DECIMAL tipi, ilk dönüşüm hedefimizde EGL decimal tipine
    /// karşılık gelecektir.
    ///
    /// Örnek EGL:
    ///
    /// decimal(8,0)
    ///
    /// Bu modelde:
    /// - Precision: 8
    /// - Scale: 0
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler içerisinde
    /// - EGL VariableDeclaration üzerinde
    /// - EGL Code Generator içerisinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// FIXED DECIMAL(8,2) gibi PL/I tipleri desteklendiğinde,
    /// precision ve scale bilgisi doğrudan EGL decimal üretiminde kullanılacaktır.
    /// </summary>
    public sealed class EglDecimalType : EglDataType
    {
        /// <summary>
        /// Decimal alanın toplam basamak sayısıdır.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        /// Decimal alanın virgülden sonraki basamak sayısıdır.
        /// </summary>
        public int Scale { get; }

        public EglDecimalType(
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
