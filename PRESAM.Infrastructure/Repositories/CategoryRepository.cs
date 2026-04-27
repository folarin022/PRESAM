// PRESAM.Infrastructure/Repositories/CategoryRepository.cs
using Microsoft.EntityFrameworkCore;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;
using PRESAM.Infrastructure.Context;

namespace PRESAM.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(PresamDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Products)
                .ToListAsync(cancellationToken);
        }

        public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            if (category.Id == Guid.Empty)
            {
                category.Id = Guid.NewGuid();
            }

            await _dbSet.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return category;
        }

        public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(category);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await GetByIdAsync(id, cancellationToken);
            if (category != null)
            {
                _dbSet.Remove(category);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .Include(c => c.Products)
                .ToListAsync();
        }
    }
}