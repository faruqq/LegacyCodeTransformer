using LegacyCodeTransformer.Pl1.Declarations;

namespace LegacyCodeTransformer.Pl1.Syntax;

/// <summary>
/// PL/I syntax tree üzerinde varsayılan recursive traversal davranışı sağlayan walker sınıfıdır.
///
/// Neden var?
/// ----------------------
/// Visitor sınıfı node dispatch standardını sağlar fakat child node dolaşımını otomatik
/// yapmak her visitor için gerekli değildir.
///
/// Ne çözüyor?
/// ----------------------
/// Syntax tree, declaration, structure member, data type ve initial value traversal
/// davranışını merkezi ve override edilebilir hale getirir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - Syntax tree içindeki tüm declaration'ları dolaşmak
/// - Structure member child hiyerarşisini recursive dolaşmak
/// - Variable declaration data type ve initial value node'larına ulaşmak
///
/// Nerede kullanılır?
/// ----------------------
/// - İleride semantic analyzer traversal işlemlerinde
/// - İleride metrics ve dependency analyzer işlemlerinde
/// - İleride transpiler visitor refactor çalışmalarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 statement modelleri eklendiğinde Pl1BlockStatement, Pl1IfStatement,
/// Pl1DoStatement ve expression tree traversal davranışı bu walker içinde
/// genişletilecektir.
/// </summary>
public class Pl1SyntaxWalker : Pl1SyntaxVisitor
{
    /// <summary>
    /// Syntax tree root modelinin child declaration listesini dolaşır.
    ///
    /// Neden var?
    /// ----------------------
    /// Pl1SyntaxTree parser çıktısının ana root modelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Syntax tree altındaki declaration listesini sırayla ziyaret eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL PARAM1 CHAR(08); DCL PARAM2 FIXED DECIMAL(5);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Syntax tree genel traversal işlemlerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Syntax tree içine statement veya procedure listesi eklendiğinde traversal
    /// davranışı bu method üzerinden genişletilecektir.
    /// </summary>
    protected override void VisitSyntaxTree(Pl1SyntaxTree syntaxTree)
    {
        foreach (var declaration in syntaxTree.Declarations)
        {
            Visit(declaration);
        }
    }

    /// <summary>
    /// Variable declaration altındaki data type ve initial value node'larını dolaşır.
    ///
    /// Neden var?
    /// ----------------------
    /// Variable declaration yalnızca isimden ibaret değildir; data type ve optional
    /// initial value child bilgilerini de taşır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Variable declaration içindeki data type ve initial value modellerine recursive
    /// traversal erişimi sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL PARAM CHAR(08) INIT(' ');
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Type analysis
    /// - Initial value analysis
    /// - Transpiler traversal altyapısı
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Declaration attribute node'ları eklendiğinde bu method genişletilecektir.
    /// </summary>
    protected override void VisitVariableDeclaration(Pl1VariableDeclaration declaration)
    {
        Visit(declaration.DataType);

        if (declaration.InitialValue is not null)
        {
            Visit(declaration.InitialValue);
        }
    }

    /// <summary>
    /// Structure declaration altındaki member listesini dolaşır.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure declaration child member listesi taşır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Structure altındaki tüm root member modellerini sırayla ziyaret eder.
    ///
    /// Hangi örneği destekliyor?
    /// DCL 1 REC, 5 PARAM CHAR(08);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Structure analysis
    /// - Record mapping
    /// - Nested member traversal
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Structure attribute ve sqlRecord metadata traversal davranışları burada
    /// genişletilebilir.
    /// </summary>
    protected override void VisitStructureDeclaration(Pl1StructureDeclaration declaration)
    {
        foreach (var member in declaration.Members)
        {
            Visit(member);
        }
    }

    /// <summary>
    /// Structure member altındaki data type, initial value ve child member listesini dolaşır.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure member typed field veya group field olabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Typed member için data type / initial value, group member için child member
    /// traversal davranışını tek noktada toplar.
    ///
    /// Hangi örneği destekliyor?
    /// - 5 PARAM CHAR(08)
    /// - 5 ADRES, 10 IL CHAR(02)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Nested structure traversal
    /// - Record mapping
    /// - Semantic analysis
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Group-level array initialization veya metadata analysis bu method üzerinden
    /// genişletilebilir.
    /// </summary>
    protected override void VisitStructureMember(Pl1StructureMember member)
    {
        if (member.DataType is not null)
        {
            Visit(member.DataType);
        }

        if (member.InitialValue is not null)
        {
            Visit(member.InitialValue);
        }

        foreach (var childMember in member.Members)
        {
            Visit(childMember);
        }
    }
}