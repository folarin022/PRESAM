using PRESAM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Domain.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
        Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    }
}
