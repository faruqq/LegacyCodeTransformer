using System;
using System.Collections.Generic;
using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Parsing;

/// <summary>
/// PL/I token listesini Pl1SyntaxTree modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// Lexer yalnızca kaynak kodu token'lara ayırır.
/// Ancak token listesi henüz anlamlı bir program modeli değildir.
///
/// Parser, token listesini okuyarak PL/I diline ait declaration
/// modellerini oluşturur.
///
/// Örnek PL/I:
///
/// DCL MUST_NO FIXED DECIMAL(8);
/// DCL PARAM CHAR(08) INIT(' ');
/// DCL 1 PARAME_LIST,
///     5 PARAM CHAR(08) INIT(' '),
///     5 PARAM2 CHAR(01) INIT(';');
///
/// Bu parser ilgili ifadeleri:
/// - Pl1VariableDeclaration
/// - Pl1StructureDeclaration
/// - Pl1FixedDecimalType
/// - Pl1CharacterType
/// - Pl1InitialValue
///
/// modellerine dönüştürür.
///
/// Nerede kullanılır?
/// ----------------------
/// - Application pipeline içerisinde
/// - PL/I kaynak kodunu SyntaxTree'ye dönüştürmek için
/// - Normalizer ve Transpiler aşamalarından önce
///
/// Gelecekte ne işe yarayacak?
/// ----------------------
/// PL/I desteği genişledikçe IF, CALL, DO/END, PROCEDURE, assignment,
/// embedded SQL, array declaration ve nested structure gibi yapılar
/// bu parser üzerinden SyntaxTree'ye çevrilecektir.
/// </summary>
public sealed class Pl1Parser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics = new();
    private int _position;

    /// <summary>
    /// PL/I parser instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser, lexer tarafından üretilen token listesini sırayla okuyarak
    /// syntax tree üretir.
    ///
    /// Token listesi null gelirse boş liste olarak ele alınır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application conversion pipeline içerisinde
    /// - Parser unit testlerinde
    /// </summary>
    public Pl1Parser(IReadOnlyList<Pl1Token> tokens)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
    }

    /// <summary>
    /// Token listesini okuyarak Pl1SyntaxTree üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodundan gelen token listesi, dönüşüm pipeline'ında
    /// kullanılabilecek güçlü tipli syntax tree modeline çevrilmelidir.
    ///
    /// Bu method şu anda declaration odaklı çalışır:
    /// - Tekil variable declaration
    /// - Basit structure declaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ConversionService içerisinde
    /// - Parser unit testlerinde
    /// - Normalizer ve Transpiler öncesinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I program yapısı genişledikçe declaration dışındaki statement
    /// türleri de bu ana parse akışı üzerinden yönlendirilecektir.
    /// </summary>
    public ParseResult<Pl1SyntaxTree> Parse()
    {
        var declarations = new List<Pl1Declaration>();

        while (!IsAtEnd())
        {
            if (Current.Kind == Pl1TokenKind.DclKeyword)
            {
                var declaration = ParseDeclaration();
                if (declaration is not null)
                {
                    declarations.Add(declaration);
                }

                continue;
            }

            AddUnexpectedTokenDiagnostic(
                Current,
                "DCL");

            Advance();
        }

        var syntaxTree = new Pl1SyntaxTree(
            declarations,
            SourceLocation.Unknown);

        return new ParseResult<Pl1SyntaxTree>(
            syntaxTree,
            _diagnostics.Diagnostics);
    }

    /// <summary>
    /// PL/I declaration ifadesinin tekil değişken mi yoksa structure mı olduğunu belirler.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında DCL / DECLARE ifadesi hem tekil değişken hem de
    /// seviye numaralı structure declaration için kullanılabilir.
    ///
    /// DCL sonrasında Identifier gelirse tekil değişken, Number gelirse
    /// structure declaration parse edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana akışı DclKeyword gördüğünde
    /// - PL/I SyntaxTree declaration listesi oluşturulurken
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// File declaration, based declaration, factored declaration ve farklı
    /// DCL varyasyonları eklendiğinde declaration dispatch sorumluluğu
    /// bu method üzerinde genişletilecektir.
    /// </summary>
    private Pl1Declaration? ParseDeclaration()
    {
        if (Peek(1).Kind == Pl1TokenKind.Number)
        {
            return ParseStructureDeclaration();
        }

        return ParseVariableDeclaration();
    }

    /// <summary>
    /// PL/I değişken declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodunda DCL / DECLARE ile başlayan ve seviye numarası içermeyen ifadeler tekil değişken tanımı oluşturur.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Değişken adını, optional name-based array bilgisini, veri tipini, optional DIM / DIMENSION bilgisini ve optional INIT / INITIAL değerini parse eder.
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
    /// - ParseDeclaration dispatch methodunda
    /// - Tekil PL/I variable declaration üretiminde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu DIMENSION ve farklı array declaration varyasyonları bu method üzerinden genişletilecektir.
    /// </summary>
    private Pl1VariableDeclaration? ParseVariableDeclaration()
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
            return null;
        }

        return new Pl1VariableDeclaration(
            identifierToken.Text,
            dataType,
            dclToken.Location,
            initialValue,
            arraySize);
    }

    /// <summary>
    /// PL/I seviye numaralı structure declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDeclaration methodu DCL sonrasında Number gördüğünde structure declaration
    /// parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat structure declaration,
    /// member, nested member, member data type, member INIT ve member dimension parsing
    /// sorumluluğunu StructureParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL 1 REC, 5 PARAM CHAR(08);
    /// - DCL 1 DIZI(6), 3 KOD CHAR(01);
    /// - DCL 1 MUSTERI, 5 ADRES, 10 IL CHAR(02);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDeclaration dispatch methodu içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Pl1Parser structure detaylarıyla büyümeden StructureParser içinde geliştirilebilir.
    /// </summary>
    private Pl1StructureDeclaration? ParseStructureDeclaration()
    {
        var parser = new StructureParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseStructureDeclaration();

        _position = result.Position;

        return result.Declaration;
    }

    /// <summary>
    /// PL/I declaration adından sonra gelen opsiyonel array dimension bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Declaration adı sonrasında parantez içinde array size verilebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat name-based array size
    /// parsing sorumluluğunu DimensionParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM(2) CHAR(10);
    /// - DCL 1 DIZI(6), ...
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içinde
    /// - ParseStructureDeclaration içinde
    /// - ParseStructureMember içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu array syntax Pl1Parser büyütülmeden DimensionParser içinde geliştirilebilir.
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
    /// PL/I DIM / DIMENSION attribute bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Veri tipi sonrasında DIM veya DIMENSION attribute ile array size verilebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat DIM / DIMENSION parsing
    /// sorumluluğunu DimensionParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DIM(2)
    /// - DIMENSION(2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içinde
    /// - ParseStructureMember içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DIMENSION range ve çok boyutlu syntax Pl1Parser büyütülmeden DimensionParser içinde geliştirilebilir.
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
    /// ParseVariableDeclaration ve ParseStructureMember iki farklı array size kaynağı okuyabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Array size conflict çözümleme sorumluluğunu DimensionParser helper sınıfına devreder.
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
    /// - ParseStructureMember içinde
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
    /// PL/I değişken tanımındaki veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseVariableDeclaration ve ParseStructureMember akışları değişken veya member
    /// adından sonra veri tipi okuyabilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat veri tipi dispatch
    /// ve veri tipi helper parser çağırma sorumluluğunu DataTypeParser sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(15)
    /// - CHAR(08)
    /// - VARCHAR(50)
    /// - PIC '999'
    /// - BIT(8)
    /// - FLOAT BIN(53)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içinde
    /// - ParseStructureMember içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Pl1Parser data type detaylarıyla büyümeden yeni data type aileleri
    /// DataTypeParser içinde geliştirilebilir.
    /// </summary>
    private Pl1DataType? ParseDataType()
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
    /// PL/I declaration içindeki opsiyonel INIT / INITIAL başlangıç değerini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseVariableDeclaration ve ParseStructureMember akışları veri tipinden sonra
    /// opsiyonel initialization bilgisi okuyabilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat INIT / INITIAL syntax
    /// çözümleme sorumluluğunu InitialValueParser helper sınıfına devreder.
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
    /// - ParseStructureMember içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Initialization parsing davranışı Pl1Parser büyütülmeden InitialValueParser içinde geliştirilebilir.
    /// </summary>
    private Pl1InitialValue? ParseOptionalInitialValue()
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
    /// Parser grammar ilerletirken belirli noktalarda belirli token
    /// türlerini bekler.
    ///
    /// Beklenen token gelirse token tüketilir.
    /// Beklenen token gelmezse diagnostic üretilir ve null döner.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Tüm parse methodlarında
    /// - Grammar doğrulamasında
    /// - Diagnostic üretiminde
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
    /// Beklenmeyen token için diagnostic üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser desteklenmeyen veya grammar dışı bir token gördüğünde
    /// kullanıcıya anlamlı hata bilgisi üretmelidir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana akışında
    /// - Structure member ayracı beklenirken
    /// - Gelecekte statement parse hatalarında
    /// </summary>
    private void AddUnexpectedTokenDiagnostic(
        Pl1Token token,
        string expectedText)
    {
        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenmeyen token: {token.Text}. Beklenen: {expectedText}.",
            token.Location));
    }

    /// <summary>
    /// Mevcut pozisyona göre ileri bakış token'ını döndürür.
    ///
    /// Neden var?
    /// ----------------------
    /// DCL sonrasında gelen token'a bakarak declaration tipini seçmemiz
    /// gerekir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDeclaration içerisinde
    /// - Gelecekte lookahead gerektiren grammar ayrımlarında
    /// </summary>
    private Pl1Token Peek(int offset)
    {
        var index = _position + offset;

        if (index >= _tokens.Count)
        {
            return _tokens[^1];
        }

        return _tokens[index];
    }

    /// <summary>
    /// Mevcut token'ı tüketip bir sonraki token'a ilerler.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser token listesinde sırayla ilerlemelidir.
    /// Bu method Current token'ı tüketir ve tüketilen token'ı döndürür.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Consume methodunda
    /// - Hata toparlama sırasında
    /// - Ayracı manuel geçmek gerektiğinde
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
    /// Parser'ın kaynak sonu token'ına gelip gelmediğini belirtir.
    /// </summary>
    private bool IsAtEnd()
    {
        return Current.Kind == Pl1TokenKind.EndOfFile;
    }

    /// <summary>
    /// Mevcut parser pozisyonundaki token'ı döndürür.
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

    /// <summary>
    /// INIT / INITIAL tekrar faktörü parse sonucunu taşır.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseOptionalInitialRepeatFactor methodunun iki ayrı bilgi döndürmesi
    /// gerekir:
    /// - Sayısal tekrar değeri
    /// - (*) kullanımının varlığı
    ///
    /// Bu küçük taşıyıcı model, tuple kullanımına göre daha okunabilir ve
    /// parser kodunun niyetini daha açık gösterir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseOptionalInitialRepeatFactor dönüş değerinde
    /// - ParseOptionalInitialValue içerisinde Pl1InitialValue oluştururken
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Repeat factor davranışı genişletilirse bu model yeni alanlarla
    /// genişletilebilir.
    /// </summary>
    private sealed record InitialRepeatInfo(
        int? RepeatCount,
        bool AppliesToAllElements)
    {
        /// <summary>
        /// Tekrar faktörü bulunmadığı durumu temsil eder.
        /// </summary>
        public static InitialRepeatInfo None { get; } = new(
            null,
            false);
    }
}