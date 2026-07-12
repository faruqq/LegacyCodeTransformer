using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Statements
{
    /// <summary>
    /// PL/I CALL dönüşümünden üretilen EGL function invocation statement
    /// modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I CALL statement modellerinin EGL syntax tree üzerinde güçlü tipli
    /// bir function invocation modeli olarak taşınması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Çağrılacak function adını ve argument listesini EGL statement modeli
    /// üzerinde taşır.
    ///
    /// Mevcut sınıf adı geçmişte EglCallStatement olarak belirlenmiştir.
    /// Ancak generator bu modeli EGL `call` keyword'ü olarak değil,
    /// doğrudan function invocation olarak üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    /// CALL FETCH_CURSOR;
    /// CALL PROC1(A, B);
    ///
    /// EGL:
    ///
    /// FetchCursor();
    /// Proc1(A, B);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementTranspiler içinde Pl1CallStatement dönüşüm çıktısı olarak ve
    /// EglCodeGenerator içinde function invocation üretiminde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Full expression parser, parameter direction, service invocation veya
    /// model adı refactor'u gerektiğinde kontrollü biçimde genişletilebilir.
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
            Arguments = arguments?.ToList() ??
                new List<string>();
        }
    }
}