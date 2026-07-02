using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Declarations
{
    /// <summary>
    /// PL/I değişken tanımını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kodunda DCL / DECLARE ifadesiyle tanımlanan değişkenlerin
    /// syntax tree içerisinde tip güvenli şekilde tutulması için
    /// oluşturulmuştur.
    ///
    /// Örnek PL/I kodu:
    ///
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL PARAM CHAR(08) INIT(' ');
    ///
    /// Bu modelde:
    /// - Name: MUST_NO
    /// - DataType: Pl1FixedDecimalType
    /// - InitialValue: null
    ///
    /// veya:
    ///
    /// - Name: PARAM
    /// - DataType: Pl1CharacterType
    /// - InitialValue: Pl1InitialValue
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - PL/I Normalizer içerisinde
    /// - PL/I → EGL Transpiler dönüşüm kurallarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İleride CHAR, BIT, BINARY, STRUCTURE, LIKE gibi PL/I declaration
    /// tipleri de aynı model üzerinden taşınabilir.
    ///
    /// INIT / INITIAL desteği ile birlikte başlangıç değeri bilgisi de
    /// Syntax Tree üzerinde kaybedilmeden korunacaktır.
    /// </summary>
    public sealed class Pl1VariableDeclaration : Pl1Declaration
    {
        /// <summary>
        /// PL/I değişken adıdır.
        ///
        /// Örneğin:
        /// - MUST_NO
        /// - PARAM
        /// - CUSTOMER_NAME
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// PL/I değişken tipidir.
        ///
        /// Örneğin:
        /// - FIXED DECIMAL(8)
        /// - CHAR(08)
        /// - CHARACTER(25)
        /// </summary>
        public Pl1DataType DataType { get; }

        /// <summary>
        /// PL/I değişken başlangıç değeridir.
        ///
        /// Örneğin:
        /// - INIT(' ')
        /// - INITIAL('ABCD')
        /// - INIT((08)' ')
        /// - INIT((*)' ')
        ///
        /// Başlangıç değeri yoksa null olur.
        /// </summary>
        public Pl1InitialValue? InitialValue { get; }

        /// <summary>
        /// PL/I değişken declaration modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser, DCL / DECLARE söz dizimini okuduğunda değişken adını,
        /// veri tipini ve varsa başlangıç değerini tek bir declaration modeli
        /// üzerinde saklamalıdır.
        ///
        /// Örnek:
        ///
        /// DCL PARAM CHAR(08) INIT(' ');
        ///
        /// Bu constructor çağrıldığında:
        /// - name: PARAM
        /// - dataType: Pl1CharacterType
        /// - initialValue: Pl1InitialValue
        ///
        /// bilgileri model üzerinde tutulur.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I Parser içerisinde
        /// - Unit testlerde declaration modelini doğrulamada
        /// - Transpiler katmanına PL/I declaration bilgisini taşımada
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Structure field declaration, array declaration ve default value
        /// dönüşümleri geldiğinde declaration modeli merkezi taşıyıcı olarak
        /// kullanılmaya devam edecektir.
        /// </summary>
        public Pl1VariableDeclaration(
            string name,
            Pl1DataType dataType,
            SourceLocation location,
            Pl1InitialValue? initialValue = null)
            : base(location)
        {
            Name = name;
            DataType = dataType;
            InitialValue = initialValue;
        }
    }
}