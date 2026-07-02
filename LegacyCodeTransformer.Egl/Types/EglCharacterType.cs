using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Types
{
    /// <summary>
    /// EGL char veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafındaki CHAR(n) ve CHARACTER(n) tanımları, bu projede
    /// EGL tarafında char(n) olarak üretilecektir.
    ///
    /// Bu nedenle hedef syntax tree üzerinde EGL karakter tipini temsil eden
    /// ayrı bir modele ihtiyaç vardır.
    ///
    /// Örnek EGL:
    ///
    /// param char(8);
    /// customerName char(25);
    ///
    /// Bu modelde:
    /// - Length: 8
    /// - Length: 25
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler çıktısında
    /// - EGL Code Generator içerisinde char(n) kaynak kodu üretiminde
    /// - Application katmanındaki uçtan uca dönüşüm sonucunda
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// EGL record field üretimi desteklendiğinde aşağıdaki gibi alanlarda
    /// bu tip kullanılacaktır:
    ///
    /// 10 param1 char(32);
    /// 10 processCode char(6);
    ///
    /// Ayrıca PL/I structure field dönüşümlerinde de hedef field tipi olarak
    /// bu model kullanılacaktır.
    /// </summary>
    public sealed class EglCharacterType : EglDataType
    {
        /// <summary>
        /// EGL char alanının sabit uzunluğudur.
        ///
        /// Örneğin:
        /// - char(8) için 8
        /// - char(25) için 25
        ///
        /// olarak tutulur.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// EGL char veri tipi modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Transpiler, PL/I CHAR / CHARACTER veri tipini EGL tarafındaki
        /// güçlü tipli karşılığına dönüştürmelidir.
        ///
        /// Örnek:
        ///
        /// PL/I:
        /// DCL PARAM CHAR(08);
        ///
        /// EGL:
        /// param char(8);
        ///
        /// Bu constructor, hedef syntax tree üzerinde char uzunluğunu ve
        /// kaynak konum bilgisini korumak için kullanılır.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I → EGL Transpiler içerisinde
        /// - EGL Code Generator içerisinde
        /// - Unit testlerde veri tipi doğrulamada
        /// </summary>
        public EglCharacterType(
            int length,
            SourceLocation location)
            : base(location)
        {
            Length = length;
        }
    }
}