using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I syntax tree üzerindeki güvenli symbol reference kullanımlarını toplar.
    ///
    /// Neden var?
    /// ----------------------
    /// Pl1SemanticAnalyzer symbol table üretmenin yanında kaynak kodda kullanılan
    /// identifier reference bilgilerini de analiz etmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Pl1SyntaxWalker traversal altyapısını kullanarak Pl1RawExpression modellerini
    /// ziyaret eder. Yalnızca güvenle sınıflandırılabilen basit identifier ve qualified
    /// identifier ifadelerini reference olarak toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_NO = MUST_NO;
    ///
    /// CALL FETCH_CUSTOMER(CUSTOMER_NO);
    ///
    /// CUSTOMER_INFO.MUST_NO = CUSTOMER_NO;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer içinde symbol table oluşturulduktan sonra kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Full expression modelleri eklendiğinde binary expression, function call,
    /// array access ve structure member reference analizleri aynı collector
    /// davranışı üzerinden genişletilebilir.
    /// </summary>
    internal sealed class Pl1ReferenceCollector : Pl1SyntaxWalker
    {
        private readonly SymbolTable _symbolTable;
        private readonly List<Pl1SymbolReference> _references = new();

        public IReadOnlyList<Pl1SymbolReference> References => _references;

        public Pl1ReferenceCollector(SymbolTable symbolTable)
        {
            _symbolTable = symbolTable;
        }

        protected override void VisitRawExpression(Pl1RawExpression expression)
        {
            if (!TryGetRootSymbolName(
                    expression.Text,
                    out var rootSymbolName))
            {
                return;
            }

            var referenceText = expression.Text.Trim();
            var isResolved = _symbolTable.Symbols.ContainsKey(rootSymbolName);

            _references.Add(
                new Pl1SymbolReference(
                    referenceText,
                    rootSymbolName,
                    isResolved,
                    expression.Location));
        }

        /// <summary>
        /// Raw expression metninden güvenli biçimde root symbol adı çıkarmaya çalışır.
        ///
        /// Neden var?
        /// ----------------------
        /// Full expression parser henüz bulunmadığı için operator içeren karmaşık
        /// ifadeleri semantic olarak yorumlamak güvenli değildir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Yalnızca aşağıdaki iki güvenli formu kabul eder:
        ///
        /// IDENTIFIER
        ///
        /// IDENTIFIER.MEMBER
        ///
        /// Operator, literal, function call veya array access içeren ifadeleri
        /// bilinçli olarak analiz kapsamı dışında bırakır.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// CUSTOMER_NO
        ///
        /// CUSTOMER_INFO.MUST_NO
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// VisitRawExpression methodunda reference classification için kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Expression tree eklendiğinde bu geçici raw text sınıflandırması daha güçlü
        /// expression node analizleriyle değiştirilebilir.
        /// </summary>
        private static bool TryGetRootSymbolName(
            string expressionText,
            out string rootSymbolName)
        {
            rootSymbolName = string.Empty;

            if (string.IsNullOrWhiteSpace(expressionText))
            {
                return false;
            }

            var segments = expressionText
                .Split(
                    '.',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            if (segments.Length == 0 ||
                segments.Any(segment => !IsIdentifier(segment)))
            {
                return false;
            }

            rootSymbolName = segments[0];

            return true;
        }

        private static bool IsIdentifier(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (!char.IsLetter(text[0]) && text[0] != '_')
            {
                return false;
            }

            return text
                .Skip(1)
                .All(character =>
                    char.IsLetterOrDigit(character) ||
                    character == '_');
        }
    }
}