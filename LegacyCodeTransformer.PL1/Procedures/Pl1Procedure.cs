using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Procedures;

/// <summary>
/// PL/I PROCEDURE bloğunu temsil eden syntax node modelidir.
///
/// Neden var?
/// ----------------------
/// PL/I tarafında business logic çoğunlukla PROCEDURE blokları içinde
/// yer alır. Procedure header bilgileri, procedure içindeki declaration
/// modelleri ve executable statement'lar kaybedilmeden korunmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// Procedure adını, ordered parameter listesini, option listesini,
/// procedure içindeki declaration modellerini ve executable statement
/// listesini tek bir güçlü tipli syntax node üzerinde taşır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
///     DCL PROCESS_TEXT CHAR(50);
///
///     ERROR_TEXT = PROCESS_TEXT;
///     CALL WRITE_ERROR(ERROR_TEXT);
/// END CUSTOMER_PROCESS;
///
/// Nerede kullanılır?
/// ----------------------
/// ProcedureParser çıktısı olarak Pl1SyntaxTree.Procedures listesinde
/// kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Header parameter ile parameter declaration eşleştirmesi, procedure
/// scope, local symbol table, parameter type/direction analizi ve
/// PL/I procedure → EGL function dönüşümü bu model üzerinden
/// geliştirilecektir.
/// </summary>
public sealed class Pl1Procedure : SyntaxNode
{
    public string Name { get; }

    public IReadOnlyList<string> Parameters { get; }

    public IReadOnlyList<string> Options { get; }

    public IReadOnlyList<Pl1Declaration> Declarations { get; }

    public IReadOnlyList<Pl1Statement> Statements { get; }

    public Pl1Procedure(
        string name,
        IEnumerable<string>? options,
        IEnumerable<Pl1Statement>? statements,
        SourceLocation location,
        IEnumerable<string>? parameters = null,
        IEnumerable<Pl1Declaration>? declarations = null)
        : base(location)
    {
        Name = name;
        Parameters = parameters?.ToList() ?? new List<string>();
        Options = options?.ToList() ?? new List<string>();
        Declarations = declarations?.ToList() ??
            new List<Pl1Declaration>();
        Statements = statements?.ToList() ??
            new List<Pl1Statement>();
    }
}