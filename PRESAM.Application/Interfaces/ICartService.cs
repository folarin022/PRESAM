using PRESAM.Application.DTOs;

namespace PRESAM.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);
        Task AddToCartAsync(string userId, Guid productId, int quantity);
        Task UpdateCartItemAsync(string userId, Guid cartItemId, int quantity);
        Task RemoveFromCartAsync(string userId, Guid cartItemId);
        Task ClearCartAsync(string userId);
    }
}