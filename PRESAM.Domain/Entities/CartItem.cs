using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public virtual Cart Cart { get; set; }
        public virtual Product Product { get; set; }
    }
}
