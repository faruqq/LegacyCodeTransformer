using System.Collections.Generic;
using System.Linq;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;

namespace LegacyCodeTransformer.Pl1.Syntax
{
    /// <summary>
    /// PL/I kaynak kodunun syntax tree karşılığını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser, PL/I kaynak kodunu okuduktan sonra string veya satır listesi
    /// yerine bu modeli üretir.
    ///
    /// Bu sınıf PL/I tarafındaki kök syntax tree modelidir.
    ///
    /// İlk aşamada yalnızca tekil değişken declaration listesi taşınıyordu.
    /// Structure desteğiyle birlikte declaration listesi artık ortak
    /// Pl1Declaration base type üzerinden taşınır.
    ///
    /// Desteklenen declaration örnekleri:
    ///
    /// - Pl1VariableDeclaration
    /// - Pl1StructureDeclaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - PL/I Normalizer giriş ve çıkışında
    /// - PL/I → EGL Transpiler girişinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Statement, procedure, include, embedded SQL, based declaration,
    /// factored declaration ve diğer PL/I öğeleri eklendikçe bu model
    /// üzerinden genişletilecektir.
    /// </summary>
    public sealed class Pl1SyntaxTree : SyntaxTree
    {
        /// <summary>
        /// PL/I kaynak kodunda bulunan declaration listesidir.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I kaynak kodunda DCL / DECLARE ifadeleri farklı declaration
        /// türleri üretebilir.
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
        /// Bu nedenle liste tipi yalnızca Pl1VariableDeclaration değil,
        /// ortak Pl1Declaration base type olmalıdır.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Parser tarafından declaration eklenirken
        /// - Normalizer declaration listesi üzerinde çalışırken
        /// - Transpiler declaration türüne göre dispatch yaparken
        /// </summary>
        public IReadOnlyList<Pl1Declaration> Declarations { get; }

        /// <summary>
        /// PL/I syntax tree modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser tarafından üretilen declaration modellerini tek bir kök
        /// syntax tree altında toplamak için kullanılır.
        ///
        /// Structure desteğiyle birlikte declarations parametresi ortak
        /// Pl1Declaration base type üzerinden alınır.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Pl1Parser.Parse sonucunda
        /// - Unit testlerde elle syntax tree oluştururken
        /// - Normalizer ve Transpiler girişlerinde
        /// </summary>
        public Pl1SyntaxTree(
            IEnumerable<Pl1Declaration>? declarations,
            SourceLocation location)
            : base(location)
        {
            Declarations = declarations?.ToList() ?? new List<Pl1Declaration>();
        }
    }
}