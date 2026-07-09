namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I semantic symbol modelidir.
    ///
    /// Neden var?
    /// ----------------------
    /// Semantic analyzer, declaration'lardan sembol bilgisi üretmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// İlk kapsamda yalnızca declaration adını semantic symbol olarak taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// Bu declaration için MUST_NO sembolü oluşturulur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// SymbolTable içinde global declaration sembollerini temsil etmek için kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Symbol kind, type info, source location ve scope bilgileri gerektiğinde bu model
    /// kontrollü şekilde genişletilebilir.
    /// </summary>
    public sealed class Symbol
    {
        public string Name { get; }

        public Symbol(string name)
        {
            Name = name;
        }
    }
}