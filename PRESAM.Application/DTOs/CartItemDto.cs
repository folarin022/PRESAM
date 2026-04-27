namespace PRESAM.Application.DTOs
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class CartDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);
    }
}