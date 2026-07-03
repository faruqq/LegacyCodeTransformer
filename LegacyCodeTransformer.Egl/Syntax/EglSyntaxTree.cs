using System.Collections.Generic;
using System.Linq;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;

namespace LegacyCodeTransformer.Egl.Syntax
{
    /// <summary>
    /// EGL kaynak kodunun syntax tree karşılığını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I → EGL dönüşüm pipeline'ında Transpiler aşaması doğrudan string
    /// üretmez.
    ///
    /// Bunun yerine EGL diline ait güçlü tipli bir syntax tree üretir.
    ///
    /// Code Generator ise bu EGL syntax tree modelini okuyarak gerçek EGL
    /// kaynak kodunu üretir.
    ///
    /// İlk aşamada bu model yalnızca tekil variable declaration listesi
    /// taşıyordu.
    ///
    /// Structure desteğiyle birlikte EGL tarafında record declaration da
    /// üretileceği için declaration listesi ortak EglDeclaration base type
    /// üzerinden taşınır hale getirilmiştir.
    ///
    /// Desteklenen declaration örnekleri:
    ///
    /// - EglVariableDeclaration
    /// - EglRecordDeclaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler çıktısında
    /// - EGL Code Generator girişinde
    /// - EGL unit testlerinde
    /// - Application uçtan uca dönüşüm pipeline'ında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// EGL program, service, library, function, record, variable ve benzeri
    /// yeni declaration türleri eklendikçe aynı syntax tree kökü üzerinden
    /// taşınabilecektir.
    /// </summary>
    public sealed class EglSyntaxTree : SyntaxTree
    {
        /// <summary>
        /// EGL syntax tree içerisindeki declaration listesidir.
        ///
        /// Neden var?
        /// ----------------------
        /// EGL çıktısı yalnızca tekil değişken declaration ifadelerinden
        /// oluşmayacaktır.
        ///
        /// Örnek EGL variable declaration:
        ///
        /// MustNo decimal(8,0);
        ///
        /// Örnek EGL record declaration:
        ///
        /// record ParameList type basicRecord
        ///     10 Param char(8);
        ///     10 Param2 char(1);
        /// end
        ///
        /// Bu nedenle liste tipi yalnızca EglVariableDeclaration değil,
        /// ortak EglDeclaration base type olmalıdır.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Transpiler tarafından EGL declaration modelleri eklenirken
        /// - Code Generator declaration türüne göre dispatch yaparken
        /// - Unit testlerde EGL syntax tree elle oluşturulurken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// EGL tarafında yeni declaration türleri desteklendiğinde mevcut
        /// syntax tree modeli bozulmadan genişletilebilecektir.
        /// </summary>
        public IReadOnlyList<EglDeclaration> Declarations { get; }

        /// <summary>
        /// EGL syntax tree modelini oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Transpiler tarafından üretilen EGL declaration modellerini tek bir
        /// kök syntax tree altında toplamak için kullanılır.
        ///
        /// Structure desteğiyle birlikte declarations parametresi ortak
        /// EglDeclaration base type üzerinden alınır.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Pl1ToEglTranspiler.Transpile sonucunda
        /// - EglCodeGenerator.Generate girişinde
        /// - Unit testlerde elle EGL syntax tree oluşturulurken
        /// </summary>
        public EglSyntaxTree(
            IEnumerable<EglDeclaration>? declarations,
            SourceLocation location)
            : base(location)
        {
            Declarations = declarations?.ToList() ?? new List<EglDeclaration>();
        }
    }
}