using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I structure declaration token akışını Pl1StructureDeclaration modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// Pl1Parser içinde structure declaration, nested member, dimension, data type ve INIT parsing
/// davranışları birlikte büyüdükçe ana parser sınıfı fazla sorumluluk taşımaya başladı.
///
/// Ne çözüyor?
/// ----------------------
/// DCL 1 ... ile başlayan structure declaration parsing davranışını Pl1Parser dışına taşır.
/// Root structure, structure array, member listesi, nested group, member array, DIM / DIMENSION
/// ve INIT / INITIAL parsing akışlarını tek structure parser içinde koordine eder.
/// Ortak token okuma davranışını ParserBase üzerinden kullanır.
/// Parse sonucu generic HelperParseResult modeli ile döner.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL 1 REC, 5 PARAM CHAR(08);
/// - DCL 1 DIZI(6), 3 KOD CHAR(01);
/// - DCL 1 REC, 5 PARAM CHAR(10) DIM(2);
/// - DCL 1 MUSTERI, 5 ADRES, 10 IL CHAR(02);
///
/// Nerede kullanılır?
/// ----------------------
/// - DeclarationParser içinde structure declaration branch'inde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 öncesi parser sadeleşmesini tamamlar. İleride based structure, include tabanlı
/// structure veya DCLGEN benzeri veri tanımları bu sınıfta geliştirilebilir.
/// </summary>
internal sealed class StructureParser : ParserBase
{
    public StructureParser(ParseContext context)
        : base(context)
    {
    }

    public StructureParser(
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
    /// PL/I seviye numaralı structure declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure declaration ifadeleri DCL sonrasında seviye numarası ile başlar
    /// ve altında member alanlar içerir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Structure root level, name, optional array size ve member hiyerarşisini okuyarak
    /// Pl1StructureDeclaration modelini üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL 1 PARAME_LIST, 5 PARAM CHAR(08);
    /// - DCL 1 DIZI(6), 3 DIZI_PARAM1 CHAR(01);
    /// - DCL 1 MUSTERI, 5 ADRES, 10 IL CHAR(02);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - DeclarationParser içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Structure parser davranışı Pl1Parser büyütülmeden bu sınıfta genişletilebilir.
    /// </summary>
    public HelperParseResult<Pl1StructureDeclaration> ParseStructureDeclaration()
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
            return new HelperParseResult<Pl1StructureDeclaration>(
                null,
                Position);
        }

