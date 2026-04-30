using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Application.Services;
using PRESAM.Domain.Entities;

namespace PRESAM.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly PaymentCalculatorService _paymentCalculator;

        public CartController(
    ICartService cartService,
    IProductService productService,
    IOrderService orderService,
    UserManager<User> userManager,
    PaymentCalculatorService paymentCalculator)
        {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _userManager = userManager;
            _paymentCalculator = paymentCalculator;
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
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _cartService.GetCartAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _cartService.AddToCartAsync(userId, productId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(Guid cartItemId, int quantity)
        {
            var userId = await GetCurrentUserId();
            if (userId != null)
            {
                await _cartService.UpdateCartItemAsync(userId, cartItemId, quantity);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            var userId = await GetCurrentUserId();
            if (userId != null)
            {
                await _cartService.RemoveFromCartAsync(userId, cartItemId);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = await GetCurrentUserId();
            if (userId != null)
            {
                await _cartService.ClearCartAsync(userId);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetCartCount()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return Json(0);
            }

            var cart = await _cartService.GetCartAsync(userId);
            return Json(cart?.TotalItems ?? 0);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _cartService.GetCartAsync(userId);
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var subtotal = cart.Items.Sum(i => i.TotalPrice);
            var shippingFee = 2000m;
            var taxAmount = subtotal * 0.075m;
            var grandTotal = subtotal + shippingFee + taxAmount;
            var paymentOptions = _paymentCalculator.GetPaymentOptions(subtotal);

            var checkoutDto = new CheckoutDto
            {
                Items = cart.Items.ToList(),
                SubTotal = subtotal,
                ShippingFee = shippingFee,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                PaymentOptions = paymentOptions
            };

            return View(checkoutDto);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutDto checkoutDto)
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(checkoutDto.ShippingAddress))
            {
                ModelState.AddModelError("", "Shipping address is required");
                return View(checkoutDto);
            }

            var order = await _orderService.CreateOrderAsync(userId, new CreateOrderDto
            {
                ShippingAddress = checkoutDto.ShippingAddress,
                PaymentMethod = checkoutDto.PaymentMethod
            });

            return RedirectToAction("OrderConfirmation", "Order", new { id = order.Id });
        }
    }
}