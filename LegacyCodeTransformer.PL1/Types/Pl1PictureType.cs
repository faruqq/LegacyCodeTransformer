using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types
{
    /// <summary>
    /// PL/I PIC / PICTURE veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında PIC / PICTURE ifadeleri yalnızca numeric type değil,
    /// digit layout, sign, implied decimal ve format bilgisi de taşıyabilir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL PARAM1 PIC '999';
    /// DCL PARAM2 PIC '999V99';
    /// DCL PARAM3 PIC 'S999';
    /// DCL PARAM4 PIC 'ZZ9';
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PIC pattern bilgisini kaybetmeden syntax tree üzerinde ayrı bir model
    /// olarak taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - PIC '999'
    /// - PIC '999V99'
    /// - PIC 'S999'
    /// - PIC '(13)9V99'
    /// - PIC 'ZZ9'
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser veri tipi parse işleminde
    /// - İleride PL/I → EGL PIC mapping işleminde
    /// - Diagnostic üretiminde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Numeric PIC pattern'lerinin EGL num(p,s) mapping işlemine,
    /// formatted PIC pattern'lerinin diagnostic veya özel mapping kararlarına
    /// temel olur.
    /// </summary>
    public sealed class Pl1PictureType : Pl1DataType
    {
        public string RawPattern { get; }

        public int? Precision { get; }

        public int? Scale { get; }

        public bool IsSigned { get; }

        public bool IsNumeric { get; }

        public bool IsFormatted { get; }

        public Pl1PictureType(
            string rawPattern,
            int? precision,
            int? scale,
            bool isSigned,
            bool isNumeric,
            bool isFormatted,
            SourceLocation location)
            : base(location)
        {
            RawPattern = rawPattern;
            Precision = precision;
            Scale = scale;
            IsSigned = isSigned;
            IsNumeric = isNumeric;
            IsFormatted = isFormatted;
        }
    }
}