namespace LegacyRenewalApp;

public class LoyaltyYearsDiscountStrategy : IDiscountStrategy
{
    public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, bool useLoyaltyPoints)
    {
        decimal baseAmount = plan.MonthlyPricePerSeat * seatCount * 12m + plan.SetupFee;
        
        if (customer.YearsWithCompany >= 5)
            return new DiscountResult(baseAmount * 0.07m, "long-term loyalty discount");
        
        if (customer.YearsWithCompany >= 2)
            return new DiscountResult(baseAmount * 0.03m, "basic loyalty discount");
        
        return new DiscountResult(0m, string.Empty);
    }
}
