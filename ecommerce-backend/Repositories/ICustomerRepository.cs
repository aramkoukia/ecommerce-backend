using EcommerceApi.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerViewModel>> GetCustomers();
        Task<IEnumerable<CustomerViewModel>> GetCustomersWithBalance(bool showDisabled);
        Task<CustomerViewModel> GetCustomer(int customerId);
        Task<decimal> GetCustomerBalance(int customerId);
        Task<IEnumerable<CustomerOrderSummaryViewModel>> GetCustomerOrderSummary(int customerId);
    }
}
