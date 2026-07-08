using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Procedures;

/// <summary>
/// PL/I PROCEDURE bloğunu temsil eden syntax node modelidir.
///
/// Neden var?
/// PL/I tarafında business logic çoğunlukla PROCEDURE blokları içinde yer alır.
/// P06 ile birlikte bu blokların parser çıktısında güçlü tipli bir modelle
/// temsil edilmesi gerekir.
///
/// Ne çözüyor?
/// Procedure adını, procedure option bilgilerini ve procedure içindeki executable
/// statement listesini tek bir syntax node altında toplar.
///
/// Hangi örneği destekliyor?
/// PROCEDURE_NAME: PROCEDURE;
///     CALL OTHER_PROCEDURE;
/// END PROCEDURE_NAME;
///
/// PROGRAM_NAME: PROCEDURE OPTIONS(MAIN);
///     CALL INIT_PROCESS;
/// END PROGRAM_NAME;
///
/// Nerede kullanılır?
/// Procedure parser eklendiğinde Pl1SyntaxTree üzerinde taşınacak procedure
/// modellerinde kullanılır.
///
/// Gelecekte neye temel olur?
/// Procedure transpiler, procedure-level semantic analysis, main procedure
/// tespiti ve procedure statement traversal işlemleri için temel olur.
/// </summary>
public sealed class Pl1Procedure : SyntaxNode
{
    public string Name { get; }

    public IReadOnlyList<string> Options { get; }

    public IReadOnlyList<Pl1Statement> Statements { get; }

    public Pl1Procedure(
        string name,
        IEnumerable<string>? options,
        IEnumerable<Pl1Statement>? statements,
        SourceLocation location)
        : base(location)
    {
        Name = name;
        Options = options?.ToList() ?? new List<string>();
        Statements = statements?.ToList() ?? new List<Pl1Statement>();
    }
}