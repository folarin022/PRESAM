// PRESAM.Application/Services/OrderService.cs
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;

namespace PRESAM.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly PaymentCalculatorService _paymentCalculator;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            PaymentCalculatorService paymentCalculator)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _paymentCalculator = paymentCalculator;
        }

        private string GenerateOrderNumber()
        {
            return $"PRESAM-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private PaymentPlan GetPaymentPlanEnum(string planType)
        {
            return planType switch
            {
                "FullPayment" => PaymentPlan.FullPayment,
                "Weekly4" => PaymentPlan.Weekly4,
                "Weekly8" => PaymentPlan.Weekly8,
                "Weekly12" => PaymentPlan.Weekly12,
                "Monthly3" => PaymentPlan.Monthly3,
                "Monthly6" => PaymentPlan.Monthly6,
                "Monthly12" => PaymentPlan.Monthly12,
                _ => PaymentPlan.FullPayment
            };
        }

        private int GetInstallmentCount(string planType)
        {
            return planType switch
            {
                "Weekly4" => 4,
                "Weekly8" => 8,
                "Weekly12" => 12,
                "Monthly3" => 3,
                "Monthly6" => 6,
                "Monthly12" => 12,
                _ => 1
            };
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                PaymentReference = order.PaymentReference,
                Items = order.OrderItems?.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList() ?? new List<OrderItemDto>()
            };
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto orderDto)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty. Cannot create order.");
            }

            var totalAmount = cart.CartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0));
            var orderNumber = GenerateOrderNumber();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                ShippingAddress = orderDto.ShippingAddress,
                PaymentMethod = orderDto.PaymentMethod,
                PaymentReference = orderDto.PaymentReference,
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cart.CartItems)
            {
                var product = cartItem.Product;
                if (product == null) continue;

                if (product.StockQuantity < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");
                }

                product.StockQuantity -= cartItem.Quantity;
                await _productRepository.UpdateAsync(product);

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    TotalPrice = product.Price * cartItem.Quantity
                };
                order.OrderItems.Add(orderItem);
            }

            var createdOrder = await _orderRepository.AddAsync(order);
            await _cartRepository.ClearCartAsync(cart.Id);

            return MapToOrderDto(createdOrder);
        }

        public async Task<OrderDto> GetOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null) return null;

            return MapToOrderDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return orders.Select(MapToOrderDto);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with id {orderId} not found.");
            }

            order.Status = status switch
            {
                "Pending" => OrderStatus.Pending,
                "Processing" => OrderStatus.Processing,
                "Shipped" => OrderStatus.Shipped,
                "Delivered" => OrderStatus.Delivered,
                "Cancelled" => OrderStatus.Cancelled,
                _ => order.Status
            };

            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
        }
    }
}