using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Declarations
{
    /// <summary>
    /// EGL record içerisindeki field declaration bilgisini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure member alanları EGL tarafında record field olarak
    /// üretilecektir.
    ///
    /// Örnek EGL:
    ///
    /// 10 Param char(8);
    /// 10 Param2 char(1);
    ///
    /// Bu modelde:
    /// - Level: 10
    /// - Name: Param
    /// - DataType: EglCharacterType
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - EglRecordDeclaration Fields listesinde
    /// - PL/I → EGL Transpiler structure mapping işleminde
    /// - EGL Code Generator record field üretiminde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Field description metadata, nullable bilgisi, default value ve
    /// field-level annotation desteği geldiğinde bu model genişletilecektir.
    /// </summary>
    public sealed class EglRecordFieldDeclaration : SyntaxNode
    {
        /// <summary>
        /// EGL record field seviye numarasıdır.
        ///
        /// İlk kapsamda sabit olarak 10 üretilecektir.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// EGL record field adıdır.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// EGL record field veri tipidir.
        /// </summary>
        public EglDataType DataType { get; }

        /// <summary>
        /// EGL field array boyutudur.
        ///
        /// Neden var?
        /// ----------------------
        /// EGL basicRecord yapılarında array alanlar veri tipinden sonra köşeli
        /// parantez ile ifade edilir.
        ///
        /// Örnek:
        ///
        /// 5 ARR char(80)[999];
        ///
        /// Bu örnekte:
        /// - Field name: ARR
        /// - DataType: char(80)
        /// - ArraySize: 999
        ///
        /// PL/I tarafındaki `DCL 1 DIZI(6)` gibi structure array tanımları
        /// EGL tarafında parent array field olarak üretileceği için bu bilgi
        /// record field modelinde taşınmalıdır.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - EglCodeGenerator içinde `[n]` suffix üretiminde
        /// - PL/I → EGL Transpiler structure array mapping işleminde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Field-level array mapping desteği geldiğinde aşağıdaki gibi alanlar da
        /// aynı property üzerinden üretilebilecektir:
        ///
        /// 10 AH char(10)[2];
        /// 10 AK num(9,4)[2];
        /// </summary>
        public int? ArraySize { get; }

        /// <summary>
        /// EGL record field declaration modelini oluşturur.
        /// </summary>
        public EglRecordFieldDeclaration(
         int level,
         string name,
         EglDataType dataType,
         SourceLocation location,
         int? arraySize = null)
         : base(location)
        {
            Level = level;
            Name = name;
            DataType = dataType;
            ArraySize = arraySize;
        }
    }
}