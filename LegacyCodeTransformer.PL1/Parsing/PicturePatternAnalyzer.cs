using System;
using System.Linq;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing;

/// <summary>
/// PL/I PIC / PICTURE pattern değerini semantic olarak analiz eder.
///
/// Neden var?
/// ----------------------
/// Pl1Parser yalnızca token akışını syntax tree modeline dönüştürmelidir.
/// PIC pattern çözümleme, parser içinde kaldıkça parser sınıfı gereksiz büyür ve
/// signed / formatted / alphanumeric destekleri geldikçe bakım maliyeti artar.
///
/// Ne çözüyor?
/// ----------------------
/// PIC pattern classification davranışını parser dışına taşır.
/// Pattern'ın numeric, alphanumeric veya formatted olup olmadığını belirler.
/// Numeric pattern için precision / scale, alphanumeric pattern için length hesaplar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - 999       => Numeric, Precision 3
/// - 999V99    => Numeric, Precision 5, Scale 2
/// - (13)9V99  => Numeric, Precision 15, Scale 2
/// - XXX       => Alphanumeric, Length 3
/// - (20)X     => Alphanumeric, Length 20
/// - AXXAA     => Alphanumeric, Length 5
/// - ZZ9       => Formatted
/// - S999      => Numeric, IsSigned true
///
/// Nerede kullanılır?
/// ----------------------
/// Pl1Parser.CreatePictureTypeFromPattern methodu içinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P04-I3 signed PIC, P04-I4 formatted PIC ve ileride format metadata üretimi
/// bu analyzer üzerinde genişletilecektir.
/// </summary>
public static class PicturePatternAnalyzer
{
    /// <summary>
    /// Raw PIC / PICTURE pattern değerini analiz sonucuna dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser'ın PIC pattern string'ini doğrudan yorumlaması parser sınıfını
    /// semantic analiz sorumluluğuyla büyütür.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Pattern üzerinde sign, category, precision, scale, length, formatted
    /// ve doğrudan EGL mapping desteği bilgilerini tek yerde hesaplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 999       => Numeric, Precision 3, Length 3
    /// - 999V99    => Numeric, Precision 5, Scale 2, Length 5
    /// - (13)9V99  => Numeric, Precision 15, Scale 2, Length 15
    /// - XXX       => Alphanumeric, Length 3
    /// - (20)X     => Alphanumeric, Length 20
    /// - Z,ZZ9V.99 => Formatted, SupportsDirectEglMapping false
    /// - S999      => Numeric, IsSigned true
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1Parser.CreatePictureTypeFromPattern methodunda Pl1PictureType
    /// oluşturulmadan önce çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Signed PIC, formatted PIC, display metadata ve unsupported PIC diagnostic
    /// kurallarının merkezi karar noktası olur.
    /// </summary>
    public static PicturePatternAnalysis Analyze(string rawPattern)
    {
        var pattern = rawPattern.Trim();

        var isSigned = pattern.StartsWith("S", StringComparison.OrdinalIgnoreCase);
        if (isSigned)
        {
            pattern = pattern.Substring(1);
        }

        var isFormatted = ContainsFormattedPictureCharacters(pattern);
        var isNumeric = !isFormatted && IsNumericPicturePattern(pattern);
        var isAlphanumeric = !isFormatted &&
                             !isNumeric &&
                             IsAlphanumericPicturePattern(pattern);

        int? precision = null;
        int? scale = null;
        int? length = null;

        if (isNumeric)
        {
            precision = CalculatePicturePrecision(pattern);
            scale = CalculatePictureScale(pattern);
            length = precision;
        }
        else if (isAlphanumeric)
        {
            length = CalculateAlphanumericPictureLength(pattern);
        }

        var category = Pl1PictureCategory.Unknown;

        if (isFormatted)
        {
            category = Pl1PictureCategory.Formatted;
        }
        else if (isNumeric)
        {
            category = Pl1PictureCategory.Numeric;
        }
        else if (isAlphanumeric)
        {
            category = Pl1PictureCategory.Alphanumeric;
        }

        return new PicturePatternAnalysis(
            category,
            precision,
            scale,
            length,
            isSigned,
            isNumeric,
            isAlphanumeric,
            isFormatted,
            isNumeric || isAlphanumeric);
    }

