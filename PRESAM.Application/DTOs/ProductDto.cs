using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public Guid CategoryId { get; set; }
        public IFormFile? ProductImage { get; set; }
        public IFormFile? ProductImage2 { get; set; }
        public IFormFile? ProductImage3 { get; set; }
    }
}