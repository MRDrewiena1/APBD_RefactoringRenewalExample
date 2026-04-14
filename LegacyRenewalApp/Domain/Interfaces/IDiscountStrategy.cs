namespace LegacyRenewalApp;

public interface IDiscountStrategy
{
    DiscountResult Calculate(
        Customer customer,
        SubscriptionPlan plan,
        int seatCount,
        bool useLoyaltyPoints
        );
}

public record DiscountResult(decimal Amount, string Note);