using System;
using System.Collections.Generic;

namespace LegacyRenewalApp;

public class PaymentFeeCalculator
{
    private static readonly Dictionary<string, (decimal Rate, string Note)> Rates = new()
    {
        ["CARD"] = (0.020m, "card payment fee"), 
        ["BANK_TRANSFER"] = (0.010m, "bank transfer fee"), 
        ["PAYPAL"] = (0.035m, "paypal fee"), 
        ["INVOICE"] = (0.000m, "invoice payment")
    };
    public (decimal Fee, string Note) Calculate(string paymentMethod, decimal subtotal)
    {
        if (!Rates.TryGetValue(paymentMethod, out var entry))
            throw new ArgumentException("Unsupported payment method");
        
        return (subtotal * entry.Rate, entry.Note);
    }
}
