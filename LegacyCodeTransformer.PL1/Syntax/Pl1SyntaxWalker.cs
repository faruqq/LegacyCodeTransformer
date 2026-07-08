using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Syntax
{
    /// <summary>
    /// PL/I syntax tree üzerinde varsayılan recursive traversal davranışı sağlayan walker sınıfıdır.
    ///
    /// Neden var?
    /// Visitor sınıfı node dispatch standardını sağlar fakat child node dolaşımını
    /// otomatik yapmak her visitor için gerekli değildir. Walker, varsayılan
    /// recursive dolaşımı merkezi hale getirir.
    ///
    /// Ne çözüyor?
    /// Syntax tree, declaration, procedure, structure member, data type, initial value,
    /// statement ve expression traversal davranışını merkezi ve override edilebilir
    /// hale getirir.
    ///
    /// Hangi örneği destekliyor?
    /// DCL PARAM CHAR(08);
    /// PROCEDURE_NAME: PROCEDURE;
    ///     EXEC SQL INCLUDE SQLCA;
    /// END PROCEDURE_NAME;
    ///
    /// Nerede kullanılır?
    /// Semantic analyzer traversal işlemlerinde, metrics/dependency analyzer
    /// işlemlerinde ve ileride transpiler visitor refactor çalışmalarında kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// SELECT, READ, WRITE, RETURN, STOP, LEAVE, embedded SQL ve procedure
    /// modelleri eklendiğinde recursive traversal davranışı bu sınıf üzerinden
    /// genişletilecektir.
    /// </summary>
    public class Pl1SyntaxWalker : Pl1SyntaxVisitor
    {
        protected override void VisitSyntaxTree(Pl1SyntaxTree syntaxTree)
        {
            foreach (var declaration in syntaxTree.Declarations)
            {
                Visit(declaration);
            }

            foreach (var procedure in syntaxTree.Procedures)
            {
                Visit(procedure);
            }

            foreach (var statement in syntaxTree.Statements)
            {
                Visit(statement);
            }
        }

        protected override void VisitProcedure(Pl1Procedure procedure)
        {
            foreach (var statement in procedure.Statements)
            {
                Visit(statement);
            }
        }

        protected override void VisitVariableDeclaration(Pl1VariableDeclaration declaration)
        {
            Visit(declaration.DataType);

            if (declaration.InitialValue is not null)
            {
                Visit(declaration.InitialValue);
            }
        }

        protected override void VisitStructureDeclaration(Pl1StructureDeclaration declaration)
        {
            foreach (var member in declaration.Members)
            {
                Visit(member);
            }
        }

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

        protected override void VisitAssignmentStatement(Pl1AssignmentStatement statement)
        {
            foreach (var target in statement.Targets)
            {
                Visit(target);
            }

            Visit(statement.Value);
        }

        protected override void VisitCallStatement(Pl1CallStatement statement)
        {
            foreach (var argument in statement.Arguments)
            {
                Visit(argument);
            }
        }

        protected override void VisitIfStatement(Pl1IfStatement statement)
        {
            Visit(statement.Condition);
            Visit(statement.ThenStatement);

            if (statement.ElseStatement is not null)
            {
                Visit(statement.ElseStatement);
            }
        }

        protected override void VisitDoStatement(Pl1DoStatement statement)
        {
            if (statement.Condition is not null)
            {
                Visit(statement.Condition);
            }

            Visit(statement.Body);
        }

        protected override void VisitBlockStatement(Pl1BlockStatement statement)
        {
            foreach (var childStatement in statement.Statements)
            {
                Visit(childStatement);
            }
        }

        protected override void VisitEmbeddedSqlStatement(Pl1EmbeddedSqlStatement statement)
        {
        }

        protected override void VisitCompilerDirectiveStatement(Pl1CompilerDirectiveStatement statement)
        {
        }

        protected override void VisitRawExpression(Pl1RawExpression expression)
        {
        }
    }
}