using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
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
    /// Parser, token listesini okuyarak PL/I diline ait yapıları oluşturur.
    /// Örneğin:
    ///
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// ifadesini Pl1VariableDeclaration ve Pl1FixedDecimalType modellerine
    /// dönüştürür.
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
    /// embedded SQL gibi yapılar bu parser üzerinden SyntaxTree'ye çevrilecektir.
    ///
    /// İlk sürümde yalnızca DCL FIXED DECIMAL declaration parse edilir.
    /// </summary>
    public sealed class Pl1Parser
    {
        private readonly IReadOnlyList<Pl1Token> _tokens;
        private readonly DiagnosticBag _diagnostics = new();

        private int _position;

        public Pl1Parser(IReadOnlyList<Pl1Token> tokens)
        {
            _tokens = tokens ?? Array.Empty<Pl1Token>();
        }

        /// <summary>
        /// Token listesini okuyarak Pl1SyntaxTree üretir.
        /// </summary>
        public ParseResult<Pl1SyntaxTree> Parse()
        {
            var declarations = new List<Pl1VariableDeclaration>();

            while (!IsAtEnd())
            {
                if (Current.Kind == Pl1TokenKind.DclKeyword)
                {
                    var declaration = ParseVariableDeclaration();

                    if (declaration is not null)
                    {
                        declarations.Add(declaration);
                    }

                    continue;
                }

                AddUnexpectedTokenDiagnostic(Current, "DCL");

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
        /// PL/I değişken declaration ifadesini parse eder.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I kaynak kodunda DCL / DECLARE ile başlayan ifadeler değişken
        /// tanımı oluşturur.
        /// Parser bu ifadeyi okuyarak Pl1VariableDeclaration modeli üretmelidir.
        ///
        /// Örnek PL/I:
        ///
        /// DCL MUST_NO FIXED DECIMAL(8);
        /// DCL PARAM CHAR(08);
        /// DECLARE CUSTOMER_NAME CHARACTER(25);
        ///
        /// Bu method:
        /// - declaration başlangıcını okur
        /// - değişken adını okur
        /// - veri tipini ParseDataType methoduna devreder
        /// - statement sonundaki noktalı virgülü doğrular
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Parse ana akışı DclKeyword gördüğünde
        /// - PL/I SyntaxTree declaration listesi oluşturulurken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// INIT / INITIAL, array dimension, structure declaration ve çoklu
        /// declaration desteği eklendiğinde bu method genişletilecektir.
        /// Ancak veri tipi parse sorumluluğu ayrı methodlarda kalacaktır.
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
                dclToken.Location);
        }

        /// <summary>
        /// PL/I değişken tanımındaki veri tipini parse eder.
        ///
        /// Neden var?
        /// ----------------------
        /// DCL ifadesinde değişken adı okunduktan sonra gelen bölüm veri tipini
        /// temsil eder.
        /// Parser bu bölümü okuyarak PL/I syntax tree üzerinde güçlü tipli bir
        /// data type modeli üretmelidir.
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
        /// - DCL declaration parse edilirken
        /// - PL/I Syntax Tree oluşturulurken
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
        /// Bu yapı mevcut ilk POC kapsamının temel veri tipidir.
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
        /// Bu söz dizimi FIXED DECIMAL'dan farklı olduğu için ayrı bir parse
        /// methodu ile ele alınmalıdır.
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

        private void AddUnexpectedTokenDiagnostic(
            Pl1Token token,
            string expectedText)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Beklenmeyen token: {token.Text}. Beklenen: {expectedText}.",
                token.Location));
        }

        private Pl1Token Advance()
        {
            if (!IsAtEnd())
            {
                _position++;
            }

            return Previous;
        }

        private bool IsAtEnd()
        {
            return Current.Kind == Pl1TokenKind.EndOfFile;
        }

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

        private Pl1Token Previous => _tokens[_position - 1];
    }
}
