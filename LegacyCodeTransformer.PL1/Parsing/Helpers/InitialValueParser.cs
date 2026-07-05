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
/// Pl1Parser dışına taşır. Ortak token okuma davranışını ParserBase üzerinden kullanır.
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
/// - VariableDeclarationParser içinde
/// - StructureParser içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Array initialization, repeat factor expansion ve record field default value mapping
/// davranışları bu helper üzerinde geliştirilebilir.
/// </summary>
internal sealed class InitialValueParser : ParserBase
{
    public InitialValueParser(ParseContext context)
        : base(context)
    {
    }

    public InitialValueParser(
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
    /// - VariableDeclarationParser içinde
    /// - StructureParser içinde
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
                Position);
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
                Position);
        }

        return new InitialValueParseResult(
            new Pl1InitialValue(
                valueToken.Text,
                repeatInfo.RepeatCount,
                repeatInfo.AppliesToAllElements,
                initToken.Location),
            Position);
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
    /// - INIT(' ') => InitialRepeatInfo.None
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
            Diagnostics.Add(new Diagnostic(
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
}

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

internal sealed record InitialRepeatInfo(
    int? RepeatCount,
    bool AppliesToAllElements)
{
    public static InitialRepeatInfo None { get; } = new(
        null,
        false);
}