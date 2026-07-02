using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Lexing
{
    /// <summary>
    /// PL/I kaynak kodunu token listesine dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser'ın ham kaynak metin üzerinde karakter karakter çalışmasını
    /// engellemek için oluşturulmuştur.
    ///
    /// Lexer, kaynak kodu küçük ve anlamlı parçalara ayırır.
    /// Parser bu parçaları kullanarak SyntaxTree oluşturur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser öncesinde
    /// - Application pipeline içerisinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I desteği büyüdükçe yeni keyword, operator ve sembollerin tamamı
    /// bu sınıf üzerinden token'lara dönüştürülecektir.
    ///
    /// İlk sürümde yalnızca DCL MUST_NO FIXED DECIMAL(8); hedefini
    /// destekleyecek kadar token üretir.
    /// </summary>
    public sealed class Pl1Lexer
    {
        private readonly string _source;

        private int _position;
        private int _line = 1;
        private int _column = 1;

        public Pl1Lexer(string source)
        {
            _source = source ?? string.Empty;
        }

        public IReadOnlyList<Pl1Token> Tokenize()
        {
            var tokens = new List<Pl1Token>();

            while (!IsAtEnd())
            {
                var current = Current;

                if (char.IsWhiteSpace(current))
                {
                    ReadWhiteSpace();
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                    continue;
                }

                if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber());
                    continue;
                }

                tokens.Add(current switch
                {
                    '(' => CreateSingleCharacterToken(Pl1TokenKind.OpenParenthesis),
                    ')' => CreateSingleCharacterToken(Pl1TokenKind.CloseParenthesis),
                    ';' => CreateSingleCharacterToken(Pl1TokenKind.Semicolon),
                    ',' => CreateSingleCharacterToken(Pl1TokenKind.Comma),
                    _ => CreateSingleCharacterToken(Pl1TokenKind.Unknown)
                });
            }

            tokens.Add(new Pl1Token(
                Pl1TokenKind.EndOfFile,
                string.Empty,
                GetCurrentLocation()));

            return tokens;
        }

        private char Current => IsAtEnd() ? '\0' : _source[_position];

        private bool IsAtEnd()
        {
            return _position >= _source.Length;
        }

        private void ReadWhiteSpace()
        {
            while (!IsAtEnd() && char.IsWhiteSpace(Current))
            {
                Advance();
            }
        }

        private Pl1Token ReadIdentifierOrKeyword()
        {
            var startPosition = _position;
            var location = GetCurrentLocation();

            while (!IsAtEnd() && (char.IsLetterOrDigit(Current) || Current == '_'))
            {
                Advance();
            }

            var text = _source[startPosition.._position];
            var upperText = text.ToUpperInvariant();

            var kind = upperText switch
            {
                "DCL" => Pl1TokenKind.DclKeyword,
                "FIXED" => Pl1TokenKind.FixedKeyword,
                "DECIMAL" => Pl1TokenKind.DecimalKeyword,
                _ => Pl1TokenKind.Identifier
            };

            return new Pl1Token(kind, text, location);
        }

        private Pl1Token ReadNumber()
        {
            var startPosition = _position;
            var location = GetCurrentLocation();

            while (!IsAtEnd() && char.IsDigit(Current))
            {
                Advance();
            }

            var text = _source[startPosition.._position];

            return new Pl1Token(Pl1TokenKind.Number, text, location);
        }

        private Pl1Token CreateSingleCharacterToken(Pl1TokenKind kind)
        {
            var location = GetCurrentLocation();
            var text = Current.ToString();

            Advance();

            return new Pl1Token(kind, text, location);
        }

        private void Advance()
        {
            if (IsAtEnd())
            {
                return;
            }

            if (Current == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }

            _position++;
        }

        private SourceLocation GetCurrentLocation()
        {
            return new SourceLocation(_line, _column, _position);
        }
    }
}
