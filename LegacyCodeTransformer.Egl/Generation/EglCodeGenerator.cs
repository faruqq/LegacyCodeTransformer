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
/// EGL declaration, data type, assignment statement ve CALL statement modellerini
/// kurum EGL output standardına uygun string çıktıya dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - Param char(10);
/// - Param = "ABC";
/// - call FetchCursor();
/// - call Proc1(A, B);
///
/// Nerede kullanılır?
/// ----------------------
/// - Application pipeline içerisinde
/// - PL/I → EGL dönüşümünün son aşamasında
/// - Gelecekte CLI, GUI veya IDE entegrasyonlarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// EGL desteği genişledikçe function, record metadata, service, IF, DO ve expression
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
            builder.Append(GenerateStatement(statement));
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

    /// <summary>
    /// EGL statement modelinden kaynak kod karşılığını üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// EglSyntaxTree executable statement listesi taşıyabilmektedir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Statement türüne göre doğru generator methoduna yönlendirme yapar.
    /// P05.9 kapsamında EglAssignmentStatement ve EglCallStatement desteklenir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     Param = "ABC";
    ///     call FetchCursor();
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Generate ana akışı içerisinde.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EglIfStatement ve EglDoStatement eklendiğinde dispatch davranışı burada
    /// genişletilecektir.
    /// </summary>
    private static string GenerateStatement(EglStatement statement)
    {
        return statement switch
        {
            EglAssignmentStatement assignmentStatement => GenerateAssignmentStatement(assignmentStatement) + Environment.NewLine,
            EglCallStatement callStatement => GenerateCallStatement(callStatement) + Environment.NewLine,
            _ => string.Empty
        };
    }

    private static string GenerateAssignmentStatement(EglAssignmentStatement statement)
    {
        return $"{statement.Target} = {statement.Value};";
    }

    /// <summary>
    /// EGL CALL statement modelinden kaynak kod satırı üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.9 kapsamında CALL statement modeli gerçek EGL kaynak kodu satırına
    /// çevrilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Procedure adı ve argument listesini EGL call syntax standardına göre yazdırır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     call FetchCursor();
    ///     call Proc1(A, B);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// GenerateStatement içinde EglCallStatement branch'inde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Named argument veya service invocation output standardı gerektiğinde bu method
    /// genişletilebilir.
    /// </summary>
    private static string GenerateCallStatement(EglCallStatement statement)
    {
        var arguments = string.Join(", ", statement.Arguments);

        return $"call {statement.ProcedureName}({arguments});";
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