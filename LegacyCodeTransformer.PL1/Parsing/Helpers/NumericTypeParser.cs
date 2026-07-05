using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I FIXED DECIMAL / FIXED BINARY numeric veri tipi token akışını parse eder.
///
/// Neden var?
/// ----------------------
/// Pl1Parser içinde numeric type parsing sorumluluğu büyüdükçe ana parser sınıfı
/// numeric synonym ve precision / scale detaylarıyla gereksiz şekilde genişler.
///
/// Ne çözüyor?
/// ----------------------
/// FIXED DECIMAL, FIXED DEC, DECIMAL FIXED, DEC FIXED, FIXED BINARY,
/// FIXED BIN, BINARY FIXED ve BIN FIXED parsing davranışlarını Pl1Parser dışına taşır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - FIXED DECIMAL(15)
/// - FIXED DEC(15,0)
/// - DECIMAL FIXED(17,2)
/// - DEC FIXED(17,2)
/// - FIXED BINARY(15)
/// - FIXED BIN(31)
/// - BINARY FIXED(15,0)
/// - BIN FIXED(31)
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1Parser.ParseFixedBasedType
/// - Pl1Parser.ParseDecimalBasedType
/// - Pl1Parser.ParseBinaryBasedType
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Numeric precision / scale validation, unsupported binary scale diagnostic ve
/// yeni numeric synonym destekleri bu helper üzerinde geliştirilebilir.
/// </summary>
internal sealed class NumericTypeParser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics;
    private int _position;

    public NumericTypeParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
        _position = position;
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// FIXED keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında FIXED keyword'ünden sonra DECIMAL / DEC veya BINARY / BIN
    /// gelebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// FIXED tabanlı numeric syntax'ı doğru decimal veya binary parser akışına yönlendirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(15)
    /// - FIXED DEC(17,2)
    /// - FIXED BINARY(15)
    /// - FIXED BIN(31)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseFixedBasedType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// FIXED FLOAT gibi farklı numeric family varyasyonları gerektiğinde bu dispatch
    /// noktası genişletilebilir.
    /// </summary>
    public NumericTypeParseResult ParseFixedBasedType()
    {
        var fixedToken = Consume(
            Pl1TokenKind.FixedKeyword,
            "FIXED bekleniyordu.");

        if (Current.Kind == Pl1TokenKind.DecimalKeyword ||
            Current.Kind == Pl1TokenKind.DecKeyword)
        {
            var dataType = ParseFixedDecimalTypeAfterPrefix(
                fixedToken?.Location ?? SourceLocation.Unknown);

            return new NumericTypeParseResult(
                dataType,
                _position);
        }

        if (Current.Kind == Pl1TokenKind.BinaryKeyword ||
            Current.Kind == Pl1TokenKind.BinKeyword)
        {
            var dataType = ParseFixedBinaryTypeAfterPrefix(
                fixedToken?.Location ?? SourceLocation.Unknown);

            return new NumericTypeParseResult(
                dataType,
                _position);
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"FIXED sonrasında DECIMAL, DEC, BINARY veya BIN bekleniyordu. Gelen token: {Current.Text}",
            Current.Location));

        return new NumericTypeParseResult(
            null,
            _position);
    }

    /// <summary>
    /// DECIMAL / DEC keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I decimal fixed tipler ters keyword sırasıyla yazılabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DECIMAL FIXED ve DEC FIXED syntax'ını Pl1FixedDecimalType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DECIMAL FIXED(15)
    /// - DEC FIXED(17,2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseDecimalBasedType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DECIMAL FLOAT gibi ileride desteklenebilecek decimal family tipler için
    /// ayrı dispatch davranışına temel olur.
    /// </summary>
    public NumericTypeParseResult ParseDecimalBasedType()
    {
        var decimalToken = Current;
        Advance();

        Consume(
            Pl1TokenKind.FixedKeyword,
            "FIXED bekleniyordu.");

        var dataType = ParseDecimalPrecisionAndScale(
            decimalToken.Location);

        return new NumericTypeParseResult(
            dataType,
            _position);
    }

    /// <summary>
    /// BINARY / BIN keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I binary fixed tipler ters keyword sırasıyla yazılabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// BINARY FIXED ve BIN FIXED syntax'ını Pl1FixedBinaryType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - BINARY FIXED(15)
    /// - BIN FIXED(31)
    /// - BIN FIXED(15,0)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseBinaryBasedType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Binary precision / scale validation veya unsupported binary diagnostic kuralları
    /// bu method üzerinde genişletilebilir.
    /// </summary>
    public NumericTypeParseResult ParseBinaryBasedType()
    {
        var binaryToken = Current;
        Advance();

        Consume(
            Pl1TokenKind.FixedKeyword,
            "FIXED bekleniyordu.");

        var dataType = ParseBinaryPrecisionAndScale(
            binaryToken.Location);

        return new NumericTypeParseResult(
            dataType,
            _position);
    }

    /// <summary>
    /// FIXED prefix'i okunduktan sonra gelen DECIMAL / DEC decimal tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// FIXED DECIMAL ve FIXED DEC aynı semantic decimal fixed tipi ifade eder.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// FIXED sonrasındaki DECIMAL / DEC keyword'ünü tüketir ve ortak precision /
    /// scale parser'ına yönlendirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(15)
    /// - FIXED DEC(15,0)
    /// - FIXED DEC(17,2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseFixedBasedType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Decimal fixed synonym desteği genişledikçe ortak prefix sonrası parser davranışı
    /// burada korunur.
    /// </summary>
    private Pl1FixedDecimalType? ParseFixedDecimalTypeAfterPrefix(
        SourceLocation location)
    {
        if (Current.Kind == Pl1TokenKind.DecimalKeyword ||
            Current.Kind == Pl1TokenKind.DecKeyword)
        {
            Advance();

            return ParseDecimalPrecisionAndScale(location);
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"DECIMAL veya DEC bekleniyordu. Gelen token: {Current.Text}",
            Current.Location));

        return null;
    }

    /// <summary>
    /// FIXED prefix'i okunduktan sonra gelen BINARY / BIN tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// FIXED BINARY ve FIXED BIN aynı semantic binary fixed tipi ifade eder.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// FIXED sonrasındaki BINARY / BIN keyword'ünü tüketir ve ortak precision /
    /// scale parser'ına yönlendirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED BINARY(15)
    /// - FIXED BIN(15)
    /// - FIXED BIN(31)
    /// - FIXED BIN(15,0)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseFixedBasedType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Binary fixed synonym desteği genişledikçe ortak prefix sonrası parser davranışı
    /// burada korunur.
    /// </summary>
    private Pl1FixedBinaryType? ParseFixedBinaryTypeAfterPrefix(
        SourceLocation location)
    {
        if (Current.Kind == Pl1TokenKind.BinaryKeyword ||
            Current.Kind == Pl1TokenKind.BinKeyword)
        {
            Advance();

            return ParseBinaryPrecisionAndScale(location);
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"BINARY veya BIN bekleniyordu. Gelen token: {Current.Text}",
            Current.Location));

        return null;
    }

    /// <summary>
    /// Decimal numeric type için precision ve optional scale bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// FIXED DECIMAL, FIXED DEC, DECIMAL FIXED ve DEC FIXED aynı `(p)` veya
    /// `(p,s)` söz dizimini kullanır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Decimal precision / scale parse davranışını tek noktada toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - (15) => Precision 15, Scale null
    /// - (15,0) => Precision 15, Scale 0
    /// - (17,2) => Precision 17, Scale 2
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseFixedDecimalTypeAfterPrefix içinde
    /// - ParseDecimalBasedType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Decimal precision / scale limit validation ve unsupported diagnostic kuralları
    /// burada merkezi olarak geliştirilebilir.
    /// </summary>
    private Pl1FixedDecimalType? ParseDecimalPrecisionAndScale(
        SourceLocation location)
    {
        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        var precisionToken = Consume(
            Pl1TokenKind.Number,
            "Precision değeri bekleniyordu.");

        int? scale = null;

        if (Current.Kind == Pl1TokenKind.Comma)
        {
            Advance();

            var scaleToken = Consume(
                Pl1TokenKind.Number,
                "Scale değeri bekleniyordu.");

            if (scaleToken is not null &&
                int.TryParse(scaleToken.Text, out var parsedScale))
            {
                scale = parsedScale;
            }
        }

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (precisionToken is null)
        {
            return null;
        }

        if (!int.TryParse(precisionToken.Text, out var precision))
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Precision değeri sayısal olmalıdır: {precisionToken.Text}",
                precisionToken.Location));

            return null;
        }

        return new Pl1FixedDecimalType(
            precision,
            scale,
            location);
    }

    /// <summary>
    /// Binary fixed numeric type için precision ve optional scale bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// FIXED BINARY, FIXED BIN, BINARY FIXED ve BIN FIXED aynı `(p)` veya
    /// `(p,s)` söz dizimini kullanır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Binary precision / scale parse davranışını tek noktada toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - (15) => Precision 15, Scale null
    /// - (15,0) => Precision 15, Scale 0
    /// - (31) => Precision 31, Scale null
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseFixedBinaryTypeAfterPrefix içinde
    /// - ParseBinaryBasedType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Binary precision / scale validation ve unsupported diagnostic kuralları
    /// burada merkezi olarak geliştirilebilir.
    /// </summary>
    private Pl1FixedBinaryType? ParseBinaryPrecisionAndScale(
        SourceLocation location)
    {
        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        var precisionToken = Consume(
            Pl1TokenKind.Number,
            "Binary precision değeri bekleniyordu.");

        int? scale = null;

        if (Current.Kind == Pl1TokenKind.Comma)
        {
            Advance();

            var scaleToken = Consume(
                Pl1TokenKind.Number,
                "Binary scale değeri bekleniyordu.");

            if (scaleToken is not null &&
                int.TryParse(scaleToken.Text, out var parsedScale))
            {
                scale = parsedScale;
            }
        }

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (precisionToken is null)
        {
            return null;
        }

        if (!int.TryParse(precisionToken.Text, out var precision))
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Binary precision değeri sayısal olmalıdır: {precisionToken.Text}",
                precisionToken.Location));

            return null;
        }

        return new Pl1FixedBinaryType(
            precision,
            scale,
            location);
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

internal sealed class NumericTypeParseResult
{
    public Pl1DataType? DataType { get; }

    public int Position { get; }

    public NumericTypeParseResult(
        Pl1DataType? dataType,
        int position)
    {
        DataType = dataType;
        Position = position;
    }
}