using LegacyCodeTransformer.Core.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyCodeTransformer.Egl.Types
{
    /// <summary>
    /// EGL veri tipleri için temel sınıftır.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL içerisinde decimal, string, int, boolean gibi farklı veri tipleri
    /// bulunacaktır.
    /// Bu tiplerin ortak bir temel sınıf üzerinden temsil edilmesi için
    /// oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - EGL değişken declaration modellerinde
    /// - EGL Code Generator içerisinde
    /// - Transpiler tarafından hedef tip üretiminde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Yeni EGL veri tipleri eklendikçe bu sınıftan türeyen yeni modeller
    /// oluşturulacaktır.
    /// </summary>
    public abstract class EglDataType : SyntaxNode
    {
        protected EglDataType(SourceLocation location)
            : base(location)
        {
        }
    }
}
