using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing;

/// <summary>
/// PL/I PIC / PICTURE pattern analiz sonucunu temsil eder.
///
/// Neden var?
/// ----------------------
/// PIC / PICTURE pattern analizi parser içinde doğrudan yapılırsa parser hem token okuma
/// hem de semantic çözümleme sorumluluğunu aynı anda taşır.
///
/// Ne çözüyor?
/// ----------------------
/// Pattern analizinden çıkan category, precision, scale, length, sign ve mapping desteği
/// bilgilerini tek modelde toplar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - PIC '999'      => Numeric, Precision 3
/// - PIC '999V99'   => Numeric, Precision 5, Scale 2
/// - PIC 'XXX'      => Alphanumeric, Length 3
/// - PIC '(20)X'    => Alphanumeric, Length 20
/// - PIC 'ZZ9'      => Formatted
/// - PIC 'S999'     => Numeric, IsSigned true
///
/// Nerede kullanılır?
/// ----------------------
/// PicturePatternAnalyzer tarafından üretilir ve Pl1Parser içinde Pl1PictureType
/// oluşturulurken kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Signed PIC, formatted PIC, alphanumeric PIC ve display-format metadata desteği
/// genişletildikçe parser yerine analyzer modelinin büyümesine temel olur.
/// </summary>
public sealed class PicturePatternAnalysis
{
    public PicturePatternAnalysis(
        Pl1PictureCategory category,
        int? precision,
        int? scale,
        int? length,
        bool isSigned,
        bool isNumeric,
        bool isAlphanumeric,
        bool isFormatted,
        bool supportsDirectEglMapping)
    {
        Category = category;
        Precision = precision;
        Scale = scale;
        Length = length;
        IsSigned = isSigned;
        IsNumeric = isNumeric;
        IsAlphanumeric = isAlphanumeric;
        IsFormatted = isFormatted;
        SupportsDirectEglMapping = supportsDirectEglMapping;
    }

    public Pl1PictureCategory Category { get; }

    public int? Precision { get; }

    public int? Scale { get; }

    public int? Length { get; }

    public bool IsSigned { get; }

    public bool IsNumeric { get; }

    public bool IsAlphanumeric { get; }

    public bool IsFormatted { get; }

    public bool SupportsDirectEglMapping { get; }
}