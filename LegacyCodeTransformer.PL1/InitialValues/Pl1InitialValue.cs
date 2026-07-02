using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.InitialValues
{
    /// <summary>
    /// PL/I INIT / INITIAL başlangıç değeri bilgisini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I declaration ifadelerinde değişkenlere başlangıç değeri
    /// INIT veya INITIAL söz dizimi ile verilebilir.
    ///
    /// Gerçek PL/I kaynaklarında özellikle CHAR alanlarda boşluk, noktalı
    /// virgül veya tekrar faktörlü başlangıç değerleri yaygın kullanılır.
    ///
    /// Örnek PL/I:
    ///
    /// DCL PARAM CHAR(08) INIT(' ');
    /// DCL PARAM2 CHAR(01) INIT(';');
    /// DCL PARAM3 CHAR(8) INIT((08)' ');
    /// DCL PARAM4 CHAR(8) INIT((*)' ');
    ///
    /// Bu modelde:
    /// - Value: başlangıç değerinin karakter karşılığı
    /// - RepeatCount: tekrar sayısı varsa sayısal karşılığı
    /// - AppliesToAllElements: (*) kullanımı varsa true
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - PL/I Syntax Tree üzerinde declaration bilgisini zenginleştirmede
    /// - İleride EGL default value üretimi kararlaştırıldığında generator
    ///   girdisi olarak
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// INIT / INITIAL bilgisinin şimdiden Syntax Tree üzerinde korunması,
    /// bilgi kaybını engeller.
    ///
    /// EGL tarafındaki başlangıç değeri standardı netleştiğinde bu model
    /// doğrudan hedef dil çıktısı üretiminde kullanılabilecektir.
    ///
    /// Structure ve array desteği geldiğinde INIT((*)' ') gibi tüm elemanlara
    /// uygulanan başlangıç değerleri de bu model üzerinden temsil edilecektir.
    /// </summary>
    public sealed class Pl1InitialValue : SyntaxNode
    {
        /// <summary>
        /// Başlangıç değerinin karakter karşılığıdır.
        ///
        /// Örneğin:
        /// - INIT(' ') için " "
        /// - INIT(';') için ";"
        /// - INIT((08)' ') için " "
        ///
        /// olarak tutulur.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Tekrar faktörü sayısal olarak verilmişse bu değeri temsil eder.
        ///
        /// Örneğin:
        /// - INIT((08)' ') için 8
        /// - INITIAL((4)'*') için 4
        ///
        /// olarak tutulur.
        ///
        /// Basit INIT(' ') kullanımında null olur.
        /// </summary>
        public int? RepeatCount { get; }

        /// <summary>
        /// Tekrar faktörünün (*) olarak verilip verilmediğini temsil eder.
        ///
        /// Örneğin:
        /// - INIT((*)' ') için true
        /// - INIT((08)' ') için false
        /// - INIT(' ') için false
        ///
        /// olarak tutulur.
        /// </summary>
        public bool AppliesToAllElements { get; }

        /// <summary>
        /// PL/I başlangıç değeri modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser INIT / INITIAL söz dizimini okuduğunda bu bilgiyi string
        /// olarak kaybetmeden, anlamlı ve güçlü tipli bir model üzerinde
        /// saklamalıdır.
        ///
        /// Örnek:
        ///
        /// INIT((08)' ')
        ///
        /// Bu constructor çağrıldığında:
        /// - value: " "
        /// - repeatCount: 8
        /// - appliesToAllElements: false
        ///
        /// bilgileri model üzerinde tutulur.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I Parser içerisinde
        /// - Unit testlerde INIT parse davranışını doğrulamada
        /// - İleride EGL default value mapping kararlarında
        /// </summary>
        public Pl1InitialValue(
            string value,
            int? repeatCount,
            bool appliesToAllElements,
            SourceLocation location)
            : base(location)
        {
            Value = value;
            RepeatCount = repeatCount;
            AppliesToAllElements = appliesToAllElements;
        }
    }
}