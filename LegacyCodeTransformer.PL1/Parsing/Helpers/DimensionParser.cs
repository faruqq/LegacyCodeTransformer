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
/// conflict resolve davranışını Pl1Parser dışına taşır. Ortak token okuma davranışını
/// ParserBase üzerinden kullanır.
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
/// - VariableDeclarationParser içinde
/// - StructureParser içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Çok boyutlu DIMENSION, lower-bound / upper-bound syntax ve array metadata
/// davranışları bu helper üzerinde geliştirilebilir.
/// </summary>
internal sealed class DimensionParser : ParserBase
{
    public DimensionParser(ParseContext context)
        : base(context)
    {
    }

    public DimensionParser(
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
    /// - VariableDeclarationParser içinde
    /// - StructureParser içinde
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
                Position);
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
                Position);
        }

        if (int.TryParse(sizeToken.Text, out var arraySize))
        {
            return new DimensionParseResult(
                arraySize,
                Position);
        }

        Diagnostics.Add(
            ParserDiagnosticFactory.InvalidNumber(
                "Array boyutu sayısal olmalıdır",
                sizeToken));

        return new DimensionParseResult(
            null,
            Position);
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
    /// - VariableDeclarationParser içinde
    /// - StructureParser içinde
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
                Position);
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
                Position);
        }

        if (int.TryParse(sizeToken.Text, out var arraySize))
        {
            return new DimensionParseResult(
                arraySize,
                Position);
        }

        Diagnostics.Add(
            ParserDiagnosticFactory.InvalidNumber(
                "DIM / DIMENSION boyutu sayısal olmalıdır",
                dimensionToken));

        return new DimensionParseResult(
            null,
            Position);
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
    /// - VariableDeclarationParser içinde
    /// - StructureParser içinde
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
            Diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                "Array boyutu hem isim sonrasında hem de DIM / DIMENSION attribute ile verilemez.",
                location));

            return nameArraySize;
        }

        return nameArraySize ?? dimensionArraySize;
    }
}

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