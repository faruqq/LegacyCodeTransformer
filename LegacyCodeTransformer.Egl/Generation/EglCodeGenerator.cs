using System.Text;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.InitialValues;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;
using LegacyCodeTransformer.Egl.Functions;

namespace LegacyCodeTransformer.Egl.Generation;

/// <summary>
/// EglSyntaxTree modelinden EGL kaynak kodu üretir.
///
/// Neden var?
/// ----------------------
/// Transpiler yalnızca güçlü tipli EGL syntax modeli üretir.
/// Bu modelin gerçek EGL kaynak koduna dönüştürülmesi ayrı bir generator
/// sorumluluğudur.
///
/// Ne çözüyor?
/// ----------------------
/// EGL declaration ve statement modellerini indentation, casing ve syntax
/// standartlarına uygun kaynak metne dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// CustomerNo decimal(8);
/// CustomerNo = MustNo;
/// FetchCustomer(CustomerNo);
///
/// Nerede kullanılır?
/// ----------------------
/// ConversionService pipeline içinde transpiler aşamasından sonra kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// EGL function, program part, embedded SQL ve daha gelişmiş generator
/// davranışları bu sınıf üzerinden genişletilecektir.
/// </summary>
public sealed class EglCodeGenerator
{
    /// <summary>
    /// EGL syntax tree modelinden EGL kaynak kodu üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL declaration, function ve top-level statement modelleri hedef
    /// kaynak dosyada kontrollü sırayla yazılmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Declaration'ları önce, function modellerini sonra ve top-level
    /// statement'ları en son üretir. Farklı root bölüm aileleri arasında
    /// okunabilir bir boş satır bırakır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CustomerNo decimal(8);
    ///
    /// function CustomerProcess()
    ///     CustomerNo = MustNo;
    /// end
    ///
    /// CustomerProcess();
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ConversionService pipeline içinde transpiler sonrasında kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL program part ve function parameter üretimi geldiğinde root output
    /// sıralaması bu method üzerinden genişletilecektir.
    /// </summary>
    public string Generate(EglSyntaxTree syntaxTree)
    {
        if (syntaxTree is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var declaration in syntaxTree.Declarations)
        {
            builder.Append(
                GenerateDeclaration(declaration));
        }

        if (syntaxTree.Declarations.Count > 0 &&
            syntaxTree.Functions.Count > 0)
        {
            builder.AppendLine();
        }

        for (var index = 0;
             index < syntaxTree.Functions.Count;
             index++)
        {
            builder.Append(
                GenerateFunction(
                    syntaxTree.Functions[index]));

            if (index < syntaxTree.Functions.Count - 1)
            {
                builder.AppendLine();
            }
        }

        if (syntaxTree.Functions.Count > 0 &&
            syntaxTree.Statements.Count > 0)
        {
            builder.AppendLine();
        }

        foreach (var statement in syntaxTree.Statements)
        {
            builder.Append(
                GenerateStatement(
                    statement,
                    0));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Parametresiz EGL function modelinden function kaynak bloğu üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I procedure body statement'larının EGL çıktısında kaybolmadan
    /// function bloğu içinde üretilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Function header'ını, bir seviye içeri girintilenen body
    /// statement'larını ve function sonlandırma satırını üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// function CustomerProcess()
    ///     CustomerNo = MustNo;
    ///     FetchCustomer(CustomerNo);
    /// end
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Generate methodunda EglSyntaxTree.Functions koleksiyonu işlenirken
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Function parameter, local declaration ve return type output
    /// davranışları bu method içinde kontrollü olarak genişletilecektir.
    /// </summary>
    private static string GenerateFunction(
        EglFunction function)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            $"function {function.Name}()");

        foreach (var statement in function.Statements)
        {
            builder.Append(
                GenerateStatement(
                    statement,
                    1));
        }

        builder.AppendLine("end");

        return builder.ToString();
    }

    private static string GenerateDeclaration(
        EglDeclaration declaration)
    {
        return declaration switch
        {
            EglVariableDeclaration variableDeclaration =>
                GenerateVariableDeclaration(variableDeclaration) +
                Environment.NewLine,

            EglRecordDeclaration recordDeclaration =>
                GenerateRecordDeclaration(recordDeclaration),

            _ => string.Empty
        };
    }

    private static string GenerateStatement(
        EglStatement statement,
        int indentationLevel)
    {
        return statement switch
        {
            EglAssignmentStatement assignmentStatement =>
                GenerateAssignmentStatement(
                    assignmentStatement,
                    indentationLevel) +
                Environment.NewLine,

            EglCallStatement callStatement =>
                GenerateFunctionInvocationStatement(
                    callStatement,
                    indentationLevel) +
                Environment.NewLine,

            EglIfStatement ifStatement =>
                GenerateIfStatement(
                    ifStatement,
                    indentationLevel),

            EglDoStatement doStatement =>
                GenerateDoStatement(
                    doStatement,
                    indentationLevel),

            _ => string.Empty
        };
    }

    private static string GenerateAssignmentStatement(
        EglAssignmentStatement statement,
        int indentationLevel)
    {
        return
            $"{GetStatementIndentation(indentationLevel)}" +
            $"{statement.Target} = {statement.Value};";
    }

