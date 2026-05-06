using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;

namespace PRESAM.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products?.Count ?? 0,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ProductCount = category.Products?.Count ?? 0,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ImageUrl = "/images/categories/default.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _categoryRepository.AddAsync(category);

            return new CategoryDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                IsActive = created.IsActive,
                ProductCount = 0,
                CreatedAt = created.CreatedAt
            };
        }

        public async Task UpdateCategoryAsync(CategoryDto categoryDto)
        {
            var existing = await _categoryRepository.GetByIdAsync(categoryDto.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Category with id {categoryDto.Id} not found.");
            }

            // Manual mapping of updated values
            existing.Name = categoryDto.Name;
            existing.Description = categoryDto.Description;
            existing.IsActive = categoryDto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(existing);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
        }
    }
}