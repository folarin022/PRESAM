// PRESAM.Application/Interfaces/IAdminService.cs
using PRESAM.Application.DTOs;

namespace PRESAM.Application.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardDataAsync();
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string userId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> GetOrderDetailsAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }

    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }
}