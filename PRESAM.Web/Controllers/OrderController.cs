// PRESAM.Web/Controllers/OrderController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Application.Services;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;

namespace PRESAM.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly UserManager<User> _userManager;
        private readonly PaymentCalculatorService _paymentCalculator;
        private readonly IOrderRepository _orderRepository;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            IProductService productService,
            UserManager<User> userManager,
            PaymentCalculatorService paymentCalculator,
            IOrderRepository orderRepository)  
        {
            _orderService = orderService;
            _cartService = cartService;
            _productService = productService;
            _userManager = userManager;
            _paymentCalculator = paymentCalculator;
            _orderRepository = orderRepository;  
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

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            if (product.StockQuantity < quantity)
            {
                TempData["Error"] = "Insufficient stock";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            TempData["BuyNowProductId"] = productId.ToString();
            TempData["BuyNowQuantity"] = quantity.ToString();

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

            var product = await _productService.GetProductByIdAsync(Guid.Parse(productIdStr));

            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            var quantity = int.Parse(quantityStr ?? "1");

            var subtotal = product.Price * quantity;
            var shippingFee = 2000m;
            var taxAmount = subtotal * 0.075m;
            var grandTotal = subtotal + shippingFee + taxAmount;
            var paymentOptions = _paymentCalculator.GetPaymentOptions(subtotal);

            var checkoutDto = new CheckoutDto
            {
                Items = new List<CartItemDto>
                {
                    new CartItemDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = quantity,
                        ProductImage = product.ImageUrl
                    }
                },
                SubTotal = subtotal,
                ShippingFee = shippingFee,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                PaymentOptions = paymentOptions
            };

            return View("~/Views/Cart/Checkout.cshtml", checkoutDto);
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
                checkoutDto.PaymentOptions = _paymentCalculator.GetPaymentOptions(subtotal);

                return View("~/Views/Cart/Checkout.cshtml", checkoutDto);
            }

            var productItem = checkoutDto.Items.FirstOrDefault();
            if (productItem == null)
            {
                return RedirectToAction("Index", "Product");
            }

            // Create order directly WITHOUT using cart
            var orderNumber = GenerateOrderNumber();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = checkoutDto.GrandTotal,
                Status = OrderStatus.Pending,
                ShippingAddress = checkoutDto.ShippingAddress,
                PaymentMethod = checkoutDto.PaymentMethod,
                PaymentPlan = GetPaymentPlanEnum(checkoutDto.SelectedPaymentPlan),
                OrderItems = new List<OrderItem>()
            };

            // Add the order item
            order.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = productItem.ProductId,
                ProductName = productItem.ProductName,
                UnitPrice = productItem.UnitPrice,
                Quantity = productItem.Quantity,
                TotalPrice = productItem.UnitPrice * productItem.Quantity
            });

            // Save directly to database using repository
            await _orderRepository.AddAsync(order);

            // Clear TempData
            TempData.Remove("BuyNowProductId");
            TempData.Remove("BuyNowQuantity");

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }


        private string GenerateOrderNumber()
        {
            return $"PRESAM-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private PaymentPlan GetPaymentPlanEnum(string planType)
        {
            return planType switch
            {
                "FullPayment" => PaymentPlan.FullPayment,
                "Weekly4" => PaymentPlan.Weekly4,
                "Weekly8" => PaymentPlan.Weekly8,
                "Weekly12" => PaymentPlan.Weekly12,
                "Monthly3" => PaymentPlan.Monthly3,
                "Monthly6" => PaymentPlan.Monthly6,
                "Monthly12" => PaymentPlan.Monthly12,
                _ => PaymentPlan.FullPayment
            };
        }
    }
}