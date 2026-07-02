using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types
{
    /// <summary>
    /// PL/I CHAR / CHARACTER veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kodunda karakter alanlar sıklıkla CHAR(n) veya CHARACTER(n)
    /// söz dizimi ile tanımlanır.
    ///
    /// Gerçek PL/I kaynaklarında müşteri adı, işlem kodu, tarih, açıklama,
    /// durum kodu ve parametrik alanlar çoğunlukla sabit uzunluklu karakter
    /// alanlar olarak kullanılır.
    ///
    /// Örnek PL/I:
    ///
    /// DCL PARAM CHAR(08);
    /// DCL CUSTOMER_NAME CHARACTER(25);
    ///
    /// Bu modelde:
    /// - Length: 8
    /// - Length: 25
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - PL/I Normalizer içerisinde karakter uzunluğu normalizasyonunda
    /// - PL/I → EGL Transpiler içerisinde char tip dönüşümünde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// CHAR(08) gibi başında sıfır bulunan uzunluk değerleri modelde
    /// sayısal olarak 8 şeklinde tutulacaktır.
    ///
    /// INIT / INITIAL desteği eklendiğinde bu tip, karakter başlangıç
    /// değerleriyle birlikte kullanılacaktır.
    ///
    /// Structure ve array desteği geldiğinde structure field tiplerinde de
    /// bu model kullanılacaktır.
    /// </summary>
    public sealed class Pl1CharacterType : Pl1DataType
    {
        /// <summary>
        /// Karakter alanın sabit uzunluğudur.
        ///
        /// Örneğin:
        /// - CHAR(08) için 8
        /// - CHARACTER(25) için 25
        ///
        /// olarak tutulur.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// PL/I CHAR / CHARACTER veri tipi modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser, CHAR(n) veya CHARACTER(n) söz dizimini okuduğunda
        /// elde ettiği uzunluk bilgisini PL/I syntax tree üzerinde
        /// güçlü tipli bir model olarak saklamalıdır.
        ///
        /// Örnek:
        ///
        /// DCL PARAM CHAR(08);
        ///
        /// Bu constructor çağrıldığında:
        /// - length: 8
        /// - location: CHAR / CHARACTER token konumu
        ///
        /// bilgileri model üzerinde tutulur.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I Parser içerisinde
        /// - Unit testlerde beklenen veri tipini doğrulamada
        /// - Transpiler içerisinde EGL char tipine dönüştürmede
        /// </summary>
        public Pl1CharacterType(
            int length,
            SourceLocation location)
            : base(location)
        {
            Length = length;
        }
    }
}