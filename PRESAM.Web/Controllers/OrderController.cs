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

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            UserManager<User> userManager)
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

            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return View(orderDto);
            }

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

        public async Task<IActionResult> OrderConfirmation(Guid id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> BuyNow(Guid productId, int quantity = 1)
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _orderService.PrepareBuyNowAsync(userId, productId, quantity);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            TempData["BuyNowProductId"] = result.ProductId.ToString();
            TempData["BuyNowQuantity"] = result.Quantity.ToString();

            return RedirectToAction("BuyNowCheckout");
        }

        [HttpGet]
        public async Task<IActionResult> BuyNowCheckout()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var productIdStr = TempData["BuyNowProductId"]?.ToString();
            var quantityStr = TempData["BuyNowQuantity"]?.ToString();

            if (string.IsNullOrEmpty(productIdStr))
            {
                return RedirectToAction("Index", "Product");
            }

            var checkoutData = await _orderService.GetBuyNowCheckoutAsync(
                userId,
                Guid.Parse(productIdStr),
                int.Parse(quantityStr ?? "1"));

            if (checkoutData == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            return View("~/Views/Cart/Checkout.cshtml", checkoutData);
        }
        
        [HttpPost]
        public async Task<IActionResult> ProcessBuyNow(CheckoutDto checkoutDto)
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(checkoutDto.ShippingAddress))
            {
                ModelState.AddModelError("", "Shipping address is required");

                var subtotal = checkoutDto.SubTotal;
                checkoutDto.GrandTotal = subtotal + checkoutDto.ShippingFee + checkoutDto.TaxAmount;

                return View("~/Views/Cart/Checkout.cshtml", checkoutDto);
            }

            var order = await _orderService.ProcessBuyNowAsync(userId, checkoutDto);

            if (order == null)
            {
                TempData["Error"] = "Failed to process order";
                return RedirectToAction("Index", "Product");
            }

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }
    }
}