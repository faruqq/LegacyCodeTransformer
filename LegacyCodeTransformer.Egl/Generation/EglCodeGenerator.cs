using System.Text;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.InitialValues;
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
/// EGL declaration ve data type modellerini kurum EGL output standardına uygun string çıktıya dönüştürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - Param char(10);
/// - Param char(10)[2];
/// - Param char(4) = "ABCD";
/// - record CustomerInfo type basicRecord
/// - record CustomerInfo type sqlRecord
///
/// Nerede kullanılır?
/// ----------------------
/// - Application pipeline içerisinde
/// - PL/I → EGL dönüşümünün son aşamasında
/// - Gelecekte CLI, GUI veya IDE entegrasyonlarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// EGL desteği genişledikçe function, record metadata, service, statement ve expression üretimi bu sınıfta geliştirilecektir.
/// </summary>
public sealed class EglCodeGenerator
{
    /// <summary>
    /// Verilen EglSyntaxTree modelini EGL kaynak koduna dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Generator'ın dışarıya açılan ana methodudur.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Syntax tree üzerindeki declaration listesini sırayla EGL kaynak koduna çevirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Tekil variable declaration
    /// - Array variable declaration
    /// - Initial value taşıyan variable declaration
    /// - Record declaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ConversionService içinde
    /// - Generator unit testlerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Program, function, service ve statement seviyesinde EGL output üretimi bu method üzerinden genişletilebilir.
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
            builder.Append(GenerateDeclaration(declaration));
        }

        return builder.ToString();
    }

    /// <summary>
    /// EGL declaration modelinden kaynak kod karşılığını üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL syntax tree yalnızca variable declaration taşımaz. Record declaration gibi farklı declaration türleri de desteklenir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Declaration türüne göre doğru generator methoduna yönlendirme yapar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - EglVariableDeclaration
    /// - EglRecordDeclaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Generate ana akışı içerisinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Function, service, library ve statement üretimleri eklendiğinde generator dispatch davranışı burada genişletilecektir.
    /// </summary>
    private static string GenerateDeclaration(EglDeclaration declaration)
    {
        return declaration switch
        {
            EglVariableDeclaration variableDeclaration =>
                GenerateVariableDeclaration(variableDeclaration) + Environment.NewLine,
            EglRecordDeclaration recordDeclaration =>
                GenerateRecordDeclaration(recordDeclaration),
            _ => string.Empty
        };
    }

    /// <summary>
    /// EGL record declaration modelinden record kaynak kodunu üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I structure declaration ifadeleri EGL tarafında record olarak üretilecektir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EglRecordDeclaration modelindeki name, record type ve field listesini EGL record syntax'ına dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// record CustomerInfo type basicRecord
    ///     10 CustomerName char(20);
    /// end
    ///
    /// record CustomerInfo type sqlRecord
    ///     10 CustomerName char(20);
    /// end
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - GenerateDeclaration içerisinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Record description metadata, field metadata, nested structure ve annotation üretimi geldiğinde bu method genişletilecektir.
    /// </summary>
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

    /// <summary>
    /// EGL record field declaration modelinden kaynak kod satırı üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Record field'lar level, indentation, name, data type ve optional array suffix bilgisiyle yazdırılmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Field indentation ve `[n]` array suffix üretimini merkezi hale getirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - 10 Param char(8);
    /// - 10 Param char(8)[2];
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - GenerateRecordDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Field-level default value, nullable metadata veya SQL column annotation üretimi bu method üzerinden genişletilebilir.
    /// </summary>
    private static string GenerateRecordFieldDeclaration(
        EglRecordFieldDeclaration field)
    {
        var indentation = GetRecordFieldIndentation(field.Level);
        var arraySuffix = field.ArraySize.HasValue
            ? $"[{field.ArraySize.Value}]"
            : string.Empty;

        return $"{indentation}{field.Level} {field.Name} {GenerateDataType(field.DataType)}{arraySuffix};";
    }

    /// <summary>
    /// EGL record field level değerine göre standart indentation üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I ve EGL record/structure alanlarında baştaki 5, 10, 15 gibi değerler field'ın hiyerarşik seviyesini gösterir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Proje standardı olan indentationSpaceCount = (level / 5) * 4 kuralını tek yerde uygular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Level 5 => 4 boşluk
    /// - Level 10 => 8 boşluk
    /// - Level 15 => 12 boşluk
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - GenerateRecordFieldDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nested structure ve multi-level record desteği için tutarlı indentation üretimini sağlar.
    /// </summary>
    private static string GetRecordFieldIndentation(int level)
    {
        var depth = Math.Max(level / 5, 1);

        return new string(' ', depth * 4);
    }

    /// <summary>
    /// EGL variable declaration modelinden kaynak kod satırı üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// EglVariableDeclaration modeli doğrudan string değildir. Generator aşamasında gerçek EGL kaynak kodu satırına çevrilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Scalar variable, array variable ve initial value taşıyan variable declaration output formatını merkezi olarak üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Param char(10);
    /// - Param char(10)[2];
    /// - Param char(4) = "ABCD";
    /// - Amount decimal(15,2);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - GenerateDeclaration içinde EglVariableDeclaration branch'inde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Nullable annotation, numeric default value ve daha gelişmiş EGL variable syntax üretimi bu method üzerinden genişletilebilir.
    /// </summary>
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

    /// <summary>
    /// EGL initial value modelinden kaynak kod karşılığını üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Initial value modelindeki raw değer doğrudan output'a yazılırsa string quoting ve escaping kuralları dağınık hale gelir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// String tabanlı EGL default value output'unu merkezi olarak üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - ABCD => "ABCD"
    /// - ; => ";"
    /// - boşluk => " "
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - GenerateVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Numeric literal, escaped quote, boolean ve array initialization formatları bu method üzerinden genişletilebilir.
    /// </summary>
    private static string GenerateInitialValue(
        EglInitialValue initialValue)
    {
        return $"\"{EscapeStringLiteral(initialValue.Value)}\"";
    }

    /// <summary>
    /// EGL string literal içinde güvenli kullanılacak escaped değer üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Başlangıç değeri içinde çift tırnak veya backslash gibi karakterler varsa doğrudan output üretmek geçersiz EGL syntax oluşturabilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// String literal içinde özel karakterleri escape eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - A"B => A\"B
    /// - A\B => A\\B
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - GenerateInitialValue içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Daha geniş EGL string literal escaping kuralları gerektiğinde merkezi helper olarak genişletilir.
    /// </summary>
    private static string EscapeStringLiteral(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    /// <summary>
    /// EGL veri tipi modelinden kaynak kod karşılığını üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL syntax tree üzerindeki veri tipi modelleri doğrudan string değildir.
    /// Code generator aşamasında bu modellerin gerçek EGL kaynak koduna dönüştürülmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Data type modellerini kurum EGL output standardına uygun string karşılıklarına dönüştürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - EglDecimalType(15, null) => decimal(15)
    /// - EglDecimalType(15, 0) => decimal(15,0)
    /// - EglCharacterType(8) => char(8)
    /// - EglSmallIntType => smallint
    /// - EglIntType => int
    /// - EglNumType(3, null) => num(3)
    /// - EglNumType(5, 2) => num(5,2)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - EGL variable declaration üretiminde
    /// - EGL record field üretiminde
    /// - Application pipeline sonucunda hedef kaynak kod oluşturulurken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Formatted PIC ve farklı numeric output türleri eklendiğinde data type üretimi merkezi olarak buradan yönetilecektir.
    /// </summary>
    private static string GenerateDataType(EglDataType dataType)
    {
        return dataType switch
        {
            EglDecimalType decimalType when decimalType.Scale.HasValue =>
                $"decimal({decimalType.Precision},{decimalType.Scale.Value})",
            EglDecimalType decimalType =>
                $"decimal({decimalType.Precision})",
            EglCharacterType characterType =>
                $"char({characterType.Length})",
            EglSmallIntType =>
                "smallint",
            EglIntType =>
                "int",
            EglNumType numType when numType.Scale.HasValue =>
                $"num({numType.Precision},{numType.Scale.Value})",
            EglNumType numType =>
                $"num({numType.Precision})",
            _ =>
                "unknown"
        };
    }
}