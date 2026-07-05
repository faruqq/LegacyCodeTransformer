using System;
using System.Collections.Generic;
using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Parsing;

/// <summary>
/// PL/I token listesini Pl1SyntaxTree modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// Lexer yalnızca kaynak kodu token'lara ayırır.
/// Ancak token listesi henüz anlamlı bir program modeli değildir.
///
/// Parser, token listesini okuyarak PL/I diline ait declaration
/// modellerini oluşturur.
///
/// Örnek PL/I:
///
/// DCL MUST_NO FIXED DECIMAL(8);
/// DCL PARAM CHAR(08) INIT(' ');
/// DCL 1 PARAME_LIST,
///     5 PARAM CHAR(08) INIT(' '),
///     5 PARAM2 CHAR(01) INIT(';');
///
/// Bu parser ilgili ifadeleri:
/// - Pl1VariableDeclaration
/// - Pl1StructureDeclaration
/// - Pl1FixedDecimalType
/// - Pl1CharacterType
/// - Pl1InitialValue
///
/// modellerine dönüştürür.
///
/// Nerede kullanılır?
/// ----------------------
/// - Application pipeline içerisinde
/// - PL/I kaynak kodunu SyntaxTree'ye dönüştürmek için
/// - Normalizer ve Transpiler aşamalarından önce
///
/// Gelecekte ne işe yarayacak?
/// ----------------------
/// PL/I desteği genişledikçe IF, CALL, DO/END, PROCEDURE, assignment,
/// embedded SQL, array declaration ve nested structure gibi yapılar
/// bu parser üzerinden SyntaxTree'ye çevrilecektir.
/// </summary>
public sealed class Pl1Parser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics = new();
    private int _position;

    /// <summary>
    /// PL/I parser instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser, lexer tarafından üretilen token listesini sırayla okuyarak
    /// syntax tree üretir.
    ///
    /// Token listesi null gelirse boş liste olarak ele alınır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application conversion pipeline içerisinde
    /// - Parser unit testlerinde
    /// </summary>
    public Pl1Parser(IReadOnlyList<Pl1Token> tokens)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
    }

    /// <summary>
    /// Token listesini okuyarak Pl1SyntaxTree üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodundan gelen token listesi, dönüşüm pipeline'ında
    /// kullanılabilecek güçlü tipli syntax tree modeline çevrilmelidir.
    ///
    /// Bu method şu anda declaration odaklı çalışır:
    /// - Tekil variable declaration
    /// - Basit structure declaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ConversionService içerisinde
    /// - Parser unit testlerinde
    /// - Normalizer ve Transpiler öncesinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I program yapısı genişledikçe declaration dışındaki statement
    /// türleri de bu ana parse akışı üzerinden yönlendirilecektir.
    /// </summary>
    public ParseResult<Pl1SyntaxTree> Parse()
    {
        var declarations = new List<Pl1Declaration>();

        while (!IsAtEnd())
        {
            if (Current.Kind == Pl1TokenKind.DclKeyword)
            {
                var declaration = ParseDeclaration();
                if (declaration is not null)
                {
                    declarations.Add(declaration);
                }

                continue;
            }

            AddUnexpectedTokenDiagnostic(
                Current,
                "DCL");

            Advance();
        }

        var syntaxTree = new Pl1SyntaxTree(
            declarations,
            SourceLocation.Unknown);

        return new ParseResult<Pl1SyntaxTree>(
            syntaxTree,
            _diagnostics.Diagnostics);
    }

    /// <summary>
    /// PL/I declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parse ana döngüsü DCL / DECLARE token gördüğünde declaration parse davranışına
    /// yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat DCL sonrası variable /
    /// structure declaration seçimi ve detay parsing sorumluluğunu DeclarationParser helper
    /// sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM CHAR(08);
    /// - DCL 1 REC, 5 PARAM CHAR(08);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana döngüsü içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// P05 statement parser eklendiğinde Pl1Parser ana orchestration sınıfı olarak kalır;
    /// declaration detayları DeclarationParser içinde büyür.
    /// </summary>
    private Pl1Declaration? ParseDeclaration()
    {
        var parser = new DeclarationParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseDeclaration();

        _position = result.Position;

        return result.Declaration;
    }

    /// <summary>
    /// Beklenen token türünü tüketir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser grammar ilerletirken belirli noktalarda belirli token
    /// türlerini bekler.
    ///
    /// Beklenen token gelirse token tüketilir.
    /// Beklenen token gelmezse diagnostic üretilir ve null döner.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Tüm parse methodlarında
    /// - Grammar doğrulamasında
    /// - Diagnostic üretiminde
    /// </summary>
    private Pl1Token? Consume(
        Pl1TokenKind expectedKind,
        string errorMessage)
    {
        if (Current.Kind == expectedKind)
        {
            return Advance();
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            errorMessage,
            Current.Location));

        return null;
    }

    /// <summary>
    /// Beklenmeyen token için diagnostic üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser desteklenmeyen veya grammar dışı bir token gördüğünde
    /// kullanıcıya anlamlı hata bilgisi üretmelidir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana akışında
    /// - Structure member ayracı beklenirken
    /// - Gelecekte statement parse hatalarında
    /// </summary>
    private void AddUnexpectedTokenDiagnostic(
        Pl1Token token,
        string expectedText)
    {
        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenmeyen token: {token.Text}. Beklenen: {expectedText}.",
            token.Location));
    }

    /// <summary>
    /// Mevcut pozisyona göre ileri bakış token'ını döndürür.
    ///
    /// Neden var?
    /// ----------------------
    /// DCL sonrasında gelen token'a bakarak declaration tipini seçmemiz
    /// gerekir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDeclaration içerisinde
    /// - Gelecekte lookahead gerektiren grammar ayrımlarında
    /// </summary>
    private Pl1Token Peek(int offset)
    {
        var index = _position + offset;

        if (index >= _tokens.Count)
        {
            return _tokens[^1];
        }

        return _tokens[index];
    }

    /// <summary>
    /// Mevcut token'ı tüketip bir sonraki token'a ilerler.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser token listesinde sırayla ilerlemelidir.
    /// Bu method Current token'ı tüketir ve tüketilen token'ı döndürür.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Consume methodunda
    /// - Hata toparlama sırasında
    /// - Ayracı manuel geçmek gerektiğinde
    /// </summary>
    private Pl1Token Advance()
    {
        if (!IsAtEnd())
        {
            _position++;
        }

        return Previous;
    }

    /// <summary>
    /// Parser'ın kaynak sonu token'ına gelip gelmediğini belirtir.
    /// </summary>
    private bool IsAtEnd()
    {
        return Current.Kind == Pl1TokenKind.EndOfFile;
    }

    /// <summary>
    /// Mevcut parser pozisyonundaki token'ı döndürür.
    /// </summary>
    private Pl1Token Current
    {
        get
        {
            if (_position >= _tokens.Count)
            {
                return _tokens[^1];
            }

            return _tokens[_position];
        }
    }

    /// <summary>
    /// Bir önce tüketilen token'ı döndürür.
    /// </summary>
    private Pl1Token Previous => _tokens[_position - 1];
}