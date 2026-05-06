// PRESAM.Web/Controllers/ProductController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;

namespace PRESAM.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public async Task<IActionResult> ByCategory(Guid categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            ViewBag.CategoryName = category?.Name ?? "Products";
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(products);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction("Index");
            }
            var products = await _productService.SearchProductsAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                await _productService.CreateProductAsync(productDto);
                return RedirectToAction("Index");
            }
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(productDto);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                await _productService.UpdateProductAsync(productDto);
                return RedirectToAction("Index");
            }
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(productDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction("Index");
        }
    }
}