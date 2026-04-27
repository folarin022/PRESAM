// PRESAM.Infrastructure/Context/PresamDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PRESAM.Domain.Entities;

namespace PRESAM.Infrastructure.Context
{
    public class PresamDbContext : IdentityDbContext<User>
    {
        public PresamDbContext(DbContextOptions<PresamDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            builder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });

            var fixedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<Category>().HasData(
                new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Electronics", Description = "Electronic items", ImageUrl = "/images/electronics.jpg", IsActive = true, CreatedAt = fixedDate },
                new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Clothing", Description = "Fashion items", ImageUrl = "/images/clothing.jpg", IsActive = true, CreatedAt = fixedDate },
                new Category { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Books", Description = "Educational books", ImageUrl = "/images/books.jpg", IsActive = true, CreatedAt = fixedDate }
            );
        }
    }
}