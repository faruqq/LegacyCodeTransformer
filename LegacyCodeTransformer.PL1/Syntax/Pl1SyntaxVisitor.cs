using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Syntax;

/// <summary>
/// PL/I syntax tree üzerinde tip güvenli ziyaret davranışı sağlayan temel visitor sınıfıdır.
///
/// Neden var?
/// ----------------------
/// Parser tarafında declaration, data type, structure ve ileride statement modelleri
/// büyüdükçe transpiler, analyzer ve semantic katmanlarında sürekli switch / if type-check
/// yazmak bakım maliyetini artırır.
///
/// Ne çözüyor?
/// ----------------------
/// PL/I syntax modelini merkezi bir ziyaret altyapısıyla dolaşılabilir hale getirir.
/// Her syntax node ailesi için override edilebilir Visit methodları sunar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - Pl1SyntaxTree
/// - Pl1VariableDeclaration
/// - Pl1StructureDeclaration
/// - Pl1StructureMember
/// - Pl1CharacterType
/// - Pl1FixedDecimalType
/// - Pl1PictureType
///
/// Nerede kullanılır?
/// ----------------------
/// - SyntaxWalker içinde
/// - İleride Semantic Analysis katmanında
/// - İleride visitor tabanlı transpiler refactor çalışmalarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 statement parser ile Pl1AssignmentStatement, Pl1CallStatement,
/// Pl1IfStatement ve Pl1DoStatement modelleri eklendiğinde visitor standardı
/// genişletilerek tüm PL/I syntax ailesi aynı traversal modeliyle yönetilecektir.
/// </summary>
public abstract class Pl1SyntaxVisitor
{
    /// <summary>
    /// PL/I syntax tree root modelini ziyaret eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Syntax tree traversal genellikle root modelden başlar.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Root syntax tree için override edilebilir tek giriş noktası sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Pl1Parser.Parse sonucu oluşan Pl1SyntaxTree modeli.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1SyntaxWalker içinde
    /// - Syntax tree üzerinde analiz yapan servislerde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Program unit, procedure ve statement listesi syntax tree'ye eklendiğinde
    /// root traversal davranışı bu method üzerinden genişletilecektir.
    /// </summary>
    public virtual void Visit(Pl1SyntaxTree syntaxTree)
    {
        if (syntaxTree is null)
        {
            return;
        }

        VisitSyntaxTree(syntaxTree);
    }

    /// <summary>
    /// PL/I declaration base modelini gerçek declaration tipine göre ziyaret eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Syntax tree declaration listesi base Pl1Declaration tipinden oluşur.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Declaration base tipi üzerinden gelen modeli gerçek concrete tipe dispatch eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Pl1VariableDeclaration
    /// - Pl1StructureDeclaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1SyntaxWalker.VisitSyntaxTree içinde
    /// - Declaration odaklı analizlerde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Procedure declaration veya statement-aware declaration modelleri eklendiğinde
    /// dispatch davranışı buradan genişletilecektir.
    /// </summary>
    public virtual void Visit(Pl1Declaration declaration)
    {
        switch (declaration)
        {
            case null:
                return;

            case Pl1VariableDeclaration variableDeclaration:
                Visit(variableDeclaration);
                return;

            case Pl1StructureDeclaration structureDeclaration:
                Visit(structureDeclaration);
                return;

            default:
                DefaultVisit(declaration);
                return;
        }
    }

    /// <summary>
    /// PL/I variable declaration modelini ziyaret eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Variable declaration; isim, veri tipi, optional array size ve optional initial value
    /// bilgilerini taşır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Variable declaration özelinde override edilebilir ziyaret noktası sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL PARAM CHAR(08) INIT(' ');
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - SyntaxWalker traversal içinde
    /// - Transpiler ve semantic analyzer tarafında
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// BASED, STATIC, AUTOMATIC gibi declaration attribute analizleri burada
    /// genişletilebilir.
    /// </summary>
    public virtual void Visit(Pl1VariableDeclaration declaration)
    {
        if (declaration is null)
        {
            return;
        }

        VisitVariableDeclaration(declaration);
    }

    /// <summary>
    /// PL/I structure declaration modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1StructureDeclaration declaration)
    {
        if (declaration is null)
        {
            return;
        }

        VisitStructureDeclaration(declaration);
    }

    /// <summary>
    /// PL/I structure member modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1StructureMember member)
    {
        if (member is null)
        {
            return;
        }

        VisitStructureMember(member);
    }

    /// <summary>
    /// PL/I initial value modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1InitialValue initialValue)
    {
        if (initialValue is null)
        {
            return;
        }

        VisitInitialValue(initialValue);
    }

