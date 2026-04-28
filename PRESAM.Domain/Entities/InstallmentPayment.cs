
namespace PRESAM.Domain.Entities
{
    public class InstallmentPayment : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public InstallmentStatus Status { get; set; }
        public string? PaymentReference { get; set; }

        public virtual Order Order { get; set; }
    }
}