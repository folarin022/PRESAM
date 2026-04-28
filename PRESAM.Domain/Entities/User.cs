using Microsoft.AspNetCore.Identity;

namespace PRESAM.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }

        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public string? EmailConfirmationCode { get; set; }
        public DateTime? EmailConfirmationCodeExpiry { get; set; }
    }
}