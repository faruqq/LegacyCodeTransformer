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

namespace LegacyCodeTransformer.Pl1.Parsing
{
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

            if (dclToken is null ||
                identifierToken is null ||
                dataType is null)
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
        /// Bu method:
        /// - DCL keyword'ünü okur
        /// - ana structure level değerini okur
        /// - ana structure adını okur
        /// - member alanları parse eder
        /// - son semicolon karakterini doğrular
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - ParseDeclaration methodu DCL sonrasında Number gördüğünde
        /// - PL/I SyntaxTree içerisinde Pl1StructureDeclaration üretmek için
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Structure array, nested structure ve multi-level hierarchy desteği
        /// geldiğinde bu methodun kapsamı kontrollü şekilde genişletilecektir.
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

            var members = new List<Pl1StructureMember>();

            while (!IsAtEnd() &&
                   Current.Kind != Pl1TokenKind.Semicolon)
            {
                var member = ParseStructureMember();

                if (member is not null)
                {
                    members.Add(member);
                }

                if (Current.Kind == Pl1TokenKind.Comma)
                {
                    Advance();
                    continue;
                }

                if (Current.Kind == Pl1TokenKind.Semicolon)
                {
                    break;
                }

                AddUnexpectedTokenDiagnostic(
                    Current,
                    "',' veya ';'");

                Advance();
            }

            Consume(
                Pl1TokenKind.Semicolon,
                "';' bekleniyordu.");

            if (dclToken is null ||
                levelToken is null ||
                nameToken is null)
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

            return new Pl1StructureDeclaration(
                level,
                nameToken.Text,
                members,
                dclToken.Location,
                arraySize);
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
        /// PL/I structure içerisindeki member declaration satırını parse eder.
        ///
        /// Neden var?
        /// ----------------------
        /// Structure declaration içindeki her field ayrı bir member olarak
        /// modellenmelidir.
        ///
        /// Örnek PL/I:
        ///
        /// 5 PARAM CHAR(08) INIT(' ')
        /// 5 PARAM2 CHAR(01) INIT(';')
        ///
        /// Bu method:
        /// - member level değerini okur
        /// - member adını okur
        /// - member veri tipini ParseDataType ile okur
        /// - varsa INIT / INITIAL bilgisini parse eder
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - ParseStructureDeclaration içerisinde
        /// - Pl1StructureDeclaration.Members listesini oluştururken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Nested structure, member array ve field-level attribute desteği
        /// eklendiğinde bu method genişletilecektir.
        /// </summary>
        private Pl1StructureMember? ParseStructureMember()
        {
            var levelToken = Consume(
                Pl1TokenKind.Number,
                "Structure member seviye numarası bekleniyordu.");

            var nameToken = Consume(
                Pl1TokenKind.Identifier,
                "Structure member adı bekleniyordu.");

            var dataType = ParseDataType();
            var initialValue = ParseOptionalInitialValue();

            if (levelToken is null ||
                nameToken is null ||
                dataType is null)
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

            return new Pl1StructureMember(
                level,
                nameToken.Text,
                dataType,
                levelToken.Location,
                initialValue);
        }

        /// <summary>
        /// PL/I değişken tanımındaki veri tipini parse eder.
        ///
        /// Neden var?
        /// ----------------------
        /// DCL ifadesinde değişken adı veya structure member adı okunduktan
        /// sonra gelen bölüm veri tipini temsil eder.
        ///
        /// Örnek PL/I:
        ///
        /// DCL MUST_NO FIXED DECIMAL(8);
        /// DCL PARAM CHAR(08);
        /// DCL CUSTOMER_NAME CHARACTER(25);
        ///
        /// Bu method:
        /// - FIXED DECIMAL için Pl1FixedDecimalType
        /// - CHAR / CHARACTER için Pl1CharacterType
        ///
        /// üretir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Tekil DCL declaration parse edilirken
        /// - Structure member parse edilirken
        /// - Hatalı veya desteklenmeyen veri tipleri için diagnostic üretiminde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// BIT, FIXED BINARY, PIC / PICTURE, VARYING gibi yeni veri tipleri
        /// desteklendikçe bu method ilgili parse methodlarına yönlendirme
        /// yapacak şekilde genişletilecektir.
        /// </summary>
        private Pl1DataType? ParseDataType()
        {
            if (Current.Kind == Pl1TokenKind.FixedKeyword)
            {
                return ParseFixedDecimalType();
            }

            if (Current.Kind == Pl1TokenKind.CharKeyword ||
                Current.Kind == Pl1TokenKind.CharacterKeyword)
            {
                return ParseCharacterType();
            }

            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Beklenen PL/I veri tipi bulunamadı. Gelen token: {Current.Text}",
                Current.Location));

            return null;
        }

        /// <summary>
        /// PL/I FIXED DECIMAL veri tipini parse eder.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I kodunda numerik alanlar sıklıkla FIXED DECIMAL(p) veya
        /// FIXED DECIMAL(p,s) söz dizimi ile tanımlanır.
        ///
        /// Örnek PL/I:
        ///
        /// DCL MUST_NO FIXED DECIMAL(8);
        /// DCL AMOUNT FIXED DECIMAL(18,2);
        ///
        /// Bu method ilgili veri tipini:
        /// - Precision
        /// - Scale
        ///
        /// bilgileriyle Pl1FixedDecimalType modeline dönüştürür.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - ParseDataType methodu FixedKeyword gördüğünde
        /// - Basit DCL declaration parse edilirken
        /// - Structure member veri tipi parse edilirken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// DECIMAL kısaltması, FIXED DECIMAL scale desteği ve farklı numeric
        /// varyasyonlar bu method üzerinden genişletilecektir.
        /// </summary>
        private Pl1FixedDecimalType? ParseFixedDecimalType()
        {
            var fixedToken = Consume(
                Pl1TokenKind.FixedKeyword,
                "FIXED bekleniyordu.");

            Consume(
                Pl1TokenKind.DecimalKeyword,
                "DECIMAL bekleniyordu.");

            Consume(
                Pl1TokenKind.OpenParenthesis,
                "'(' bekleniyordu.");

            var precisionToken = Consume(
                Pl1TokenKind.Number,
                "Precision değeri bekleniyordu.");

            var scale = 0;

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

            if (fixedToken is null ||
                precisionToken is null)
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
                fixedToken.Location);
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
}