using PRESAM.Application.DTOs;

namespace PRESAM.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto orderDto);
        Task<OrderDto> GetOrderAsync(Guid orderId);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
        Task<BuyNowResultDto> PrepareBuyNowAsync(string userId, Guid productId, int quantity);
        Task<CheckoutDto> GetBuyNowCheckoutAsync(string userId, Guid productId, int quantity);
        Task<OrderDto> ProcessBuyNowAsync(string userId, CheckoutDto checkoutDto);
    }
}