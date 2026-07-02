namespace LegacyCodeTransformer.Transpilers.Naming
{
    /// <summary>
    /// Kaynak identifier adını seçilen naming style değerine göre dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I identifier adları genellikle büyük harfli ve alt çizgili yazılır.
    ///
    /// Örnek:
    ///
    /// MUST_NO
    /// CUSTOMER_NO
    /// PROCESS_CODE
    ///
    /// EGL tarafında bu adların hangi casing kuralıyla üretileceği Parser veya
    /// Generator sorumluluğu olmamalıdır.
    ///
    /// Parser kaynak adı aynen korur.
    /// Generator kendisine gelen EGL modelini aynen yazar.
    /// Bu yüzden isim dönüşümü Transpiler katmanında merkezi bir yardımcı ile
    /// yapılmalıdır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler declaration dönüşümünde
    /// - Unit testlerde isim dönüşüm kurallarını doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Record field, structure member, procedure parameter, function name gibi
    /// farklı identifier türleri dönüştürülürken aynı merkezi davranış
    /// kullanılacaktır.
    /// </summary>
    public static class IdentifierNameTransformer
    {
        /// <summary>
        /// Identifier adını seçilen naming style değerine göre dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I → EGL dönüşümünde isim casing kuralı tek bir hardcoded methoda
        /// bağlı kalmamalıdır.
        ///
        /// Örnek dönüşümler:
        ///
        /// Preserve:
        /// MUST_NO -> MUST_NO
        ///
        /// CamelCase:
        /// MUST_NO -> mustNo
        ///
        /// PascalCase:
        /// MUST_NO -> MustNo
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Pl1ToEglTranspiler içerisinde
        /// - Naming strategy unit testlerinde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Yeni naming style değerleri eklendiğinde switch genişletilerek
        /// mevcut çağrı noktaları değiştirilmeden destek sağlanabilir.
        /// </summary>
        public static string Transform(
            string sourceName,
            IdentifierNamingStyle style)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return string.Empty;
            }

            return style switch
            {
                IdentifierNamingStyle.Preserve => sourceName,
                IdentifierNamingStyle.CamelCase => ToCamelCase(sourceName),
                IdentifierNamingStyle.PascalCase => ToPascalCase(sourceName),
                _ => ToPascalCase(sourceName)
            };
        }

        /// <summary>
        /// Identifier adını PascalCase formatına dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Firma EGL kodlarında çoğunlukla PascalCase kullanıldığı için
        /// varsayılan dönüşüm bu formata yapılacaktır.
        ///
        /// Örnek:
        ///
        /// MUST_NO -> MustNo
        /// CUSTOMER_NO -> CustomerNo
        /// DIZI_PARAM1 -> DiziParam1
        /// PARAM2 -> Param2
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Transform methodunda PascalCase seçildiğinde
        /// - CamelCase üretimi öncesinde ara format olarak
        /// </summary>
        private static string ToPascalCase(string sourceName)
        {
            var parts = sourceName
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(ToPascalCasePart)
                .ToArray();

            if (parts.Length == 0)
            {
                return string.Empty;
            }

            return string.Concat(parts);
        }

        /// <summary>
        /// Identifier adını camelCase formatına dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Mevcut projede daha önce lower camel case davranışı kullanılmıştı.
        /// Yeni strategy yapısında bu davranış geriye dönük desteklenmelidir.
        ///
        /// Örnek:
        ///
        /// MUST_NO -> mustNo
        /// CUSTOMER_NO -> customerNo
        /// DIZI_PARAM1 -> diziParam1
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Transform methodunda CamelCase seçildiğinde
        /// - Geriye dönük testlerde lower camel case davranışını doğrulamada
        /// </summary>
        private static string ToCamelCase(string sourceName)
        {
            var pascalCase = ToPascalCase(sourceName);

            if (string.IsNullOrEmpty(pascalCase))
            {
                return string.Empty;
            }

            return char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
        }

        /// <summary>
        /// Identifier parçasını PascalCase parçasına dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Alt çizgi ile ayrılmış PL/I identifier parçalarının her biri ayrı
        /// kelime gibi ele alınmalıdır.
        ///
        /// Örnek:
        ///
        /// MUST -> Must
        /// NO -> No
        /// PARAM1 -> Param1
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - ToPascalCase methodunda her identifier parçası için
        /// </summary>
        private static string ToPascalCasePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var lowerValue = value.ToLowerInvariant();

            return char.ToUpperInvariant(lowerValue[0]) + lowerValue[1..];
        }
    }
}