    /// <summary>
    /// PL/I data type base modelini gerçek data type tipine göre ziyaret eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Declaration ve structure member modelleri data type bilgisini base Pl1DataType
    /// olarak taşır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Base data type modelini concrete data type visitor methodlarına yönlendirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Pl1CharacterType
    /// - Pl1FixedDecimalType
    /// - Pl1FixedBinaryType
    /// - Pl1PictureType
    /// - Pl1BitType
    /// - Pl1FloatingType
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - SyntaxWalker içinde
    /// - Type analysis ve transpiler mapping sırasında
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// POINTER, AREA, ENTRY, FILE gibi yeni PL/I data type modelleri eklendiğinde
    /// dispatch davranışı buradan genişletilecektir.
    /// </summary>
    public virtual void Visit(Pl1DataType dataType)
    {
        switch (dataType)
        {
            case null:
                return;

            case Pl1CharacterType characterType:
                Visit(characterType);
                return;

            case Pl1VarcharType varcharType:
                Visit(varcharType);
                return;

            case Pl1FixedDecimalType fixedDecimalType:
                Visit(fixedDecimalType);
                return;

            case Pl1FixedBinaryType fixedBinaryType:
                Visit(fixedBinaryType);
                return;

            case Pl1PictureType pictureType:
                Visit(pictureType);
                return;

            case Pl1BitType bitType:
                Visit(bitType);
                return;

            case Pl1FloatingType floatingType:
                Visit(floatingType);
                return;

            default:
                DefaultVisit(dataType);
                return;
        }
    }

    /// <summary>
    /// PL/I CHAR / CHARACTER data type modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1CharacterType dataType)
    {
        if (dataType is null)
        {
            return;
        }

        VisitCharacterType(dataType);
    }

    /// <summary>
    /// PL/I VARCHAR data type modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1VarcharType dataType)
    {
        if (dataType is null)
        {
            return;
        }

        VisitVarcharType(dataType);
    }

    /// <summary>
    /// PL/I FIXED DECIMAL data type modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1FixedDecimalType dataType)
    {
        if (dataType is null)
        {
            return;
        }

        VisitFixedDecimalType(dataType);
    }

    /// <summary>
    /// PL/I FIXED BINARY data type modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1FixedBinaryType dataType)
    {
        if (dataType is null)
        {
            return;
        }

        VisitFixedBinaryType(dataType);
    }

    /// <summary>
    /// PL/I PIC / PICTURE data type modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1PictureType dataType)
    {
        if (dataType is null)
        {
            return;
        }

        VisitPictureType(dataType);
    }

    /// <summary>
    /// PL/I BIT data type modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1BitType dataType)
    {
        if (dataType is null)
        {
            return;
        }

        VisitBitType(dataType);
    }

    /// <summary>
    /// PL/I FLOAT / REAL / DOUBLE data type modelini ziyaret eder.
    /// </summary>
    public virtual void Visit(Pl1FloatingType dataType)
    {
        if (dataType is null)
        {
            return;
        }

        VisitFloatingType(dataType);
    }

    protected virtual void VisitSyntaxTree(Pl1SyntaxTree syntaxTree)
    {
        DefaultVisit(syntaxTree);
    }

    protected virtual void VisitVariableDeclaration(Pl1VariableDeclaration declaration)
    {
        DefaultVisit(declaration);
    }

    protected virtual void VisitStructureDeclaration(Pl1StructureDeclaration declaration)
    {
        DefaultVisit(declaration);
    }

    protected virtual void VisitStructureMember(Pl1StructureMember member)
    {
        DefaultVisit(member);
    }

    protected virtual void VisitInitialValue(Pl1InitialValue initialValue)
    {
        DefaultVisit(initialValue);
    }

    protected virtual void VisitCharacterType(Pl1CharacterType dataType)
    {
        DefaultVisit(dataType);
    }

    protected virtual void VisitVarcharType(Pl1VarcharType dataType)
    {
        DefaultVisit(dataType);
    }

    protected virtual void VisitFixedDecimalType(Pl1FixedDecimalType dataType)
    {
        DefaultVisit(dataType);
    }

    protected virtual void VisitFixedBinaryType(Pl1FixedBinaryType dataType)
    {
        DefaultVisit(dataType);
    }

    protected virtual void VisitPictureType(Pl1PictureType dataType)
    {
        DefaultVisit(dataType);
    }

    protected virtual void VisitBitType(Pl1BitType dataType)
    {
        DefaultVisit(dataType);
    }

    protected virtual void VisitFloatingType(Pl1FloatingType dataType)
    {
        DefaultVisit(dataType);
    }

    protected virtual void DefaultVisit(object node)
    {
    }
}