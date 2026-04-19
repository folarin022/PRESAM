using PRESAM.Application.DTOs;

namespace PRESAM.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);
        Task AddToCartAsync(string userId, int productId, int quantity);
        Task UpdateCartItemAsync(string userId, int cartItemId, int quantity);
        Task RemoveFromCartAsync(string userId, int cartItemId);
        Task ClearCartAsync(string userId);
    }
}