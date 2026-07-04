using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Declarations
{
    /// <summary>
    /// EGL değişken tanımını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I declaration ifadeleri hedef dilde EGL değişken declaration
    /// modeline dönüştürülecektir.
    ///
    /// Örnek EGL kodu:
    ///
    /// mustNo decimal(8);
    ///
    /// Bu modelde:
    /// - Name: mustNo
    /// - DataType: EglDecimalType
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler çıktısında
    /// - EGL Code Generator içerisinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İleride string, int, boolean, record veya daha karmaşık EGL tipleri
    /// aynı declaration modeli üzerinden üretilebilir.
    /// </summary>
    public sealed class EglVariableDeclaration : EglDeclaration
    {
        /// <summary>
        /// EGL değişken adıdır.
        /// Örneğin: mustNo
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// EGL değişken tipidir.
        /// Örneğin: decimal(8,0)
        /// </summary>
        public EglDataType DataType { get; }

        public EglVariableDeclaration(
            string name,
            EglDataType dataType,
            SourceLocation location)
            : base(location)
        {
            Name = name;
            DataType = dataType;
        }
    }
}
