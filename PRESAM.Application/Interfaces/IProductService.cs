using PRESAM.Application.DTOs;
using PRESAM.Domain.Entities;

namespace PRESAM.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(Guid id);
        Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
    }
}