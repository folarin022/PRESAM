using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;
using PRESAM.Infrastructure.Context;

namespace PRESAM.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly PresamDbContext _dbContext;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            PresamDbContext dbContext,
            ICloudinaryService cloudinaryService,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _dbContext = dbContext;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return ToDtoList(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdWithRelationsAsync(id) ?? await _productRepository.GetByIdAsync(id);
            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            string imageUrl = "/images/placeholder.jpg";
            string? imageUrl2 = null;
            string? imageUrl3 = null;

            if (productDto.ProductImage != null && productDto.ProductImage.Length > 0)
                imageUrl = await _cloudinaryService.UploadImageAsync(productDto.ProductImage) ?? "/images/placeholder.jpg";

            if (productDto.ProductImage2 != null && productDto.ProductImage2.Length > 0)
                imageUrl2 = await _cloudinaryService.UploadImageAsync(productDto.ProductImage2);

            if (productDto.ProductImage3 != null && productDto.ProductImage3.Length > 0)
                imageUrl3 = await _cloudinaryService.UploadImageAsync(productDto.ProductImage3);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                ImageUrl = imageUrl,
                ImageUrl2 = imageUrl2,
                ImageUrl3 = imageUrl3,
                CategoryId = productDto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _productRepository.AddAsync(product);
            return MapToDto(created);
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            var existing = await _productRepository.GetByIdAsync(productDto.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Product with id {productDto.Id} not found.");

            existing.Name = productDto.Name;
            existing.Description = productDto.Description;
            existing.Price = productDto.Price;
            existing.StockQuantity = productDto.StockQuantity;
            existing.CategoryId = productDto.CategoryId;
            existing.IsActive = productDto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            // Update Image 1 - use ProductImage (IFormFile)
            if (productDto.ProductImage != null && productDto.ProductImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(existing.ImageUrl) && existing.ImageUrl != "/images/placeholder.jpg")
                    _ = _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                var newUrl = await _cloudinaryService.UploadImageAsync(productDto.ProductImage);
                if (!string.IsNullOrEmpty(newUrl))
                    existing.ImageUrl = newUrl;
            }

            // Update Image 2 - use ProductImage2 (IFormFile)
            if (productDto.ProductImage2 != null && productDto.ProductImage2.Length > 0)
            {
                if (!string.IsNullOrEmpty(existing.ImageUrl2))
                    _ = _cloudinaryService.DeleteImageAsync(existing.ImageUrl2);
                var newUrl = await _cloudinaryService.UploadImageAsync(productDto.ProductImage2);
                if (!string.IsNullOrEmpty(newUrl))
                    existing.ImageUrl2 = newUrl;
            }

            // Update Image 3 - use ProductImage3 (IFormFile)
            if (productDto.ProductImage3 != null && productDto.ProductImage3.Length > 0)
            {
                if (!string.IsNullOrEmpty(existing.ImageUrl3))
                    _ = _cloudinaryService.DeleteImageAsync(existing.ImageUrl3);
                var newUrl = await _cloudinaryService.UploadImageAsync(productDto.ProductImage3);
                if (!string.IsNullOrEmpty(newUrl))
                    existing.ImageUrl3 = newUrl;
            }

            await _productRepository.UpdateAsync(existing);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdWithRelationsAsync(id) ?? await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with id {id} not found.");

            var hasOrderItems = false;
            var hasCartItems = false;

            try { hasOrderItems = await _productRepository.HasOrderItemsAsync(id); }
            catch (Exception ex) { _logger.LogWarning(ex, "HasOrderItemsAsync failed for product {ProductId}", id); }

            try { hasCartItems = await _productRepository.HasCartItemsAsync(id); }
            catch (Exception ex) { _logger.LogWarning(ex, "HasCartItemsAsync failed for product {ProductId}", id); }

            if (hasOrderItems || hasCartItems)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);
                return;
            }

            // Delete images from Cloudinary
            try
            {
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/placeholder.jpg" && !product.ImageUrl.Contains("placeholder"))
                    _ = _cloudinaryService.DeleteImageAsync(product.ImageUrl);
                if (!string.IsNullOrEmpty(product.ImageUrl2))
                    _ = _cloudinaryService.DeleteImageAsync(product.ImageUrl2);
                if (!string.IsNullOrEmpty(product.ImageUrl3))
                    _ = _cloudinaryService.DeleteImageAsync(product.ImageUrl3);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete images for product {ProductId}", id);
            }

            await _productRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            return ToDtoList(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchProductsAsync(searchTerm);
            return ToDtoList(products);
        }

        public async Task<Result> DeleteAsync(Guid Id)
        {
            var product = await _dbContext.Products.FindAsync(Id);
            if (product == null)
                return Result.Failure("Product not found");

            var inCart = await _dbContext.CartItems.Where(ci => ci.ProductId == Id).CountAsync() > 0;
            if (inCart)
                return Result.Failure("Cannot delete product. It exists in active shopping carts.");

            var hasOrders = await _dbContext.OrderItems.AnyAsync(oi => oi.ProductId == Id);
            if (hasOrders)
                return Result.Failure("Cannot delete product. It has order history.");

            // Delete images from Cloudinary
            try
            {
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/placeholder.jpg" && !product.ImageUrl.Contains("placeholder"))
                    _ = _cloudinaryService.DeleteImageAsync(product.ImageUrl);
                if (!string.IsNullOrEmpty(product.ImageUrl2))
                    _ = _cloudinaryService.DeleteImageAsync(product.ImageUrl2);
                if (!string.IsNullOrEmpty(product.ImageUrl3))
                    _ = _cloudinaryService.DeleteImageAsync(product.ImageUrl3);
            }
            catch { }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return Result.Success("Product deleted successfully");
        }

        private static ProductDto MapToDto(Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                ImageUrl2 = p.ImageUrl2,
                ImageUrl3 = p.ImageUrl3,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                CreatedAt = p.CreatedAt,
                IsActive = p.IsActive
            };
        }

        private static List<ProductDto> ToDtoList(IEnumerable<Product> products)
        {
            if (products == null) return new List<ProductDto>();
            var result = new List<ProductDto>();
            foreach (var p in products)
                result.Add(MapToDto(p));
            return result;
        }
    }
}