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
    /// Parameter veri tipi body içindeki DCL declaration üzerinden,
    /// parameter yönü ise procedure body kullanımları üzerinden
    /// çözümlenebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Header parameter adını, ait olduğu procedure adını, eşleşen
    /// variable declaration modelini ve güvenli biçimde çözümlenen
    /// direction bilgisini tek bir semantic sonuçta toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END CUSTOMER_PROCESS;
    ///
    /// Bu örnekte PROCESS_TEXT header parametresi declaration modeliyle
    /// eşleştirilir ve yalnızca okunduğu için direction değeri In olur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer tarafından üretilir ve SemanticResult içinde
    /// taşınır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL function parameter type ve direction mapping, procedure scope
    /// ve parameter diagnostic davranışlarına temel olur.
    /// </summary>
    public sealed class Pl1ProcedureParameterBinding
    {
        public string ProcedureName { get; }

        public string ParameterName { get; }

        public Pl1VariableDeclaration? Declaration { get; }

        public Pl1ProcedureParameterDirection Direction { get; }

        public bool IsResolved => Declaration is not null;

        public Pl1ProcedureParameterBinding(
            string procedureName,
            string parameterName,
            Pl1VariableDeclaration? declaration,
            Pl1ProcedureParameterDirection direction =
                Pl1ProcedureParameterDirection.Unknown)
        {
            ProcedureName = procedureName;
            ParameterName = parameterName;
            Declaration = declaration;
            Direction = direction;
        }
    }
}