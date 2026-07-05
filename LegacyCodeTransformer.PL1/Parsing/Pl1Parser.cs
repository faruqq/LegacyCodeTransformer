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
    /// PL/I kaynak kodunda DCL / DECLARE ile başlayan ve seviye numarası
    /// içermeyen ifadeler tekil değişken tanımı oluşturur.
    ///
    /// Örnek PL/I:
    ///
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL PARAM CHAR(08);
    /// DCL PARAM CHAR(08) INIT(' ');
    /// DECLARE CUSTOMER_NAME CHARACTER(25) INITIAL(' ');
    ///
    /// Bu method:
    /// - declaration başlangıcını okur
    /// - değişken adını okur
    /// - veri tipini ParseDataType methoduna devreder
    /// - varsa INIT / INITIAL başlangıç değerini parse eder
    /// - statement sonundaki noktalı virgülü doğrular
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDeclaration dispatch methodunda
    /// - Tekil PL/I variable declaration üretiminde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Array dimension ve çoklu declaration desteği eklendiğinde bu method
    /// kontrollü şekilde genişletilecektir.
    /// </summary>
    private Pl1VariableDeclaration? ParseVariableDeclaration()
    {
        var dclToken = Consume(
            Pl1TokenKind.DclKeyword,
            "DCL bekleniyordu.");

        var identifierToken = Consume(
            Pl1TokenKind.Identifier,
            "Değişken adı bekleniyordu.");

        var dataType = ParseDataType();
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
            initialValue);
    }

    /// <summary>
    /// PL/I seviye numaralı structure declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure declaration ifadeleri DCL sonrasında seviye numarası
    /// ile başlar ve altında member alanlar içerir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08) INIT(' '),
    ///     5 PARAM2 CHAR(01) INIT(';');
    ///
    /// P04-F kapsamında nested group field yapıları da desteklenir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 ADRES_BILGI,
    ///         10 IL_KOD CHAR(02),
    ///         10 ILCE_KOD CHAR(03);
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Ana structure bilgisini okur, varsa structure array dimension bilgisini
    /// parse eder ve alt member hiyerarşisini ParseStructureMembers helper'ı ile
    /// oluşturur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL 1 PARAME_LIST, 5 PARAM CHAR(08);
    /// - DCL 1 DIZI(6), 3 DIZI_PARAM1 CHAR(01);
    /// - DCL 1 PARAME_LIST, 5 ADRES_BILGI, 10 IL_KOD CHAR(02);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDeclaration methodu DCL sonrasında Number gördüğünde
    /// - PL/I SyntaxTree içerisinde Pl1StructureDeclaration üretmek için
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok seviyeli nested structure, group array ve layout length hesabı
    /// davranışlarına temel olur.
    /// </summary>
    private Pl1StructureDeclaration? ParseStructureDeclaration()
    {
        var dclToken = Consume(
            Pl1TokenKind.DclKeyword,
            "DCL bekleniyordu.");

        var levelToken = Consume(
            Pl1TokenKind.Number,
            "Structure seviye numarası bekleniyordu.");

        var nameToken = Consume(
            Pl1TokenKind.Identifier,
            "Structure adı bekleniyordu.");

        var arraySize = ParseOptionalArraySize();

        Consume(
            Pl1TokenKind.Comma,
            "',' bekleniyordu.");

        if (dclToken is null || levelToken is null || nameToken is null)
        {
            return null;
        }

        if (!int.TryParse(levelToken.Text, out var level))
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Structure seviye numarası sayısal olmalıdır: {levelToken.Text}",
                levelToken.Location));

            return null;
        }

        var members = ParseStructureMembers(level);

        Consume(
            Pl1TokenKind.Semicolon,
            "';' bekleniyordu.");

        return new Pl1StructureDeclaration(
            level,
            nameToken.Text,
            members,
            dclToken.Location,
            arraySize);
    }

    /// <summary>
    /// Verilen parent level altında bulunan structure member listesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure içinde member alanları düz bir liste gibi görünse de level
    /// değerleri aslında hiyerarşiyi belirler.
    ///
    /// Örnek:
    ///
    /// 5 ADRES_BILGI,
    ///     10 IL_KOD CHAR(02),
    ///     10 ILCE_KOD CHAR(03)
    ///
    /// Burada IL_KOD ve ILCE_KOD, ADRES_BILGI altında child member olarak
    /// modellenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Current token seviyesini parentLevel ile karşılaştırarak hangi member'ın
    /// mevcut parent altında kalacağını belirler.
    ///
    /// Current level parentLevel değerinden küçük veya eşitse mevcut nested
    /// scope tamamlanmış kabul edilir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 5 PARAM CHAR(08)
    /// - 5 ADRES_BILGI, 10 IL_KOD CHAR(02)
    /// - 5 GROUP1, 10 GROUP2, 15 FIELD1 CHAR(01)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseStructureDeclaration içinde root member listesini parse ederken
    /// - ParseStructureMember içinde nested group child member listesini parse ederken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok seviyeli nested structure ve group field desteğinin parser tarafındaki
    /// merkezi hiyerarşi kurucusudur.
    /// </summary>
    private IReadOnlyList<Pl1StructureMember> ParseStructureMembers(int parentLevel)
    {
        var members = new List<Pl1StructureMember>();

        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.Semicolon)
        {
            if (Current.Kind != Pl1TokenKind.Number)
            {
                AddUnexpectedTokenDiagnostic(
                    Current,
                    "Structure member seviye numarası");

                Advance();
                continue;
            }

            if (!int.TryParse(Current.Text, out var currentLevel))
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    $"Structure member seviye numarası sayısal olmalıdır: {Current.Text}",
                    Current.Location));

                Advance();
                continue;
            }

            if (currentLevel <= parentLevel)
            {
                break;
            }

            var member = ParseStructureMember();
            if (member is not null)
            {
                members.Add(member);
            }

            if (Current.Kind == Pl1TokenKind.Comma)
            {
                Advance();
            }
        }

        return members;
    }

    /// <summary>
    /// PL/I declaration adından sonra gelen opsiyonel array dimension bilgisini parse eder.
    ///
    /// Örnek:
    ///
    /// DCL 1 DIZI(6),
    ///
    /// Bu örnekte array size değeri 6 olarak döner.
    /// </summary>
    private int? ParseOptionalArraySize()
    {
        if (Current.Kind != Pl1TokenKind.OpenParenthesis)
        {
            return null;
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
            return null;
        }

        if (int.TryParse(sizeToken.Text, out var arraySize))
        {
            return arraySize;
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Array boyutu sayısal olmalıdır: {sizeToken.Text}",
            sizeToken.Location));

        return null;
    }

    /// <summary>
    /// PL/I structure içerisindeki tek bir member declaration satırını parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure declaration içindeki her field ayrı bir member olarak
    /// modellenmelidir.
    ///
    /// P04-F öncesinde member alanlarının veri tipi taşıdığı varsayılıyordu.
    /// P04-F ile birlikte veri tipi taşımayan nested group field yapıları da
    /// desteklenir.
    ///
    /// Normal field örneği:
    ///
    /// 5 PARAM CHAR(08) INIT(' ')
    ///
    /// Field array örneği:
    ///
    /// 5 PARAM_LIST(2) CHAR(10)
    ///
    /// Nested group örneği:
    ///
    /// 5 ADRES_BILGI,
    ///     10 IL_KOD CHAR(02),
    ///     10 ILCE_KOD CHAR(03)
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Member level, name, optional array dimension, optional data type,
    /// optional initial value ve varsa child member listesini tek modelde toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 5 PARAM CHAR(08)
    /// - 5 PARAM_LIST(2) CHAR(10)
    /// - 5 ADRES_BILGI, 10 IL_KOD CHAR(02)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseStructureMembers içinde
    /// - Pl1StructureDeclaration.Members listesini oluştururken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested structure mapping, group field length hesabı ve çok seviyeli layout
    /// üretimi için parser tarafındaki temel member parse davranışıdır.
    /// </summary>
    private Pl1StructureMember? ParseStructureMember()
    {
        var levelToken = Consume(
            Pl1TokenKind.Number,
            "Structure member seviye numarası bekleniyordu.");

        var nameToken = Consume(
            Pl1TokenKind.Identifier,
            "Structure member adı bekleniyordu.");

        var arraySize = ParseOptionalArraySize();

        if (levelToken is null || nameToken is null)
        {
            return null;
        }

        if (!int.TryParse(levelToken.Text, out var level))
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Structure member seviye numarası sayısal olmalıdır: {levelToken.Text}",
                levelToken.Location));

            return null;
        }

        if (Current.Kind == Pl1TokenKind.Comma)
        {
            Advance();

            var childMembers = ParseStructureMembers(level);

            return new Pl1StructureMember(
                level,
                nameToken.Text,
                null,
                levelToken.Location,
                null,
                arraySize,
                childMembers);
        }

        if (Current.Kind == Pl1TokenKind.Semicolon)
        {
            return new Pl1StructureMember(
                level,
                nameToken.Text,
                null,
                levelToken.Location,
                null,
                arraySize);
        }

        var dataType = ParseDataType();
        var initialValue = ParseOptionalInitialValue();

        if (dataType is null)
        {
            return null;
        }

        return new Pl1StructureMember(
            level,
            nameToken.Text,
            dataType,
            levelToken.Location,
            initialValue,
            arraySize);
    }

    /// <summary>
    /// PL/I değişken tanımındaki veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// DCL ifadesinde değişken adı veya structure member adı okunduktan
    /// sonra gelen bölüm veri tipini temsil eder.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Desteklenen PL/I veri tipi keyword'lerini ilgili güçlü tipli model
    /// sınıflarına yönlendirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(p,s)
    /// - FIXED DEC(p,s)
    /// - DECIMAL FIXED(p,s)
    /// - DEC FIXED(p,s)
    /// - FIXED BINARY(p)
    /// - FIXED BIN(p)
    /// - BINARY FIXED(p)
    /// - BIN FIXED(p)
    /// - CHAR(n)
    /// - CHARACTER(n)
    /// - VARCHAR(n)
    /// - PIC '999'
    /// - PICTURE '999V99'
    /// - BIT(n)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Tekil DCL declaration parse edilirken
    /// - Structure member parse edilirken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DIMENSION, BIT literal INIT ve daha gelişmiş BIT mapping davranışları
    /// desteklendikçe bu method genişletilecektir.
    /// </summary>
    private Pl1DataType? ParseDataType()
    {
        if (Current.Kind == Pl1TokenKind.FixedKeyword)
        {
            return ParseFixedBasedType();
        }

        if (Current.Kind == Pl1TokenKind.DecimalKeyword ||
            Current.Kind == Pl1TokenKind.DecKeyword)
        {
            return ParseDecimalBasedType();
        }

        if (Current.Kind == Pl1TokenKind.BinaryKeyword ||
            Current.Kind == Pl1TokenKind.BinKeyword)
        {
            return ParseBinaryBasedType();
        }

        if (Current.Kind == Pl1TokenKind.CharKeyword ||
            Current.Kind == Pl1TokenKind.CharacterKeyword)
        {
            return ParseCharacterType();
        }

        if (Current.Kind == Pl1TokenKind.VarcharKeyword)
        {
            return ParseVarcharType();
        }

        if (Current.Kind == Pl1TokenKind.PicKeyword ||
            Current.Kind == Pl1TokenKind.PictureKeyword)
        {
            return ParsePictureType();
        }

        if (Current.Kind == Pl1TokenKind.BitKeyword)
        {
            return ParseBitType();
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenen PL/I veri tipi bulunamadı. Gelen token: {Current.Text}",
            Current.Location));

        return null;
    }

    /// <summary>
    /// PL/I BIT(n) veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodunda bit string alanları BIT keyword'ü ile tanımlanır.
    /// BIT tipi CHAR veya numeric veri tipi değildir; ayrı modelle korunmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// BIT keyword'ünden sonra gelen parantez içi uzunluk değerini okuyarak
    /// Pl1BitType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL FLAG BIT(1);
    /// - DCL MASK BIT(8);
    /// - 5 STATUS_FLAGS BIT(8);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu BitKeyword gördüğünde
    /// - Tekil variable declaration parse edilirken
    /// - Structure member veri tipi parse edilirken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// BIT literal INIT, BIT(1) boolean mapping veya bit string preserving
    /// mapping kararları alındığında merkezi parser davranışı olarak kalır.
    /// </summary>
    private Pl1BitType? ParseBitType()
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
            return null;
        }

        if (!int.TryParse(lengthToken.Text, out var length))
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"BIT uzunluk değeri sayısal olmalıdır: {lengthToken.Text}",
                lengthToken.Location));

            return null;
        }

        return new Pl1BitType(
            length,
            bitToken.Location);
    }

    /// <summary>
    /// PL/I PIC / PICTURE veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PIC / PICTURE ifadeleri PL/I tarafında numeric, signed, implied decimal,
    /// alphanumeric veya formatted alan tanımlamak için kullanılabilir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL PARAM1 PIC '999';
    /// DCL PARAM2 PIC '999V99';
    /// DCL PARAM3 PIC 'S999';
    /// DCL PARAM4 PIC '(13)9V99';
    /// DCL PARAM5 PIC 'XXX';
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PIC keyword'ünden sonra gelen pattern string literal değerini okuyarak
    /// Pl1PictureType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - PIC '999'
    /// - PICTURE '999V99'
    /// - PIC 'S999'
    /// - PIC '(13)9V99'
    /// - PIC 'XXX'
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu PicKeyword veya PictureKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Numeric PIC, signed PIC, alphanumeric PIC ve formatted PIC davranışlarının
    /// syntax tree'ye taşınmasına temel olur.
    /// </summary>
    private Pl1PictureType? ParsePictureType()
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

            return null;
        }

        var patternToken = Consume(
            Pl1TokenKind.StringLiteral,
            "PIC pattern string literal bekleniyordu.");

        if (patternToken is null)
        {
            return null;
        }

        return CreatePictureTypeFromPattern(
            patternToken.Text,
            pictureToken.Location);
    }

    /// <summary>
    /// PIC pattern değerinden Pl1PictureType modeli oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// ParsePictureType methodu keyword ve string literal okumaktan sorumludur.
    /// Okunan raw PIC pattern değerinin Pl1PictureType modeline çevrilmesi ise
    /// ayrı bir model oluşturma adımıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PIC pattern semantic analizini PicturePatternAnalyzer sınıfına devreder.
    /// Böylece parser içinde pattern classification, precision, scale ve length hesabı
    /// tekrar tutulmaz.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - PIC '999'
    /// - PIC '999V99'
    /// - PIC '(13)9V99'
    /// - PIC 'XXX'
    /// - PIC '(20)X'
    /// - PIC 'AXXAA'
    /// - PIC 'ZZ9'
    /// - PIC 'S999'
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ParsePictureType methodu PIC / PICTURE keyword'ünden sonra gelen string literal
    /// pattern değerini okuduktan sonra bu methodu çağırır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parser sabit kalırken signed PIC, formatted PIC ve display metadata davranışları
    /// PicturePatternAnalyzer üzerinde genişletilebilir.
    /// </summary>
    private static Pl1PictureType CreatePictureTypeFromPattern(
        string rawPattern,
        SourceLocation location)
    {
        var analysis = PicturePatternAnalyzer.Analyze(rawPattern);

        return new Pl1PictureType(
            rawPattern,
            analysis.Category,
            analysis.Precision,
            analysis.Scale,
            analysis.Length,
            analysis.IsSigned,
            analysis.IsNumeric,
            analysis.IsAlphanumeric,
            analysis.IsFormatted,
            analysis.SupportsDirectEglMapping,
            location);
    }

    /// <summary>
    /// FIXED keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında numeric tipler farklı keyword sıralamalarıyla
    /// yazılabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// FIXED ile başlayan numeric type söz dizimini ilgili semantic parser'a
    /// yönlendirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - FIXED DECIMAL(p,s)
    /// - FIXED DEC(p,s)
    /// - FIXED BINARY(p)
    /// - FIXED BIN(p)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType içerisinde Current token FixedKeyword olduğunda
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// FIXED FLOAT gibi farklı numeric family desteği gerekirse bu method
    /// genişletilecektir.
    /// </summary>
    private Pl1DataType? ParseFixedBasedType()
    {
        var fixedToken = Consume(
            Pl1TokenKind.FixedKeyword,
            "FIXED bekleniyordu.");

        if (Current.Kind == Pl1TokenKind.DecimalKeyword ||
            Current.Kind == Pl1TokenKind.DecKeyword)
        {
            return ParseFixedDecimalTypeAfterPrefix(
                fixedToken?.Location ?? SourceLocation.Unknown);
        }

        if (Current.Kind == Pl1TokenKind.BinaryKeyword ||
            Current.Kind == Pl1TokenKind.BinKeyword)
        {
            return ParseFixedBinaryTypeAfterPrefix(
                fixedToken?.Location ?? SourceLocation.Unknown);
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"FIXED sonrasında DECIMAL, DEC, BINARY veya BIN bekleniyordu. Gelen token: {Current.Text}",
            Current.Location));

        return null;
    }

    /// <summary>
    /// DECIMAL / DEC keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında decimal fixed tipler ters keyword sırasıyla da
    /// yazılabilir.
    ///
    /// Örnek:
    ///
    /// DECIMAL FIXED(17,2)
    /// DEC FIXED(17,2)
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DECIMAL veya DEC ile başlayan numeric type söz dizimini aynı
    /// Pl1FixedDecimalType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DECIMAL FIXED(p,s)
    /// - DEC FIXED(p,s)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType içerisinde Current token DecimalKeyword veya DecKeyword olduğunda
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DECIMAL FLOAT gibi farklı decimal tabanlı tipler desteklenirse bu method
    /// genişletilecektir.
    /// </summary>
    private Pl1DataType? ParseDecimalBasedType()
    {
        var decimalToken = Current;
        Advance();

        Consume(
            Pl1TokenKind.FixedKeyword,
            "FIXED bekleniyordu.");

        return ParseDecimalPrecisionAndScale(
            decimalToken.Location);
    }

    /// <summary>
    /// BINARY / BIN keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında binary fixed tipler ters keyword sırasıyla da
    /// yazılabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// BINARY veya BIN ile başlayan numeric type söz dizimini aynı
    /// Pl1FixedBinaryType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - BINARY FIXED(p)
    /// - BIN FIXED(p)
    /// - BINARY FIXED(p,0)
    /// - BIN FIXED(p,0)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType içerisinde Current token BinaryKeyword veya BinKeyword olduğunda
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Binary fractional mapping veya farklı binary numeric tipler desteklenirse
    /// bu method genişletilecektir.
    /// </summary>
    private Pl1DataType? ParseBinaryBasedType()
    {
        var binaryToken = Current;
        Advance();

        Consume(
            Pl1TokenKind.FixedKeyword,
            "FIXED bekleniyordu.");

        return ParseBinaryPrecisionAndScale(
            binaryToken.Location);
    }

    /// <summary>
    /// FIXED prefix'i okunduktan sonra gelen DECIMAL / DEC decimal tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// FIXED DECIMAL ve FIXED DEC aynı semantic decimal tipi ifade eder.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// FIXED sonrasındaki DECIMAL / DEC keyword'ünü tüketir ve ortak
    /// precision / scale parser'ına yönlendirir.
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
    /// FIXED DECIMAL, FIXED DEC, DECIMAL FIXED ve DEC FIXED aynı precision /
    /// scale söz dizimini kullanır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Ortak `(p)` ve `(p,s)` parse davranışını tek noktada toplar.
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
    /// Decimal precision/scale validasyonları ve farklı numeric synonym'ler
    /// eklendiğinde merkezi parse noktasıdır.
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
    /// FIXED BINARY, FIXED BIN, BINARY FIXED ve BIN FIXED aynı precision /
    /// scale söz dizimini kullanır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Ortak `(p)` ve `(p,s)` parse davranışını tek noktada toplar.
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
    /// Binary precision/scale validasyonları ve unsupported diagnostic kuralları
    /// eklendiğinde merkezi parse noktasıdır.
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

    /// <summary>
    /// PL/I CHAR(n) veya CHARACTER(n) veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodunda sabit uzunluklu karakter alanlar CHAR veya
    /// CHARACTER keyword'ü ile tanımlanır.
    ///
    /// Örnek PL/I:
    ///
    /// DCL PARAM CHAR(08);
    /// DCL CUSTOMER_NAME CHARACTER(25);
    ///
    /// Bu method ilgili veri tipini:
    ///
    /// Pl1CharacterType
    /// - Length: 8
    /// - Length: 25
    ///
    /// olarak syntax tree'ye taşır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu CHAR veya CHARACTER token gördüğünde
    /// - Basit DCL declaration parse edilirken
    /// - Structure member veri tipi parse edilirken
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// CHAR(n) VARYING, INIT(' '), INITIAL((4)'*') gibi ek söz dizimleri
    /// desteklendiğinde bu method veya bu methodun çağırdığı alt parser
    /// yapıları genişletilecektir.
    /// </summary>
    private Pl1CharacterType? ParseCharacterType()
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

        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        var lengthToken = Consume(
            Pl1TokenKind.Number,
            "CHAR uzunluğu bekleniyordu.");

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (lengthToken is null)
        {
            return null;
        }

        if (!int.TryParse(lengthToken.Text, out var length))
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"CHAR uzunluğu sayısal olmalıdır: {lengthToken.Text}",
                lengthToken.Location));

            return null;
        }

        return new Pl1CharacterType(
            length,
            typeToken.Location);
    }

    /// <summary>
    /// PL/I VARCHAR(n) veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodunda değişken uzunluklu karakter alanlar VARCHAR keyword'ü
    /// ile tanımlanabilir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL CUSTOMER_NAME VARCHAR(50);
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// VARCHAR keyword'ü ile parantez içindeki uzunluk bilgisini okuyarak
    /// Pl1VarcharType modeline dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL CUSTOMER_NAME VARCHAR(50);
    /// - 5 CUSTOMER_NAME VARCHAR(50);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu VarcharKeyword gördüğünde
    /// - Tekil variable declaration parse edilirken
    /// - Structure member veri tipi parse edilirken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// VARCHAR alanların EGL char(n) mapping işlemine, structure length
    /// hesabına ve ileride sqlRecord metadata üretimine temel olur.
    /// </summary>
    private Pl1VarcharType? ParseVarcharType()
    {
        var varcharToken = Consume(
            Pl1TokenKind.VarcharKeyword,
            "VARCHAR bekleniyordu.");

        Consume(
            Pl1TokenKind.OpenParenthesis,
            "'(' bekleniyordu.");

        var lengthToken = Consume(
            Pl1TokenKind.Number,
            "VARCHAR uzunluk değeri bekleniyordu.");

        Consume(
            Pl1TokenKind.CloseParenthesis,
            "')' bekleniyordu.");

        if (varcharToken is null || lengthToken is null)
        {
            return null;
        }

        if (!int.TryParse(lengthToken.Text, out var length))
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"VARCHAR uzunluk değeri sayısal olmalıdır: {lengthToken.Text}",
                lengthToken.Location));

            return null;
        }

        return new Pl1VarcharType(
            length,
            varcharToken.Location);
    }

    /// <summary>
    /// PL/I declaration içindeki opsiyonel INIT / INITIAL başlangıç değerini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I değişken tanımlarında veri tipinden sonra başlangıç değeri
    /// verilebilir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL PARAM CHAR(08) INIT(' ');
    /// DCL PARAM2 CHAR(01) INIT(';');
    /// DCL PARAM3 CHAR(8) INIT((08)' ');
    /// DCL PARAM4 CHAR(8) INIT((*)' ');
    /// DCL PARAM5 CHAR(4) INITIAL('ABCD');
    ///
    /// Bu method:
    /// - INIT / INITIAL yoksa null döner
    /// - INIT(' ') için Pl1InitialValue üretir
    /// - INIT((08)' ') için RepeatCount = 8 üretir
    /// - INIT((*)' ') için AppliesToAllElements = true üretir
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseVariableDeclaration içerisinde
    /// - ParseStructureMember içerisinde
    /// - PL/I declaration modeline başlangıç değeri bilgisini eklemek için
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// EGL default value üretimi kararlaştırıldığında bu bilgi Transpiler
    /// veya Generator tarafında kullanılabilecektir.
    /// Structure array desteği geldiğinde INIT((*)' ') kullanımı bu model
    /// üzerinden anlamlandırılacaktır.
    /// </summary>
    private Pl1InitialValue? ParseOptionalInitialValue()
    {
        if (Current.Kind != Pl1TokenKind.InitKeyword &&
            Current.Kind != Pl1TokenKind.InitialKeyword)
        {
            return null;
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
            return null;
        }

        return new Pl1InitialValue(
            valueToken.Text,
            repeatInfo.RepeatCount,
            repeatInfo.AppliesToAllElements,
            initToken.Location);
    }

    /// <summary>
    /// PL/I INIT / INITIAL içindeki opsiyonel tekrar faktörünü parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I başlangıç değeri söz diziminde aynı değerin tekrar ettirilmesi
    /// veya tüm elemanlara uygulanması için tekrar faktörü kullanılabilir.
    ///
    /// Örnek PL/I:
    ///
    /// INIT((08)' ')
    /// INIT((*)' ')
    /// INITIAL((4)'*')
    ///
    /// Bu method:
    /// - (08) için RepeatCount = 8
    /// - (*) için AppliesToAllElements = true
    /// - tekrar faktörü yoksa varsayılan değerler
    ///
    /// üretir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseOptionalInitialValue içerisinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Array ve structure initialization desteği geldiğinde repeat factor
    /// davranışı bu method üzerinden genişletilecektir.
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
            _diagnostics.Add(new Diagnostic(
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