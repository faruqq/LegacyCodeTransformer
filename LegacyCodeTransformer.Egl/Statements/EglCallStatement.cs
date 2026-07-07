using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Statements
{
    /// <summary>
    /// EGL CALL statement modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.9 kapsamında PL/I CALL statement modellerinin EGL syntax tree üzerinde
    /// güçlü tipli statement modeli olarak taşınması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Procedure adı ile argument listesini EGL statement modeli üzerinde taşır.
    /// Bu aşamada expression AST üretilmez; argument değerleri string olarak korunur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     CALL FETCH_CURSOR;
    ///     CALL PROC1(A, B);
    ///
    /// EGL model:
    ///
    ///     ProcedureName: FetchCursor
    ///     Arguments: []
    ///
    ///     ProcedureName: Proc1
    ///     Arguments: [A, B]
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementTranspiler içinde Pl1CallStatement dönüşüm çıktısı olarak ve
    /// EglCodeGenerator içinde CALL output üretiminde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Full expression parser, named argument, OUT parameter veya service invocation
    /// desteği gerektiğinde bu model genişletilebilir.
    /// </summary>
    public sealed class EglCallStatement : EglStatement
    {
        public string ProcedureName { get; }

        public IReadOnlyList<string> Arguments { get; }

        public EglCallStatement(
            string procedureName,
            IEnumerable<string>? arguments,
            SourceLocation location)
            : base(location)
        {
            ProcedureName = procedureName;
            Arguments = arguments?.ToList() ?? new List<string>();
        }
    }
}