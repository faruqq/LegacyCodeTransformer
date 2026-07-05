using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I veri tipi token akışını ilgili Pl1DataType modeline yönlendirir.
///
/// Neden var?
/// ----------------------
/// Pl1Parser içinde data type dispatch davranışı büyüdükçe ana parser sınıfı
/// tüm veri tipi ailesinin detaylarını taşımaya başladı.
///
/// Ne çözüyor?
/// ----------------------
/// CHAR, VARCHAR, FIXED DECIMAL, FIXED BIN, PIC, BIT, FLOAT, REAL ve DOUBLE
/// gibi veri tipi ailelerini ilgili helper parser sınıflarına yönlendirir.
/// Ortak token okuma davranışını ParserBase üzerinden kullanır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - CHAR(08)
/// - VARCHAR(50)
/// - FIXED DECIMAL(15,2)
/// - FIXED BIN(31)
/// - PIC '999V99'
/// - BIT(8)
/// - FLOAT BIN(53)
/// - REAL
/// - DOUBLE PRECISION
///
/// Nerede kullanılır?
/// ----------------------
/// - VariableDeclarationParser içinde
/// - StructureParser içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 statement parser aşamasında typed declaration, parameter declaration veya
/// procedure signature parsing gerektiğinde merkezi data type parser olarak kullanılabilir.
/// </summary>
internal sealed class DataTypeParser : ParserBase
{
    public DataTypeParser(ParseContext context)
        : base(context)
    {
    }

    public DataTypeParser(
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
    /// Mevcut token'dan başlayarak PL/I veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I declaration içinde değişken veya structure member adından sonra gelen
    /// bölüm veri tipini temsil eder.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Veri tipi başlangıç keyword'üne göre ilgili parser helper'ını çağırır ve
    /// parse sonucunu tek DataTypeParseResult modeliyle döndürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(p,s)
    /// - DEC FIXED(p,s)
    /// - FIXED BIN(p)
    /// - BIN FIXED(p)
    /// - CHAR(n)
    /// - CHARACTER(n)
    /// - VARCHAR(n)
    /// - PIC '999'
    /// - PICTURE 'XXX'
    /// - BIT(n)
    /// - FLOAT
    /// - REAL
    /// - DOUBLE PRECISION
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - VariableDeclarationParser içinde
    /// - StructureParser içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Yeni data type aileleri eklendiğinde Pl1Parser veya declaration parser sınıfları
    /// büyümeden bu dispatch sınıfı genişletilir.
    /// </summary>
    public DataTypeParseResult Parse()
    {
        if (Current.Kind == Pl1TokenKind.FixedKeyword)
        {
            return ParseWithNumericParser(x => x.ParseFixedBasedType());
        }

        if (Current.Kind == Pl1TokenKind.DecimalKeyword ||
            Current.Kind == Pl1TokenKind.DecKeyword)
        {
            return ParseWithNumericParser(x => x.ParseDecimalBasedType());
        }

        if (Current.Kind == Pl1TokenKind.BinaryKeyword ||
            Current.Kind == Pl1TokenKind.BinKeyword)
        {
            return ParseWithNumericParser(x => x.ParseBinaryBasedType());
        }

        if (Current.Kind == Pl1TokenKind.CharKeyword ||
            Current.Kind == Pl1TokenKind.CharacterKeyword)
        {
            return ParseWithCharacterParser(x => x.ParseCharacterType());
        }

        if (Current.Kind == Pl1TokenKind.VarcharKeyword)
        {
            return ParseWithCharacterParser(x => x.ParseVarcharType());
        }

        if (Current.Kind == Pl1TokenKind.PicKeyword ||
            Current.Kind == Pl1TokenKind.PictureKeyword)
        {
            return ParsePictureType();
        }

        if (Current.Kind == Pl1TokenKind.BitKeyword)
        {
            var parser = new BitTypeParser(Context);
            var result = parser.Parse();

            return new DataTypeParseResult(
                result.DataType,
                Position);
        }

        if (Current.Kind == Pl1TokenKind.FloatKeyword ||
            Current.Kind == Pl1TokenKind.RealKeyword ||
            Current.Kind == Pl1TokenKind.DoubleKeyword)
        {
            var parser = new FloatingTypeParser(Context);
            var result = parser.Parse();

            return new DataTypeParseResult(
                result.DataType,
                Position);
        }

        Diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenen PL/I veri tipi bulunamadı. Gelen token: {Current.Text}",
            Current.Location));

