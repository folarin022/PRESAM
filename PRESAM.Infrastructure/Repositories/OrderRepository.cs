// PRESAM.Infrastructure/Repositories/OrderRepository.cs
using Microsoft.EntityFrameworkCore;
using PRESAM.Domain.Entities;
using PRESAM.Domain.Interfaces;
using PRESAM.Infrastructure.Context;

namespace PRESAM.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(PresamDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderWithItemsAsync(Guid orderId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}