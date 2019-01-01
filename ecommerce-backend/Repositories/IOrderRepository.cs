using EcommerceApi.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderViewModel>> GetOrders(int? locationId);
        Task<IEnumerable<OrderViewModel>> GetOrdersByCustomer(int customerId);
    }
}
