using PRESAM.Application.DTOs;

namespace PRESAM.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto orderDto);
        Task<OrderDto> GetOrderAsync(int orderId);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
        Task UpdateOrderStatusAsync(int orderId, string status);
    }
}