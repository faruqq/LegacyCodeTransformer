using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Declarations
{
    /// <summary>
    /// PL/I structure içerisindeki field/member declaration bilgisini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure declaration içinde ana structure altında seviye numaralı
    /// alanlar bulunur.
    ///
    /// Örnek PL/I:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08) INIT(' '),
    ///     5 PARAM2 CHAR(01) INIT(';');
    ///
    /// Bu örnekte PARAM ve PARAM2, PARAME_LIST structure'ının member
    /// alanlarıdır.
    ///
    /// Bu modelde:
    /// - Level: 5
    /// - Name: PARAM
    /// - DataType: Pl1CharacterType
    /// - InitialValue: Pl1InitialValue veya null
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1StructureDeclaration Members listesinde
    /// - Parser structure member parse adımında
    /// - PL/I → EGL Transpiler record field mapping işleminde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Nested structure, redefinition, array field ve field-level INIT
    /// desteği geldiğinde bu model genişletilecektir.
    /// </summary>
    public sealed class Pl1StructureMember : SyntaxNode
    {
        /// <summary>
        /// PL/I structure member seviye numarasıdır.
        ///
        /// Örneğin:
        /// - 5 PARAM CHAR(08)
        ///
        /// için değer 5 olur.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// PL/I structure member adıdır.
        ///
        /// Örneğin:
        /// - PARAM
        /// - PARAM2
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// PL/I structure member veri tipidir.
        ///
        /// İlk kapsamda CHAR / CHARACTER ve mevcut FIXED DECIMAL tipleri
        /// desteklenebilir.
        /// </summary>
        public Pl1DataType DataType { get; }

        /// <summary>
        /// PL/I structure member başlangıç değeridir.
        ///
        /// Örneğin:
        /// - INIT(' ')
        /// - INIT(';')
        ///
        /// Başlangıç değeri yoksa null olur.
        /// </summary>
        public Pl1InitialValue? InitialValue { get; }

        /// <summary>
        /// PL/I structure member modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser, structure içindeki field satırlarını ayrı ayrı okuyup
        /// güçlü tipli member modellerine dönüştürmelidir.
        ///
        /// Örnek:
        ///
        /// 5 PARAM CHAR(08) INIT(' ')
        ///
        /// Bu constructor çağrıldığında:
        /// - level: 5
        /// - name: PARAM
        /// - dataType: Pl1CharacterType
        /// - initialValue: Pl1InitialValue
        ///
        /// bilgileri model üzerinde tutulur.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I Parser içerisinde
        /// - PL/I → EGL Transpiler içerisinde
        /// - Unit testlerde structure member doğrulamasında
        /// </summary>
        public Pl1StructureMember(
            int level,
            string name,
            Pl1DataType dataType,
            SourceLocation location,
            Pl1InitialValue? initialValue = null)
            : base(location)
        {
            Level = level;
            Name = name;
            DataType = dataType;
            InitialValue = initialValue;
        }
    }
}