// PRESAM.Application/Services/AdminService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;

namespace PRESAM.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public AdminService(
            UserManager<User> userManager,
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _userManager = userManager;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardDataAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var products = await _productRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();
            var pendingOrders = orders.Where(o => o.Status == OrderStatus.Pending).ToList();

            var recentOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString()
                }).ToList();

            var recentUsers = users
                .OrderByDescending(u => u.RegistrationDate)
                .Take(5)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    RegistrationDate = u.RegistrationDate
                }).ToList();

            return new AdminDashboardDto
            {
                TotalUsers = users.Count,
                TotalProducts = products.Count(),
                TotalOrders = orders.Count(),
                PendingOrders = pendingOrders.Count(),
                TotalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                RecentOrders = recentOrders,
                RecentUsers = recentUsers
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var orders = await _orderRepository.GetOrdersByUserIdAsync(user.Id);
                result.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    RegistrationDate = user.RegistrationDate,
                    TotalOrders = orders.Count(),
                    TotalSpent = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount)
                });
            }

            return result;
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                RegistrationDate = user.RegistrationDate,
                TotalOrders = orders.Count(),
                TotalSpent = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount)
            };
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                ShippingAddress = o.ShippingAddress,
                PaymentMethod = o.PaymentMethod
            });
        }

        public async Task<OrderDto> GetOrderDetailsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                Items = order.OrderItems.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
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

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var products = await _productRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();
            var deliveredOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var monthlyRevenue = deliveredOrders
                .Where(o => o.OrderDate.Month == currentMonth && o.OrderDate.Year == currentYear)
                .Sum(o => o.TotalAmount);

            return new DashboardStatsDto
            {
                TotalUsers = users.Count,
                TotalProducts = products.Count(),
                TotalOrders = orders.Count(),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
                ShippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped),
                DeliveredOrders = deliveredOrders.Count,
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = deliveredOrders.Sum(o => o.TotalAmount),
                MonthlyRevenue = monthlyRevenue
            };
        }
    }
}