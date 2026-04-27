using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; internal set; }
        public bool IsActive { get; internal set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public Guid CategoryId { get; set; }
    }
}
