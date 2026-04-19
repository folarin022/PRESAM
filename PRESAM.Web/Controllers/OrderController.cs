// Web/Controllers/OrderController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;

namespace PRESAM.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;

        public OrderController(IOrderService orderService, ICartService cartService, UserManager<User> userManager)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
        }

        private async Task<string> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = await GetCurrentUserId();
            var cart = await _cartService.GetCartAsync(userId);

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
                return View(orderDto);

            var userId = await GetCurrentUserId();
            var order = await _orderService.CreateOrderAsync(userId, orderDto);

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = await GetCurrentUserId();
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _orderService.GetOrderAsync(id);
            return View(order);
        }
    }
}