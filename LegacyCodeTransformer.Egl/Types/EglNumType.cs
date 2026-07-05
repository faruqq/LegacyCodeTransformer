using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Types
{
    /// <summary>
    /// EGL num veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I numeric PIC / PICTURE pattern'leri EGL tarafında num(p) veya
    /// num(p,s) olarak üretilecektir.
    ///
    /// Örnek EGL:
    ///
    /// num(3)
    /// num(5,2)
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Picture numeric layout bilgisinden elde edilen precision ve optional
    /// scale bilgisini EGL modelinde taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - PIC '999' => num(3)
    /// - PIC '999V99' => num(5,2)
    /// - PIC '(13)9V99' => num(15,2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler PIC mapping işleminde
    /// - EGL Code Generator data type üretiminde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Formatted PIC mapping, signed numeric metadata ve farklı numeric
    /// layout kararları için temel EGL numeric modelidir.
    /// </summary>
    public sealed class EglNumType : EglDataType
    {
        public int Precision { get; }

        public int? Scale { get; }

        public EglNumType(
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