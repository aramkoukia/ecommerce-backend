using EcommerceApi.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerViewModel>> GetCustomers();
    }
}
