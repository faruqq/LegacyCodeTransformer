using LegacyCodeTransformer.Cli;

namespace LegacyCodeTransformer.Cli.Tests
{
    /// <summary>
    /// CLI argüman parser davranışını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Dosya tabanlı dönüşümün doğru giriş ve çıkış yollarıyla çalışabilmesi için
    /// komut satırı seçeneklerinin güvenilir biçimde parse edilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Geçerli --input / --output kullanımlarını ve temel hatalı argüman
    /// senaryolarını regression testleriyle sabitler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// --input samples/Case001/input.pl1
    /// --output samples/Case001/actual.egl
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// P10.1 CLI unit testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Case klasörü veya yeni CLI seçenekleri eklendiğinde argüman sözleşmesinin
    /// bozulmadığını doğrulamaya devam eder.
    /// </summary>
    public sealed class CliArgumentParserTests
    {
        [Fact]
        public void TryParse_WithInputArgument_ShouldCreateOptions()
        {
            var args = new[]
            {
                "--input",
                "samples/Case001/input.pl1"
            };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.True(success);
            Assert.Null(errorMessage);
            Assert.NotNull(options);
            Assert.Equal(
                "samples/Case001/input.pl1",
                options!.InputFilePath);
            Assert.Null(options.OutputFilePath);
        }

        [Fact]
        public void TryParse_WithInputAndOutputArguments_ShouldCreateOptions()
        {
            var args = new[]
            {
                "--input",
                "samples/Case001/input.pl1",
                "--output",
                "samples/Case001/actual.egl"
            };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.True(success);
            Assert.Null(errorMessage);
            Assert.NotNull(options);
            Assert.Equal(
                "samples/Case001/input.pl1",
                options!.InputFilePath);
            Assert.Equal(
                "samples/Case001/actual.egl",
                options.OutputFilePath);
        }

        [Fact]
        public void TryParse_WithoutInputArgument_ShouldReturnError()
        {
            var args = new[]
            {
                "--output",
                "samples/Case001/actual.egl"
            };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.False(success);
            Assert.Null(options);
            Assert.Equal(
                "--input veya --case argümanlarından biri zorunludur.",
                errorMessage);
        }

        [Fact]
        public void TryParse_WithMissingInputValue_ShouldReturnError()
        {
            var args = new[]
            {
                "--input"
            };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.False(success);
            Assert.Null(options);
            Assert.Equal(
                "--input argümanı için yol bekleniyordu.",
                errorMessage);
        }

        [Fact]
        public void TryParse_WithUnknownArgument_ShouldReturnError()
        {
            var args = new[]
            {
                "--source",
                "samples/Case001/input.pl1"
            };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.False(success);
            Assert.Null(options);
            Assert.Equal(
                "Bilinmeyen CLI argümanı: --source.",
                errorMessage);
        }

        [Fact]
        public void TryParse_WithDuplicateInputArgument_ShouldReturnError()
        {
            var args = new[]
            {
                "--input",
                "samples/Case001/input.pl1",
                "--input",
                "samples/Case002/input.pl1"
            };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.False(success);
            Assert.Null(options);
            Assert.Equal(
                "--input argümanı birden fazla kez kullanılamaz.",
                errorMessage);
        }

        [Fact]
        public void TryParse_WithCaseArgument_ShouldCreateCaseOptions()
        {
            var args = new[]
            {
        "--case",
        "samples/Case001"
    };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.True(success);
            Assert.Null(errorMessage);
            Assert.NotNull(options);
            Assert.True(options!.IsCaseMode);
            Assert.Equal(
                "samples/Case001",
                options.CaseDirectoryPath);
            Assert.Null(options.InputFilePath);
            Assert.Null(options.OutputFilePath);
        }

        [Fact]
        public void TryParse_WithCaseAndInputArguments_ShouldReturnError()
        {
            var args = new[]
            {
        "--case",
        "samples/Case001",
        "--input",
        "samples/Case002/input.pl1"
    };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.False(success);
            Assert.Null(options);
            Assert.Equal(
                "--case argümanı --input veya --output ile " +
                "birlikte kullanılamaz.",
                errorMessage);
        }

        [Fact]
        public void TryParse_WithCaseAndOutputArguments_ShouldReturnError()
        {
            var args = new[]
            {
        "--case",
        "samples/Case001",
        "--output",
        "samples/Case001/actual.egl"
    };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.False(success);
            Assert.Null(options);
            Assert.Equal(
                "--case argümanı --input veya --output ile " +
                "birlikte kullanılamaz.",
                errorMessage);
        }

        [Fact]
        public void TryParse_WithDuplicateCaseArgument_ShouldReturnError()
        {
            var args = new[]
            {
        "--case",
        "samples/Case001",
        "--case",
        "samples/Case002"
    };

            var parser = new CliArgumentParser();

            var success = parser.TryParse(
                args,
                out var options,
                out var errorMessage);

            Assert.False(success);
            Assert.Null(options);
            Assert.Equal(
                "--case argümanı birden fazla kez kullanılamaz.",
                errorMessage);
        }
    }
}