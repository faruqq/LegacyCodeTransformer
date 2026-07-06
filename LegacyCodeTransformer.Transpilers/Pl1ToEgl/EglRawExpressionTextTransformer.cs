using System.Text;
using LegacyCodeTransformer.Transpilers.Naming;

namespace LegacyCodeTransformer.Transpilers.Pl1ToEgl
{
    /// <summary>
    /// PL/I raw expression metnini güvenli EGL raw expression metnine dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// P05 statement parser expression alanlarını Pl1RawExpression olarak taşımaktadır.
    /// Bu raw metinler EGL tarafına aynen geçirilemez; identifier casing ve string literal
    /// quoting kuralları hedef EGL standardına göre dönüştürülmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PL/I raw expression içindeki basit identifier tokenlarını naming strategy ile
    /// dönüştürür ve tek tırnaklı PL/I string literal değerlerini çift tırnaklı EGL
    /// string literal formatına çevirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     PARAM           -> Param
    ///     'ABC'           -> "ABC"
    ///     SQLCODE = 0     -> Sqlcode = 0
    ///     DCLGLAU.BRM_KOD -> Dclglau.BrmKod
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ExpressionTranspiler içinde Pl1RawExpression dönüşümünde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Gerçek expression parser eklendiğinde bu helper raw fallback dönüştürücü olarak
    /// kalabilir. Operator mapping, literal escaping ve function call dönüşümleri ihtiyaç
    /// ortaya çıktıkça burada geliştirilebilir.
    /// </summary>
    internal sealed class EglRawExpressionTextTransformer
    {
        private readonly IdentifierNamingStyle _namingStyle;

        public EglRawExpressionTextTransformer(IdentifierNamingStyle namingStyle)
        {
            _namingStyle = namingStyle;
        }

        /// <summary>
        /// PL/I raw expression text değerini EGL raw expression text değerine dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Transpiler katmanı EGL syntax modeli üretmeden önce expression text değerini
        /// hedef dil standardına uygun hale getirmelidir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Identifier casing dönüşümünü ve string literal quote dönüşümünü tek noktada
        /// toplar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        ///     PARAM = 'ABC'
        ///
        /// expression parçaları:
        ///
        ///     PARAM -> Param
        ///     'ABC' -> "ABC"
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// ExpressionTranspiler.TranspileExpression içinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// EGL expression output standardı geliştikçe bu method raw fallback davranışının
        /// merkezi noktası olarak genişletilecektir.
        /// </summary>
        public string Transform(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var index = 0;

            while (index < text.Length)
            {
                var current = text[index];

                if (current == '\'')
                {
                    index = AppendStringLiteral(text, index, builder);
                    continue;
                }

                if (IsIdentifierStart(current))
                {
                    index = AppendIdentifier(text, index, builder);
                    continue;
                }

                builder.Append(current);
                index++;
            }

            return builder.ToString();
        }

        private int AppendStringLiteral(
            string text,
            int startIndex,
            StringBuilder builder)
        {
            builder.Append('"');

            var index = startIndex + 1;

            while (index < text.Length && text[index] != '\'')
            {
                var current = text[index];

                if (current == '"' || current == '\\')
                {
                    builder.Append('\\');
                }

                builder.Append(current);
                index++;
            }

            builder.Append('"');

            return index < text.Length
                ? index + 1
                : index;
        }

        private int AppendIdentifier(
            string text,
            int startIndex,
            StringBuilder builder)
        {
            var index = startIndex;

            while (index < text.Length && IsIdentifierPart(text[index]))
            {
                index++;
            }

            var identifier = text.Substring(
                startIndex,
                index - startIndex);

            builder.Append(
                IdentifierNameTransformer.Transform(
                    identifier,
                    _namingStyle));

            return index;
        }

        private static bool IsIdentifierStart(char value)
        {
            return char.IsLetter(value) || value == '_';
        }

        private static bool IsIdentifierPart(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }
    }
}