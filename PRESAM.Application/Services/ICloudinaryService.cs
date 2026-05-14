using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            _logger.LogInformation("🔧 Initializing Cloudinary with CloudName: {CloudName}", cloudname);

            if (string.IsNullOrEmpty(cloudname) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apisecret))
            {
                _logger.LogError("❌ Cloudinary configuration is missing! Check your appsettings.json");
                throw new ArgumentException("Cloudinary configuration is not properly set");
            }

            var account = new Account(cloudname, apiKey, apisecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; 
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("⚠️ UploadImageAsync called with null or empty file");
                return null;
            }

            try
            {
                _logger.LogInformation("☁️ Starting Cloudinary upload - File: {FileName}, Size: {Size} bytes, ContentType: {ContentType}",
                    file.FileName, file.Length, file.ContentType);

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogError("❌ Invalid file extension: {Extension}. Allowed: {Allowed}",
                        fileExtension, string.Join(", ", allowedExtensions));
                    return null;
                }

                if (file.Length > 10 * 1024 * 1024)
                {
                    _logger.LogError("❌ File too large: {Size} bytes. Max: 10MB", file.Length);
                    return null;
                }

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "products",
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                    Overwrite = false,
                    UseFilename = true,
                    UniqueFilename = true
                };

                _logger.LogInformation("📤 Sending upload request to Cloudinary...");
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("❌ Cloudinary upload failed - Status: {Status}, Error: {Error}",
                        uploadResult.StatusCode, uploadResult.Error.Message);
                    return null;
                }

                if (string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                {
                    _logger.LogError("❌ Cloudinary returned empty SecureUrl - Status: {Status}",
                        uploadResult.StatusCode);
                    return null;
                }

                var secureUrl = uploadResult.SecureUrl.ToString();
                _logger.LogInformation("✅ Upload successful - PublicId: {PublicId}, URL: {Url}, Size: {Size}, Format: {Format}",
                    uploadResult.PublicId, secureUrl, uploadResult.Bytes, uploadResult.Format);

                return secureUrl;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Network error during Cloudinary upload - File: {FileName}", file.FileName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error during Cloudinary upload - File: {FileName}, Type: {ExceptionType}",
                    file.FileName, ex.GetType().Name);
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogWarning("⚠️ DeleteImageAsync called with null/empty URL");
                return false;
            }

            if (imageUrl.Contains("placeholder") || imageUrl.StartsWith("/images/"))
            {
                _logger.LogInformation("ℹ️ Skipping deletion of placeholder/local image: {Url}", imageUrl);
                return false;
            }

            try
            {
                _logger.LogInformation("🗑️ Attempting to delete image: {Url}", imageUrl);

                var publicId = ExtractPublicIdFromUrl(imageUrl);

                if (string.IsNullOrEmpty(publicId))
                {
                    _logger.LogWarning("⚠️ Could not extract public_id from URL: {Url}", imageUrl);
                    return false;
                }

                _logger.LogInformation("🔍 Extracted PublicId: {PublicId}", publicId);

                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Error != null)
                {
                    _logger.LogError("❌ Delete failed - Status: {Status}, Error: {Error}",
                        result.StatusCode, result.Error.Message);
                    return false;
                }

                _logger.LogInformation("✅ Successfully deleted image - PublicId: {PublicId}, Result: {Result}",
                    publicId, result.Result);
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception during image deletion - URL: {Url}", imageUrl);
                return false;
            }
        }

        private string ExtractPublicIdFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;

                var uri = new Uri(url);

                var segments = uri.AbsolutePath.Split('/');

                var uploadIndex = -1;
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i].Equals("upload", StringComparison.OrdinalIgnoreCase))
                    {
                        uploadIndex = i;
                        break;
                    }
                }

                if (uploadIndex >= 0 && uploadIndex < segments.Length - 1)
                {
                    var pathAfterUpload = string.Join("/", segments.Skip(uploadIndex + 1));

                    var versionMatch = System.Text.RegularExpressions.Regex.Match(pathAfterUpload, @"^v\d+/");
                    if (versionMatch.Success)
                    {
                        pathAfterUpload = pathAfterUpload.Substring(versionMatch.Length);
                    }

                    var lastDotIndex = pathAfterUpload.LastIndexOf('.');
                    if (lastDotIndex > 0)
                    {
                        pathAfterUpload = pathAfterUpload.Substring(0, lastDotIndex);
                    }

                    _logger.LogInformation("📝 Extracted public_id: {PublicId} from URL: {Url}", pathAfterUpload, url);
                    return pathAfterUpload;
                }

                _logger.LogWarning("⚠️ Could not find 'upload' segment in URL: {Url}", url);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error extracting public_id from URL: {Url}", url);
                return null;
            }
        }
    }
}