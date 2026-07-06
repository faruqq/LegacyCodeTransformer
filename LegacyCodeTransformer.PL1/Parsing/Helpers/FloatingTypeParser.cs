using LegacyCodeTransformer.Core.Diagnostics;
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
/// Ortak token okuma davranışını ParserBase üzerinden kullanır.
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
/// - DataTypeParser içinde floating data type branch'inde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Floating precision validation, FLOAT DECIMAL mapping kararı ve P05 öncesi parser sorumluluk ayrımı bu sınıf üzerinde geliştirilebilir.
/// </summary>
internal sealed class FloatingTypeParser : ParserBase
{
    public FloatingTypeParser(ParseContext context)
        : base(context)
    {
    }

    public FloatingTypeParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
        : this(new ParseContext(
            tokens,
            position,
            diagnostics))
    {
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
    /// - DataTypeParser içinde floating branch'inde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Floating type validasyonları ve mapping öncesi semantic enrichment bu method üzerinde genişletilebilir.
    /// </summary>
    public HelperParseResult<Pl1FloatingType> Parse()
    {
        var typeToken = Current;

        if (Current.Kind == Pl1TokenKind.RealKeyword)
        {
            Advance();

            return new HelperParseResult<Pl1FloatingType>(
                new Pl1FloatingType(
                    Pl1FloatingTypeKind.Real,
                    Pl1FloatingBase.Unspecified,
                    null,
                    typeToken.Location),
                Position);
        }

        if (Current.Kind == Pl1TokenKind.DoubleKeyword)
        {
            Advance();

            if (Current.Kind == Pl1TokenKind.PrecisionKeyword)
            {
                Advance();
            }

            return new HelperParseResult<Pl1FloatingType>(
                new Pl1FloatingType(
                    Pl1FloatingTypeKind.DoublePrecision,
                    Pl1FloatingBase.Unspecified,
                    null,
                    typeToken.Location),
                Position);
        }

        if (Current.Kind != Pl1TokenKind.FloatKeyword)
        {
            Diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"FLOAT, REAL veya DOUBLE bekleniyordu. Gelen token: {Current.Text}",
                Current.Location));

            return new HelperParseResult<Pl1FloatingType>(
                null,
                Position);
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

        return new HelperParseResult<Pl1FloatingType>(
            new Pl1FloatingType(
                Pl1FloatingTypeKind.Float,
                floatingBase,
                precision,
                typeToken.Location),
            Position);
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

        Diagnostics.Add(
            ParserDiagnosticFactory.InvalidNumber(
                "Precision değeri sayısal olmalıdır",
                precisionToken));

        return null;
    }
}