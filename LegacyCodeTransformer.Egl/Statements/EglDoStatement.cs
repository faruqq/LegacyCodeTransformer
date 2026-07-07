using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Statements
{
    /// <summary>
    /// EGL DO / loop statement modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.11 kapsamında PL/I DO statement modellerinin EGL syntax tree üzerinde
    /// güçlü tipli statement modeli olarak taşınması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// DO türünü, optional condition bilgisini ve body içindeki child statement listesini
    /// EGL statement modeli üzerinde taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     DO;
    ///         CALL PROC1;
    ///     END;
    ///
    ///     DO WHILE(SQLCODE = 0);
    ///         CALL FETCH_CURSOR;
    ///     END;
    ///
    /// EGL model:
    ///
    ///     Kind: Block / While / Until
    ///     Condition: Sqlcode = 0
    ///     Statements: EglStatement[]
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementTranspiler içinde Pl1DoStatement dönüşüm çıktısı olarak ve
    /// EglCodeGenerator içinde DO / loop output üretiminde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested block, IF THEN DO, ELSE DO, DO WHILE ve DO UNTIL output üretimleri
    /// bu model üzerinden ilerleyecektir.
    /// </summary>
    public sealed class EglDoStatement : EglStatement
    {
        public EglDoStatementKind Kind { get; }

        public string? Condition { get; }

        public IReadOnlyList<EglStatement> Statements { get; }

        public EglDoStatement(
            EglDoStatementKind kind,
            string? condition,
            IEnumerable<EglStatement>? statements,
            SourceLocation location)
            : base(location)
        {
            Kind = kind;
            Condition = condition;
            Statements = statements?.ToList() ?? new List<EglStatement>();
        }
    }
}