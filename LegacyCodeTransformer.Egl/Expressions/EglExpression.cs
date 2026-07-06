using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Expressions
{
    /// <summary>
    /// EGL expression modellerinin ortak temel sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL statement modelleri assignment target/value, CALL argument, IF condition
    /// ve DO condition gibi expression alanları taşıyacaktır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EGL expression ailesini güçlü tipli bir syntax node hiyerarşisi altında toplar.
    /// Böylece transpiler katmanı expression bilgisini string output olarak değil,
    /// EGL syntax modeli olarak taşıyabilir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     param
    ///     "ABC"
    ///     sqlCode == 0
    ///     recordField.customerNo
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// EglAssignmentStatement target/value alanlarında ve ileride CALL, IF, DO
    /// statement modellerinde kullanılacaktır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EglIdentifierExpression, EglLiteralExpression, EglBinaryExpression ve
    /// EglFunctionCallExpression gibi daha detaylı expression modelleri bu base type
    /// üzerinden eklenecektir.
    /// </summary>
    public abstract class EglExpression : SyntaxNode
    {
        protected EglExpression(SourceLocation location)
            : base(location)
        {
        }
    }
}