using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;
using PRESAM.Infrastructure.Context;

namespace PRESAM.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly PresamDbContext _context;
        public CategoryRepository(PresamDbContext context)
        {
            _context = context;
        }
        public Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
