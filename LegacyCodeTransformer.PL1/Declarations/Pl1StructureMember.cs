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
    /// Bu alanlar iki farklı şekilde olabilir:
    ///
    /// 1. Veri tipi olan normal field:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08);
    ///
    /// 2. Veri tipi olmayan nested group field:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 ADRES_BILGI,
    ///         10 IL_KOD CHAR(02),
    ///         10 ILCE_KOD CHAR(03);
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Bu model hem normal structure member alanlarını hem de altında child
    /// member taşıyan nested group alanlarını tek model üzerinden temsil eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 5 PARAM CHAR(08)
    /// - 5 PARAM_LIST(2) CHAR(10)
    /// - 5 ADRES_BILGI
    /// - 10 IL_KOD CHAR(02)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1StructureDeclaration Members listesinde
    /// - Parser structure member parse adımında
    /// - PL/I → EGL Transpiler record field mapping işleminde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested structure, multi-level group field, field-level array,
    /// layout length hesabı ve ileride gelebilecek daha karmaşık PL/I
    /// structure mapping kuralları için temel modeldir.
    /// </summary>
    public sealed class Pl1StructureMember : SyntaxNode
    {
        /// <summary>
        /// PL/I structure member seviye numarasıdır.
        ///
        /// Örneğin:
        /// - 5 PARAM CHAR(08)
        /// - 10 IL_KOD CHAR(02)
        ///
        /// için değer sırasıyla 5 ve 10 olur.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// PL/I structure member adıdır.
        ///
        /// Örneğin:
        /// - PARAM
        /// - PARAM_LIST
        /// - ADRES_BILGI
        /// - IL_KOD
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// PL/I structure member array boyutudur.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I structure member alanları kendi üzerlerinde dimension
        /// bilgisi taşıyebilir.
        ///
        /// Örnek:
        ///
        /// 5 PARAM_LIST(2) CHAR(10)
        ///
        /// Bu örnekte PARAM_LIST alanının array boyutu 2'dir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Parser structure member parse işleminde
        /// - Transpiler'ın EGL field array üretiminde
        /// - Structure / nested group length hesabında
        /// </summary>
        public int? ArraySize { get; }

        /// <summary>
        /// PL/I structure member veri tipidir.
        ///
        /// Normal field alanlarında doludur.
        ///
        /// Örnek:
        ///
        /// 5 PARAM CHAR(08)
        ///
        /// Nested group alanlarında null olur.
        ///
        /// Örnek:
        ///
        /// 5 ADRES_BILGI,
        ///     10 IL_KOD CHAR(02)
        ///
        /// Burada ADRES_BILGI veri tipi taşımadığı için DataType null olur.
        /// </summary>
        public Pl1DataType? DataType { get; }

        /// <summary>
        /// PL/I structure member başlangıç değeridir.
        ///
        /// Örneğin:
        /// - INIT(' ')
        /// - INIT(';')
        /// - INIT((*)' ')
        ///
        /// Başlangıç değeri yoksa null olur.
        ///
        /// Nested group alanlarında ilk kapsamda null beklenir.
        /// </summary>
        public Pl1InitialValue? InitialValue { get; }

        /// <summary>
        /// Nested group field altında bulunan child member listesidir.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I structure içinde bazı member satırları veri tipi taşımaz ve
        /// kendisinden daha büyük level değerine sahip alt alanları gruplayan
        /// parent alan olarak kullanılır.
        ///
        /// Örnek:
        ///
        /// 5 ADRES_BILGI,
        ///     10 IL_KOD CHAR(02),
        ///     10 ILCE_KOD CHAR(03)
        ///
        /// Bu örnekte ADRES_BILGI bir group field'dır ve IL_KOD / ILCE_KOD
        /// alanları bu listenin içinde tutulur.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Parser'ın nested structure hiyerarşisini düz listeye indirgemeden
        /// model üzerinde korumasını sağlar.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Parser nested member parse işleminde
        /// - Transpiler nested EGL field üretiminde
        /// - Group field total length hesabında
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Çok seviyeli nested structure, group array ve layout hesaplama
        /// davranışlarının merkezi şekilde modellenmesine temel olur.
        /// </summary>
        public IReadOnlyList<Pl1StructureMember> Members { get; }

        /// <summary>
        /// PL/I structure member modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser, structure içindeki field satırlarını güçlü tipli member
        /// modellerine dönüştürmelidir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Structure member'a ait level, name, optional array size, optional
        /// data type, optional initial value ve optional child member listesini
        /// tek modelde toplar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// Normal field:
        ///
        /// 5 PARAM CHAR(08) INIT(' ')
        ///
        /// Field array:
        ///
        /// 5 PARAM_LIST(2) CHAR(10)
        ///
        /// Nested group:
        ///
        /// 5 ADRES_BILGI,
        ///     10 IL_KOD CHAR(02),
        ///     10 ILCE_KOD CHAR(03)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I Parser içerisinde
        /// - PL/I → EGL Transpiler içerisinde
        /// - Unit testlerde structure member doğrulamasında
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Nested structure mapping, parent group length hesabı, field-level
        /// array dönüşümü ve çok seviyeli layout üretimi için temel olur.
        /// </summary>
        public Pl1StructureMember(
            int level,
            string name,
            Pl1DataType? dataType,
            SourceLocation location,
            Pl1InitialValue? initialValue = null,
            int? arraySize = null,
            IEnumerable<Pl1StructureMember>? members = null)
            : base(location)
        {
            Level = level;
            Name = name;
            DataType = dataType;
            InitialValue = initialValue;
            ArraySize = arraySize;
            Members = members?.ToList() ?? new List<Pl1StructureMember>();
        }

        /// <summary>
        /// Member'ın nested group field olup olmadığını belirtir.
        ///
        /// Neden var?
        /// ----------------------
        /// Transpiler ve length hesaplama tarafında normal typed field ile
        /// nested group field ayrımı yapılmalıdır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// DataType null ve Members dolu olan alanların group field olarak
        /// kolayca anlaşılmasını sağlar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// 5 ADRES_BILGI,
        ///     10 IL_KOD CHAR(02)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Transpiler nested field mapping işleminde
        /// - Group field length hesabında
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Çok seviyeli nested structure mapping sırasında branch kararlarını
        /// sadeleştirmeye temel olur.
        /// </summary>
        public bool IsGroup => DataType is null && Members.Count > 0;
    }
}