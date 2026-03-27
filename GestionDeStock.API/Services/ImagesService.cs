namespace GestionDeStock.API.Services
{
    public interface IImageService
    {
        Task<string?> SaveImageAsync(IFormFile? image);
        void DeleteImage(string? relativePath);
        string BuildPublicUrl(HttpRequest request, string? relativePath);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string[] _allowed = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        public ImageService(IWebHostEnvironment env) => _env = env;

        public async Task<string?> SaveImageAsync(IFormFile? image)
        {
            if (image == null || image.Length == 0) return null;

            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!_allowed.Contains(ext))
                throw new InvalidOperationException($"Format non supporté : {ext}. Utilisez jpg, png ou webp.");
            if (image.Length > MaxBytes)
                throw new InvalidOperationException("Image trop lourde. Maximum 5 MB.");

            // Dossier : wwwroot/uploads/products/
            var folder = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(folder); // crée si absent

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await image.CopyToAsync(stream);

            return $"/uploads/products/{fileName}"; // chemin relatif stocké en DB
        }

        public void DeleteImage(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            var full = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(full)) File.Delete(full);
        }

        // Construit l'URL absolue pour le site PHP : https://votre-api.com/uploads/products/xyz.jpg
        public string BuildPublicUrl(HttpRequest request, string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return string.Empty;
            return $"{request.Scheme}://{request.Host}{relativePath}";
        }
    }
}