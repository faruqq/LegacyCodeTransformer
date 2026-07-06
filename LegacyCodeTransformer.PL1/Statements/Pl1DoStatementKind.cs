namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I DO statement varyantını belirtir.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// DO statement modelinin hangi PL/I DO formunu temsil ettiğini açık ve
    /// type-safe şekilde taşımak için vardır.
    ///
    /// Ne çözüyor?
    /// Koşulsuz DO block, DO WHILE ve DO UNTIL yapılarını birbirinden ayırır.
    ///
    /// Hangi örneği destekliyor?
    /// DO;
    ///     A = B;
    /// END;
    ///
    /// DO WHILE(SQLCODE = 0);
    ///     CALL FETCH_CURSOR;
    /// END;
    ///
    /// DO UNTIL(SAHA1 = 'ABC');
    ///     SAHA1 = 'ABC';
    /// END;
    ///
    /// Nerede kullanılır?
    /// Pl1DoStatement.Kind property’sinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// DO statement semantic validation, EGL loop/block generation ve ileride
    /// iterative DO modellerinin ayrıştırılması için temel olur.
    /// </remarks>
    public enum Pl1DoStatementKind
    {
        Block,
        While,
        Until
    }
}