using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types
{
    /// <summary>
    /// PL/I veri tipleri için temel sınıftır.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I içerisinde FIXED DECIMAL, CHAR, BIT, BINARY gibi farklı veri tipleri
    /// bulunur. Bu tiplerin ortak bir temel sınıf üzerinden temsil edilmesi için
    /// oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I değişken declaration modellerinde
    /// - PL/I Parser tarafından veri tipi üretiminde
    /// - Transpiler katmanında PL/I tiplerinin hedef dile çevrilmesinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Yeni PL/I veri tipleri eklendikçe bu sınıftan türeyen yeni modeller
    /// oluşturulacaktır.
    ///
    /// Örneğin:
    /// - Pl1CharType
    /// - Pl1FixedBinaryType
    /// - Pl1BitType
    /// - Pl1StructureType
    /// </summary>
    public abstract class Pl1DataType : SyntaxNode
    {
        protected Pl1DataType(SourceLocation location)
            : base(location)
        {
        }
    }
}
