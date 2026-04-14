using System.Collections.Generic;

namespace LegacyRenewalApp;

public class SupportFeeResolver
{
    private static readonly Dictionary<string, decimal> Fees = new()
    {
        ["START"] = 250m, 
        ["PRO"] = 400m, 
        ["ENTERPRISE"] = 700m
    };
    
    public (decimal Fee, string Note) Resolve(string planCode, bool includePremiumSupport)
    {
        if (!includePremiumSupport) 
            return (0m, string.Empty);
        
        decimal fee = Fees.GetValueOrDefault(planCode, 0m);
        
        return (fee, "premium support included");
    }
}
