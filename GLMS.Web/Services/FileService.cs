namespace GLMS.Web.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file);
        bool IsValidPdf(IFormFile file);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        // This logic is what we unit test - only .pdf allowed
        public bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".pdf")
                return false;

            // Also check file size - max 10MB
            if (file.Length > 10 * 1024 * 1024)
                return false;

            return true;
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            // Save to wwwroot/uploads folder
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Give it a unique name so files don't overwrite each other
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File saved: {FileName}", uniqueFileName);

            return uniqueFileName;
        }
    }
}