    /// <summary>
    /// PL/I CALL dönüşümünden gelen EGL statement modelini doğrudan function
    /// invocation syntax'ına dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak dilinde procedure çağrıları CALL keyword'üyle yazılır.
    /// EGL hedef dilinde ise function çağrısı için call keyword'ü kullanılmaz.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Eski ve hatalı `call FetchCustomer();` çıktısı yerine
    /// `FetchCustomer();` biçiminde geçerli EGL function invocation üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    /// CALL FETCH_CUSTOMER(CUSTOMER_NO, CUSTOMER_NAME);
    ///
    /// EGL:
    /// FetchCustomer(CustomerNo, CustomerName);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// GenerateStatement içinde EglCallStatement modeli görüldüğünde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EglCallStatement modelinin ileride EglFunctionInvocationStatement olarak
    /// yeniden adlandırılması değerlendirilirse generator davranışı aynı kalır.
    /// </summary>
    private static string GenerateFunctionInvocationStatement(
        EglCallStatement statement,
        int indentationLevel)
    {
        var arguments = string.Join(
            ", ",
            statement.Arguments);

        return
            $"{GetStatementIndentation(indentationLevel)}" +
            $"{statement.ProcedureName}({arguments});";
    }

    private static string GenerateIfStatement(
        EglIfStatement statement,
        int indentationLevel)
    {
        var builder = new StringBuilder();
        var indentation = GetStatementIndentation(
            indentationLevel);

        builder.AppendLine(
            $"{indentation}if ({statement.Condition})");

        builder.Append(
            GenerateStatement(
                statement.ThenStatement,
                indentationLevel + 1));

        if (statement.ElseStatement is not null)
        {
            builder.AppendLine(
                $"{indentation}else");

            builder.Append(
                GenerateStatement(
                    statement.ElseStatement,
                    indentationLevel + 1));
        }

        builder.AppendLine(
            $"{indentation}end");

        return builder.ToString();
    }

    /// <summary>
    /// EGL DO / loop statement modelinden kaynak kod bloğu üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I DO statement modellerinin gerçek EGL block veya loop syntax'ına
    /// dönüştürülmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Block, While ve Until DO türlerini ortak indentation standardıyla
    /// output'a yazar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// do
    ///     ProcessCustomer();
    /// end
    ///
    /// while (Sqlcode = 0)
    ///     FetchCursor();
    /// end
    ///
    /// while (!(Eof))
    ///     CloseCursor();
    /// end
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// GenerateStatement içinde EglDoStatement branch'inde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested DO, IF THEN DO, ELSE DO ve daha gelişmiş loop formatting
    /// davranışları bu method üzerinden genişletilecektir.
    /// </summary>
    private static string GenerateDoStatement(
        EglDoStatement statement,
        int indentationLevel)
    {
        var builder = new StringBuilder();
        var indentation = GetStatementIndentation(
            indentationLevel);

        builder.AppendLine(
            GenerateDoHeader(
                statement,
                indentation));

        foreach (var childStatement in statement.Statements)
        {
            builder.Append(
                GenerateStatement(
                    childStatement,
                    indentationLevel + 1));
        }

        builder.AppendLine(
            $"{indentation}end");

        return builder.ToString();
    }

    private static string GenerateDoHeader(
        EglDoStatement statement,
        string indentation)
    {
        return statement.Kind switch
        {
            EglDoStatementKind.While =>
                $"{indentation}while ({statement.Condition})",

            EglDoStatementKind.Until =>
                $"{indentation}while (!({statement.Condition}))",

            _ =>
                $"{indentation}do"
        };
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
        var indentation = GetRecordFieldIndentation(
            field.Level);

        var arraySuffix = field.ArraySize.HasValue
            ? $"[{field.ArraySize.Value}]"
            : string.Empty;

        return
            $"{indentation}{field.Level} {field.Name} " +
            $"{GenerateDataType(field.DataType)}{arraySuffix};";
    }

    private static string GetRecordFieldIndentation(int level)
    {
        var depth = Math.Max(
            level / 5,
            1);

        return new string(
            ' ',
            depth * 4);
    }

    private static string GetStatementIndentation(
        int indentationLevel)
    {
        return new string(
            ' ',
            Math.Max(
                indentationLevel,
                0) * 4);
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

        return
            $"{declaration.Name} " +
            $"{GenerateDataType(declaration.DataType)}" +
            $"{arraySuffix}" +
            $"{initialValueSuffix};";
    }

    private static string GenerateInitialValue(
        EglInitialValue initialValue)
    {
        return
            $"\"{EscapeStringLiteral(initialValue.Value)}\"";
    }

    private static string EscapeStringLiteral(string value)
    {
        return value
            .Replace(
                "\\",
                "\\\\")
            .Replace(
                "\"",
                "\\\"");
    }

    private static string GenerateDataType(
        EglDataType dataType)
    {
        return dataType switch
        {
            EglDecimalType decimalType
                when decimalType.Scale.HasValue =>
                $"decimal({decimalType.Precision},{decimalType.Scale.Value})",

            EglDecimalType decimalType =>
                $"decimal({decimalType.Precision})",

            EglCharacterType characterType =>
                $"char({characterType.Length})",

            EglSmallIntType =>
                "smallint",

            EglIntType =>
                "int",

            EglNumType numType
                when numType.Scale.HasValue =>
                $"num({numType.Precision},{numType.Scale.Value})",

            EglNumType numType =>
                $"num({numType.Precision})",

            EglFloatType =>
                "float",

            EglSmallFloatType =>
                "smallfloat",

            _ =>
                "unknown"
        };
    }
}