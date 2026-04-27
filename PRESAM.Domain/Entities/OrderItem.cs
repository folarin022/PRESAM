using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
