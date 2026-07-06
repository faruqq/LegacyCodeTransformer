using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Transpilers.Pl1ToEgl
{
    /// <summary>
    /// PL/I executable statement modellerini EGL statement modellerine dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Pl1ToEglTranspiler ana sınıfı declaration dönüşümlerini yönetmektedir.
    /// Statement dönüşümleri büyüdükçe bu sorumluluğun ayrı bir bileşene ayrılması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// P05.7 foundation aşamasında statement transpiler giriş noktasını oluşturur.
    /// Henüz concrete EGL statement mapping yapılmadığı için desteklenmeyen statement
    /// türleri için diagnostic üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Şimdilik diagnostic üretir:
    ///
    ///     PARAM = 'ABC';
    ///     CALL PROC1;
    ///     IF A = B THEN CALL PROC2;
    ///     DO; CALL PROC3; END;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1ToEglTranspiler.Transpile methodu içinde SyntaxTree.Statements listesi
    /// işlenirken kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Assignment, CALL, IF ve DO EGL generation milestone'larında concrete EGL statement
    /// modelleri bu sınıf üzerinden üretilecektir.
    /// </summary>
    internal sealed class StatementTranspiler
    {
        private readonly DiagnosticBag _diagnostics;

        public StatementTranspiler(DiagnosticBag diagnostics)
        {
            _diagnostics = diagnostics;
        }

        /// <summary>
        /// PL/I statement modelini EGL statement modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Transpiler katmanının statement dönüşümü için tek bir entrypoint'e ihtiyacı vardır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// P05.7 aşamasında statement pipeline'ı ana transpiler akışına bağlar.
        /// Concrete mapping henüz eklenmediği için diagnostic üretir ve null döner.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// Pl1AssignmentStatement görüldüğünde şimdilik desteklenmeyen statement
        /// diagnostic'i üretir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Pl1ToEglTranspiler.Transpile içerisinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// P05.8 ile Assignment, P05.9 ile CALL, P05.10 ile IF ve P05.11 ile DO mapping
        /// bu method üzerinden genişletilecektir.
        /// </summary>
        public EglStatement? TranspileStatement(Pl1Statement statement)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Desteklenmeyen PL/I statement türü: {statement.GetType().Name}",
                statement.Location));

            return null;
        }
    }
}