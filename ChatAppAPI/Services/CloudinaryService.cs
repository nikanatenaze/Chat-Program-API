using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ChatAppAPI.Services
{
    public class CloudinaryService 
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config) {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]);

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file.Length == 0) return null;

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "profile-images"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            return result.SecureUrl.ToString();
        }
    }
}
