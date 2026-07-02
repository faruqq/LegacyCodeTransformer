namespace LegacyCodeTransformer.Pl1.Lexing
{
    /// <summary>
    /// PL/I lexer tarafından üretilecek token türlerini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Lexer kaynak kodu okurken her anlamlı parçayı bir token olarak üretir.
    /// Parser ise bu token türlerine bakarak PL/I yapısını anlamlandırır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Lexer içerisinde
    /// - PL/I Parser içerisinde
    /// - Parser hata mesajlarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I desteği genişledikçe yeni token türleri bu enum'a eklenecektir.
    ///
    /// Örneğin:
    /// - IF
    /// - THEN
    /// - ELSE
    /// - DO
    /// - END
    /// - CALL
    /// - PROCEDURE
    /// </summary>
    public enum Pl1TokenKind
    {
        /// <summary>
        /// Tanınamayan karakter veya token.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Kaynak kodun sonunu temsil eder.
        /// </summary>
        EndOfFile = 1,

        /// <summary>
        /// PL/I DCL keyword'ü.
        /// </summary>
        DclKeyword = 2,

        /// <summary>
        /// PL/I FIXED keyword'ü.
        /// </summary>
        FixedKeyword = 3,

        /// <summary>
        /// PL/I DECIMAL keyword'ü.
        /// </summary>
        DecimalKeyword = 4,

        /// <summary>
        /// Değişken, procedure veya benzeri isimleri temsil eder.
        /// Örneğin: MUST_NO
        /// </summary>
        Identifier = 5,

        /// <summary>
        /// Sayısal değerleri temsil eder.
        /// Örneğin: 8
        /// </summary>
        Number = 6,

        /// <summary>
        /// Sol parantez karakteri.
        /// </summary>
        OpenParenthesis = 7,

        /// <summary>
        /// Sağ parantez karakteri.
        /// </summary>
        CloseParenthesis = 8,

        /// <summary>
        /// Noktalı virgül karakteri.
        /// </summary>
        Semicolon = 9,

        /// <summary>
        /// Virgül karakteri.
        /// FIXED DECIMAL(8,2) desteği için kullanılacaktır.
        /// </summary>
        Comma = 10,

        /// <summary>
        /// PL/I CHAR kısa veri tipi keyword'ü.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I dilinde CHARACTER veri tipinin kısa kullanımı CHAR olarak geçer.
        /// Gerçek kaynak kodlarda karakter alanlar çoğunlukla CHAR(n) şeklinde
        /// tanımlanır.
        ///
        /// Örnek:
        ///
        /// DCL PARAM CHAR(08);
        ///
        /// Parser bu token sayesinde CHAR söz dizimini veri tipi başlangıcı
        /// olarak algılar.
        /// </summary>
        CharKeyword = 11,

        /// <summary>
        /// PL/I CHARACTER uzun veri tipi keyword'ü.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I dilinde CHAR ve CHARACTER aynı karakter veri tipi ailesini
        /// temsil eder.
        ///
        /// Bazı kaynak kodlarda kısa form CHAR, bazı kaynaklarda uzun form
        /// CHARACTER kullanılabilir.
        ///
        /// Örnek:
        ///
        /// DECLARE PARAM CHARACTER(08);
        ///
        /// Parser bu token sayesinde CHARACTER söz dizimini CHAR ile aynı
        /// veri tipi modeline dönüştürür.
        /// </summary>
        CharacterKeyword = 12
    }
}