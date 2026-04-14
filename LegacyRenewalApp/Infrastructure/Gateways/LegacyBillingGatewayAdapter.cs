namespace LegacyRenewalApp;

public class LegacyBillingGatewayAdapter : IBillingGateway
{
    public void SaveInvoice(RenewalInvoice invoice)
        => LegacyBillingGateway.SaveInvoice(invoice);
    

    public void SendEmail(string to, string subject, string body)
        => LegacyBillingGateway.SendEmail(to, subject, body);
}