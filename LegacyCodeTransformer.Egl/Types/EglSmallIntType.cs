using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Types
{
    /// <summary>
    /// EGL smallint veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I FIXED BIN(15) / BIN FIXED(15) gibi binary integer alanlar
    /// EGL tarafında smallint olarak üretilecektir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Generator'ın küçük integer alanları standart casing ile `smallint`
    /// olarak yazdırmasını sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED BIN(15) => smallint
    /// - BIN FIXED(15) => smallint
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler binary type mapping işleminde
    /// - EGL Code Generator data type üretiminde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Numeric type mapping stratejisinin integer tarafındaki küçük integer
    /// karşılığıdır.
    /// </summary>
    public sealed class EglSmallIntType : EglDataType
    {
        public EglSmallIntType(SourceLocation location)
            : base(location)
        {
        }
    }
}