using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I syntax tree üzerinde semantic analysis yapacak analyzer sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser yalnızca syntax model üretir. Duplicate declaration, undefined
    /// identifier, type mismatch veya scope gibi anlam kontrolleri parser'ın
    /// sorumluluğu değildir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PL/I syntax tree ile transpiler arasına semantic analysis katmanını yerleştirir.
    /// P09.2 kapsamında global declaration'lardan symbol table foundation üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// Bu input için MUST_NO ve CUSTOMER_NO sembolleri üretilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ConversionService pipeline içinde normalizer sonrasında çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Duplicate declaration diagnostics, basic reference analysis ve type resolution
    /// adımları bu sınıf üzerinden geliştirilecektir.
    /// </summary>
    public sealed class Pl1SemanticAnalyzer
    {
        public SemanticResult Analyze(Pl1SyntaxTree syntaxTree)
        {
            if (syntaxTree is null)
            {
                return new SemanticResult();
            }

            var symbols = syntaxTree.Declarations
                .Select(CreateSymbol)
                .Where(symbol => symbol is not null)
                .Cast<Symbol>()
                .ToList();

            var symbolTable = new SymbolTable(symbols);

            return new SemanticResult(
                diagnostics: null,
                symbolTable);
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