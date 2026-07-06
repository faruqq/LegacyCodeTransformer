using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Statements
{
    /// <summary>
    /// EGL executable statement modellerinin ortak temel sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// Transpiler katmanı PL/I statement modellerini doğrudan string'e çevirmeyecektir.
    /// Önce EGL tarafında güçlü tipli statement syntax modelleri üretilecektir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Assignment, CALL, IF, DO ve ileride eklenecek diğer EGL statement modellerini
    /// ortak bir base type altında toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     PARAM = 'ABC';
    ///
    /// İleride EGL tarafında:
    ///
    ///     param = "ABC";
    ///
    /// bu statement ailesi altında temsil edilecektir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// EglSyntaxTree.Statements listesinde ve statement generator katmanında kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EglAssignmentStatement, EglCallStatement, EglIfStatement ve EglDoStatement
    /// modellerine temel olur.
    /// </summary>
    public abstract class EglStatement : SyntaxNode
    {
        protected EglStatement(SourceLocation location)
            : base(location)
        {
        }
    }
}