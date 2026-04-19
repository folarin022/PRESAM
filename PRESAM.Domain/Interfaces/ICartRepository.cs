using PRESAM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Domain.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
        Task<CartItem> GetCartItemAsync(int cartId, int productId);
        Task ClearCartAsync(int cartId);
    }
}
