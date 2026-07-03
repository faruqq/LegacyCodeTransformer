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
        ///     10 Param char(8);
        ///     10 Param2 char(1);
        /// end
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - GenerateDeclaration içerisinde
        /// - EglRecordDeclaration kaynak kodu üretiminde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Record description metadata, field metadata ve annotation üretimi
        /// geldiğinde bu method genişletilecektir.
        /// </summary>
        private static string GenerateRecordDeclaration(
    EglRecordDeclaration declaration)
        {
            var builder = new StringBuilder();

            builder.AppendLine(
                $"record {declaration.Name} type {declaration.RecordType}");

            var indentationByLevel = CreateIndentationByLevel(declaration.Fields);

            foreach (var field in declaration.Fields)
            {
                builder.AppendLine(
                    GenerateRecordFieldDeclaration(field, indentationByLevel));
            }

            builder.AppendLine("end");

            return builder.ToString();
        }

        /// <summary>
        /// EGL record field level değerlerinden indentation depth haritası üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// EGL record field'larının başındaki 5, 10, 15 gibi değerler hiyerarşik
        /// level bilgisidir.
        /// Bu değerler genelde artan sırayla parent-child ilişkisini gösterir.
        ///
        /// Ancak indentation hesabını doğrudan level / 5 gibi matematiksel bir
        /// formüle bağlamak doğru değildir.
        /// Çünkü legacy kaynaklarda farklı level değerleri kullanılabilir.
        ///
        /// Örneğin:
        ///
        /// 5 A
        ///     10 B
        ///         15 C
        ///
        /// veya:
        ///
        /// 3 A
        ///     7 B
        ///         11 C
        ///
        /// iki durumda da üç seviye vardır.
        ///
        /// Bu method record içindeki farklı level değerlerini küçükten büyüğe sıralar
        /// ve her level için bir indentation depth üretir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - EglCodeGenerator record field üretiminde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Nested structure, multi-level record ve farklı kurum level standartları
        /// desteklendiğinde generator'ın sabit level varsayımına bağlı kalmamasını sağlar.
        /// </summary>
        private static IReadOnlyDictionary<int, int> CreateIndentationByLevel(
            IReadOnlyList<EglRecordFieldDeclaration> fields)
        {
            return fields
                .Select(x => x.Level)
                .Distinct()
                .OrderBy(x => x)
                .Select((level, index) => new
                {
                    Level = level,
                    Depth = index + 1
                })
                .ToDictionary(
                    x => x.Level,
                    x => x.Depth);
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
        /// Örnek modeller:
        ///
        /// EglDecimalType
        /// - Precision: 8
        /// - Scale: 0
        ///
        /// Üretilen EGL:
        /// decimal(8,0)
        ///
        /// EglCharacterType
        /// - Length: 8
        ///
        /// Üretilen EGL:
        /// char(8)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - EGL variable declaration üretiminde
        /// - Application pipeline sonucunda hedef kaynak kod oluşturulurken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Record field üretimi, array tipleri, nullable/default value gibi
        /// EGL söz dizimleri desteklendiğinde data type üretimi merkezi olarak
        /// buradan yönetilecektir.
        /// </summary>
        private static string GenerateDataType(EglDataType dataType)
        {
            return dataType switch
            {
                EglDecimalType decimalType =>
                    $"decimal({decimalType.Precision},{decimalType.Scale})",

                EglCharacterType characterType =>
                    $"char({characterType.Length})",

                _ => "unknown"
            };
        }
    }
}
