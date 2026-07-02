using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Declarations
{
    /// <summary>
    /// EGL record declaration modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure declaration ifadeleri EGL tarafında record olarak
    /// üretilecektir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08),
    ///     5 PARAM2 CHAR(01);
    ///
    /// Örnek EGL:
    ///
    /// record ParameList type BasicRecord
    ///     10 Param char(8);
    ///     10 Param2 char(1);
    /// end
    ///
    /// Bu modelde:
    /// - Name: ParameList
    /// - RecordType: BasicRecord
    /// - Fields: Param, Param2
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler çıktısında
    /// - EglSyntaxTree declaration listesinde
    /// - EglCodeGenerator record üretiminde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Description metadata, annotations, record subtype seçenekleri ve
    /// nested record üretimi desteklendiğinde bu model genişletilecektir.
    /// </summary>
    public sealed class EglRecordDeclaration : EglDeclaration
    {
        /// <summary>
        /// EGL record adıdır.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// EGL record tipidir.
        ///
        /// İlk kapsamda BasicRecord kullanılacaktır.
        /// </summary>
        public string RecordType { get; }

        /// <summary>
        /// EGL record field listesidir.
        /// </summary>
        public IReadOnlyList<EglRecordFieldDeclaration> Fields { get; }

        /// <summary>
        /// EGL record declaration modelini oluşturur.
        /// </summary>
        public EglRecordDeclaration(
            string name,
            string recordType,
            IEnumerable<EglRecordFieldDeclaration>? fields,
            SourceLocation location)
            : base(location)
        {
            Name = name;
            RecordType = recordType;
            Fields = fields?.ToList() ?? new List<EglRecordFieldDeclaration>();
        }
    }
}