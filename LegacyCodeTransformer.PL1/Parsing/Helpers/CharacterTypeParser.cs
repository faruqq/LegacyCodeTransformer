using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I CHAR / CHARACTER / VARCHAR veri tipi token akışını ilgili PL/I data type modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// Pl1Parser içinde karakter veri tipi parsing sorumluluğu büyüdükçe ana parser sınıfı
/// gereksiz detay taşımaya başladı.
///
/// Ne çözüyor?
/// ----------------------
/// CHAR, CHARACTER ve VARCHAR parsing davranışını Pl1Parser dışına taşır.
/// Ortak token okuma davranışını ParserBase üzerinden kullanır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL PARAM CHAR(08);
/// - DCL PARAM CHARACTER(25);
/// - DCL PARAM VARCHAR(50);
///
/// Nerede kullanılır?
/// ----------------------
/// - DataTypeParser içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// CHAR VARYING, GRAPHIC, WIDECHAR veya farklı character-family tipler eklendiğinde
/// bu helper genişletilebilir.
/// </summary>
internal sealed class CharacterTypeParser : ParserBase
{
    public CharacterTypeParser(ParseContext context)
        : base(context)
    {
    }

    public CharacterTypeParser(
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
    /// CHAR veya CHARACTER token akışını Pl1CharacterType modeline dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// CHAR ve CHARACTER aynı semantic karakter tipini ifade eder.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// CHAR(n) ve CHARACTER(n) söz dizimlerini ortak Pl1CharacterType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - CHAR(08)
    /// - CHARACTER(25)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - DataTypeParser içinde character branch'inde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Character-family veri tipleri genişledikçe aynı helper altında yönetilebilir.
    /// </summary>
    public CharacterTypeParseResult ParseCharacterType()
    {
        var typeToken = Current;

        if (Current.Kind == Pl1TokenKind.CharKeyword)
        {
            Consume(
                Pl1TokenKind.CharKeyword,
                "CHAR bekleniyordu.");
        }
        else
        {
            Consume(
                Pl1TokenKind.CharacterKeyword,
                "CHARACTER bekleniyordu.");
        }

        var length = ParseRequiredLength(
            "CHAR uzunluğu bekleniyordu.",
            "CHAR uzunluğu sayısal olmalıdır");

        if (!length.HasValue)
        {
            return new CharacterTypeParseResult(
                null,
                Position);
        }

        return new CharacterTypeParseResult(
            new Pl1CharacterType(
                length.Value,
                typeToken.Location),
            Position);
    }

    /// <summary>
    /// VARCHAR token akışını Pl1VarcharType modeline dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// VARCHAR değişken uzunluklu karakter alanı temsil eder ve CHAR ile aynı parser
    /// içinde karıştırılmadan modellenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// VARCHAR(n) söz dizimini Pl1VarcharType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - VARCHAR(50)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - DataTypeParser içinde varchar branch'inde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// VARCHAR mapping, sqlRecord column metadata ve length validation davranışları
    /// bu method üzerinde genişletilebilir.
    /// </summary>
    public CharacterTypeParseResult ParseVarcharType()
    {
        var varcharToken = Consume(
            Pl1TokenKind.VarcharKeyword,
            "VARCHAR bekleniyordu.");

        var length = ParseRequiredLength(
            "VARCHAR uzunluk değeri bekleniyordu.",
            "VARCHAR uzunluk değeri sayısal olmalıdır");

        if (varcharToken is null || !length.HasValue)
        {
            return new CharacterTypeParseResult(
                null,
                Position);
        }

        return new CharacterTypeParseResult(
            new Pl1VarcharType(
                length.Value,
                varcharToken.Location),
            Position);
    }

    /// <summary>
    /// Character-family veri tipleri için zorunlu parantez içi length değerini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// CHAR, CHARACTER ve VARCHAR aynı `(n)` length söz dizimini kullanır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Ortak length parse davranışını tek noktada toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - CHAR(08) => 8
    /// - VARCHAR(50) => 50
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseCharacterType içinde
    /// - ParseVarcharType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Length validation veya maximum length kuralları gerektiğinde merkezi helper olarak
    /// genişletilir.
    /// </summary>
    private int? ParseRequiredLength(
        string missingNumberMessage,
        string invalidNumberMessagePrefix)
    {
        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        var lengthToken = Consume(
            Pl1TokenKind.Number,
            missingNumberMessage);

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (lengthToken is null)
        {
            return null;
        }

        if (int.TryParse(lengthToken.Text, out var length))
        {
            return length;
        }

        Diagnostics.Add(
            ParserDiagnosticFactory.InvalidNumber(
                invalidNumberMessagePrefix,
                lengthToken));

        return null;
    }
}

internal sealed class CharacterTypeParseResult
{
    public Pl1DataType? DataType { get; }

    public int Position { get; }

    public CharacterTypeParseResult(
        Pl1DataType? dataType,
        int position)
    {
        DataType = dataType;
        Position = position;
    }
}