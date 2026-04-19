using AutoMapper;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;

namespace PRESAM.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            // set UTC timestamp here; mapping ignores CreatedAt so set explicitly
            product.CreatedAt = DateTime.UtcNow;

            var created = await _productRepository.AddAsync(product);
            return _mapper.Map<ProductDto>(created);
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            // Load existing entity to avoid overwriting fields not present in the DTO
            var existing = await _productRepository.GetByIdAsync(productDto.Id);
            if (existing == null)
            {
                // Optionally throw a domain-specific exception or return
                throw new KeyNotFoundException($"Product with id {productDto.Id} not found.");
            }

            // Map incoming DTO onto existing entity to preserve CreatedAt, navigation props, etc.
            _mapper.Map(productDto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(existing);
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchProductsAsync(searchTerm);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}