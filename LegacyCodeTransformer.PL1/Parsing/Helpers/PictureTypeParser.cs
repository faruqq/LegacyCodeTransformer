using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I PIC / PICTURE pattern bilgisinden Pl1PictureType modeli üretir.
///
/// Neden var?
/// ----------------------
/// Pl1Parser token akışını yönetmekten sorumludur. PIC / PICTURE pattern bilgisinin
/// semantic model haline getirilmesi ise ayrı bir sorumluluktur.
///
/// Ne çözüyor?
/// ----------------------
/// Pl1Parser içindeki PIC / PICTURE model oluşturma sorumluluğunu ayrıştırır.
/// Pattern semantic classification bilgisini PicturePatternAnalyzer üzerinden alır
/// ve Pl1PictureType modelini tek noktada oluşturur.
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
/// - PIC 'S999'
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1Parser.ParsePictureType methodu içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 öncesi parser sorumluluk ayrımının ilk adımıdır. İleride PIC / PICTURE
/// validation, formatted PIC metadata veya farklı PIC category davranışları
/// Pl1Parser büyütülmeden bu sınıfta geliştirilebilir.
/// </summary>
internal static class PictureTypeParser
{
    /// <summary>
    /// Raw PIC / PICTURE pattern değerinden Pl1PictureType modeli oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser string literal olarak okunan PIC pattern bilgisini syntax tree'de
    /// güçlü tipli Pl1PictureType modeline çevirmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PIC pattern analizini PicturePatternAnalyzer'a yaptırır ve analiz sonucunu
    /// Pl1PictureType constructor parametrelerine dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 999 => Numeric, Precision 3
    /// - 999V99 => Numeric, Precision 5, Scale 2
    /// - XXX => Alphanumeric, Length 3
    /// - ZZ9 => Formatted
    /// - S999 => Numeric, IsSigned true
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParsePictureType methodunda, PIC / PICTURE keyword ve pattern
    ///   token okunduktan sonra
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// PIC / PICTURE parsing davranışları genişledikçe Pl1Parser'a yeni helper
    /// method eklemek yerine bu sınıf genişletilecektir.
    /// </summary>
    public static Pl1PictureType Parse(
        string rawPattern,
        SourceLocation location)
    {
        var analysis = PicturePatternAnalyzer.Analyze(rawPattern);

        return new Pl1PictureType(
            rawPattern,
            analysis.Category,
            analysis.Precision,
            analysis.Scale,
            analysis.Length,
            analysis.IsSigned,
            analysis.IsNumeric,
            analysis.IsAlphanumeric,
            analysis.IsFormatted,
            analysis.SupportsDirectEglMapping,
            location);
    }
}