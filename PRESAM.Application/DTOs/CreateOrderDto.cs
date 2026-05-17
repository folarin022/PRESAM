namespace PRESAM.Application.DTOs
{
    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Guid ProductId { get;  set; }
    }

    public class BuyNowResultDto
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}