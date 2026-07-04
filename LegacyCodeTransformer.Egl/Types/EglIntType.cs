using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Types
{
    /// <summary>
    /// EGL int veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I FIXED BIN(31) / BIN FIXED(31) gibi binary integer alanlar
    /// EGL tarafında int olarak üretilecektir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Generator'ın integer alanları standart casing ile `int` olarak
    /// yazdırmasını sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED BIN(31) => int
    /// - BIN FIXED(31) => int
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler binary type mapping işleminde
    /// - EGL Code Generator data type üretiminde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Numeric type mapping stratejisinin integer tarafındaki temel integer
    /// karşılığıdır.
    /// </summary>
    public sealed class EglIntType : EglDataType
    {
        public EglIntType(SourceLocation location)
            : base(location)
        {
        }
    }
}