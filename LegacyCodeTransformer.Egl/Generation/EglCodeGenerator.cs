using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;
using System.Text;

namespace LegacyCodeTransformer.Egl.Generation
{
    /// <summary>
    /// EglSyntaxTree modelinden EGL kaynak kodu üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Transpiler yalnızca hedef dil modeli olan EglSyntaxTree üretir.
    /// Gerçek .egl kaynak kodunu yazdırma sorumluluğu bu sınıfa aittir.
    ///
    /// Bu sayede Transpiler dönüşüm kurallarına odaklanır,
    /// Generator ise yalnızca EGL kod formatına odaklanır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application pipeline içerisinde
    /// - PL/I → EGL dönüşümünün son aşamasında
    /// - Gelecekte CLI, GUI veya IDE entegrasyonlarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// EGL desteği genişledikçe variable declaration dışında function,
    /// record, service, statement ve expression üretimi de bu sınıfta
    /// geliştirilecektir.
    ///
    /// İlk sürümde yalnızca EGL variable declaration üretir.
    /// </summary>
    public sealed class EglCodeGenerator
    {
        /// <summary>
        /// Verilen EglSyntaxTree modelini EGL kaynak koduna dönüştürür.
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
        /// EGL syntax tree artık yalnızca variable declaration taşımaz.
        /// Record declaration gibi farklı declaration türleri de desteklenir.
        ///
        /// Bu method declaration türüne göre doğru generator methoduna yönlendirir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Generate ana akışı içerisinde
        /// - EGL declaration listesinden kaynak kod üretirken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Function, service, library ve statement üretimleri eklendiğinde
        /// generator dispatch davranışı burada genişletilecektir.
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
        /// PL/I structure declaration ifadeleri EGL tarafında record olarak
        /// üretilecektir.
        ///
        /// Örnek EGL:
        ///
        /// record ParameList type basicRecord
        ///         10 Param char(8);
        ///         10 Param2 char(1);
        /// end
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - GenerateDeclaration içerisinde
        /// - EglRecordDeclaration kaynak kodu üretiminde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Record description metadata, field metadata, nested structure ve
        /// annotation üretimi geldiğinde bu method genişletilecektir.
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

        private static string GenerateRecordFieldDeclaration(EglRecordFieldDeclaration field)
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
        /// PL/I ve EGL record/structure alanlarında baştaki 5, 10, 15 gibi değerler
        /// field'ın hiyerarşik seviyesini gösterir.
        ///
        /// Proje standardı olarak EGL output level değerleri 5 ve 5'in katları
        /// şeklinde üretilecektir.
        ///
        /// Bu nedenle indentation hesabı level değerinin 5'e bölünmesiyle bulunur.
        ///
        /// Örnek:
        ///
        /// 5  -> 4 boşluk
        /// 10 -> 8 boşluk
        /// 15 -> 12 boşluk
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - EglCodeGenerator record field üretiminde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Nested structure ve multi-level record desteği geldiğinde her level için
        /// tutarlı indentation üretimini sağlar.
        /// </summary>
        private static string GetRecordFieldIndentation(int level)
        {
            var depth = Math.Max(level / 5, 1);

            return new string(' ', depth * 4);
        }

        private static string GenerateVariableDeclaration(EglVariableDeclaration declaration)
        {
            return $"{declaration.Name} {GenerateDataType(declaration.DataType)};";
        }

        /// <summary>
        /// EGL veri tipi modelinden kaynak kod karşılığını üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// EGL syntax tree üzerindeki veri tipi modelleri doğrudan string değildir.
        /// Code generator aşamasında bu modellerin gerçek EGL kaynak koduna
        /// dönüştürülmesi gerekir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Data type modellerini kurum EGL output standardına uygun string
        /// karşılıklarına dönüştürür.
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
        /// Formatted PIC ve farklı numeric output türleri eklendiğinde data type
        /// üretimi merkezi olarak buradan yönetilecektir.
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

                _ => "unknown"
            };
        }
    }
}
