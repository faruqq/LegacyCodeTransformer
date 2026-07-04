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
    /// Önce kaynak metnin keyword, identifier, number, string literal ve
    /// punctuation gibi anlamlı parçalara ayrılması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PL/I kaynak kodunu parser'ın okuyabileceği Pl1Token listesine çevirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL MUST_NO FIXED DECIMAL(8);
    /// - DCL PARAM CHAR(08) INIT(' ');
    /// - DCL CUSTOMER_NAME VARCHAR(50);
    /// - DCL 1 DIZI(6), 3 DIZI_PARAM1 CHAR(01);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application conversion pipeline içinde
    /// - Parser testlerinde
    /// - Lexer testlerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Yeni PL/I keyword, operator, comment, compiler directive ve statement
    /// destekleri eklendikçe ilk genişletilecek katmandır.
    /// </summary>
    public sealed class Pl1Lexer
    {
        private readonly string _source;
        private readonly List<Pl1Token> _tokens = new();

        private int _position;

        /// <summary>
        /// PL/I lexer instance'ını oluşturur.
        /// </summary>
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
        /// Whitespace karakterlerini atlar, keyword / identifier ayrımını yapar
        /// ve statement punctuation karakterlerini ayrı token olarak üretir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// DCL CUSTOMER_NAME VARCHAR(50);
        ///
        /// Bu input için sırasıyla:
        /// - DclKeyword
        /// - Identifier
        /// - VarcharKeyword
        /// - OpenParenthesis
        /// - Number
        /// - CloseParenthesis
        /// - Semicolon
        ///
        /// tokenları üretilir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - ConversionService içinde parser öncesinde
        /// - Lexer ve parser unit testlerinde
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

                    case '*':
                        AddToken(Pl1TokenKind.Asterisk, current.ToString());
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

            while (!IsAtEnd() &&
                   (char.IsLetterOrDigit(Current) || Current == '_'))
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

            while (!IsAtEnd() &&
                   char.IsDigit(Current))
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
        /// PL/I INIT / INITIAL ifadelerinde karakter sabitleri tek tırnak içinde
        /// yazılır.
        ///
        /// Örnek PL/I:
        ///
        /// INIT(' ')
        /// INIT(';')
        /// INITIAL('ABCD')
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Kaynak metindeki tek tırnakları syntax işareti olarak kabul eder ve
        /// token text değerine yalnızca tırnak içindeki gerçek karakter değerini
        /// taşır.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - ' '   => " "
        /// - ';'   => ";"
        /// - 'ABCD' => "ABCD"
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - ParseOptionalInitialValue içerisinde
        /// - INIT / INITIAL başlangıç değeri modellemesinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Escape edilmiş karakter sabitleri veya daha gelişmiş string literal
        /// parse kuralları gerektiğinde bu method genişletilecektir.
        /// </summary>
        private void TokenizeStringLiteral()
        {
            var tokenStart = _position;

            Advance();

            var valueStart = _position;

            while (!IsAtEnd() &&
                   Current != '\'')
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
        /// - FIXED DECIMAL
        /// - FIXED DEC
        /// - DECIMAL FIXED
        /// - DEC FIXED
        /// - FIXED BINARY
        /// - FIXED BIN
        /// - BINARY FIXED
        /// - BIN FIXED
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TokenizeIdentifierOrKeyword içerisinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// PIC / PICTURE, DIM / DIMENSION gibi keyword synonym destekleri
        /// eklendikçe bu mapping genişletilecektir.
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
                "INIT" => Pl1TokenKind.InitKeyword,
                "INITIAL" => Pl1TokenKind.InitialKeyword,
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

        private char Current =>
            IsAtEnd()
                ? '\0'
                : _source[_position];

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