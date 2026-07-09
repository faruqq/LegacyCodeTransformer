using LegacyCodeTransformer.Core.Diagnostics;
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
    /// P09.3 kapsamında global declaration duplicate kontrollerini semantic diagnostic
    /// olarak üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL MUST_NO CHAR(8);
    ///
    /// Bu input için duplicate declaration diagnostic'i üretilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ConversionService pipeline içinde normalizer sonrasında çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Basic reference analysis, type resolution ve scope analysis adımları bu sınıf
    /// üzerinden geliştirilecektir.
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

            return new SemanticResult(
                diagnostics,
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