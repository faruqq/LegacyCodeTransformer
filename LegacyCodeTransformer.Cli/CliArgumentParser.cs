namespace LegacyCodeTransformer.Cli
{
    /// <summary>
    /// LegacyCodeTransformer CLI argümanlarını CliOptions modeline dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Program.cs yalnızca uygulama akışını koordine etmelidir. Komut satırı
    /// argümanlarının doğrulanması ve seçenek modeline dönüştürülmesi ayrı bir
    /// sorumluluktur.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// --input ve --output argümanlarını case-insensitive olarak parse eder.
    /// Eksik değer, tekrar eden seçenek veya bilinmeyen argüman durumlarında
    /// kullanıcıya anlamlı hata mesajı döndürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// --input samples/Case001/input.pl1
    /// --output samples/Case001/actual.egl
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Program.cs başlangıcında CLI argümanlarını doğrulamak için kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Gerçek ihtiyaç oluştuğunda --case veya --help gibi yeni seçeneklerin
    /// merkezi biçimde eklenmesine temel olur.
    /// </summary>
    public sealed class CliArgumentParser
    {
        private const string InputArgument = "--input";
        private const string OutputArgument = "--output";

        /// <summary>
        /// Komut satırı argümanlarını parse etmeye çalışır.
        ///
        /// Neden var?
        /// ----------------------
        /// CLI girişinin conversion pipeline çalıştırılmadan önce doğrulanması gerekir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Geçerli argümanlarda CliOptions üretir. Geçersiz argümanlarda exception
        /// fırlatmadan false ve açıklayıcı hata mesajı döndürür.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// --input "samples/Case001/input.pl1"
        /// --output "samples/Case001/actual.egl"
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Program.cs içinde dönüşüm başlamadan önce çağrılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Case runner seçenekleri eklendiğinde aynı doğrulama giriş noktası
        /// genişletilebilir.
        /// </summary>
        public bool TryParse(
            IReadOnlyList<string>? args,
            out CliOptions? options,
            out string? errorMessage)
        {
            options = null;
            errorMessage = null;

            if (args is null || args.Count == 0)
            {
                errorMessage =
                    "Giriş dosyası belirtilmelidir. Kullanım: " +
                    "--input <input.pl1> [--output <actual.egl>]";

                return false;
            }

            string? inputFilePath = null;
            string? outputFilePath = null;

            for (var index = 0; index < args.Count; index++)
            {
                var argument = args[index];

                if (string.Equals(
                        argument,
                        InputArgument,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (inputFilePath is not null)
                    {
                        errorMessage =
                            "--input argümanı birden fazla kez kullanılamaz.";

                        return false;
                    }

                    if (!TryReadValue(
                            args,
                            ref index,
                            InputArgument,
                            out inputFilePath,
                            out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (string.Equals(
                        argument,
                        OutputArgument,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (outputFilePath is not null)
                    {
                        errorMessage =
                            "--output argümanı birden fazla kez kullanılamaz.";

                        return false;
                    }

                    if (!TryReadValue(
                            args,
                            ref index,
                            OutputArgument,
                            out outputFilePath,
                            out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                errorMessage = $"Bilinmeyen CLI argümanı: {argument}.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(inputFilePath))
            {
                errorMessage = "--input argümanı zorunludur.";

                return false;
            }

            options = new CliOptions(
                inputFilePath,
                outputFilePath);

            return true;
        }

        private static bool TryReadValue(
            IReadOnlyList<string> args,
            ref int index,
            string argumentName,
            out string? value,
            out string? errorMessage)
        {
            value = null;
            errorMessage = null;

            var valueIndex = index + 1;

            if (valueIndex >= args.Count ||
                string.IsNullOrWhiteSpace(args[valueIndex]) ||
                args[valueIndex].StartsWith(
                    "--",
                    StringComparison.Ordinal))
            {
                errorMessage =
                    $"{argumentName} argümanı için dosya yolu bekleniyordu.";

                return false;
            }

            value = args[valueIndex];
            index = valueIndex;

            return true;
        }
    }
}