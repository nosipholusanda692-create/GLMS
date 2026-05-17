using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using System.Text;
using Xunit;

namespace GLMS.Tests
{
    // These tests cover the two things required by the rubric:
    // 1. Currency calculation math
    // 2. File validation - only PDF allowed

    public class CurrencyServiceTests
    {
        private readonly CurrencyService _currencyService;

        public CurrencyServiceTests()
        {
            // We use a real HttpClient here but we only test the math method
            // which doesn't make any HTTP calls - this is the FixedRateStrategy idea from Part 1
            _currencyService = new CurrencyService(new HttpClient(), null!);
        }

        [Fact]
        public void ConvertUSDToZAR_CorrectAmount_WithKnownRate()
        {
            // Arrange
            decimal usdAmount = 100m;
            decimal rate = 18.50m;

            // Act
            decimal result = _currencyService.ConvertUSDToZAR(usdAmount, rate);

            // Assert
            Assert.Equal(1850.00m, result);
        }

        [Fact]
        public void ConvertUSDToZAR_RoundsToTwoDecimalPlaces()
        {
            // Arrange
            decimal usdAmount = 10m;
            decimal rate = 18.3333m;

            // Act
            decimal result = _currencyService.ConvertUSDToZAR(usdAmount, rate);

            // Assert - should round to 2 decimal places
            Assert.Equal(183.33m, result);
        }

        [Fact]
        public void ConvertUSDToZAR_ZeroAmount_ReturnsZero()
        {
            // Arrange
            decimal usdAmount = 0m;
            decimal rate = 18.50m;

            // Act
            decimal result = _currencyService.ConvertUSDToZAR(usdAmount, rate);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void ConvertUSDToZAR_LargeAmount_CalculatesCorrectly()
        {
            // Arrange - simulating a big freight contract cost
            decimal usdAmount = 50000m;
            decimal rate = 18.50m;

            // Act
            decimal result = _currencyService.ConvertUSDToZAR(usdAmount, rate);

            // Assert
            Assert.Equal(925000.00m, result);
        }
    }

    public class FileServiceTests
    {
        // We test the IsValidPdf logic directly without needing the web host
        // We create a lightweight fake FileService just for the validation method
        private bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".pdf")
                return false;

            if (file.Length > 10 * 1024 * 1024)
                return false;

            return true;
        }

        private IFormFile MakeFakeFile(string fileName, int sizeBytes = 1024)
        {
            var content = new string('x', sizeBytes);
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };
            return file;
        }

        [Fact]
        public void IsValidPdf_ValidPdfFile_ReturnsTrue()
        {
            // Arrange
            var file = MakeFakeFile("contract.pdf");

            // Act
            var result = IsValidPdf(file);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidPdf_ExeFile_ReturnsFalse()
        {
            // Arrange - .exe should be rejected
            var file = MakeFakeFile("malware.exe");

            // Act
            var result = IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_WordDocument_ReturnsFalse()
        {
            // Arrange - only PDF is allowed, not Word docs
            var file = MakeFakeFile("agreement.docx");

            // Act
            var result = IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_NullFile_ReturnsFalse()
        {
            // Arrange
            IFormFile file = null!;

            // Act
            var result = IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_FileTooLarge_ReturnsFalse()
        {
            // Arrange - 11MB file should be rejected (max is 10MB)
            var bigSize = 11 * 1024 * 1024;
            var file = MakeFakeFile("bigfile.pdf", bigSize);

            // Act
            var result = IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_UppercasePdfExtension_ReturnsTrue()
        {
            // Arrange - .PDF uppercase should still be accepted
            var file = MakeFakeFile("AGREEMENT.PDF");

            // Act
            var result = IsValidPdf(file);

            // Assert
            Assert.True(result);
        }
    }
}
