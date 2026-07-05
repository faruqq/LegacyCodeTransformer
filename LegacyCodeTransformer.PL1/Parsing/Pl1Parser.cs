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
    /// PL/I structure içerisindeki tek bir member declaration satırını parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure declaration içindeki her field ayrı bir member olarak modellenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Member level, name, optional name-based array size, optional data type, optional DIM / DIMENSION size, optional initial value ve varsa child member listesini tek modelde toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 5 PARAM CHAR(08)
    /// - 5 PARAM_LIST(2) CHAR(10)
    /// - 5 PARAM_LIST CHAR(10) DIM(2)
    /// - 5 PARAM_LIST CHAR(10) DIMENSION(2)
    /// - 5 ADRES_BILGI, 10 IL_KOD CHAR(02)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseStructureMembers içinde
    /// - Pl1StructureDeclaration.Members listesini oluştururken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested structure mapping, group field length hesabı, field-level DIMENSION ve çok seviyeli layout üretimi için parser tarafındaki temel member parse davranışıdır.
    /// </summary>
    private Pl1StructureMember? ParseStructureMember()
    {
        var levelToken = Consume(
            Pl1TokenKind.Number,
            "Structure member seviye numarası bekleniyordu.");

        var nameToken = Consume(
            Pl1TokenKind.Identifier,
            "Structure member adı bekleniyordu.");

        var nameArraySize = ParseOptionalArraySize();

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
                nameArraySize,
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
                nameArraySize);
        }

        var dataType = ParseDataType();
        var dimensionArraySize = ParseOptionalDimensionSize();
        var arraySize = ResolveArraySize(
            nameArraySize,
            dimensionArraySize,
            nameToken.Location);

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
    /// DCL ifadesinde değişken adı veya structure member adı okunduktan sonra gelen
    /// bölüm veri tipini temsil eder.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Desteklenen PL/I veri tipi keyword'lerini ilgili güçlü tipli model sınıflarına
    /// yönlendirir.
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
    /// - FLOAT
    /// - FLOAT DECIMAL(16)
    /// - FLOAT BIN(53)
    /// - REAL
    /// - DOUBLE
    /// - DOUBLE PRECISION
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Tekil DCL declaration parse edilirken
    /// - Structure member parse edilirken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DIMENSION, floating type mapping ve daha gelişmiş numeric declaration
    /// davranışları desteklendikçe bu method genişletilecektir.
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

        if (Current.Kind == Pl1TokenKind.FloatKeyword ||
            Current.Kind == Pl1TokenKind.RealKeyword ||
            Current.Kind == Pl1TokenKind.DoubleKeyword)
        {
            return ParseFloatingType();
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenen PL/I veri tipi bulunamadı. Gelen token: {Current.Text}",
            Current.Location));

        return null;
    }

    /// <summary>
    /// PL/I FLOAT / REAL / DOUBLE veri tiplerini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDataType methodu FLOAT / REAL / DOUBLE keyword gördüğünde floating type parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat FLOAT / REAL / DOUBLE syntax çözümleme sorumluluğunu FloatingTypeParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL RATE FLOAT;
    /// - DCL RATE FLOAT DECIMAL(16);
    /// - DCL RATE FLOAT BIN(53);
    /// - DCL RATE REAL;
    /// - DCL RATE DOUBLE PRECISION;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu FloatKeyword, RealKeyword veya DoubleKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// P05 öncesi parser sorumluluk ayrımı yaklaşımını sürdürür. Floating type davranışı Pl1Parser büyütülmeden FloatingTypeParser içinde geliştirilebilir.
    /// </summary>
    private Pl1FloatingType? ParseFloatingType()
    {
        var parser = new FloatingTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.Parse();

        _position = result.Position;

        return result.DataType;
    }

    /// <summary>
    /// PL/I BIT(n) veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDataType methodu BIT keyword gördüğünde bit type parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat BIT(n) syntax çözümleme sorumluluğunu BitTypeParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL FLAG BIT(1);
    /// - DCL MASK BIT(8);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu BitKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// BIT parsing davranışı Pl1Parser büyütülmeden BitTypeParser içinde geliştirilebilir.
    /// </summary>
    private Pl1BitType? ParseBitType()
    {
        var parser = new BitTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.Parse();

        _position = result.Position;

        return result.DataType;
    }

    /// <summary>
    /// PL/I PIC / PICTURE veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PIC / PICTURE ifadeleri PL/I tarafında numeric, signed, implied decimal,
    /// alphanumeric veya formatted alan tanımlamak için kullanılabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PIC / PICTURE keyword'ünden sonra gelen pattern string literal değerini okur.
    /// Pattern değerinden Pl1PictureType oluşturma sorumluluğunu PictureTypeParser
    /// sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM1 PIC '999';
    /// - DCL PARAM2 PICTURE '999V99';
    /// - DCL PARAM3 PIC 'S999';
    /// - DCL PARAM4 PIC '(13)9V99';
    /// - DCL PARAM5 PIC 'XXX';
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu PicKeyword veya PictureKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// PIC / PICTURE token okuma davranışı Pl1Parser içinde kalırken, semantic model
    /// üretimi PictureTypeParser içinde genişletilebilir.
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

        return PictureTypeParser.Parse(
            patternToken.Text,
            pictureToken.Location);
    }

    /// <summary>
    /// FIXED keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDataType methodu FIXED keyword gördüğünde numeric type parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat FIXED DECIMAL / FIXED BINARY syntax çözümleme sorumluluğunu NumericTypeParser helper sınıfına devreder.
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
    /// - ParseDataType methodu FixedKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Numeric parsing davranışı Pl1Parser büyütülmeden NumericTypeParser içinde geliştirilebilir.
    /// </summary>
    private Pl1DataType? ParseFixedBasedType()
    {
        var parser = new NumericTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseFixedBasedType();

        _position = result.Position;

        return result.DataType;
    }

    /// <summary>
    /// DECIMAL / DEC keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDataType methodu DECIMAL veya DEC keyword gördüğünde numeric type parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat DECIMAL FIXED / DEC FIXED syntax çözümleme sorumluluğunu NumericTypeParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DECIMAL FIXED(15)
    /// - DEC FIXED(17,2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu DecimalKeyword veya DecKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Decimal-family parsing davranışı Pl1Parser büyütülmeden NumericTypeParser içinde geliştirilebilir.
    /// </summary>
    private Pl1DataType? ParseDecimalBasedType()
    {
        var parser = new NumericTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseDecimalBasedType();

        _position = result.Position;

        return result.DataType;
    }

    /// <summary>
    /// BINARY / BIN keyword'ü ile başlayan PL/I numeric veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDataType methodu BINARY veya BIN keyword gördüğünde numeric type parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat BINARY FIXED / BIN FIXED syntax çözümleme sorumluluğunu NumericTypeParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - BINARY FIXED(15)
    /// - BIN FIXED(31)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu BinaryKeyword veya BinKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Binary-family parsing davranışı Pl1Parser büyütülmeden NumericTypeParser içinde geliştirilebilir.
    /// </summary>
    private Pl1DataType? ParseBinaryBasedType()
    {
        var parser = new NumericTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseBinaryBasedType();

        _position = result.Position;

        return result.DataType;
    }

    /// <summary>
    /// PL/I CHAR(n) veya CHARACTER(n) veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDataType methodu CHAR veya CHARACTER keyword gördüğünde character type parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat CHAR / CHARACTER syntax çözümleme sorumluluğunu CharacterTypeParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM CHAR(08);
    /// - DCL CUSTOMER_NAME CHARACTER(25);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu CharKeyword veya CharacterKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Character-family parsing davranışı Pl1Parser büyütülmeden CharacterTypeParser içinde geliştirilebilir.
    /// </summary>
    private Pl1CharacterType? ParseCharacterType()
    {
        var parser = new CharacterTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseCharacterType();

        _position = result.Position;

        return result.DataType as Pl1CharacterType;
    }

    /// <summary>
    /// PL/I VARCHAR(n) veri tipini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDataType methodu VARCHAR keyword gördüğünde varchar type parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat VARCHAR syntax çözümleme sorumluluğunu CharacterTypeParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL CUSTOMER_NAME VARCHAR(50);
    /// - 5 CUSTOMER_NAME VARCHAR(50);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDataType methodu VarcharKeyword gördüğünde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// VARCHAR parsing davranışı Pl1Parser büyütülmeden CharacterTypeParser içinde geliştirilebilir.
    /// </summary>
    private Pl1VarcharType? ParseVarcharType()
    {
        var parser = new CharacterTypeParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseVarcharType();

        _position = result.Position;

        return result.DataType as Pl1VarcharType;
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