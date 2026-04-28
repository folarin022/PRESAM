using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Domain.Entities
{
    public enum PaymentPlan
    {
        FullPayment,
        Weekly4,
        Weekly8,
        Weekly12,
        Monthly3,
        Monthly6,
        Monthly12
    }

    public enum InstallmentStatus
    {
        Pending,
        Paid,
        Overdue
    }

    public class Order : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }

        // New installment fields
        public PaymentPlan PaymentPlan { get; set; } = PaymentPlan.FullPayment;
        public decimal InstallmentAmount { get; set; }
        public decimal TotalWithInterest { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; }
        public int InstallmentCount { get; set; }
        public int InstallmentsPaid { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<InstallmentPayment> InstallmentPayments { get; set; }
    }
}