    /// <summary>
    /// PIC pattern içinde formatted / edit mask karakteri olup olmadığını belirler.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I PIC / PICTURE syntax'ı yalnızca storage uzunluğu belirtmez; bazı
    /// karakterler görüntüleme formatı veya edit mask anlamı taşır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Safe numeric ve alphanumeric pattern'lar ile format maskesi içeren
    /// pattern'ları birbirinden ayırır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 999       => false
    /// - 999V99    => false
    /// - XXX       => false
    /// - ZZ9       => true
    /// - Z,ZZ9V.99 => true
    /// - +999      => true
    /// - -999      => true
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Analyze, IsNumericPicturePattern ve IsAlphanumericPicturePattern
    /// methodları içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Formatted PIC diagnostic, display format metadata ve format-preserving
    /// mapping davranışlarının ilk ayrım noktasıdır.
    /// </summary>
    private static bool ContainsFormattedPictureCharacters(string pattern)
    {
        return pattern.Any(x =>
            x == 'Z' ||
            x == 'z' ||
            x == ',' ||
            x == '.' ||
            x == '+' ||
            x == '-' ||
            x == '/' ||
            x == '$');
    }

    /// <summary>
    /// PIC pattern'in safe numeric subset içinde olup olmadığını belirler.
    ///
    /// Neden var?
    /// ----------------------
    /// Her PIC pattern numeric storage anlamına gelmez. Bazı pattern'lar
    /// karakter alanı, bazıları ise formatted display alanıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Yalnızca 9, V ve (n)9 tekrar syntax'ından oluşan güvenli numeric
    /// pattern'ları ayırır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 9
    /// - 999
    /// - 999V99
    /// - (13)9V99
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Analyze methodu içinde Category, Precision, Scale ve Length
    /// hesaplamasına karar verirken kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Signed numeric PIC, numeric diagnostic ve ileride numeric PIC limit
    /// validasyonlarının merkezi doğrulama noktası olur.
    /// </summary>
    private static bool IsNumericPicturePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        if (ContainsFormattedPictureCharacters(pattern))
        {
            return false;
        }

        var index = 0;

