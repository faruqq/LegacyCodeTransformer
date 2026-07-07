namespace LegacyCodeTransformer.Egl.Statements
{
    /// <summary>
    /// EGL DO statement modelinin hangi control-flow türünü temsil ettiğini belirtir.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında DO statement farklı anlamlarla gelebilir: koşulsuz block,
    /// DO WHILE ve DO UNTIL.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EglDoStatement modelinin block, while veya until davranışını açık ve güçlü tipli
    /// şekilde taşımasını sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     DO;
    ///         CALL PROC1;
    ///     END;
    ///
    ///     DO WHILE(SQLCODE = 0);
    ///         CALL FETCH_CURSOR;
    ///     END;
    ///
    ///     DO UNTIL(EOF);
    ///         CALL CLOSE_CURSOR;
    ///     END;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// EglDoStatement üzerinde DO türünü taşımak için kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Sayaçlı DO, range-based loop veya daha özel EGL loop mapping ihtiyaçları
    /// oluşursa bu enum genişletilebilir.
    /// </summary>
    public enum EglDoStatementKind
    {
        Block,
        While,
        Until
    }
}