using LegacyCodeTransformer.Pl1.Declarations;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I procedure header parametresi ile procedure body declaration
    /// modeli arasındaki semantic eşleşmeyi temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I procedure header yalnızca parameter adlarını taşır.
    /// Parameter veri tipi ise procedure body içindeki DCL declaration
    /// üzerinden tanımlanabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Header parameter adını, ait olduğu procedure adını ve eşleşen
    /// variable declaration modelini tek bir semantic sonuçta toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    /// END CUSTOMER_PROCESS;
    ///
    /// Bu örnekte PROCESS_TEXT header parametresi ile
    /// DCL PROCESS_TEXT CHAR(50) declaration modeli eşleştirilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer tarafından üretilir ve SemanticResult içinde
    /// taşınır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL function parameter type mapping, parameter direction analizi,
    /// procedure scope ve parameter diagnostic davranışlarına temel olur.
    /// </summary>
    public sealed class Pl1ProcedureParameterBinding
    {
        public string ProcedureName { get; }

        public string ParameterName { get; }

        public Pl1VariableDeclaration? Declaration { get; }

        public bool IsResolved => Declaration is not null;

        public Pl1ProcedureParameterBinding(
            string procedureName,
            string parameterName,
            Pl1VariableDeclaration? declaration)
        {
            ProcedureName = procedureName;
            ParameterName = parameterName;
            Declaration = declaration;
        }
    }
}