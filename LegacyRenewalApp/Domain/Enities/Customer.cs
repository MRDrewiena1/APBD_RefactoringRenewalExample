using System.Collections.Generic;

namespace LegacyRenewalApp
{
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Segment { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int YearsWithCompany { get; set; }
        public int LoyaltyPoints { get; set; }
        public bool IsActive { get; set; }

        public decimal GetTaxRate()
        {
            var taxDictionary = new Dictionary<string, decimal>();
            
            taxDictionary.Add("Poland", 0.23m);
            taxDictionary.Add("Germany", 0.19m);
            taxDictionary.Add("Czech Republic", 0.21m);
            taxDictionary.Add("Norway", 0.25m);
            
            return taxDictionary.GetValueOrDefault(Country,0.20m);
        }
    }
}