        return new DataTypeParseResult(
            null,
            Position);
    }

    /// <summary>
    /// NumericTypeParser ile veri tipi parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// FIXED, DECIMAL ve BINARY başlangıçları aynı NumericTypeParser helper'ını kullanır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// NumericTypeParser oluşturma ve parse sonucunu data type result modeline çevirme
    /// tekrarını tek helper methodda toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(15)
    /// - DEC FIXED(17,2)
    /// - BIN FIXED(31)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse methodu içinde numeric veri tipi branch'lerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Numeric parser davranışı genişledikçe DataTypeParser içindeki tekrarları düşük tutar.
    /// </summary>
    private DataTypeParseResult ParseWithNumericParser(
        Func<NumericTypeParser, NumericTypeParseResult> parse)
    {
        var parser = new NumericTypeParser(Context);
        var result = parse(parser);

        return new DataTypeParseResult(
            result.DataType,
            Position);
    }

    /// <summary>
    /// CharacterTypeParser ile veri tipi parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// CHAR, CHARACTER ve VARCHAR aynı character-family helper parser'ını kullanır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// CharacterTypeParser oluşturma ve parse sonucunu data type result modeline çevirme
    /// tekrarını tek helper methodda toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - CHAR(08)
    /// - CHARACTER(25)
    /// - VARCHAR(50)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse methodu içinde character-family veri tipi branch'lerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Character-family syntax genişledikçe DataTypeParser içindeki tekrarları düşük tutar.
    /// </summary>
    private DataTypeParseResult ParseWithCharacterParser(
        Func<CharacterTypeParser, CharacterTypeParseResult> parse)
    {
        var parser = new CharacterTypeParser(Context);
        var result = parse(parser);

        return new DataTypeParseResult(
            result.DataType,
            Position);
    }

    /// <summary>
    /// PIC / PICTURE veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PIC / PICTURE token okuma davranışı keyword ve string literal tüketmeyi gerektirir.
    /// Pattern semantic model üretimi ise PictureTypeParser tarafından yapılır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PIC veya PICTURE keyword'ünü tüketir, pattern string literal değerini okur ve
    /// PictureTypeParser.Parse ile Pl1PictureType modeli üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - PIC '999'
    /// - PICTURE '999V99'
    /// - PIC 'XXX'
    /// - PIC 'S999'
    /// - PIC 'ZZ9'
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - DataTypeParser.Parse içinde PIC / PICTURE branch'inde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// PIC token okuma davranışı tüm declaration parser akışlarında aynı şekilde
    /// yeniden kullanılabilir.
    /// </summary>
    private DataTypeParseResult ParsePictureType()
    {
        var pictureToken = Current;

        if (Current.Kind == Pl1TokenKind.PicKeyword ||
            Current.Kind == Pl1TokenKind.PictureKeyword)
        {
            Advance();
        }
        else
        {
            Diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"PIC veya PICTURE bekleniyordu. Gelen token: {Current.Text}",
                Current.Location));

            return new DataTypeParseResult(
                null,
                Position);
        }

        var patternToken = Consume(
            Pl1TokenKind.StringLiteral,
            "PIC pattern string literal bekleniyordu.");

        if (patternToken is null)
        {
            return new DataTypeParseResult(
                null,
                Position);
        }

        return new DataTypeParseResult(
            PictureTypeParser.Parse(
                patternToken.Text,
                pictureToken.Location),
            Position);
    }
}

internal sealed class DataTypeParseResult
{
    public Pl1DataType? DataType { get; }

    public int Position { get; }

    public DataTypeParseResult(
        Pl1DataType? dataType,
        int position)
    {
        DataType = dataType;
        Position = position;
    }
}