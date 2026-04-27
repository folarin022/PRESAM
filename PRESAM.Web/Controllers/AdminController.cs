// PRESAM.Web/Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.Interfaces;

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
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, string status)
        {
            await _adminService.UpdateOrderStatusAsync(orderId, status);
            return RedirectToAction("OrderDetails", new { id = orderId });
        }

        public async Task<IActionResult> Statistics()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return View(stats);
        }
    }
}