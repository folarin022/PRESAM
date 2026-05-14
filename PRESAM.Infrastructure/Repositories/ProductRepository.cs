using Microsoft.EntityFrameworkCore;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;
using PRESAM.Infrastructure.Context;

namespace PRESAM.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(PresamDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product> GetByIdWithRelationsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.CartItems)
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<bool> UpdateStockAsync(Guid productId, int newStockQuantity)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return false;
            product.StockQuantity = newStockQuantity;
            await UpdateAsync(product);
            return true;
        }

        public async Task<bool> HasOrderItemsAsync(Guid productId)
        { return await _context.OrderItems.AnyAsync(oi => oi.ProductId == productId); }

        public async Task<bool> HasCartItemsAsync(Guid productId)
        { return await _context.CartItems.AnyAsync(ci => ci.ProductId == productId); }
    }
}