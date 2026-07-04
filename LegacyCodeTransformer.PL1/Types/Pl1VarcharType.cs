using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types
{
    /// <summary>
    /// PL/I VARCHAR(n) veri tipini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında değişken uzunluklu karakter alanlar VARCHAR(n)
    /// söz dizimiyle tanımlanabilir.
    ///
    /// Örnek PL/I:
    ///
    /// DCL CUSTOMER_NAME VARCHAR(50);
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Parser'ın VARCHAR(n) bilgisini genel karakter tipi gibi kaybetmeden
    /// ayrı bir PL/I veri tipi modeli olarak taşımasını sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL CUSTOMER_NAME VARCHAR(50);
    /// - 5 CUSTOMER_NAME VARCHAR(50);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser veri tipi parse işleminde
    /// - PL/I → EGL Transpiler veri tipi mapping işleminde
    /// - Structure length hesabında
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// VARCHAR alanların metadata, validation, sqlRecord mapping veya
    /// kurum standardına özel farklı EGL çıktılarında ayrı ele alınmasına
    /// temel olur.
    /// </summary>
    public sealed class Pl1VarcharType : Pl1DataType
    {
        /// <summary>
        /// VARCHAR maksimum karakter uzunluğudur.
        ///
        /// Örnek:
        ///
        /// VARCHAR(50)
        ///
        /// için değer 50 olur.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// PL/I VARCHAR veri tipi modelini oluşturur.
        /// </summary>
        public Pl1VarcharType(
            int length,
            SourceLocation location)
            : base(location)
        {
            Length = length;
        }
    }
}