using System;
using System.Collections.Generic;

namespace PRESAM.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        public Guid CategoryId { get; set; }
        public bool IsActive { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}