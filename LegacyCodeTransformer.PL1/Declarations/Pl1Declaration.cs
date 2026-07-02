using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Declarations
{
    /// <summary>
    /// PL/I declaration modellerinin ortak temel sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodunda DCL / DECLARE ifadesi yalnızca tekil değişken
    /// tanımlamak için kullanılmaz.
    ///
    /// Örnek tekil değişken:
    ///
    /// DCL PARAM CHAR(08);
    ///
    /// Örnek structure:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08),
    ///     5 PARAM2 CHAR(01);
    ///
    /// Bu iki yapı da PL/I declaration ailesine aittir ancak aynı modelle
    /// temsil edilmemelidir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1SyntaxTree declaration listesinde
    /// - Pl1VariableDeclaration temel sınıfı olarak
    /// - Pl1StructureDeclaration temel sınıfı olarak
    /// - Transpiler declaration dispatch işlemlerinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I desteği genişledikçe file declaration, procedure declaration,
    /// based declaration, like declaration ve farklı DCL varyasyonları da
    /// bu ortak declaration ailesi altında modellenebilir.
    /// </summary>
    public abstract class Pl1Declaration : SyntaxNode
    {
        /// <summary>
        /// PL/I declaration temel modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Bütün declaration modellerinin kaynak kod konum bilgisini
        /// taşıması gerekir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Türetilmiş PL/I declaration modellerinin constructor zincirinde
        /// - Diagnostic üretiminde kaynak konumunu korumak için
        /// </summary>
        protected Pl1Declaration(SourceLocation location)
            : base(location)
        {
        }
    }
}