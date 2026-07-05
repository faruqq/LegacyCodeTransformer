using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I array / dimension syntax bilgisini parse eder.
///
/// Neden var?
/// ----------------------
/// Array size bilgisi PL/I declaration içinde farklı yerlerde yazılabilir.
/// Bu parsing davranışı Pl1Parser içinde kaldığında ana parser dimension detaylarıyla büyür.
///
/// Ne çözüyor?
/// ----------------------
/// Name-based array size, DIM / DIMENSION attribute size ve iki kaynak arasındaki
/// conflict resolve davranışını Pl1Parser dışına taşır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - PARAM(2) CHAR(10)
/// - PARAM CHAR(10) DIM(2)
/// - PARAM CHAR(10) DIMENSION(2)
/// - PARAM(2) CHAR(10) DIM(3)
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1Parser.ParseOptionalArraySize içinde
/// - Pl1Parser.ParseOptionalDimensionSize içinde
/// - Pl1Parser.ResolveArraySize içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Çok boyutlu DIMENSION, lower-bound / upper-bound syntax ve array metadata
/// davranışları bu helper üzerinde geliştirilebilir.
/// </summary>
internal sealed class DimensionParser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics;
    private int _position;

    /// <summary>
    /// Dimension helper parser instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Helper parser'ın mevcut token listesini, başlangıç pozisyonunu ve diagnostic bag
    /// referansını bilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Pl1Parser token state bilgisini dimension parser'a aktarır ve parse sonrası
    /// yeni pozisyonun geri taşınmasını sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Current token OpenParenthesis ise name-based array size; Current token DIM veya
    /// DIMENSION ise attribute-based array size parse eder.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser dimension wrapper methodları içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Diğer attribute parser helper'larıyla aynı token-position yaklaşımını korur.
    /// </summary>
    public DimensionParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
        _position = position;
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// İsim sonrasında gelen opsiyonel array size bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I declaration adından hemen sonra parantez içinde array size verilebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PARAM(2) veya DCL 1 DIZI(6) gibi name-based array size bilgisini int değerine dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - PARAM(2)
    /// - DIZI(6)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseOptionalArraySize içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu name-based array syntax desteklendiğinde bu method genişletilecektir.
    /// </summary>
    public DimensionParseResult ParseOptionalArraySize()
    {
        if (Current.Kind != Pl1TokenKind.OpenParenthesis)
        {
            return new DimensionParseResult(
                null,
                _position);
        }

        Advance();

        var sizeToken = Consume(
            Pl1TokenKind.Number,
            "Array boyutu bekleniyordu.");

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (sizeToken is null)
        {
            return new DimensionParseResult(
                null,
                _position);
        }

        if (int.TryParse(sizeToken.Text, out var arraySize))
        {
            return new DimensionParseResult(
                arraySize,
                _position);
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Array boyutu sayısal olmalıdır: {sizeToken.Text}",
            sizeToken.Location));

        return new DimensionParseResult(
            null,
            _position);
    }

    /// <summary>
    /// DIM / DIMENSION attribute ile verilen opsiyonel array size bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I array declaration bilgisi veri tipi sonrasında DIM veya DIMENSION attribute
    /// ile de verilebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DIM(n) ve DIMENSION(n) syntax bilgisini array size değerine dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DIM(2)
    /// - DIMENSION(2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseOptionalDimensionSize içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu DIMENSION ve range syntax desteği bu method üzerinde genişletilebilir.
    /// </summary>
    public DimensionParseResult ParseOptionalDimensionSize()
    {
        if (Current.Kind != Pl1TokenKind.DimKeyword &&
            Current.Kind != Pl1TokenKind.DimensionKeyword)
        {
            return new DimensionParseResult(
                null,
                _position);
        }

        var dimensionToken = Current;
        Advance();

        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        var sizeToken = Consume(
            Pl1TokenKind.Number,
            "DIM / DIMENSION boyutu bekleniyordu.");

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (sizeToken is null)
        {
            return new DimensionParseResult(
                null,
                _position);
        }

        if (int.TryParse(sizeToken.Text, out var arraySize))
        {
            return new DimensionParseResult(
                arraySize,
                _position);
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"DIM / DIMENSION boyutu sayısal olmalıdır: {sizeToken.Text}",
            dimensionToken.Location));

        return new DimensionParseResult(
            null,
            _position);
    }

    /// <summary>
    /// Name-based array size ile DIM / DIMENSION size bilgisini tek değere indirger.
    ///
    /// Neden var?
    /// ----------------------
    /// Aynı declaration içinde hem PARAM(2) hem de DIM(3) verilirse iki farklı array
    /// size kaynağı oluşur.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Tek array size değerini seçer. İki kaynak da doluysa diagnostic üretir ve
    /// name-based size değerini öncelikli kabul eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - PARAM(2) CHAR(10) => 2
    /// - PARAM CHAR(10) DIM(2) => 2
    /// - PARAM(2) CHAR(10) DIM(3) => diagnostic + 2
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ResolveArraySize içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu dimension veya array range desteği geldiğinde array metadata merge
    /// davranışının merkezi noktası olur.
    /// </summary>
    public int? ResolveArraySize(
        int? nameArraySize,
        int? dimensionArraySize,
        SourceLocation location)
    {
        if (nameArraySize.HasValue && dimensionArraySize.HasValue)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                "Array boyutu hem isim sonrasında hem de DIM / DIMENSION attribute ile verilemez.",
                location));

            return nameArraySize;
        }

        return nameArraySize ?? dimensionArraySize;
    }

    /// <summary>
    /// Beklenen token türünü tüketir.
    ///
    /// Neden var?
    /// ----------------------
    /// Dimension syntax içinde parantez ve sayı token'ları belirli sırada beklenir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Beklenen token gelirse token'ı tüketir; gelmezse diagnostic üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PARAM(2) ve DIM(2) içindeki parantez / sayı token'larını doğrular.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseOptionalArraySize içinde
    /// - ParseOptionalDimensionSize içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Dimension helper içinde token okuma davranışını tutarlı tutar.
    /// </summary>
    private Pl1Token? Consume(
        Pl1TokenKind expectedKind,
        string errorMessage)
    {
        if (Current.Kind == expectedKind)
        {
            return Advance();
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            errorMessage,
            Current.Location));

        return null;
    }

    /// <summary>
    /// Mevcut token'ı tüketip bir sonraki token'a ilerler.
    /// </summary>
    private Pl1Token Advance()
    {
        if (!IsAtEnd())
        {
            _position++;
        }

        return Previous;
    }

    /// <summary>
    /// Helper parser'ın kaynak sonu token'ına gelip gelmediğini belirtir.
    /// </summary>
    private bool IsAtEnd()
    {
        return Current.Kind == Pl1TokenKind.EndOfFile;
    }

    /// <summary>
    /// Mevcut helper parser pozisyonundaki token'ı döndürür.
    /// </summary>
    private Pl1Token Current
    {
        get
        {
            if (_position >= _tokens.Count)
            {
                return _tokens[^1];
            }

            return _tokens[_position];
        }
    }

    /// <summary>
    /// Bir önce tüketilen token'ı döndürür.
    /// </summary>
    private Pl1Token Previous => _tokens[_position - 1];
}

/// <summary>
/// Dimension parse sonucunu ve parse sonrası token pozisyonunu taşır.
///
/// Neden var?
/// ----------------------
/// DimensionParser ayrı token position state'i ile çalışır. Parse tamamlandığında
/// Pl1Parser'ın kendi pozisyonunu güncellemesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Parse edilen array size değeri ile parse sonrası position değerini birlikte döndürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// DIM(2) parse edildiğinde array size 2 ve yeni token pozisyonu birlikte taşınır.
///
/// Nerede kullanılır?
/// ----------------------
/// - DimensionParser parse methodlarının dönüş değerinde
/// - Pl1Parser dimension wrapper methodlarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Çok boyutlu dimension desteğinde result modeli array metadata taşıyacak şekilde
/// genişletilebilir.
/// </summary>
internal sealed class DimensionParseResult
{
    public int? ArraySize { get; }

    public int Position { get; }

    public DimensionParseResult(
        int? arraySize,
        int position)
    {
        ArraySize = arraySize;
        Position = position;
    }
}