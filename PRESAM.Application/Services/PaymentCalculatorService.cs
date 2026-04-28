// PRESAM.Application/Services/PaymentCalculatorService.cs
using PRESAM.Application.DTOs;

namespace PRESAM.Application.Services
{
    public class PaymentCalculatorService
    {
        private const decimal TAX_RATE = 0.075m;
        private const decimal SHIPPING_FEE = 2000m;

        private static readonly Dictionary<string, decimal> InterestRates = new()
        {
            { "Weekly4", 0.05m }, 
            { "Weekly8", 0.08m }, 
            { "Weekly12", 0.12m },
            { "Monthly3", 0.06m }, 
            { "Monthly6", 0.10m }, 
            { "Monthly12", 0.15m } 
        };

        private static readonly Dictionary<string, int> InstallmentCounts = new()
        {
            { "Weekly4", 4 },
            { "Weekly8", 8 },
            { "Weekly12", 12 },
            { "Monthly3", 3 },
            { "Monthly6", 6 },
            { "Monthly12", 12 }
        };

        public List<PaymentOptionDto> GetPaymentOptions(decimal subtotal)
        {
            var options = new List<PaymentOptionDto>();
            var shippingFee = SHIPPING_FEE;
            var taxAmount = subtotal * TAX_RATE;
            var totalBeforeInterest = subtotal + shippingFee + taxAmount;

            options.Add(new PaymentOptionDto
            {
                PlanType = "FullPayment",
                Duration = 1,
                TotalAmount = totalBeforeInterest,
                InstallmentAmount = totalBeforeInterest,
                InterestRate = 0,
                InterestAmount = 0,
                ShippingFee = shippingFee,
                TaxAmount = taxAmount,
                Description = "Pay once - best value!"
            });

            options.Add(CreatePaymentOption("Weekly4", subtotal, shippingFee, taxAmount));
            options.Add(CreatePaymentOption("Weekly8", subtotal, shippingFee, taxAmount));
            options.Add(CreatePaymentOption("Weekly12", subtotal, shippingFee, taxAmount));


            options.Add(CreatePaymentOption("Monthly3", subtotal, shippingFee, taxAmount));
            options.Add(CreatePaymentOption("Monthly6", subtotal, shippingFee, taxAmount));
            options.Add(CreatePaymentOption("Monthly12", subtotal, shippingFee, taxAmount));

            return options;
        }

        private PaymentOptionDto CreatePaymentOption(string planType, decimal subtotal, decimal shippingFee, decimal taxAmount)
        {
            var interestRate = InterestRates[planType];
            var installmentCount = InstallmentCounts[planType];
            var totalBeforeInterest = subtotal + shippingFee + taxAmount;
            var interestAmount = totalBeforeInterest * interestRate;
            var totalWithInterest = totalBeforeInterest + interestAmount;
            var installmentAmount = totalWithInterest / installmentCount;

            return new PaymentOptionDto
            {
                PlanType = planType,
                Duration = installmentCount,
                TotalAmount = totalWithInterest,
                InstallmentAmount = Math.Round(installmentAmount, 2),
                InterestRate = interestRate * 100,
                InterestAmount = interestAmount,
                ShippingFee = shippingFee,
                TaxAmount = taxAmount,
                Description = GetPlanDescription(planType, installmentCount)
            };
        }

        private string GetPlanDescription(string planType, int count)
        {
            return planType switch
            {
                "Weekly4" => $"Pay every week for {count} weeks",
                "Weekly8" => $"Pay every week for {count} weeks",
                "Weekly12" => $"Pay every week for {count} weeks",
                "Monthly3" => $"Pay every month for {count} months",
                "Monthly6" => $"Pay every month for {count} months",
                "Monthly12" => $"Pay every month for {count} months",
                _ => "One time payment"
            };
        }

        public List<InstallmentPaymentDto> GenerateInstallmentSchedule(Guid orderId, string planType, decimal installmentAmount, DateTime startDate)
        {
            var schedule = new List<InstallmentPaymentDto>();
            var installmentCount = InstallmentCounts[planType];

            if (planType == "FullPayment")
            {
                return schedule;
            }

            var isWeekly = planType.StartsWith("Weekly");
            var intervalDays = isWeekly ? 7 : 30;

            for (int i = 1; i <= installmentCount; i++)
            {
                schedule.Add(new InstallmentPaymentDto
                {
                    InstallmentNumber = i,
                    Amount = installmentAmount,
                    DueDate = startDate.AddDays(intervalDays * i),
                    Status = "Pending"
                });
            }

            return schedule;
        }
    }
}