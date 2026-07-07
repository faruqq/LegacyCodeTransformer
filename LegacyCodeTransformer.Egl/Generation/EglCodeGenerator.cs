using System.Text;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.InitialValues;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Generation;

/// <summary>
/// EglSyntaxTree modelinden EGL kaynak kodu üretir.
///
/// Neden var?
/// ----------------------
/// Transpiler yalnızca hedef dil modeli olan EglSyntaxTree üretir.
/// Gerçek .egl kaynak kodunu yazdırma sorumluluğu bu sınıfa aittir.
///
/// Ne çözüyor?
/// ----------------------
/// EGL declaration, data type, assignment statement, CALL statement ve IF statement
/// modellerini kurum EGL output standardına uygun string çıktıya dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - Param char(10);
/// - Param = "ABC";
/// - call FetchCursor();
/// - if (A = B)
///
/// Nerede kullanılır?
/// ----------------------
/// - Application pipeline içerisinde
/// - PL/I → EGL dönüşümünün son aşamasında
/// - Gelecekte CLI, GUI veya IDE entegrasyonlarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// EGL desteği genişledikçe function, record metadata, service, DO ve expression
/// üretimi bu sınıfta geliştirilecektir.
/// </summary>
public sealed class EglCodeGenerator
{
    public string Generate(EglSyntaxTree syntaxTree)
    {
        if (syntaxTree is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var declaration in syntaxTree.Declarations)
        {
            builder.Append(GenerateDeclaration(declaration));
        }

        foreach (var statement in syntaxTree.Statements)
        {
            builder.Append(GenerateStatement(statement, 0));
        }

        return builder.ToString();
    }

    private static string GenerateDeclaration(EglDeclaration declaration)
    {
        return declaration switch
        {
            EglVariableDeclaration variableDeclaration => GenerateVariableDeclaration(variableDeclaration) + Environment.NewLine,
            EglRecordDeclaration recordDeclaration => GenerateRecordDeclaration(recordDeclaration),
            _ => string.Empty
        };
    }

    private static string GenerateStatement(
        EglStatement statement,
        int indentationLevel)
    {
        return statement switch
        {
            EglAssignmentStatement assignmentStatement => GenerateAssignmentStatement(assignmentStatement, indentationLevel) + Environment.NewLine,
            EglCallStatement callStatement => GenerateCallStatement(callStatement, indentationLevel) + Environment.NewLine,
            EglIfStatement ifStatement => GenerateIfStatement(ifStatement, indentationLevel),
            _ => string.Empty
        };
    }

    private static string GenerateAssignmentStatement(
        EglAssignmentStatement statement,
        int indentationLevel)
    {
        return $"{GetStatementIndentation(indentationLevel)}{statement.Target} = {statement.Value};";
    }

    private static string GenerateCallStatement(
        EglCallStatement statement,
        int indentationLevel)
    {
        var arguments = string.Join(", ", statement.Arguments);

        return $"{GetStatementIndentation(indentationLevel)}call {statement.ProcedureName}({arguments});";
    }

    /// <summary>
    /// EGL IF statement modelinden kaynak kod bloğu üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.10 kapsamında IF statement modeli gerçek EGL kaynak kodu bloğuna
    /// çevrilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// IF condition, THEN statement ve optional ELSE statement alanlarını indentation
    /// standardıyla EGL output'a yazar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     if (A = B)
    ///         call Proc1();
    ///     else
    ///         call Proc2();
    ///     end
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// GenerateStatement içinde EglIfStatement branch'inde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// DO block output ve nested control-flow formatting bu methodla aynı indentation
    /// standardını kullanacaktır.
    /// </summary>
    private static string GenerateIfStatement(
        EglIfStatement statement,
        int indentationLevel)
    {
        var builder = new StringBuilder();
        var indentation = GetStatementIndentation(indentationLevel);

        builder.AppendLine($"{indentation}if ({statement.Condition})");
        builder.Append(GenerateStatement(statement.ThenStatement, indentationLevel + 1));

        if (statement.ElseStatement is not null)
        {
            builder.AppendLine($"{indentation}else");
            builder.Append(GenerateStatement(statement.ElseStatement, indentationLevel + 1));
        }

        builder.AppendLine($"{indentation}end");

        return builder.ToString();
    }

    private static string GenerateRecordDeclaration(
        EglRecordDeclaration declaration)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            $"record {declaration.Name} type {declaration.RecordType}");

        foreach (var field in declaration.Fields)
        {
            builder.AppendLine(
                GenerateRecordFieldDeclaration(field));
        }

        builder.AppendLine("end");

        return builder.ToString();
    }

    private static string GenerateRecordFieldDeclaration(
        EglRecordFieldDeclaration field)
    {
        var indentation = GetRecordFieldIndentation(field.Level);
        var arraySuffix = field.ArraySize.HasValue
            ? $"[{field.ArraySize.Value}]"
            : string.Empty;

        return $"{indentation}{field.Level} {field.Name} {GenerateDataType(field.DataType)}{arraySuffix};";
    }

    private static string GetRecordFieldIndentation(int level)
    {
        var depth = Math.Max(level / 5, 1);

        return new string(' ', depth * 4);
    }

    private static string GetStatementIndentation(int indentationLevel)
    {
        return new string(' ', Math.Max(indentationLevel, 0) * 4);
    }

    private static string GenerateVariableDeclaration(
        EglVariableDeclaration declaration)
    {
        var arraySuffix = declaration.ArraySize.HasValue
            ? $"[{declaration.ArraySize.Value}]"
            : string.Empty;

        var initialValueSuffix = declaration.InitialValue is not null
            ? $" = {GenerateInitialValue(declaration.InitialValue)}"
            : string.Empty;

        return $"{declaration.Name} {GenerateDataType(declaration.DataType)}{arraySuffix}{initialValueSuffix};";
    }

    private static string GenerateInitialValue(EglInitialValue initialValue)
    {
        return $"\"{EscapeStringLiteral(initialValue.Value)}\"";
    }

    private static string EscapeStringLiteral(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    private static string GenerateDataType(EglDataType dataType)
    {
        return dataType switch
        {
            EglDecimalType decimalType when decimalType.Scale.HasValue => $"decimal({decimalType.Precision},{decimalType.Scale.Value})",
            EglDecimalType decimalType => $"decimal({decimalType.Precision})",
            EglCharacterType characterType => $"char({characterType.Length})",
            EglSmallIntType => "smallint",
            EglIntType => "int",
            EglNumType numType when numType.Scale.HasValue => $"num({numType.Precision},{numType.Scale.Value})",
            EglNumType numType => $"num({numType.Precision})",
            EglFloatType => "float",
            EglSmallFloatType => "smallfloat",
            _ => "unknown"
        };
    }
}