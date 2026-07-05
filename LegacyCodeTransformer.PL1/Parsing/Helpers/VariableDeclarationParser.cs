using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I top-level variable declaration token akışını Pl1VariableDeclaration modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// Pl1Parser içinde tekil değişken declaration parsing sorumluluğu; DCL, identifier,
/// array size, data type, DIM / DIMENSION ve INIT / INITIAL parsing detaylarını birlikte
/// yönetiyordu.
///
/// Ne çözüyor?
/// ----------------------
/// Top-level variable declaration parsing davranışını Pl1Parser dışına taşır.
/// Pl1Parser artık yalnızca declaration dispatch yapar; değişken declaration detaylarını
/// VariableDeclarationParser yönetir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL MUST_NO FIXED DECIMAL(8);
/// - DCL PARAM CHAR(08) INIT(' ');
/// - DCL PARAM(2) CHAR(10);
/// - DCL PARAM CHAR(10) DIM(2);
/// - DCL PARAM CHAR(10) DIMENSION(2);
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1Parser.ParseVariableDeclaration içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Factored declaration, multiple variable declaration, BASED attribute ve procedure
/// parameter declaration gibi P05 sonrası syntax destekleri bu helper üzerinde
/// geliştirilebilir.
/// </summary>
internal sealed class VariableDeclarationParser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics;
    private int _position;

    /// <summary>
    /// Variable declaration helper parser instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Helper parser'ın mevcut token listesini, başlangıç pozisyonunu ve diagnostic bag
    /// referansını bilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Pl1Parser token state bilgisini helper'a aktarır. Parse tamamlandığında yeni
    /// token pozisyonu result modeliyle geri döndürülür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Current token DCL olduğunda ve DCL sonrası Identifier geldiğinde variable
    /// declaration parse eder.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Declaration parser ailesinde aynı token-position pattern'inin korunmasını sağlar.
    /// </summary>
    public VariableDeclarationParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
        _position = position;
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// PL/I değişken declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodunda DCL / DECLARE ile başlayan ve seviye numarası içermeyen
    /// ifadeler tekil değişken tanımı oluşturur.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DCL keyword, değişken adı, optional name-based array size, data type,
    /// optional DIM / DIMENSION size ve optional INIT / INITIAL değerini okuyarak
    /// Pl1VariableDeclaration modeli üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL MUST_NO FIXED DECIMAL(8);
    /// - DCL PARAM CHAR(08) INIT(' ');
    /// - DCL PARAM(2) CHAR(10);
    /// - DCL PARAM CHAR(10) DIM(2);
    /// - DCL PARAM CHAR(10) DIMENSION(2);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Multiple declaration ve attribute-rich declaration parsing davranışları bu method
    /// üzerinde genişletilecektir.
    /// </summary>
    public VariableDeclarationParseResult ParseVariableDeclaration()
    {
        var dclToken = Consume(
            Pl1TokenKind.DclKeyword,
            "DCL bekleniyordu.");

        var identifierToken = Consume(
            Pl1TokenKind.Identifier,
            "Değişken adı bekleniyordu.");

        var nameArraySize = ParseOptionalArraySize();
        var dataType = ParseDataType();
        var dimensionArraySize = ParseOptionalDimensionSize();
        var arraySize = ResolveArraySize(
            nameArraySize,
            dimensionArraySize,
            identifierToken?.Location ?? SourceLocation.Unknown);

        var initialValue = ParseOptionalInitialValue();

        Consume(
            Pl1TokenKind.Semicolon,
            "';' bekleniyordu.");

        if (dclToken is null || identifierToken is null || dataType is null)
        {
            return new VariableDeclarationParseResult(
                null,
                _position);
        }

        return new VariableDeclarationParseResult(
            new Pl1VariableDeclaration(
                identifierToken.Text,
                dataType,
                dclToken.Location,
                initialValue,
                arraySize),
            _position);
    }

    /// <summary>
    /// PL/I declaration adından sonra gelen opsiyonel array dimension bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Variable declaration adından hemen sonra parantez içinde array size verilebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Name-based array size parsing sorumluluğunu DimensionParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM(2) CHAR(10);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu array syntax DimensionParser içinde geliştirilebilir.
    /// </summary>
    private int? ParseOptionalArraySize()
    {
        var parser = new DimensionParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseOptionalArraySize();

        _position = result.Position;

        return result.ArraySize;
    }

    /// <summary>
    /// PL/I variable declaration içindeki veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Variable declaration adından sonra data type bilgisi gelmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Data type dispatch ve data type specific parsing sorumluluğunu DataTypeParser
    /// helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(8)
    /// - CHAR(08)
    /// - PIC '999'
    /// - FLOAT BIN(53)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Yeni veri tipi aileleri DataTypeParser içinde geliştirildikçe variable declaration
    /// parser değişmeden kalır.
    /// </summary>
    private Types.Pl1DataType? ParseDataType()
    {
        var parser = new DataTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.Parse();

        _position = result.Position;

        return result.DataType;
    }

    /// <summary>
    /// PL/I DIM / DIMENSION attribute bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Variable declaration veri tipi sonrasında DIM veya DIMENSION attribute gelebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DIM / DIMENSION parsing sorumluluğunu DimensionParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM CHAR(10) DIM(2);
    /// - DCL PARAM CHAR(10) DIMENSION(2);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DIMENSION range ve çok boyutlu syntax DimensionParser içinde geliştirilebilir.
    /// </summary>
    private int? ParseOptionalDimensionSize()
    {
        var parser = new DimensionParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseOptionalDimensionSize();

        _position = result.Position;

        return result.ArraySize;
    }

    /// <summary>
    /// Name-based array size ile DIM / DIMENSION attribute size bilgisini tek değere indirger.
    ///
    /// Neden var?
    /// ----------------------
    /// Variable declaration üzerinde iki farklı array size kaynağı oluşabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Conflict çözümleme sorumluluğunu DimensionParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - PARAM(2) CHAR(10)
    /// - PARAM CHAR(10) DIM(2)
    /// - PARAM(2) CHAR(10) DIM(3)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu array metadata merge davranışı DimensionParser içinde geliştirilebilir.
    /// </summary>
    private int? ResolveArraySize(
        int? nameArraySize,
        int? dimensionArraySize,
        SourceLocation location)
    {
        var parser = new DimensionParser(
            _tokens,
            _position,
            _diagnostics);

        return parser.ResolveArraySize(
            nameArraySize,
            dimensionArraySize,
            location);
    }

    /// <summary>
    /// PL/I declaration içindeki opsiyonel INIT / INITIAL başlangıç değerini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Variable declaration veri tipinden sonra optional INIT / INITIAL bilgisi gelebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Initialization parsing sorumluluğunu InitialValueParser helper sınıfına devreder.
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
    /// - ParseVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Variable default value, array initialization ve repeat expansion davranışları
    /// InitialValueParser üzerinde geliştirilebilir.
    /// </summary>
    private InitialValues.Pl1InitialValue? ParseOptionalInitialValue()
    {
        var parser = new InitialValueParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseOptionalInitialValue();

        _position = result.Position;

        return result.InitialValue;
    }

    /// <summary>
    /// Beklenen token türünü tüketir.
    ///
    /// Neden var?
    /// ----------------------
    /// Variable declaration grammar belirli noktalarda DCL, identifier ve semicolon bekler.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Beklenen token gelirse token'ı tüketir; gelmezse diagnostic üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL PARAM CHAR(08); akışındaki zorunlu token'ları doğrular.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Variable parser içindeki token okuma standardını merkezi tutar.
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
/// Variable declaration parse sonucunu ve parse sonrası token pozisyonunu taşır.
///
/// Neden var?
/// ----------------------
/// VariableDeclarationParser ayrı token position state'i ile çalışır. Parse tamamlandığında
/// Pl1Parser'ın kendi pozisyonunu güncellemesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Parse edilen Pl1VariableDeclaration modeli ile parse sonrası position değerini birlikte döndürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// DCL PARAM CHAR(08) INIT(' '); parse edildiğinde declaration modeli ve semicolon sonrası
/// position birlikte taşınır.
///
/// Nerede kullanılır?
/// ----------------------
/// - VariableDeclarationParser.ParseVariableDeclaration dönüş değerinde
/// - Pl1Parser.ParseVariableDeclaration içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Multiple declaration veya factored declaration parsing sırasında result modeli
/// genişletilebilir.
/// </summary>
internal sealed class VariableDeclarationParseResult
{
    public Pl1VariableDeclaration? Declaration { get; }

    public int Position { get; }

    public VariableDeclarationParseResult(
        Pl1VariableDeclaration? declaration,
        int position)
    {
        Declaration = declaration;
        Position = position;
    }
}