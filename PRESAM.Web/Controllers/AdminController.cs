using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;

namespace PRESAM.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<User> _userManager;

        public AdminController(
            IAdminService adminService,
            ICategoryService categoryService,
            IProductService productService,
            IOrderRepository orderRepository,
            UserManager<User> userManager)
        {
            _adminService = adminService;
            _categoryService = categoryService;
            _productService = productService;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _adminService.GetDashboardDataAsync();
            return View(dashboard);
        }

        // Statistics
        public async Task<IActionResult> Statistics()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return View(stats);
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID is required");

            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                if (user.Email == "admin@presam.com")
                    return Json(new { success = false, message = "Cannot delete the main admin user" });

                await _userManager.DeleteAsync(user);
                return Json(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Order Management
        public async Task<IActionResult> Orders()
        {
            var orders = await _adminService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid order ID");

            var order = await _adminService.GetOrderDetailsAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.OrderId))
                return Json(new { success = false, message = "Invalid request" });

            try
            {
                var order = await _orderRepository.GetByIdAsync(Guid.Parse(request.OrderId));
                if (order == null)
                    return Json(new { success = false, message = "Order not found" });

                var oldStatus = order.Status.ToString();
                order.Status = request.Status switch
                {
                    "Pending" => OrderStatus.Pending,
                    "Processing" => OrderStatus.Processing,
                    "Shipped" => OrderStatus.Shipped,
                    "Delivered" => OrderStatus.Delivered,
                    "Cancelled" => OrderStatus.Cancelled,
                    _ => order.Status
                };

                if (oldStatus == order.Status.ToString())
                    return Json(new { success = false, message = "Status is already " + oldStatus });

                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                return Json(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatusForm(Guid orderId, string status)
        {
            if (orderId == Guid.Empty || string.IsNullOrEmpty(status))
            {
                TempData["Error"] = "Invalid order or status";
                return RedirectToAction("Orders");
            }

            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Order not found";
                    return RedirectToAction("Orders");
                }

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
                TempData["Success"] = "Order status updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Orders");
        }

        // Category Management
        [HttpGet]
        public IActionResult CreateCategory() => View();

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _categoryService.CreateCategoryAsync(model);
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid category ID");

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(CategoryDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _categoryService.UpdateCategoryAsync(model);
                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return Json(new { success = true, message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Product Management
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto model)
        {
            // FIX: Show exactly which fields are failing ModelState validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["Error"] = "Validation failed: " + string.Join(" | ", errors);
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(model);
            }

            try
            {
                await _productService.CreateProductAsync(model);
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                TempData["Error"] = ex.Message;
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid product ID");

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct([FromForm] ProductDto model)
        {
            // FIX: Show exactly which fields are failing ModelState validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["Error"] = "Validation failed: " + string.Join(" | ", errors);
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(model);
            }

            try
            {
                await _productService.UpdateProductAsync(model);
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                TempData["Error"] = ex.Message;
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductDto dto)
        {
            await _productService.DeleteProductAsync(dto.Id);
            return Json(new { success = true, message = "Product deleted successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Json(categories);
        }
    }

    public class DeleteProductDto
    {
        public Guid Id { get; set; }
    }

    public class UpdateOrderRequest
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
    }
}