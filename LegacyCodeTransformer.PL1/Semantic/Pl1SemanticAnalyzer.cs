using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I syntax tree üzerinde semantic analysis yapan analyzer
    /// sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser yalnızca syntax model üretir. Duplicate declaration,
    /// symbol collection, reference resolution ve procedure parameter
    /// binding gibi anlam kontrolleri parser'ın sorumluluğu değildir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Global declaration symbol table üretir, duplicate declaration
    /// diagnostic'lerini toplar, güvenli identifier reference
    /// kullanımlarını analiz eder ve procedure header parameter
    /// adlarını body declaration modelleriyle eşleştirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL ERROR_TEXT CHAR(50);
    ///
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ConversionService pipeline içinde normalizer sonrasında
    /// çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL function parameter type mapping, parameter direction
    /// analizi, procedure scope ve procedure call resolution bu sınıf
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

            var symbolsByName =
                new Dictionary<string, Symbol>(
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
                        SemanticDiagnosticFactory
                            .DuplicateDeclaration(
                                symbol.Name,
                                declaration.Location));

                    continue;
                }

                symbolsByName.Add(
                    symbol.Name,
                    symbol);
            }

            var symbolTable =
                new SymbolTable(symbolsByName.Values);

            var referenceCollector =
                new Pl1ReferenceCollector(symbolTable);

            referenceCollector.Visit(syntaxTree);

            var procedureParameterBindings =
                CreateProcedureParameterBindings(
                    syntaxTree.Procedures);

            return new SemanticResult(
                diagnostics,
                symbolTable,
                referenceCollector.References,
                procedureParameterBindings);
        }

        /// <summary>
        /// Procedure header parameter adlarını procedure body içindeki
        /// variable declaration modelleriyle eşleştirir.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I procedure header parameter adının veri tipi doğrudan
        /// header üzerinde bulunmayabilir. Veri tipi body içindeki DCL
        /// declaration üzerinden tanımlanabilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Her header parametresi için aynı procedure içindeki
        /// case-insensitive eşleşen variable declaration modelini bulur.
        /// Eşleşme yoksa parameter bilgisini unresolved olarak korur.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
        ///     DCL PROCESS_TEXT CHAR(50);
        /// END CUSTOMER_PROCESS;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Analyze methodu içinde semantic result hazırlanırken
        /// kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Missing parameter declaration diagnostic'i, parameter type
        /// mapping ve in / out / inOut analizine temel olur.
        /// </summary>
        private static IReadOnlyList<
            Pl1ProcedureParameterBinding>
            CreateProcedureParameterBindings(
                IEnumerable<Pl1Procedure> procedures)
        {
            var bindings =
                new List<Pl1ProcedureParameterBinding>();

            foreach (var procedure in procedures)
            {
                var declarationsByName =
                    procedure.Declarations
                        .OfType<Pl1VariableDeclaration>()
                        .GroupBy(
                            declaration => declaration.Name,
                            StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            group => group.Key,
                            group => group.First(),
                            StringComparer.OrdinalIgnoreCase);

                foreach (var parameterName in procedure.Parameters)
                {
                    declarationsByName.TryGetValue(
                        parameterName,
                        out var declaration);

                    bindings.Add(
                        new Pl1ProcedureParameterBinding(
                            procedure.Name,
                            parameterName,
                            declaration));
                }
            }

            return bindings;
        }

        private static Symbol? CreateSymbol(
            Pl1Declaration declaration)
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