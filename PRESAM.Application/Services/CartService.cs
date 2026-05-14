using PRESAM.Application.DTOs;
using PRESAM.Application.Interfaces;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;

namespace PRESAM.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return new CartDto { UserId = userId, Items = new List<CartItemDto>() };
            }

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems?.Select(item => new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "Unknown Product",
                    ProductImage = item.Product?.ImageUrl ?? "/images/products/default.jpg",
                    UnitPrice = item.Product?.Price ?? 0,
                    Quantity = item.Quantity
                }).ToList() ?? new List<CartItemDto>()
            };
        }

        public async Task AddToCartAsync(string userId, Guid productId, int quantity)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CartItems = new List<CartItem>()
                };
                cart = await _cartRepository.AddAsync(cart);
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new InvalidOperationException("Product not found");

            var existingItem = cart.CartItems?.FirstOrDefault(i => i.ProductId == productId);
            int currentCartQty = existingItem?.Quantity ?? 0;
            int totalRequested = currentCartQty + quantity;

            if (product.StockQuantity < totalRequested)
            {
                throw new InvalidOperationException(
                    $"Only {product.StockQuantity - currentCartQty} units available. You already have {currentCartQty} in cart.");
            }


            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                await _cartRepository.UpdateCartItemAsync(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow
                };

                if (cart.CartItems == null)
                    cart.CartItems = new List<CartItem>();

                cart.CartItems.Add(cartItem);
                await _cartRepository.UpdateAsync(cart);
            }

        }

        public async Task UpdateCartItemAsync(string userId, Guid cartItemId, int quantity)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return;

            var cartItem = cart.CartItems?.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null) return;

            if (quantity <= 0)
            {
                await RemoveFromCartAsync(userId, cartItemId);
            }
            else
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product != null)
                {
                    int difference = quantity - cartItem.Quantity;

                    if (difference > 0 && product.StockQuantity < difference)
                        throw new InvalidOperationException($"Only {product.StockQuantity} additional units available.");

                }

                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
                await _cartRepository.UpdateCartItemAsync(cartItem);
            }
        }

        public async Task RemoveFromCartAsync(string userId, Guid cartItemId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return;

            var cartItem = cart.CartItems?.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem != null)
            {

                cart.CartItems.Remove(cartItem);
                await _cartRepository.UpdateAsync(cart);
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                await _cartRepository.ClearCartAsync(cart.Id);
            }
        }
    }
}