namespace LegacyCodeTransformer.Cli
{
    /// <summary>
    /// LegacyCodeTransformer CLI çalıştırma seçeneklerini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Komut satırı argümanlarının Program.cs içinde dağınık if/switch bloklarıyla
    /// yönetilmesi, dosya tabanlı kullanım genişledikçe bakım maliyetini artırır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// CLI tarafından kullanılacak giriş ve çıkış dosyası yollarını güçlü tipli
    /// tek bir model altında toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// --input "samples/Case001/input.pl1"
    /// --output "samples/Case001/actual.egl"
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// CliArgumentParser çıktısında ve Program.cs dosya dönüşüm akışında kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Case klasörü çalıştırma, diagnostic gösterim seçenekleri veya verbose mod
    /// gibi gerçek CLI ihtiyaçları oluştuğunda kontrollü biçimde genişletilebilir.
    /// </summary>
    public sealed class CliOptions
    {
        public string InputFilePath { get; }

        public string? OutputFilePath { get; }

        public CliOptions(
            string inputFilePath,
            string? outputFilePath)
        {
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
        }
    }
}