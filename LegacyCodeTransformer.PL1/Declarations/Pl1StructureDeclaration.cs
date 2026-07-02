using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Declarations
{
    /// <summary>
    /// PL/I seviye numaralı structure declaration ifadesini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I dilinde DCL ifadesiyle hiyerarşik veri yapıları tanımlanabilir.
    /// Bu yapılar tekil değişken declaration değildir; birden fazla member
    /// alan içeren veri gruplarıdır.
    ///
    /// Örnek PL/I:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08) INIT(' '),
    ///     5 PARAM2 CHAR(01) INIT(';');
    ///
    /// Bu modelde:
    /// - Level: 1
    /// - Name: PARAME_LIST
    /// - Members: PARAM, PARAM2
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - Pl1SyntaxTree declaration listesinde
    /// - PL/I → EGL Transpiler içerisinde record dönüşümünde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Structure array, nested structure, BASED, LIKE, DEF ve daha karmaşık
    /// PL/I veri grupları desteklendiğinde bu model genişletilecektir.
    /// </summary>
    public sealed class Pl1StructureDeclaration : Pl1Declaration
    {
        /// <summary>
        /// Ana PL/I structure seviye numarasıdır.
        ///
        /// İlk kapsamda bu değer 1 olacaktır.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// PL/I structure adıdır.
        ///
        /// Örneğin:
        /// - PARAME_LIST
        /// - DCLGLAU
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Structure altında yer alan member alanlardır.
        /// </summary>
        public IReadOnlyList<Pl1StructureMember> Members { get; }

        /// <summary>
        /// PL/I structure declaration modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser, seviye numaralı DCL structure ifadelerini tekil değişken
        /// declaration yerine ayrı bir structure modeli olarak saklamalıdır.
        ///
        /// Örnek:
        ///
        /// DCL 1 PARAME_LIST,
        ///     5 PARAM CHAR(08),
        ///     5 PARAM2 CHAR(01);
        ///
        /// Bu constructor çağrıldığında:
        /// - level: 1
        /// - name: PARAME_LIST
        /// - members: PARAM, PARAM2
        ///
        /// bilgileri model üzerinde tutulur.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I Parser içerisinde
        /// - Transpiler record dönüşümünde
        /// - Unit testlerde structure declaration doğrulamasında
        /// </summary>
        public Pl1StructureDeclaration(
            int level,
            string name,
            IEnumerable<Pl1StructureMember>? members,
            SourceLocation location)
            : base(location)
        {
            Level = level;
            Name = name;
            Members = members?.ToList() ?? new List<Pl1StructureMember>();
        }
    }
}