using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types;

/// <summary>
/// PL/I PIC / PICTURE veri tipini temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I tarafında PIC / PICTURE ifadeleri yalnızca numeric type değildir.
/// Aynı syntax numeric, alphanumeric, signed numeric veya formatted alanları
/// temsil edebilir.
///
/// Ne çözüyor?
/// ----------------------
/// PIC pattern bilgisini raw string olarak kaybetmeden taşır ve parser
/// aşamasında çıkarılan semantic classification bilgisini aynı model üzerinde
/// saklar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - PIC '999'
/// - PIC '999V99'
/// - PIC '(13)9V99'
/// - PIC 'XXX'
/// - PIC '(20)X'
/// - PIC 'AXXAA'
/// - PIC 'ZZ9'
///
/// Nerede kullanılır?
/// ----------------------
/// - PL/I Parser veri tipi parse işleminde
/// - PL/I → EGL type mapping işleminde
/// - Diagnostic üretiminde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Numeric PIC, alphanumeric PIC, signed PIC ve formatted PIC ayrımının
/// generator katmanlarına tekrar pattern parse ettirmeden yapılmasına temel olur.
/// </summary>
public sealed class Pl1PictureType : Pl1DataType
{
    public string RawPattern { get; }

    public Pl1PictureCategory Category { get; }

    public int? Precision { get; }

    public int? Scale { get; }

    public int? Length { get; }

    public bool IsSigned { get; }

    public bool IsNumeric { get; }

    public bool IsAlphanumeric { get; }

    public bool IsFormatted { get; }

    public bool SupportsDirectEglMapping { get; }

    public Pl1PictureType(
        string rawPattern,
        int? precision,
        int? scale,
        bool isSigned,
        bool isNumeric,
        bool isFormatted,
        SourceLocation location)
        : this(
            rawPattern,
            isFormatted
                ? Pl1PictureCategory.Formatted
                : isNumeric
                    ? Pl1PictureCategory.Numeric
                    : Pl1PictureCategory.Unknown,
            precision,
            scale,
            null,
            isSigned,
            isNumeric,
            false,
            isFormatted,
            isNumeric && !isFormatted,
            location)
    {
    }

    public Pl1PictureType(
        string rawPattern,
        Pl1PictureCategory category,
        int? precision,
        int? scale,
        int? length,
        bool isSigned,
        bool isNumeric,
        bool isAlphanumeric,
        bool isFormatted,
        bool supportsDirectEglMapping,
        SourceLocation location)
        : base(location)
    {
        RawPattern = rawPattern;
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
}