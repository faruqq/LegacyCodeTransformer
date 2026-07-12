using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Procedures;

/// <summary>
/// PL/I PROCEDURE bloğunu temsil eden syntax node modelidir.
///
/// Neden var?
/// ----------------------
/// PL/I tarafında business logic çoğunlukla PROCEDURE blokları içinde
/// yer alır. Procedure adı, header parameter listesi, option bilgileri
/// ve executable statement'lar tek bir güçlü tipli modelde korunmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// Parametresiz ve parametreli PL/I procedure header bilgilerini,
/// procedure option listesini ve body statement listesini kaybetmeden
/// syntax tree üzerinde taşır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// PROCEDURE_NAME: PROCEDURE;
///     CALL OTHER_PROCEDURE;
/// END PROCEDURE_NAME;
///
/// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
///     ERROR_TEXT = PROCESS_TEXT;
/// END CUSTOMER_PROCESS;
///
/// PROGRAM_NAME: PROCEDURE(ARG1, ARG2) OPTIONS(MAIN);
///     CALL INIT_PROCESS;
/// END PROGRAM_NAME;
///
/// Nerede kullanılır?
/// ----------------------
/// ProcedureParser çıktısı olarak Pl1SyntaxTree.Procedures listesinde
/// kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Procedure parameter declaration eşleştirmesi, parameter scope,
/// parameter type/direction analizi ve PL/I procedure → EGL function
/// dönüşümü bu model üzerinden geliştirilecektir.
/// </summary>
public sealed class Pl1Procedure : SyntaxNode
{
    public string Name { get; }

    public IReadOnlyList<string> Parameters { get; }

    public IReadOnlyList<string> Options { get; }

    public IReadOnlyList<Pl1Statement> Statements { get; }

    public Pl1Procedure(
        string name,
        IEnumerable<string>? options,
        IEnumerable<Pl1Statement>? statements,
        SourceLocation location,
        IEnumerable<string>? parameters = null)
        : base(location)
    {
        Name = name;
        Parameters = parameters?.ToList() ?? new List<string>();
        Options = options?.ToList() ?? new List<string>();
        Statements = statements?.ToList() ?? new List<Pl1Statement>();
    }
}