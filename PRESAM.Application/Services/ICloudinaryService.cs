using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Application.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string imageUrl);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            var cloudname = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apisecret = configuration["Cloudinary:ApiSecret"];

            var account = new Account(cloudname, apiKey, apisecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Invalid file");

                if (!file.ContentType.StartsWith("image/"))
                    throw new Exception("Only image files allowed");

                if (file.Length > 2 * 1024 * 1024)
                    throw new Exception("File too large");

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "presam-products",
                    Transformation = new Transformation()
                        .Width(800)
                        .Height(800)
                        .Crop("limit")
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return uploadResult.SecureUrl.ToString(); 
                }

                _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                return null;
            }
        }
        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return true;

                https://res.cloudinary.com/CLOUD_NAME/image/upload/v1234567890/presam-products/filename.jpg
                var uri = new Uri(imageUrl);
                var pathParts = uri.AbsolutePath.Split('/');

                var uploadIndex = Array.IndexOf(pathParts, "upload");
                if (uploadIndex == -1) return false;

                var publicIdWithExtension = string.Join("/", pathParts.Skip(uploadIndex + 2));
                var publicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);
                var folder = Path.GetDirectoryName(publicIdWithExtension)?.Replace("\\", "/");
                var fullPublicId = string.IsNullOrEmpty(folder) ? publicId : $"{folder}/{publicId}";

                var deletionParams = new DeletionParams(fullPublicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting image from Cloudinary: {Url}", imageUrl);
                return false;
            }
        }
    }
}
