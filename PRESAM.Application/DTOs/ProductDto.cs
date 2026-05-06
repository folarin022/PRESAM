using Microsoft.AspNetCore.Http;
using System;

namespace PRESAM.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        public IFormFile? ProductImage { get; set; }
        public IFormFile? ProductImage2 { get; set; }
        public IFormFile? ProductImage3 { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
        public IFormFile? ProductImage { get; set; }
        public IFormFile? ProductImage2 { get; set; }
        public IFormFile? ProductImage3 { get; set; }
    }
}