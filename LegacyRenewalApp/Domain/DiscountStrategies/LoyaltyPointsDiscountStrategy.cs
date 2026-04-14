using System;

namespace LegacyRenewalApp;

public class LoyaltyPointsDiscountStrategy : IDiscountStrategy
{
    private const int MaxPointsUsable = 200;
    
    public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, bool useLoyaltyPoints)
    {
        if (!useLoyaltyPoints || customer.LoyaltyPoints <= 0)
            return new DiscountResult(0m, string.Empty);
        
        int pointsToUse = Math.Min(customer.LoyaltyPoints, MaxPointsUsable);
        
        return new DiscountResult(pointsToUse, $"loyalty points used: {pointsToUse}");
    }
}
