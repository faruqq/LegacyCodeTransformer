using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Syntax
{
    /// <summary>
    /// PL/I syntax tree üzerinde tip güvenli ziyaret davranışı sağlayan temel visitor sınıfıdır.
    ///
    /// Neden var?
    /// Parser tarafında declaration, procedure, data type, structure, initial value
    /// ve statement modelleri büyüdükçe transpiler, analyzer ve semantic
    /// katmanlarında sürekli switch / if type-check yazmak bakım maliyetini artırır.
    ///
    /// Ne çözüyor?
    /// PL/I syntax modelini merkezi bir ziyaret altyapısıyla dolaşılabilir hale getirir.
    /// Her syntax node ailesi için override edilebilir Visit methodları sunar.
    ///
    /// Hangi örneği destekliyor?
    /// Pl1SyntaxTree
    /// Pl1VariableDeclaration
    /// Pl1StructureDeclaration
    /// Pl1Procedure
    /// Pl1CharacterType
    /// Pl1FixedDecimalType
    /// Pl1PictureType
    /// Pl1AssignmentStatement
    /// Pl1CallStatement
    /// Pl1IfStatement
    /// Pl1DoStatement
    /// Pl1EmbeddedSqlStatement
    ///
    /// Nerede kullanılır?
    /// SyntaxWalker içinde, ileride semantic analysis katmanında ve visitor tabanlı
    /// transpiler refactor çalışmalarında kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// Procedure-level semantic analysis, call graph extraction, embedded SQL
    /// analysis, SQL dependency extraction ve visitor tabanlı transpiler davranışları
    /// aynı visitor standardı üzerinden genişletilecektir.
    /// </summary>
    public abstract class Pl1SyntaxVisitor
    {
        public virtual void Visit(Pl1SyntaxTree syntaxTree)
        {
            if (syntaxTree is null)
            {
                return;
            }

            VisitSyntaxTree(syntaxTree);
        }

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

        public virtual void Visit(Pl1VariableDeclaration declaration)
        {
            if (declaration is null)
            {
                return;
            }

            VisitVariableDeclaration(declaration);
        }

        public virtual void Visit(Pl1StructureDeclaration declaration)
        {
            if (declaration is null)
            {
                return;
            }

            VisitStructureDeclaration(declaration);
        }

        public virtual void Visit(Pl1StructureMember member)
        {
            if (member is null)
            {
                return;
            }

            VisitStructureMember(member);
        }

        public virtual void Visit(Pl1Procedure procedure)
        {
            if (procedure is null)
            {
                return;
            }

            VisitProcedure(procedure);
        }

        public virtual void Visit(Pl1InitialValue initialValue)
        {
            if (initialValue is null)
            {
                return;
            }

            VisitInitialValue(initialValue);
        }

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

        public virtual void Visit(Pl1CharacterType dataType)
        {
            if (dataType is null)
            {
                return;
            }

            VisitCharacterType(dataType);
        }

        public virtual void Visit(Pl1VarcharType dataType)
        {
            if (dataType is null)
            {
                return;
            }

            VisitVarcharType(dataType);
        }

        public virtual void Visit(Pl1FixedDecimalType dataType)
        {
            if (dataType is null)
            {
                return;
            }

            VisitFixedDecimalType(dataType);
        }

        public virtual void Visit(Pl1FixedBinaryType dataType)
        {
            if (dataType is null)
            {
                return;
            }

            VisitFixedBinaryType(dataType);
        }

        public virtual void Visit(Pl1PictureType dataType)
        {
            if (dataType is null)
            {
                return;
            }

            VisitPictureType(dataType);
        }

        public virtual void Visit(Pl1BitType dataType)
        {
            if (dataType is null)
            {
                return;
            }

            VisitBitType(dataType);
        }

        public virtual void Visit(Pl1FloatingType dataType)
        {
            if (dataType is null)
            {
                return;
            }

            VisitFloatingType(dataType);
        }

        public virtual void Visit(Pl1Statement statement)
        {
            switch (statement)
            {
                case null:
                    return;

                case Pl1AssignmentStatement assignmentStatement:
                    Visit(assignmentStatement);
                    return;

                case Pl1CallStatement callStatement:
                    Visit(callStatement);
                    return;

                case Pl1IfStatement ifStatement:
                    Visit(ifStatement);
                    return;

                case Pl1DoStatement doStatement:
                    Visit(doStatement);
                    return;

                case Pl1BlockStatement blockStatement:
                    Visit(blockStatement);
                    return;

                case Pl1EmbeddedSqlStatement embeddedSqlStatement:
                    Visit(embeddedSqlStatement);
                    return;

                case Pl1CompilerDirectiveStatement compilerDirectiveStatement:
                    Visit(compilerDirectiveStatement);
                    return;

                default:
                    DefaultVisit(statement);
                    return;
            }
        }

        public virtual void Visit(Pl1CompilerDirectiveStatement statement)
        {
            if (statement is null)
            {
                return;
            }

            VisitCompilerDirectiveStatement(statement);
        }

        protected virtual void VisitCompilerDirectiveStatement(Pl1CompilerDirectiveStatement statement)
        {
            DefaultVisit(statement);
        }

        public virtual void Visit(Pl1AssignmentStatement statement)
        {
            if (statement is null)
            {
                return;
            }

            VisitAssignmentStatement(statement);
        }

        public virtual void Visit(Pl1CallStatement statement)
        {
            if (statement is null)
            {
                return;
            }

            VisitCallStatement(statement);
        }

        public virtual void Visit(Pl1IfStatement statement)
        {
            if (statement is null)
            {
                return;
            }

            VisitIfStatement(statement);
        }

        public virtual void Visit(Pl1DoStatement statement)
        {
            if (statement is null)
            {
                return;
            }

            VisitDoStatement(statement);
        }

        public virtual void Visit(Pl1BlockStatement statement)
        {
            if (statement is null)
            {
                return;
            }

            VisitBlockStatement(statement);
        }

        public virtual void Visit(Pl1EmbeddedSqlStatement statement)
        {
            if (statement is null)
            {
                return;
            }

            VisitEmbeddedSqlStatement(statement);
        }

        public virtual void Visit(Pl1Expression expression)
        {
            switch (expression)
            {
                case null:
                    return;

                case Pl1RawExpression rawExpression:
                    Visit(rawExpression);
                    return;

                default:
                    DefaultVisit(expression);
                    return;
            }
        }

        public virtual void Visit(Pl1RawExpression expression)
        {
            if (expression is null)
            {
                return;
            }

            VisitRawExpression(expression);
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

        protected virtual void VisitProcedure(Pl1Procedure procedure)
        {
            DefaultVisit(procedure);
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

        protected virtual void VisitAssignmentStatement(Pl1AssignmentStatement statement)
        {
            DefaultVisit(statement);
        }

        protected virtual void VisitCallStatement(Pl1CallStatement statement)
        {
            DefaultVisit(statement);
        }

        protected virtual void VisitIfStatement(Pl1IfStatement statement)
        {
            DefaultVisit(statement);
        }

        protected virtual void VisitDoStatement(Pl1DoStatement statement)
        {
            DefaultVisit(statement);
        }

        protected virtual void VisitBlockStatement(Pl1BlockStatement statement)
        {
            DefaultVisit(statement);
        }

        protected virtual void VisitEmbeddedSqlStatement(Pl1EmbeddedSqlStatement statement)
        {
            DefaultVisit(statement);
        }

        protected virtual void VisitRawExpression(Pl1RawExpression expression)
        {
            DefaultVisit(expression);
        }

        protected virtual void DefaultVisit(object node)
        {
        }
    }
}