        if (!int.TryParse(levelToken.Text, out var level))
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.InvalidNumber(
                    "Structure seviye numarası sayısal olmalıdır",
                    levelToken));

            return new HelperParseResult<Pl1StructureDeclaration>(
                null,
                Position);
        }

        var members = ParseStructureMembers(level);

        Consume(
            Pl1TokenKind.Semicolon,
            "';' bekleniyordu.");

        return new HelperParseResult<Pl1StructureDeclaration>(
            new Pl1StructureDeclaration(
                level,
                nameToken.Text,
                members,
                dclToken.Location,
                arraySize),
            Position);
    }

    /// <summary>
    /// Verilen parent level altında bulunan structure member listesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure member hiyerarşisi token sırasından değil, level değerlerinden anlaşılır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Current member level değerini parent level ile karşılaştırarak mevcut scope içindeki
    /// child member listesini oluşturur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 5 PARAM CHAR(08)
    /// - 5 ADRES, 10 IL CHAR(02)
    /// - 5 GROUP1, 10 GROUP2, 15 FIELD1 CHAR(01)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseStructureDeclaration içinde root member listesini parse ederken
    /// - ParseStructureMember içinde nested group child member listesini parse ederken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok seviyeli nested structure desteğinin merkezi hiyerarşi kurucusudur.
    /// </summary>
    private IReadOnlyList<Pl1StructureMember> ParseStructureMembers(int parentLevel)
    {
        var members = new List<Pl1StructureMember>();

        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.Semicolon)
        {
            if (Current.Kind != Pl1TokenKind.Number)
            {
                Diagnostics.Add(
                    ParserDiagnosticFactory.UnexpectedToken(
                        Current,
                        "Structure member seviye numarası"));

                Advance();
                continue;
            }

            if (!int.TryParse(Current.Text, out var currentLevel))
            {
                Diagnostics.Add(
                    ParserDiagnosticFactory.InvalidNumber(
                        "Structure member seviye numarası sayısal olmalıdır",
                        Current));

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
    /// PL/I structure içerisindeki tek bir member declaration satırını parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure declaration içindeki her alan ayrı Pl1StructureMember modeli olarak
    /// korunmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Member level, name, optional name-based array size, optional data type,
    /// optional DIM / DIMENSION size, optional INIT / INITIAL ve nested child member
    /// listesini tek modelde toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 5 PARAM CHAR(08)
    /// - 5 PARAM_LIST(2) CHAR(10)
    /// - 5 PARAM_LIST CHAR(10) DIM(2)
    /// - 5 ADRES, 10 IL_KOD CHAR(02)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseStructureMembers içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Group array, nested structure, field-level metadata ve sqlRecord column metadata
    /// parsing davranışlarına temel olur.
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
            Diagnostics.Add(
                ParserDiagnosticFactory.InvalidNumber(
                    "Structure member seviye numarası sayısal olmalıdır",
                    levelToken));

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
    /// PL/I veri tipini DataTypeParser üzerinden parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure member parsing içinde veri tipi çözümleme gerekir. Bu sorumluluk
    /// StructureParser içinde tekrar edilmemelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Data type parsing davranışını DataTypeParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - CHAR(08)
    /// - FIXED DECIMAL(15,2)
    /// - PIC '999'
    /// - BIT(8)
    /// - FLOAT BIN(53)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseStructureMember içinde typed member parsing yapılırken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Data type parsing davranışı tek merkezde kalır; StructureParser yalnızca structure
    /// akışını yönetir.
    /// </summary>
    private Pl1DataType? ParseDataType()
    {
        var parser = new DataTypeParser(Context);
        var result = parser.Parse();

        return result.Value;
    }

    /// <summary>
    /// PL/I declaration adından sonra gelen opsiyonel array dimension bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure root veya member adından sonra parantez içinde array size verilebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Name-based array size parsing sorumluluğunu DimensionParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL 1 DIZI(6), ...
    /// - 5 PARAM(2) CHAR(10)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseStructureDeclaration içinde
    /// - ParseStructureMember içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu array syntax DimensionParser içinde geliştirilebilir.
    /// </summary>
    private int? ParseOptionalArraySize()
    {
        var parser = new DimensionParser(Context);
        var result = parser.ParseOptionalArraySize();

        return result.Value;
    }

    /// <summary>
    /// PL/I DIM / DIMENSION attribute bilgisini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure member veri tipi sonrasında DIM veya DIMENSION attribute gelebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DIM / DIMENSION parsing sorumluluğunu DimensionParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 5 PARAM CHAR(10) DIM(2)
    /// - 5 PARAM CHAR(10) DIMENSION(2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseStructureMember içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DIMENSION range ve çok boyutlu syntax DimensionParser içinde geliştirilebilir.
    /// </summary>
    private int? ParseOptionalDimensionSize()
    {
        var parser = new DimensionParser(Context);
        var result = parser.ParseOptionalDimensionSize();

        return result.Value;
    }

    /// <summary>
    /// Name-based array size ile DIM / DIMENSION attribute size bilgisini tek değere indirger.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure member üzerinde iki farklı array size kaynağı oluşabilir.
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
        var parser = new DimensionParser(Context);

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
    /// Structure member veri tipinden sonra optional INIT / INITIAL bilgisi gelebilir.
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
    /// - ParseStructureMember içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Structure member default value output veya array initialization davranışları
    /// InitialValueParser üzerinde geliştirilebilir.
    /// </summary>
    private Pl1InitialValue? ParseOptionalInitialValue()
    {
        var parser = new InitialValueParser(Context);
        var result = parser.ParseOptionalInitialValue();

        return result.Value;
    }
}