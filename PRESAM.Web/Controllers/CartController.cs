using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;

namespace PRESAM.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;

        public CartController(ICartService cartService, UserManager<User> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        private async Task<string> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var cart = await _cartService.GetCartAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            await _cartService.AddToCartAsync(userId, productId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
        {
            var userId = await GetCurrentUserId();
            if (userId != null)
                await _cartService.UpdateCartItemAsync(userId, cartItemId, quantity);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = await GetCurrentUserId();
            if (userId != null)
                await _cartService.RemoveFromCartAsync(userId, cartItemId);

            return RedirectToAction("Index");
        }
    }
}