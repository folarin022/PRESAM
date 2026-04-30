using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;

namespace PRESAM.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IOrderService _orderService;

        public AdminController(IAdminService adminService, IOrderService orderService)
        {
            _adminService = adminService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _adminService.GetDashboardDataAsync();
            return View(dashboard);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _adminService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(Guid id)
        {
            var order = await _adminService.GetOrderDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderRequest request)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(Guid.Parse(request.OrderId));
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                order.Status = request.Status switch
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

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Statistics()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return View(stats);
        }

        public class UpdateOrderRequest
        {
            public string OrderId { get; set; }
            public string Status { get; set; }
        }
    }
}