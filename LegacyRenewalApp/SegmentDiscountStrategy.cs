using System.Collections.Generic;

namespace LegacyRenewalApp;

public class SegmentDiscountStrategy : IDiscountStrategy
{
    private static Dictionary<string, (decimal Rate,string Note)> _segmentRates = new()
    {
        ["Silver"] = (0.05m,"silver discount"),
        ["Gold"] = (0.10m,"gold discount"),
        ["Platinum"] = (0.15m,"platinum discount")
    };
    
    public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, bool useLoyaltyPoints)
    {
        decimal baseAmount = ComputeBase(plan,seatCount);
        
        if (customer.Segment == "Education" && plan.IsEducationEligible)
            return new DiscountResult(baseAmount * 0.20m, "education discount");
        
        if (_segmentRates.TryGetValue(customer.Segment, out var entry))
            return new DiscountResult(baseAmount * entry.Rate, entry.Note);
        
        return new DiscountResult(0m, string.Empty);
    }
    
    private static decimal ComputeBase(SubscriptionPlan plan, int seatCount)
        => plan.MonthlyPricePerSeat * seatCount * 12m + plan.SetupFee;
}