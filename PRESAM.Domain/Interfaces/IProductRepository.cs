using PRESAM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Domain.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<Product> GetByIdWithRelationsAsync(Guid id);
        Task<bool> UpdateStockAsync(Guid productId, int newStockQuantity);
        Task<bool> HasOrderItemsAsync(Guid productId);
        Task<bool> HasCartItemsAsync(Guid productId);
    }
}
