using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I assignment statement modelidir.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// PL/I aktarma deyimini executable statement olarak temsil etmek için vardır.
    ///
    /// Ne çözüyor?
    /// Tek hedefli ve çok hedefli assignment yapılarını aynı model altında taşır.
    /// PL/I tarafında A = 0; gibi basit assignment ile A, B = C; gibi çoklu
    /// assignment aynı statement ailesine aittir.
    ///
    /// Hangi örneği destekliyor?
    /// A = 0;
    /// C = C + B;
    /// A, B = C + D;
    ///
    /// Nerede kullanılır?
    /// Statement parser, eşittir operatörü assignment bağlamında algılandığında
    /// bu modeli üretir.
    ///
    /// Gelecekte neye temel olur?
    /// EGL assignment üretimi, çoklu assignment çözümleme, type compatibility
    /// kontrolü ve expression semantic analizine temel olur.
    /// </remarks>
    public sealed class Pl1AssignmentStatement : Pl1Statement
    {
        public IReadOnlyList<Pl1Expression> Targets { get; }

        public Pl1Expression Value { get; }

        public Pl1AssignmentStatement(
            IEnumerable<Pl1Expression>? targets,
            Pl1Expression value,
            SourceLocation location)
            : base(location)
        {
            Targets = targets?.ToList() ?? new List<Pl1Expression>();
            Value = value;
        }
    }
}