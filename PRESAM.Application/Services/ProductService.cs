using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;

namespace PRESAM.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => new ProductDto
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
            });
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                CreatedAt = product.CreatedAt,
                IsActive = product.IsActive
            };
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                ImageUrl = productDto.ImageUrl,
                CategoryId = productDto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _productRepository.AddAsync(product);

            return new ProductDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                Price = created.Price,
                StockQuantity = created.StockQuantity,
                ImageUrl = created.ImageUrl,
                CategoryId = created.CategoryId,
                CategoryName = created.Category?.Name,
                CreatedAt = created.CreatedAt,
                IsActive = created.IsActive
            };
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            var existing = await _productRepository.GetByIdAsync(productDto.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Product with id {productDto.Id} not found.");
            }

            existing.Name = productDto.Name;
            existing.Description = productDto.Description;
            existing.Price = productDto.Price;
            existing.StockQuantity = productDto.StockQuantity;
            existing.ImageUrl = productDto.ImageUrl;
            existing.CategoryId = productDto.CategoryId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(existing);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            await _productRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            return products.Select(p => new ProductDto
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
            });
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchProductsAsync(searchTerm);
            return products.Select(p => new ProductDto
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
            });
        }
    }
}