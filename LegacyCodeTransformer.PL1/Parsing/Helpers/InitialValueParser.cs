using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I INIT / INITIAL başlangıç değeri token akışını Pl1InitialValue modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// INIT / INITIAL parsing davranışı Pl1Parser içinde kaldığında ana parser hem declaration
/// akışını hem de initialization syntax detaylarını yönetmek zorunda kalır.
///
/// Ne çözüyor?
/// ----------------------
/// INIT, INITIAL, repeat factor ve (*) all-elements initialization parsing sorumluluğunu
/// Pl1Parser dışına taşır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - INIT(' ')
/// - INITIAL('ABCD')
/// - INIT((08)' ')
/// - INIT((*)' ')
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1Parser.ParseOptionalInitialValue içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Array initialization, repeat factor expansion ve record field default value mapping
/// davranışları bu helper üzerinde geliştirilebilir.
/// </summary>
internal sealed class InitialValueParser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics;
    private int _position;

    /// <summary>
    /// INIT / INITIAL helper parser instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Helper parser'ın mevcut token listesini, başlangıç pozisyonunu ve diagnostic bag
    /// referansını bilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Pl1Parser token state bilgisini helper'a aktarır. Helper parse tamamlandığında
    /// yeni token pozisyonunu result modeliyle geri döndürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Current token INIT veya INITIAL olduğunda aynı token akışından initialization
    /// bilgisini parse eder.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseOptionalInitialValue içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Diğer optional attribute parser helper'larıyla aynı token-position yaklaşımını
    /// korur.
    /// </summary>
    public InitialValueParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
        _position = position;
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// Opsiyonel INIT / INITIAL başlangıç değerini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I declaration içinde başlangıç değeri zorunlu değildir. Varsa syntax tree
    /// üzerinde Pl1InitialValue olarak korunmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// INIT / INITIAL yoksa null döner. Varsa string literal başlangıç değerini,
    /// repeat count bilgisini ve (*) all-elements bilgisini tek modelde toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - INIT(' ')
    /// - INITIAL(';')
    /// - INIT((08)' ')
    /// - INIT((*)' ')
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseOptionalInitialValue içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL default value üretimi, repeat factor expansion ve array initialization
    /// davranışları bu model üzerinden geliştirilebilir.
    /// </summary>
    public InitialValueParseResult ParseOptionalInitialValue()
    {
        if (Current.Kind != Pl1TokenKind.InitKeyword &&
            Current.Kind != Pl1TokenKind.InitialKeyword)
        {
            return new InitialValueParseResult(
                null,
                _position);
        }

        var initToken = Current;

        if (Current.Kind == Pl1TokenKind.InitKeyword)
        {
            Consume(
                Pl1TokenKind.InitKeyword,
                "INIT bekleniyordu.");
        }
        else
        {
            Consume(
                Pl1TokenKind.InitialKeyword,
                "INITIAL bekleniyordu.");
        }

        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        var repeatInfo = ParseOptionalInitialRepeatFactor();

        var valueToken = Consume(
            Pl1TokenKind.StringLiteral,
            "Başlangıç değeri için karakter sabiti bekleniyordu.");

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (valueToken is null)
        {
            return new InitialValueParseResult(
                null,
                _position);
        }

        return new InitialValueParseResult(
            new Pl1InitialValue(
                valueToken.Text,
                repeatInfo.RepeatCount,
                repeatInfo.AppliesToAllElements,
                initToken.Location),
            _position);
    }

    /// <summary>
    /// INIT / INITIAL içindeki opsiyonel repeat factor bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I başlangıç değeri syntax'ı aynı değeri belirli sayıda tekrarlama veya tüm
    /// array elemanlarına uygulama bilgisini taşıyabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// İç içe parantezle verilen repeat count veya (*) all-elements bilgisini ayrıştırır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - INIT((08)' ') => RepeatCount 8
    /// - INIT((*)' ') => AppliesToAllElements true
    /// - INIT(' ') => RepeatInfo.None
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseOptionalInitialValue içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Repeat factor expansion ve array initialization mapping davranışları burada
    /// genişletilebilir.
    /// </summary>
    private InitialRepeatInfo ParseOptionalInitialRepeatFactor()
    {
        if (Current.Kind != Pl1TokenKind.OpenParenthesis)
        {
            return InitialRepeatInfo.None;
        }

        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        int? repeatCount = null;
        var appliesToAllElements = false;

        if (Current.Kind == Pl1TokenKind.Number)
        {
            var repeatToken = Consume(
                Pl1TokenKind.Number,
                "Tekrar sayısı bekleniyordu.");

            if (repeatToken is not null &&
                int.TryParse(repeatToken.Text, out var parsedRepeatCount))
            {
                repeatCount = parsedRepeatCount;
            }
        }
        else if (Current.Kind == Pl1TokenKind.Asterisk)
        {
            Consume(
                Pl1TokenKind.Asterisk,
                "'*' bekleniyordu.");

            appliesToAllElements = true;
        }
        else
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"INIT tekrar faktörü için sayı veya '*' bekleniyordu. Gelen token: {Current.Text}",
                Current.Location));
        }

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        return new InitialRepeatInfo(
            repeatCount,
            appliesToAllElements);
    }

    /// <summary>
    /// Beklenen token türünü tüketir.
    ///
    /// Neden var?
    /// ----------------------
    /// INIT / INITIAL grammar içinde belirli noktalarda belirli token türleri beklenir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Beklenen token gelirse token'ı tüketir; beklenen token gelmezse diagnostic üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// INIT((08)' ') içinde parantez, sayı ve string literal token'larını doğrular.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseOptionalInitialValue içinde
    /// - ParseOptionalInitialRepeatFactor içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Helper parser token okuma standardının bu sınıf içinde tutarlı kalmasını sağlar.
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
/// INIT / INITIAL parse sonucunu ve parse sonrası token pozisyonunu taşır.
///
/// Neden var?
/// ----------------------
/// InitialValueParser ayrı token position state'i ile çalışır. Parse tamamlandığında
/// Pl1Parser'ın kendi pozisyonunu güncellemesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Parse edilen Pl1InitialValue modeli ile parse sonrası position değerini birlikte döndürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// INIT((08)' ') parse edildiğinde value, repeat count ve yeni pozisyon birlikte taşınır.
///
/// Nerede kullanılır?
/// ----------------------
/// - InitialValueParser.ParseOptionalInitialValue dönüş değerinde
/// - Pl1Parser.ParseOptionalInitialValue içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Token-position state ile ayrıştırılan helper parser sınıfları için ortak pattern'i sürdürür.
/// </summary>
internal sealed class InitialValueParseResult
{
    public Pl1InitialValue? InitialValue { get; }

    public int Position { get; }

    public InitialValueParseResult(
        Pl1InitialValue? initialValue,
        int position)
    {
        InitialValue = initialValue;
        Position = position;
    }
}

/// <summary>
/// INIT / INITIAL repeat factor parse sonucunu taşır.
///
/// Neden var?
/// ----------------------
/// Repeat factor parse sonucu iki bilgi döndürür: sayısal tekrar değeri ve (*)
/// all-elements kullanımının varlığı.
///
/// Ne çözüyor?
/// ----------------------
/// Tuple yerine niyeti açık küçük bir taşıyıcı model kullanır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - (08) => RepeatCount 8
/// - (*) => AppliesToAllElements true
///
/// Nerede kullanılır?
/// ----------------------
/// - InitialValueParser.ParseOptionalInitialRepeatFactor içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Repeat factor davranışı genişletilirse yeni metadata alanları bu modelde toplanabilir.
/// </summary>
internal sealed record InitialRepeatInfo(
    int? RepeatCount,
    bool AppliesToAllElements)
{
    public static InitialRepeatInfo None { get; } = new(
        null,
        false);
}