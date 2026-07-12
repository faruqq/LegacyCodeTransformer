namespace LegacyCodeTransformer.Cli
{
    /// <summary>
    /// LegacyCodeTransformer CLI çalıştırma seçeneklerini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Komut satırı argümanlarının Program.cs içinde dağınık koşullarla
    /// yönetilmesi, dosya ve case tabanlı kullanımlar arttıkça bakım
    /// maliyetini yükseltir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Tek dosya ve case klasörü çalışma seçeneklerini güçlü tipli tek bir
    /// model altında toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// --input "samples/Case001/input.pl1"
    /// --output "samples/Case001/actual.egl"
    ///
    /// --case "samples/Case001"
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// CliArgumentParser çıktısında ve Program.cs dosya dönüşüm akışında
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Gerçek ihtiyaç oluştuğunda diagnostic veya verbose seçeneklerinin
    /// kontrollü biçimde eklenmesine temel olur.
    /// </summary>
    public sealed class CliOptions
    {
        public string? InputFilePath { get; }

        public string? OutputFilePath { get; }

        public string? CaseDirectoryPath { get; }

        public bool IsCaseMode =>
            !string.IsNullOrWhiteSpace(CaseDirectoryPath);

        public CliOptions(
            string? inputFilePath,
            string? outputFilePath,
            string? caseDirectoryPath)
        {
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
            CaseDirectoryPath = caseDirectoryPath;
        }
    }
}