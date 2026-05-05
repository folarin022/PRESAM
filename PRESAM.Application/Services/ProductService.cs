using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using PRESAM.Infrastructure.Context;

namespace PRESAM.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly PresamDbContext _dbContext;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProductService> _logger;
        private readonly string _imageUploadPath;

        public ProductService(IProductRepository productRepository, PresamDbContext dbContext, IWebHostEnvironment env, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _dbContext = dbContext;
            _env = env;
            _logger = logger;
            _imageUploadPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "products");
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            // Prefer repository method that returns only active products if available
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

            if (productDto.ProductImage != null && productDto.ProductImage.Length > 0)
            {
                imageUrl = await SaveImageFile(productDto.ProductImage);
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                ImageUrl = imageUrl,
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
            existing.UpdatedAt = DateTime.UtcNow;

            if (productDto.ProductImage != null && productDto.ProductImage.Length > 0)
            {
                var newImageUrl = await SaveImageFile(productDto.ProductImage);
                try
                {
                    if (!string.IsNullOrEmpty(existing.ImageUrl) && existing.ImageUrl != "/images/placeholder.jpg")
                    {
                        DeleteImageFile(existing.ImageUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old image for product {ProductId}", existing.Id);
                }

                existing.ImageUrl = newImageUrl;
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

            try
            {
                hasOrderItems = await _productRepository.HasOrderItemsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "HasOrderItemsAsync failed for product {ProductId}", id);
            }

            try
            {
                hasCartItems = await _productRepository.HasCartItemsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "HasCartItemsAsync failed for product {ProductId}", id);
            }

            // If product is referenced by orders or carts, soft-delete (deactivate) it
            if (hasOrderItems || hasCartItems)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);
                return;
            }

            // Delete image first (best-effort)
            try
            {
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/placeholder.jpg")
                {
                    DeleteImageFile(product.ImageUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image file for product {ProductId}", id);
            }

            // Finally remove from repository
            try
            {
                await _productRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete product {ProductId} from repository", id);
                throw; // rethrow so controller can return proper error
            }
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

        private async Task<string> SaveImageFile(IFormFile imageFile)
        {
            try
            {
                if (!Directory.Exists(_imageUploadPath))
                    Directory.CreateDirectory(_imageUploadPath);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(_imageUploadPath, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return "/images/products/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image file");
                return "/images/placeholder.jpg";
            }
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_imageUploadPath, fileName);

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete image file at {FilePath}", filePath);
            }
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
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                CreatedAt = p.CreatedAt,
                IsActive = p.IsActive
            };
        }

        public async Task<Result> DeleteAsync(Guid Id)
        {
            var product = await _dbContext.Products.FindAsync(Id);

            if (product == null)
                return Result.Failure("Product not found");

            var inCart = await _dbContext.CartItems
                .Where(ci => ci.ProductId == Id)
                .CountAsync() > 0;


            if (inCart)
                return Result.Failure("Cannot delete product. It exists in active shopping carts.");

            var hasOrders = await _dbContext.OrderItems
                .AnyAsync(oi => oi.ProductId == Id);

            if (hasOrders)
                return Result.Failure("Cannot delete product. It has order history.");

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return Result.Success("Product deleted successfully");
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