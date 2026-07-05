namespace LegacyCodeTransformer.Pl1.Lexing
{
    /// <summary>
    /// PL/I lexer tarafından üretilebilecek token türlerini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Lexer kaynak PL/I metnini anlamlı parçalara ayırır.
    /// Parser ise bu parçaların türüne göre syntax model üretir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Keyword, identifier, sayı, string literal ve punctuation gibi
    /// PL/I kaynak parçalarını güçlü tipli enum değerleriyle temsil eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM CHAR(08) INIT(' ');
    /// - DCL AMOUNT FIXED DEC(17,2);
    /// - DCL COUNT FIXED BIN(15);
    /// - DCL PARAM1 PIC '999';
    /// - DCL PARAM2 PICTURE '999V99';
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Lexer token üretiminde
    /// - PL/I Parser token dispatch işlemlerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Yeni PL/I keyword, operator ve declaration türleri eklendikçe
    /// lexer/parser sözleşmesinin merkezi enum modeli olarak genişletilir.
    /// </summary>
    public enum Pl1TokenKind
    {
        BadToken,
        EndOfFile,

        Identifier,
        Number,
        StringLiteral,

        DclKeyword,
        FixedKeyword,
        DecimalKeyword,
        DecKeyword,
        BinaryKeyword,
        BinKeyword,
        CharKeyword,
        CharacterKeyword,
        VarcharKeyword,
        PicKeyword,
        PictureKeyword,
        InitKeyword,
        InitialKeyword,

        OpenParenthesis,
        CloseParenthesis,
        Comma,
        Semicolon,
        Asterisk
    }
}