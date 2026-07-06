using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Statements
{
    /// <summary>
    /// EGL assignment statement modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.8 kapsamında PL/I assignment statement modellerinin EGL syntax tree üzerinde
    /// güçlü tipli statement modeli olarak taşınması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Assignment target ve value alanlarını EGL statement modeli üzerinde taşır.
    /// Bu aşamada expression AST üretilmez; çünkü full expression parser henüz yoktur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     PARAM = 'ABC';
    ///
    /// EGL model:
    ///
    ///     Target: Param
    ///     Value: "ABC"
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementTranspiler içinde Pl1AssignmentStatement dönüşüm çıktısı olarak ve
    /// EglCodeGenerator içinde assignment output üretiminde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Full expression parser geldiğinde Target ve Value alanları string yerine
    /// expression modeliyle revize edilebilir.
    /// </summary>
    public sealed class EglAssignmentStatement : EglStatement
    {
        public string Target { get; }

        public string Value { get; }

        public EglAssignmentStatement(
            string target,
            string value,
            SourceLocation location)
            : base(location)
        {
            Target = target;
            Value = value;
        }
    }
}