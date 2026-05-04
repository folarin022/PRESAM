using Microsoft.AspNetCore.Http;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;
using System.IO;

namespace PRESAM.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly string _imageUploadPath;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return ToDtoList(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
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

            // Handle new image upload
            if (productDto.ProductImage != null && productDto.ProductImage.Length > 0)
            {
                var newImageUrl = await SaveImageFile(productDto.ProductImage);
                // Delete old image if it exists and is not the placeholder
                if (!string.IsNullOrEmpty(existing.ImageUrl) && existing.ImageUrl != "/images/placeholder.jpg")
                {
                    DeleteImageFile(existing.ImageUrl);
                }
                existing.ImageUrl = newImageUrl;
            }

            await _productRepository.UpdateAsync(existing);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return;

            // 1. Delete image file (ignore errors – file might already be missing)
            try
            {
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/placeholder.jpg")
                {
                    DeleteImageFile(product.ImageUrl);
                }
            }
            catch (Exception ex)
            {
                // Log but continue – do not block product deletion
                Console.WriteLine($"Image deletion failed: {ex.Message}");
            }

            // 2. Delete product from database
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

        // ---------- Helper methods ----------
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
                Console.WriteLine($"Error saving image: {ex.Message}");
                return "/images/placeholder.jpg";
            }
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_imageUploadPath, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);
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