using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Declarations
{
    /// <summary>
    /// EGL declaration modellerinin ortak temel sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL tarafında yalnızca tekil değişken declaration üretmeyeceğiz.
    /// PL/I structure desteği ile birlikte EGL record declaration da
    /// üretilecektir.
    ///
    /// Örnek EGL variable declaration:
    ///
    /// MustNo decimal(8,0);
    ///
    /// Örnek EGL record declaration:
    ///
    /// record ParameList type basicRecord
    ///     10 Param char(8);
    ///     10 Param2 char(1);
    /// end
    ///
    /// Bu iki yapı da EGL declaration ailesine aittir ancak farklı
    /// modellerle temsil edilmelidir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - EglSyntaxTree declaration listesinde
    /// - EglVariableDeclaration temel sınıfı olarak
    /// - EglRecordDeclaration temel sınıfı olarak
    /// - EglCodeGenerator declaration dispatch işleminde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// EGL service, function, record, program ve library declaration
    /// modelleri de bu temel sınıf altında toplanabilir.
    /// </summary>
    public abstract class EglDeclaration : SyntaxNode
    {
        /// <summary>
        /// EGL declaration temel modelini oluşturur.
        /// </summary>
        protected EglDeclaration(SourceLocation location)
            : base(location)
        {
        }
    }
}