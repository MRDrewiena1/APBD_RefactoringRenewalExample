namespace LegacyRenewalApp;

public interface ICustomerRepository
{
    Customer GetById(int id);
}