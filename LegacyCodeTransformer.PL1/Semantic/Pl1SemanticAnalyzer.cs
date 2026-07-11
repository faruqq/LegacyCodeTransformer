using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I syntax tree üzerinde semantic analysis yapan analyzer sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser yalnızca syntax model üretir. Duplicate declaration, symbol collection,
    /// reference resolution, type mismatch veya scope gibi anlam kontrolleri
    /// parser'ın sorumluluğu değildir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Global declaration symbol table üretir, duplicate declaration diagnostic'lerini
    /// toplar ve güvenli identifier reference kullanımlarını analiz eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// CUSTOMER_NO = MUST_NO;
    /// CALL FETCH_CUSTOMER(CUSTOMER_NO);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ConversionService pipeline içinde normalizer sonrasında çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Undefined identifier politikası, structure member resolution, type resolution
    /// ve scope analysis adımları bu sınıf üzerinden geliştirilecektir.
    /// </summary>
    public sealed class Pl1SemanticAnalyzer
    {
        public SemanticResult Analyze(Pl1SyntaxTree syntaxTree)
        {
            if (syntaxTree is null)
            {
                return new SemanticResult();
            }

            var diagnostics = new List<Diagnostic>();
            var symbolsByName = new Dictionary<string, Symbol>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var declaration in syntaxTree.Declarations)
            {
                var symbol = CreateSymbol(declaration);

                if (symbol is null)
                {
                    continue;
                }

                if (symbolsByName.ContainsKey(symbol.Name))
                {
                    diagnostics.Add(
                        SemanticDiagnosticFactory.DuplicateDeclaration(
                            symbol.Name,
                            declaration.Location));

                    continue;
                }

                symbolsByName.Add(symbol.Name, symbol);
            }

            var symbolTable = new SymbolTable(symbolsByName.Values);

            var referenceCollector = new Pl1ReferenceCollector(symbolTable);
            referenceCollector.Visit(syntaxTree);

            return new SemanticResult(
                diagnostics,
                symbolTable,
                referenceCollector.References);
        }

        private static Symbol? CreateSymbol(Pl1Declaration declaration)
        {
            return declaration switch
            {
                Pl1VariableDeclaration variableDeclaration =>
                    new Symbol(variableDeclaration.Name),

                Pl1StructureDeclaration structureDeclaration =>
                    new Symbol(structureDeclaration.Name),

                _ => null
            };
        }
    }
}