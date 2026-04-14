namespace LegacyRenewalApp;

public interface IBillingGateway
{
    void SaveInvoice(RenewalInvoice invoice);
    void SendEmail(string to, string subject, string body);
}