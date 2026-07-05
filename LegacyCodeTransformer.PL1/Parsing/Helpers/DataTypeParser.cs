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
/// - Pl1Parser.ParseDataType içinde
/// - Sonraki adımda StructureParser içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 öncesinde StructureParser extraction için zemin hazırlar. Yeni data type
/// aileleri eklendiğinde Pl1Parser büyümeden bu dispatch sınıfı genişletilir.
/// </summary>
internal sealed class DataTypeParser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics;
    private int _position;

    /// <summary>
    /// Data type helper parser instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Helper parser'ın mevcut token listesini, başlangıç pozisyonunu ve diagnostic bag
    /// referansını bilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Pl1Parser token state bilgisini DataTypeParser'a aktarır. Parse tamamlandığında
    /// yeni token pozisyonu result modeliyle geri döner.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Current token veri tipi başlangıcı olduğunda aynı token akışından Pl1DataType
    /// modeli üretir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseDataType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// StructureParser extraction sonrasında structure member veri tipi parsing için de
    /// aynı helper kullanılacaktır.
    /// </summary>
    public DataTypeParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
        _position = position;
        _diagnostics = diagnostics;
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
    /// - Pl1Parser.ParseDataType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser aşamasında typed declaration, parameter declaration veya
    /// procedure signature parsing gerektiğinde merkezi data type parser olarak kullanılabilir.
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
            var parser = new BitTypeParser(
                _tokens,
                _position,
                _diagnostics);

            var result = parser.Parse();
            _position = result.Position;

            return new DataTypeParseResult(
                result.DataType,
                _position);
        }

        if (Current.Kind == Pl1TokenKind.FloatKeyword ||
            Current.Kind == Pl1TokenKind.RealKeyword ||
            Current.Kind == Pl1TokenKind.DoubleKeyword)
        {
            var parser = new FloatingTypeParser(
                _tokens,
                _position,
                _diagnostics);

            var result = parser.Parse();
            _position = result.Position;

            return new DataTypeParseResult(
                result.DataType,
                _position);
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenen PL/I veri tipi bulunamadı. Gelen token: {Current.Text}",
            Current.Location));

        return new DataTypeParseResult(
            null,
            _position);
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
    /// NumericTypeParser oluşturma, parse sonucunu alma ve position güncelleme tekrarını
    /// tek helper methodda toplar.
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
        var parser = new NumericTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parse(parser);
        _position = result.Position;

        return new DataTypeParseResult(
            result.DataType,
            _position);
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
    /// CharacterTypeParser oluşturma, parse sonucunu alma ve position güncelleme tekrarını
    /// tek helper methodda toplar.
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
        var parser = new CharacterTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parse(parser);
        _position = result.Position;

        return new DataTypeParseResult(
            result.DataType,
            _position);
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
    /// PIC token okuma davranışı StructureParser tarafından da aynı şekilde yeniden
    /// kullanılabilir.
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
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"PIC veya PICTURE bekleniyordu. Gelen token: {Current.Text}",
                Current.Location));

            return new DataTypeParseResult(
                null,
                _position);
        }

        var patternToken = Consume(
            Pl1TokenKind.StringLiteral,
            "PIC pattern string literal bekleniyordu.");

        if (patternToken is null)
        {
            return new DataTypeParseResult(
                null,
                _position);
        }

        return new DataTypeParseResult(
            PictureTypeParser.Parse(
                patternToken.Text,
                pictureToken.Location),
            _position);
    }

    /// <summary>
    /// Beklenen token türünü tüketir.
    ///
    /// Neden var?
    /// ----------------------
    /// DataTypeParser içindeki PIC / PICTURE syntax belirli token türleri bekler.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Beklenen token gelirse token'ı tüketir; beklenen token gelmezse diagnostic üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PIC '999' içindeki string literal token'ını doğrular.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParsePictureType içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DataTypeParser içinde ek token tüketimi gerektiren yeni veri tipi syntax'ları
    /// geldiğinde ortak helper olarak kullanılır.
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
/// Data type parse sonucunu ve parse sonrası token pozisyonunu taşır.
///
/// Neden var?
/// ----------------------
/// DataTypeParser ayrı token position state'i ile çalışır. Parse tamamlandığında
/// Pl1Parser'ın kendi pozisyonunu güncellemesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Parse edilen Pl1DataType modeli ile parse sonrası position değerini birlikte döndürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// CHAR(08) parse edildiğinde Pl1CharacterType modeli ve yeni token pozisyonu birlikte taşınır.
///
/// Nerede kullanılır?
/// ----------------------
/// - DataTypeParser.Parse dönüş değerinde
/// - Pl1Parser.ParseDataType içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// StructureParser extraction sonrasında structure member data type parsing için de
/// aynı result modeli kullanılabilir.
/// </summary>
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