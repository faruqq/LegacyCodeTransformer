using System;
using System.Collections.Generic;
using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Lexing
{
    /// <summary>
    /// PL/I kaynak kodunu token listesine dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser doğrudan raw string üzerinden çalışmamalıdır.
    /// Önce kaynak metnin keyword, identifier, number, string literal, punctuation
    /// ve statement operator gibi anlamlı parçalara ayrılması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PL/I kaynak kodunu parser'ın okuyabileceği Pl1Token listesine çevirir.
    /// Declaration, statement, procedure ve embedded SQL parser için gerekli
    /// keyword/token desteğini üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM CHAR(08) INIT(' ');
    /// - PROCEDURE_NAME: PROCEDURE;
    /// - CALL FETCH_CURSOR;
    /// - IF SQLCODE = 0 THEN DO;
    /// - DO WHILE(SQLCODE ^= 100);
    /// - EXEC SQL INCLUDE SQLCA;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application conversion pipeline içinde
    /// - Parser testlerinde
    /// - Lexer testlerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser, expression parser, procedure parser, embedded SQL parser,
    /// compiler directive parser ve IDE syntax highlighting altyapısı bu lexer
    /// sözleşmesi üzerine genişletilecektir.
    /// </summary>
    public sealed class Pl1Lexer
    {
        private readonly string _source;
        private readonly List<Pl1Token> _tokens = new();
        private int _position;

        public Pl1Lexer(string source)
        {
            _source = source ?? string.Empty;
        }

        /// <summary>
        /// Kaynak PL/I metnini token listesine dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser'ın tüketebileceği sıralı token akışını üretir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Whitespace karakterlerini atlar, keyword / identifier ayrımını yapar,
        /// string literal ve number token'larını üretir, statement punctuation ve
        /// operator karakterlerini ayrı token olarak temsil eder.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// EXEC SQL INCLUDE SQLCA;
        ///
        /// Bu input için sırasıyla:
        /// - ExecKeyword
        /// - SqlKeyword
        /// - IncludeKeyword
        /// - Identifier
        /// - Semicolon
        ///
        /// tokenları üretilir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - ConversionService içinde parser öncesinde
        /// - Lexer ve parser unit testlerinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// P07 embedded SQL parser gerçek kaynak metinden EXEC SQL statement modeli
        /// üretirken bu token akışını kullanacaktır.
        /// </summary>
        public IReadOnlyList<Pl1Token> Tokenize()
        {
            while (!IsAtEnd())
            {
                var current = Current;

                if (char.IsWhiteSpace(current))
                {
                    Advance();
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    TokenizeIdentifierOrKeyword();
                    continue;
                }

                if (char.IsDigit(current))
                {
                    TokenizeNumber();
                    continue;
                }

                if (current == '\'')
                {
                    TokenizeStringLiteral();
                    continue;
                }

                switch (current)
                {
                    case '(':
                        AddToken(Pl1TokenKind.OpenParenthesis, current.ToString());
                        Advance();
                        break;

                    case ')':
                        AddToken(Pl1TokenKind.CloseParenthesis, current.ToString());
                        Advance();
                        break;

                    case ',':
                        AddToken(Pl1TokenKind.Comma, current.ToString());
                        Advance();
                        break;

                    case ';':
                        AddToken(Pl1TokenKind.Semicolon, current.ToString());
                        Advance();
                        break;

                    case '.':
                        AddToken(Pl1TokenKind.Dot, current.ToString());
                        Advance();
                        break;

                    case ':':
                        AddToken(Pl1TokenKind.Colon, current.ToString());
                        Advance();
                        break;

                    case '=':
                        AddToken(Pl1TokenKind.Equals, current.ToString());
                        Advance();
                        break;

                    case '+':
                        AddToken(Pl1TokenKind.Plus, current.ToString());
                        Advance();
                        break;

                    case '-':
                        AddToken(Pl1TokenKind.Minus, current.ToString());
                        Advance();
                        break;

                    case '*':
                        AddToken(Pl1TokenKind.Asterisk, current.ToString());
                        Advance();
                        break;

                    case '/':
                        AddToken(Pl1TokenKind.Slash, current.ToString());
                        Advance();
                        break;

                    case '<':
                        TokenizeLessThanOrLessThanOrEqual();
                        break;

                    case '>':
                        TokenizeGreaterThanOrGreaterThanOrEqual();
                        break;

                    case '^':
                        TokenizeCaretOperator();
                        break;

                    case '&':
                        AddToken(Pl1TokenKind.Ampersand, current.ToString());
                        Advance();
                        break;

                    case '!':
                        AddToken(Pl1TokenKind.Exclamation, current.ToString());
                        Advance();
                        break;

                    default:
                        AddToken(Pl1TokenKind.BadToken, current.ToString());
                        Advance();
                        break;
                }
            }

            _tokens.Add(new Pl1Token(
                Pl1TokenKind.EndOfFile,
                string.Empty,
                new SourceLocation(_position, 0, 0)));

            return _tokens;
        }

        private void TokenizeIdentifierOrKeyword()
        {
            var start = _position;

            while (!IsAtEnd() && (char.IsLetterOrDigit(Current) || Current == '_'))
            {
                Advance();
            }

            var text = _source.Substring(start, _position - start);
            var kind = GetKeywordKind(text);

            _tokens.Add(new Pl1Token(
                kind,
                text,
                new SourceLocation(start, 0, 0)));
        }

        private void TokenizeNumber()
        {
            var start = _position;

            while (!IsAtEnd() && char.IsDigit(Current))
            {
                Advance();
            }

            var text = _source.Substring(start, _position - start);

            _tokens.Add(new Pl1Token(
                Pl1TokenKind.Number,
                text,
                new SourceLocation(start, 0, 0)));
        }

        /// <summary>
        /// PL/I karakter sabitini string literal token olarak üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I INIT / INITIAL ifadelerinde ve statement expression'larında karakter
        /// sabitleri tek tırnak içinde yazılır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Kaynak metindeki tek tırnakları syntax işareti olarak kabul eder ve token
        /// text değerine yalnızca tırnak içindeki gerçek karakter değerini taşır.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - ' ' => " "
        /// - ';' => ";"
        /// - 'ABCD' => "ABCD"
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - INIT / INITIAL başlangıç değeri modellemesinde
        /// - Statement raw expression oluşturma aşamasında
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Escape edilmiş karakter sabitleri, quote restore davranışı veya daha
        /// gelişmiş string literal parse kuralları gerektiğinde bu method
        /// genişletilecektir.
        /// </summary>
        private void TokenizeStringLiteral()
        {
            var tokenStart = _position;

            Advance();

            var valueStart = _position;

            while (!IsAtEnd() && Current != '\'')
            {
                Advance();
            }

            var value = _source.Substring(
                valueStart,
                _position - valueStart);

            if (!IsAtEnd())
            {
                Advance();
            }

            _tokens.Add(new Pl1Token(
                Pl1TokenKind.StringLiteral,
                value,
                new SourceLocation(tokenStart, 0, 0)));
        }

        private void TokenizeLessThanOrLessThanOrEqual()
        {
            var start = _position;

            Advance();

            if (!IsAtEnd() && Current == '=')
            {
                Advance();

                _tokens.Add(new Pl1Token(
                    Pl1TokenKind.LessThanOrEqual,
                    "<=",
                    new SourceLocation(start, 0, 0)));

                return;
            }

            _tokens.Add(new Pl1Token(
                Pl1TokenKind.LessThan,
                "<",
                new SourceLocation(start, 0, 0)));
        }

        private void TokenizeGreaterThanOrGreaterThanOrEqual()
        {
            var start = _position;

            Advance();

            if (!IsAtEnd() && Current == '=')
            {
                Advance();

                _tokens.Add(new Pl1Token(
                    Pl1TokenKind.GreaterThanOrEqual,
                    ">=",
                    new SourceLocation(start, 0, 0)));

                return;
            }

            _tokens.Add(new Pl1Token(
                Pl1TokenKind.GreaterThan,
                ">",
                new SourceLocation(start, 0, 0)));
        }

        private void TokenizeCaretOperator()
        {
            var start = _position;

            Advance();

            if (!IsAtEnd() && Current == '=')
            {
                Advance();

                _tokens.Add(new Pl1Token(
                    Pl1TokenKind.NotEqual,
                    "^=",
                    new SourceLocation(start, 0, 0)));

                return;
            }

            if (!IsAtEnd() && Current == '<')
            {
                Advance();

                _tokens.Add(new Pl1Token(
                    Pl1TokenKind.NotLessThan,
                    "^<",
                    new SourceLocation(start, 0, 0)));

                return;
            }

            if (!IsAtEnd() && Current == '>')
            {
                Advance();

                _tokens.Add(new Pl1Token(
                    Pl1TokenKind.NotGreaterThan,
                    "^>",
                    new SourceLocation(start, 0, 0)));

                return;
            }

            _tokens.Add(new Pl1Token(
                Pl1TokenKind.Caret,
                "^",
                new SourceLocation(start, 0, 0)));
        }

        /// <summary>
        /// Identifier olarak okunan metnin PL/I keyword olup olmadığını belirler.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I kaynak kodunda keyword ve identifier söz dizimsel olarak benzer
        /// karakterlerden oluşur.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Lexer'ın identifier metnini parser'ın anlayacağı doğru token türüne
        /// dönüştürmesini sağlar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - DCL / DECLARE
        /// - FIXED DECIMAL
        /// - PIC / PICTURE
        /// - PROCEDURE / PROC
        /// - OPTIONS
        /// - CALL
        /// - IF / THEN / ELSE
        /// - DO / END
        /// - WHILE / UNTIL
        /// - EXEC SQL INCLUDE
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TokenizeIdentifierOrKeyword içerisinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Yeni PL/I keyword synonym destekleri eklendikçe bu mapping genişletilecektir.
        /// </summary>
        private static Pl1TokenKind GetKeywordKind(string text)
        {
            return text.ToUpperInvariant() switch
            {
                "DCL" => Pl1TokenKind.DclKeyword,
                "DECLARE" => Pl1TokenKind.DclKeyword,

                "FIXED" => Pl1TokenKind.FixedKeyword,
                "DECIMAL" => Pl1TokenKind.DecimalKeyword,
                "DEC" => Pl1TokenKind.DecKeyword,
                "BINARY" => Pl1TokenKind.BinaryKeyword,
                "BIN" => Pl1TokenKind.BinKeyword,

                "CHAR" => Pl1TokenKind.CharKeyword,
                "CHARACTER" => Pl1TokenKind.CharacterKeyword,
                "VARCHAR" => Pl1TokenKind.VarcharKeyword,

                "PIC" => Pl1TokenKind.PicKeyword,
                "PICTURE" => Pl1TokenKind.PictureKeyword,

                "INIT" => Pl1TokenKind.InitKeyword,
                "INITIAL" => Pl1TokenKind.InitialKeyword,

                "BIT" => Pl1TokenKind.BitKeyword,
                "DIM" => Pl1TokenKind.DimKeyword,
                "DIMENSION" => Pl1TokenKind.DimensionKeyword,

                "FLOAT" => Pl1TokenKind.FloatKeyword,
                "REAL" => Pl1TokenKind.RealKeyword,
                "DOUBLE" => Pl1TokenKind.DoubleKeyword,
                "PRECISION" => Pl1TokenKind.PrecisionKeyword,

                "PROCEDURE" => Pl1TokenKind.ProcedureKeyword,
                "PROC" => Pl1TokenKind.ProcedureKeyword,
                "OPTIONS" => Pl1TokenKind.OptionsKeyword,

                "CALL" => Pl1TokenKind.CallKeyword,
                "IF" => Pl1TokenKind.IfKeyword,
                "THEN" => Pl1TokenKind.ThenKeyword,
                "ELSE" => Pl1TokenKind.ElseKeyword,
                "DO" => Pl1TokenKind.DoKeyword,
                "END" => Pl1TokenKind.EndKeyword,
                "WHILE" => Pl1TokenKind.WhileKeyword,
                "UNTIL" => Pl1TokenKind.UntilKeyword,

                "EXEC" => Pl1TokenKind.ExecKeyword,
                "SQL" => Pl1TokenKind.SqlKeyword,
                "INCLUDE" => Pl1TokenKind.IncludeKeyword,

                _ => Pl1TokenKind.Identifier
            };
        }

        private void AddToken(
            Pl1TokenKind kind,
            string text)
        {
            _tokens.Add(new Pl1Token(
                kind,
                text,
                new SourceLocation(_position, 0, 0)));
        }

        private char Current => IsAtEnd() ? '\0' : _source[_position];

        private bool IsAtEnd()
        {
            return _position >= _source.Length;
        }

        private void Advance()
        {
            _position++;
        }
    }
}