        while (index < pattern.Length)
        {
            var current = pattern[index];

            if (current == '9' || current == 'V' || current == 'v')
            {
                index++;
                continue;
            }

            if (current == '(')
            {
                var closeIndex = pattern.IndexOf(')', index + 1);
                if (closeIndex <= index + 1)
                {
                    return false;
                }

                var repeatText = pattern.Substring(
                    index + 1,
                    closeIndex - index - 1);

                if (!int.TryParse(repeatText, out _))
                {
                    return false;
                }

                index = closeIndex + 1;

                if (index >= pattern.Length || pattern[index] != '9')
                {
                    return false;
                }

                index++;
                continue;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// PIC pattern'in alphanumeric subset içinde olup olmadığını belirler.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I PIC / PICTURE yalnızca numeric alanları temsil etmez. X ve A
    /// karakterleriyle tanımlanan pattern'lar karakter tabanlı alanlardır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Numeric olmayan fakat karakter alanı olarak güvenli şekilde EGL char(n)
    /// tipine dönüştürülebilecek pattern'ları ayırır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - X
    /// - XXX
    /// - (20)X
    /// - A
    /// - (15)A
    /// - AXXAA
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Analyze methodu içinde Category ve Length hesaplamasına karar verirken
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Alphanumeric PIC → EGL char(n) mapping, mixed alphanumeric validation
    /// ve ileride karakter tabanlı PIC diagnostic kurallarına temel olur.
    /// </summary>
    private static bool IsAlphanumericPicturePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        if (ContainsFormattedPictureCharacters(pattern))
        {
            return false;
        }

        var index = 0;

        while (index < pattern.Length)
        {
            var current = pattern[index];

            if (current == 'X' ||
                current == 'x' ||
                current == 'A' ||
                current == 'a')
            {
                index++;
                continue;
            }

            if (current == '(')
            {
                var closeIndex = pattern.IndexOf(')', index + 1);
                if (closeIndex <= index + 1)
                {
                    return false;
                }

                var repeatText = pattern.Substring(
                    index + 1,
                    closeIndex - index - 1);

                if (!int.TryParse(repeatText, out _))
                {
                    return false;
                }

                index = closeIndex + 1;

                if (index >= pattern.Length)
                {
                    return false;
                }

                var repeatedSymbol = pattern[index];

                if (repeatedSymbol != 'X' &&
                    repeatedSymbol != 'x' &&
                    repeatedSymbol != 'A' &&
                    repeatedSymbol != 'a')
                {
                    return false;
                }

                index++;
                continue;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Numeric PIC pattern için toplam digit precision değerini hesaplar.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL num(p,s) üretimi ve numeric PIC metadata için toplam numeric digit
    /// sayısı bilinmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// 9 karakterlerini ve (n)9 repeat syntax'ını toplam precision değerine
    /// çevirir. V karakteri implied decimal point olduğu için precision'a
    /// eklenmez.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 999       => 3
    /// - 999V99    => 5
    /// - (13)9V99  => 15
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Analyze methodu içinde numeric PIC sınıflandırması yapıldıktan sonra
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Signed numeric PIC, packed/display numeric mapping ve numeric limit
    /// validasyonlarında ortak precision hesabı olarak kullanılabilir.
    /// </summary>
    private static int CalculatePicturePrecision(string pattern)
    {
        var precision = 0;
        var index = 0;

        while (index < pattern.Length)
        {
            var current = pattern[index];

            if (current == '9')
            {
                precision++;
                index++;
                continue;
            }

            if (current == 'V' || current == 'v')
            {
                index++;
                continue;
            }

            if (current == '(')
            {
                var closeIndex = pattern.IndexOf(')', index + 1);
                var repeatText = pattern.Substring(
                    index + 1,
                    closeIndex - index - 1);

                precision += int.Parse(repeatText);
                index = closeIndex + 2;
                continue;
            }

            index++;
        }

        return precision;
    }

    /// <summary>
    /// Numeric PIC pattern için implied decimal scale değerini hesaplar.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I PIC pattern içinde V karakteri gerçek nokta karakteri değildir;
    /// implied decimal point bilgisidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// V sonrasında kalan numeric digit sayısını scale değerine çevirir.
    /// V yoksa scale bilgisini null bırakır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 999       => null
    /// - 999V99    => 2
    /// - (13)9V99  => 2
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Analyze methodu içinde numeric PIC sınıflandırması yapıldıktan sonra
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL num(p,s), ileride COBOL PIC mapping veya C# decimal metadata
    /// üretiminde ortak scale hesabı olarak kullanılabilir.
    /// </summary>
    private static int? CalculatePictureScale(string pattern)
    {
        var vIndex = pattern.IndexOf('V');
        if (vIndex < 0)
        {
            vIndex = pattern.IndexOf('v');
        }

        if (vIndex < 0)
        {
            return null;
        }

        var scalePattern = pattern.Substring(vIndex + 1);

        return CalculatePicturePrecision(scalePattern);
    }

    /// <summary>
    /// Alphanumeric PIC pattern için toplam karakter uzunluğunu hesaplar.
    ///
    /// Neden var?
    /// ----------------------
    /// Alphanumeric PIC pattern'lar EGL tarafında char(n) olarak üretilecektir.
    /// Bunun için pattern'ın kaç karakterlik alan ifade ettiği bilinmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// X, A ve (n)X / (n)A repeat syntax'ını toplam character length
    /// değerine çevirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - X       => 1
    /// - XXX     => 3
    /// - (20)X   => 20
    /// - AXXAA   => 5
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Analyze methodu içinde alphanumeric PIC sınıflandırması yapıldıktan sonra
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// PIC 'X' / PIC 'A' tabanlı character mapping, field length hesabı ve
    /// structure total length hesaplamasına temel olur.
    /// </summary>
    private static int CalculateAlphanumericPictureLength(string pattern)
    {
        var length = 0;
        var index = 0;

        while (index < pattern.Length)
        {
            var current = pattern[index];

            if (current == 'X' ||
                current == 'x' ||
                current == 'A' ||
                current == 'a')
            {
                length++;
                index++;
                continue;
            }

            if (current == '(')
            {
                var closeIndex = pattern.IndexOf(')', index + 1);
                var repeatText = pattern.Substring(
                    index + 1,
                    closeIndex - index - 1);

                length += int.Parse(repeatText);
                index = closeIndex + 2;
                continue;
            }

            index++;
        }

        return length;
    }
}