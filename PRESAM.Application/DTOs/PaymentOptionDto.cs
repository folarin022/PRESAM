using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Application.DTOs
{
    public class PaymentOptionDto
    {
        public string PlanType { get; set; }
        public int Duration { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; }
        public string Description { get; set; }
    }

    public class CheckoutDto
    {
        public List<CartItemDto> Items { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string SelectedPaymentPlan { get; set; }
        public List<PaymentOptionDto> PaymentOptions { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class InstallmentPaymentDto
    {
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public DateTime? PaidDate { get; set; }
        public string PaymentReference { get; set; }
    }
}