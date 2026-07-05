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
    /// PL/I declaration ifadesinin tekil değişken mi yoksa structure mı olduğunu belirler.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I tarafında DCL / DECLARE ifadesi hem tekil değişken hem de
    /// seviye numaralı structure declaration için kullanılabilir.
    ///
    /// DCL sonrasında Identifier gelirse tekil değişken, Number gelirse
    /// structure declaration parse edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana akışı DclKeyword gördüğünde
    /// - PL/I SyntaxTree declaration listesi oluşturulurken
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// File declaration, based declaration, factored declaration ve farklı
    /// DCL varyasyonları eklendiğinde declaration dispatch sorumluluğu
    /// bu method üzerinde genişletilecektir.
    /// </summary>
    private Pl1Declaration? ParseDeclaration()
    {
        if (Peek(1).Kind == Pl1TokenKind.Number)
        {
            return ParseStructureDeclaration();
        }

        return ParseVariableDeclaration();
    }

    /// <summary>
    /// PL/I değişken declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDeclaration methodu DCL sonrasında Identifier gördüğünde variable declaration
    /// parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat DCL, identifier,
    /// array size, data type, DIM / DIMENSION ve INIT / INITIAL parsing sorumluluğunu
    /// VariableDeclarationParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL MUST_NO FIXED DECIMAL(8);
    /// - DCL PARAM CHAR(08) INIT(' ');
    /// - DCL PARAM(2) CHAR(10);
    /// - DCL PARAM CHAR(10) DIM(2);
    /// - DCL PARAM CHAR(10) DIMENSION(2);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDeclaration dispatch methodu içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Pl1Parser variable declaration detaylarıyla büyümeden VariableDeclarationParser
    /// içinde geliştirilebilir.
    /// </summary>
    private Pl1VariableDeclaration? ParseVariableDeclaration()
    {
        var parser = new VariableDeclarationParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseVariableDeclaration();

        _position = result.Position;

        return result.Declaration;
    }

    /// <summary>
    /// PL/I seviye numaralı structure declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// ParseDeclaration methodu DCL sonrasında Number gördüğünde structure declaration
    /// parse davranışına yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat structure declaration,
    /// member, nested member, member data type, member INIT ve member dimension parsing
    /// sorumluluğunu StructureParser helper sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL 1 REC, 5 PARAM CHAR(08);
    /// - DCL 1 DIZI(6), 3 KOD CHAR(01);
    /// - DCL 1 MUSTERI, 5 ADRES, 10 IL CHAR(02);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParseDeclaration dispatch methodu içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Pl1Parser structure detaylarıyla büyümeden StructureParser içinde geliştirilebilir.
    /// </summary>
    private Pl1StructureDeclaration? ParseStructureDeclaration()
    {
        var parser = new StructureParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseStructureDeclaration();

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