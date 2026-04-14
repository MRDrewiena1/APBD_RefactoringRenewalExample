using System;
using System.Collections.Generic;
using System.Linq;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private const decimal MinDiscountedSubtotal = 300m;
        private const decimal MinFinalAmount = 500m;
        
        private ICustomerRepository _customerRepository;
        private ISubscriptionPlanRepository _plans;
        private IEnumerable<IDiscountStrategy> _discountStrategies;
        private SupportFeeResolver _supportFeeResolver;
        private PaymentFeeCalculator _paymentFeeCalculator;
        private IBillingGateway _billingGateway;
        private RenewalRequestValidator _validator;
        
        public SubscriptionRenewalService()
            : this(
                new CustomerRepository(),
                new SubscriptionPlanRepository(),
                [
                    new SegmentDiscountStrategy(),
                    new LoyaltyYearsDiscountStrategy(),
                    new TeamSizeDiscountStrategy(),
                    new LoyaltyPointsDiscountStrategy()
                ],
                new SupportFeeResolver(),
                new PaymentFeeCalculator(),
                new LegacyBillingGatewayAdapter(),
                new RenewalRequestValidator()) {}

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository plans,
            IEnumerable<IDiscountStrategy> discountStrategies,
            SupportFeeResolver supportFeeResolver,
            PaymentFeeCalculator paymentFeeCalculator,
            IBillingGateway billingGateway,
            RenewalRequestValidator validator)
        {
            _customerRepository = customerRepository;
            _plans = plans;
            _discountStrategies = discountStrategies;
            _supportFeeResolver = supportFeeResolver;
            _paymentFeeCalculator = paymentFeeCalculator;
            _billingGateway = billingGateway;
            _validator = validator;
        }
        
        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            _validator.ValidateInput(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();
            
            var customer = _customerRepository.GetById(customerId);
            var plan = _plans.GetByCode(normalizedPlanCode);

            _validator.ValidateCustomerIsActive(customer);

            decimal baseAmount = plan.GetBaseAmount(seatCount);
            
            var discountResults = _discountStrategies
                .Select(s => s.Calculate(customer, plan, seatCount, useLoyaltyPoints))
                .Where(r => r.Amount > 0)
                .ToList();
            
            decimal discountAmount = discountResults.Sum(r => r.Amount);
            
            decimal subTotal = baseAmount - discountAmount;
            bool minSubtotalApplied = subTotal < MinDiscountedSubtotal;
            if (minSubtotalApplied) subTotal = MinDiscountedSubtotal;
            
            var (supportFee, supportNote) = _supportFeeResolver
                .Resolve(normalizedPlanCode, includePremiumSupport);
            
            var (paymentFee, paymentNote) = _paymentFeeCalculator
                .Calculate(normalizedPaymentMethod, subTotal+supportFee);

            decimal taxRate = customer.GetTaxRate();
            decimal taxBase = subTotal + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            
            decimal finalAmount = taxBase + taxAmount;

            bool minFinalAmountApplied = finalAmount < MinFinalAmount;
            if (minFinalAmountApplied) finalAmount = MinFinalAmount;

            string notes = BuildNotes(
                discountResults.Select(r => r.Note),
                minSubtotalApplied ? "minimum discounted subtotal applied" : null,
                supportNote,
                paymentNote,
                minFinalAmountApplied ? "minimum invoice amount applied" : null
                );

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = RoundValue(baseAmount),
                DiscountAmount = RoundValue(discountAmount),
                SupportFee = RoundValue(supportFee),
                PaymentFee = RoundValue(paymentFee),
                TaxAmount = RoundValue(taxAmount),
                FinalAmount = RoundValue(finalAmount),
                Notes = notes,
                GeneratedAt = DateTime.UtcNow
            };

            _billingGateway.SaveInvoice(invoice);
            SendConfirmationEmail(customer, invoice);

            return invoice;
        }

        private void SendConfirmationEmail(Customer customer, RenewalInvoice invoice)
        {
            if (string.IsNullOrEmpty(customer.Email))
                return;
            
            _billingGateway.SendEmail(
                customer.Email,
                "Subscription renewal invoice",
                $"Hello {customer.FullName}, your renewal for plan {invoice.PlanCode} " +
                $"has been prepared. Final amount: {invoice.FinalAmount:F2}."
                );
        }

        private static string BuildNotes(IEnumerable<string> discountNotes, params string[] notes)
            => string.Join("; ", discountNotes.Concat(notes).Where(n => !string.IsNullOrWhiteSpace(n)));

        private static decimal RoundValue(decimal value)
            => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
