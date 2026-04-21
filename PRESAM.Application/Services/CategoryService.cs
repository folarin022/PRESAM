using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Application.Services
{
    
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCategoryAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCategoryAsync(CategoryDto categoryDto)
        {
            throw new NotImplementedException();
        }
    }
}
 