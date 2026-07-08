using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I içinde yer alan EXEC SQL statement modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Kurumsal PL/I kodlarında DB2 erişimleri EXEC SQL bloklarıyla yapılır.
    /// Parser'ın bu blokları beklenmeyen token olarak kaybetmesi yerine syntax tree
    /// üzerinde güvenli şekilde taşıması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EXEC SQL ...; cümlesini ilk aşamada raw text olarak korur. Böylece embedded
    /// SQL için detaylı SQL parser yazmadan önce PL/I parser pipeline bu statement'ı
    /// tanıyabilir ve taşıyabilir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// EXEC SQL INCLUDE SQLCA;
    ///
    /// EXEC SQL SELECT CUSTOMER_NO
    ///     INTO :CUSTOMER_NO
    ///     FROM CUSTOMER_TABLE;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// EmbeddedSqlStatementParser çıktısı olarak, Pl1SyntaxTree.Statements veya
    /// Pl1Procedure.Statements listelerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// SQL statement classification, SQL include handling, SQL host variable
    /// analysis, DB2 access modelleme ve EGL SQL generation çalışmaları için temel olur.
    /// </summary>
    public sealed class Pl1EmbeddedSqlStatement : Pl1Statement
    {
        public string RawSqlText { get; }

        public Pl1EmbeddedSqlStatement(
            string rawSqlText,
            SourceLocation location)
            : base(location)
        {
            RawSqlText = rawSqlText;
        }
    }
}