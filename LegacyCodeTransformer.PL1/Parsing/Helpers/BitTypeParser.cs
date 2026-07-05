using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I BIT(n) veri tipi token akışını Pl1BitType modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// BIT parsing davranışı Pl1Parser içinde kaldığında ana parser veri tipi detaylarıyla büyümeye devam eder.
///
/// Ne çözüyor?
/// ----------------------
/// BIT keyword ve length parse sorumluluğunu Pl1Parser dışına taşır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL FLAG BIT(1);
/// - DCL MASK BIT(8);
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1Parser.ParseBitType içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// BIT literal INIT, BIT(1) boolean mapping veya BIT(n) preserving mapping kararları bu helper üzerinde geliştirilebilir.
/// </summary>
internal sealed class BitTypeParser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics;
    private int _position;

    public BitTypeParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
        _position = position;
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// BIT(n) token akışını Pl1BitType modeline dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// BIT tipi character veya numeric değildir; ayrı semantic model olarak korunmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// BIT keyword'ünü ve parantez içindeki length değerini okuyarak Pl1BitType üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - BIT(1)
    /// - BIT(8)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseBitType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// BIT length validation ve hedef dile özel BIT mapping kararları burada genişletilebilir.
    /// </summary>
    public BitTypeParseResult Parse()
    {
        var bitToken = Consume(
            Pl1TokenKind.BitKeyword,
            "BIT bekleniyordu.");

        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        var lengthToken = Consume(
            Pl1TokenKind.Number,
            "BIT uzunluk değeri bekleniyordu.");

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (bitToken is null || lengthToken is null)
        {
            return new BitTypeParseResult(
                null,
                _position);
        }

        if (!int.TryParse(lengthToken.Text, out var length))
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"BIT uzunluk değeri sayısal olmalıdır: {lengthToken.Text}",
                lengthToken.Location));

            return new BitTypeParseResult(
                null,
                _position);
        }

        return new BitTypeParseResult(
            new Pl1BitType(
                length,
                bitToken.Location),
            _position);
    }

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

    private Pl1Token Advance()
    {
        if (!IsAtEnd())
        {
            _position++;
        }

        return Previous;
    }

    private bool IsAtEnd()
    {
        return Current.Kind == Pl1TokenKind.EndOfFile;
    }

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

    private Pl1Token Previous => _tokens[_position - 1];
}

internal sealed class BitTypeParseResult
{
    public Pl1BitType? DataType { get; }

    public int Position { get; }

    public BitTypeParseResult(
        Pl1BitType? dataType,
        int position)
    {
        DataType = dataType;
        Position = position;
    }
}