using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I FLOAT / REAL / DOUBLE veri tipi token akışını Pl1FloatingType modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// Pl1Parser sınıfı P04 sonunda çok sayıda veri tipi parse sorumluluğu taşımaya başladı.
/// FLOAT / REAL / DOUBLE parsing davranışını ayrı helper sınıfa almak, parser'ın büyümesini kontrol altında tutar.
///
/// Ne çözüyor?
/// ----------------------
/// FLOAT, FLOAT DECIMAL, FLOAT BINARY, REAL, DOUBLE ve DOUBLE PRECISION parsing davranışını Pl1Parser dışına taşır.
/// Böylece Pl1Parser yalnızca ana parse akışını yönetir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL RATE FLOAT;
/// - DCL RATE FLOAT DECIMAL;
/// - DCL RATE FLOAT DECIMAL(16);
/// - DCL RATE FLOAT BIN(53);
/// - DCL RATE REAL;
/// - DCL RATE DOUBLE;
/// - DCL RATE DOUBLE PRECISION;
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1Parser.ParseFloatingType methodu içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Floating precision validation, FLOAT DECIMAL mapping kararı ve P05 öncesi parser sorumluluk ayrımı bu sınıf üzerinde geliştirilebilir.
/// </summary>
internal sealed class FloatingTypeParser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics;
    private int _position;

    /// <summary>
    /// FLOAT / REAL / DOUBLE helper parser instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Helper parser'ın mevcut token listesini, başlangıç pozisyonunu ve diagnostic bag referansını bilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Pl1Parser'ın token okuma state'ini geçici olarak bu helper'a aktarır ve parse tamamlandığında yeni pozisyonu döndürmeyi mümkün kılar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Pl1Parser Current token olarak FLOAT, REAL veya DOUBLE gördüğünde bu helper aynı token listesinden devam eder.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseFloatingType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Diğer helper parser sınıfları da aynı token-position yaklaşımını kullanarak Pl1Parser içinden ayrıştırılabilir.
    /// </summary>
    public FloatingTypeParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
        _position = position;
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// FLOAT / REAL / DOUBLE token akışını parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I floating point tip ailesi tek keyword'den ibaret değildir. DOUBLE PRECISION gibi iki keyword'lü, FLOAT DECIMAL(16) gibi base ve precision taşıyan varyasyonlar vardır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Floating type syntax varyasyonlarını Pl1FloatingType modeline dönüştürür ve parse sonrası token pozisyonunu FloatingTypeParseResult ile geri döndürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FLOAT
    /// - FLOAT DECIMAL
    /// - FLOAT DECIMAL(16)
    /// - FLOAT BINARY
    /// - FLOAT BIN(53)
    /// - REAL
    /// - DOUBLE
    /// - DOUBLE PRECISION
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseFloatingType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Floating type validasyonları ve mapping öncesi semantic enrichment bu method üzerinde genişletilebilir.
    /// </summary>
    public FloatingTypeParseResult Parse()
    {
        var typeToken = Current;

        if (Current.Kind == Pl1TokenKind.RealKeyword)
        {
            Advance();

            return new FloatingTypeParseResult(
                new Pl1FloatingType(
                    Pl1FloatingTypeKind.Real,
                    Pl1FloatingBase.Unspecified,
                    null,
                    typeToken.Location),
                _position);
        }

        if (Current.Kind == Pl1TokenKind.DoubleKeyword)
        {
            Advance();

            if (Current.Kind == Pl1TokenKind.PrecisionKeyword)
            {
                Advance();
            }

            return new FloatingTypeParseResult(
                new Pl1FloatingType(
                    Pl1FloatingTypeKind.DoublePrecision,
                    Pl1FloatingBase.Unspecified,
                    null,
                    typeToken.Location),
                _position);
        }

        if (Current.Kind != Pl1TokenKind.FloatKeyword)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"FLOAT, REAL veya DOUBLE bekleniyordu. Gelen token: {Current.Text}",
                Current.Location));

            return new FloatingTypeParseResult(
                null,
                _position);
        }

        Advance();

        var floatingBase = Pl1FloatingBase.Unspecified;

        if (Current.Kind == Pl1TokenKind.DecimalKeyword ||
            Current.Kind == Pl1TokenKind.DecKeyword)
        {
            floatingBase = Pl1FloatingBase.Decimal;
            Advance();
        }
        else if (Current.Kind == Pl1TokenKind.BinaryKeyword ||
                 Current.Kind == Pl1TokenKind.BinKeyword)
        {
            floatingBase = Pl1FloatingBase.Binary;
            Advance();
        }

        var precision = ParseOptionalParenthesizedPrecision(
            "FLOAT precision değeri bekleniyordu.");

        return new FloatingTypeParseResult(
            new Pl1FloatingType(
                Pl1FloatingTypeKind.Float,
                floatingBase,
                precision,
                typeToken.Location),
            _position);
    }

    /// <summary>
    /// Opsiyonel parantez içi precision değerini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// FLOAT DECIMAL ve FLOAT BIN söz dizimleri precision bilgisini opsiyonel olarak parantez içinde taşıyabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// FLOAT DECIMAL(16) ve FLOAT BIN(53) gibi ifadelerdeki precision değerini merkezi şekilde okur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FLOAT DECIMAL => null
    /// - FLOAT DECIMAL(16) => 16
    /// - FLOAT BIN(53) => 53
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse methodu içinde FLOAT base bilgisi okunduktan sonra
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Floating precision limit validation eklendiğinde aynı helper üzerinde genişletilebilir.
    /// </summary>
    private int? ParseOptionalParenthesizedPrecision(string expectedNumberMessage)
    {
        if (Current.Kind != Pl1TokenKind.OpenParenthesis)
        {
            return null;
        }

        Advance();

        var precisionToken = Consume(
            Pl1TokenKind.Number,
            expectedNumberMessage);

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (precisionToken is null)
        {
            return null;
        }

        if (int.TryParse(precisionToken.Text, out var precision))
        {
            return precision;
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Precision değeri sayısal olmalıdır: {precisionToken.Text}",
            precisionToken.Location));

        return null;
    }

    /// <summary>
    /// Beklenen token türünü tüketir.
    ///
    /// Neden var?
    /// ----------------------
    /// Floating type syntax içinde parantez ve sayı gibi belirli token türleri beklenir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Beklenen token gelirse token'ı tüketir; gelmezse diagnostic üretir ve null döner.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// FLOAT BIN(53) içindeki Number ve CloseParenthesis token'larını doğrular.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseOptionalParenthesizedPrecision içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Helper parser token okuma standardının diğer helper sınıflarında da aynı yaklaşım ile uygulanmasına temel olur.
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
/// Floating type parse sonucunu ve parse sonrası token pozisyonunu taşır.
///
/// Neden var?
/// ----------------------
/// FloatingTypeParser ayrı token position state'i ile çalışır. Parse tamamlandığında Pl1Parser'ın kendi pozisyonunu güncellemesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Parse edilen Pl1FloatingType modeli ile parse sonrası yeni position değerini birlikte döndürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// FLOAT BIN(53) parse edildiğinde type modeliyle birlikte pozisyon semicolon token'ına ilerlemiş olur.
///
/// Nerede kullanılır?
/// ----------------------
/// - FloatingTypeParser.Parse dönüş değerinde
/// - Pl1Parser.ParseFloatingType içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Token-position state ile ayrıştırılacak diğer helper parser sınıfları için örnek sonuç modeli olur.
/// </summary>
internal sealed class FloatingTypeParseResult
{
    public Pl1FloatingType? DataType { get; }

    public int Position { get; }

    public FloatingTypeParseResult(
        Pl1FloatingType? dataType,
        int position)
    {
        DataType = dataType;
        Position = position;
    }
}