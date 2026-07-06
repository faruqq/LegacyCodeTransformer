using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I CALL statement modelidir.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// PL/I procedure çağrılarını executable statement olarak temsil etmek için vardır.
    ///
    /// Ne çözüyor?
    /// Argümansız ve argümanlı CALL yapılarını aynı syntax model altında taşır.
    ///
    /// Hangi örneği destekliyor?
    /// CALL FETCH_CURSOR;
    /// CALL SQL_HATA_OLUSTUR('SELECT GLAU_HISTORY');
    ///
    /// Nerede kullanılır?
    /// Statement parser CALL keyword'ünü gördüğünde bu modeli üretir.
    ///
    /// Gelecekte neye temel olur?
    /// Procedure dependency çıkarımı, call graph üretimi, EGL function/service
    /// call generation ve parametre uyumluluk kontrollerine temel olur.
    /// </remarks>
    public sealed class Pl1CallStatement : Pl1Statement
    {
        public string ProcedureName { get; }

        public IReadOnlyList<Pl1Expression> Arguments { get; }

        public Pl1CallStatement(
            string procedureName,
            IEnumerable<Pl1Expression>? arguments,
            SourceLocation location)
            : base(location)
        {
            ProcedureName = procedureName;
            Arguments = arguments?.ToList() ?? new List<Pl1Expression>();
        }
    }
}