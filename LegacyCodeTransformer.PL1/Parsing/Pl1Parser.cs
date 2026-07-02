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

        private Pl1VariableDeclaration? ParseVariableDeclaration()
        {
            var dclToken = Consume(
                Pl1TokenKind.DclKeyword,
                "DCL bekleniyordu.");

            var identifierToken = Consume(
                Pl1TokenKind.Identifier,
                "Değişken adı bekleniyordu.");

            Consume(
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

            Consume(
                Pl1TokenKind.Semicolon,
                "';' bekleniyordu.");

            if (identifierToken is null || precisionToken is null)
            {
                return null;
            }

            var precision = 0;

            if (!int.TryParse(precisionToken.Text, out precision))
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    $"Precision değeri sayısal olmalıdır: {precisionToken.Text}",
                    precisionToken.Location));

                return null;
            }

            var dataType = new Pl1FixedDecimalType(
                precision,
                scale,
                precisionToken.Location);

            return new Pl1VariableDeclaration(
                identifierToken.Text,
                dataType,
                dclToken?.Location ?? SourceLocation.Unknown);
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
