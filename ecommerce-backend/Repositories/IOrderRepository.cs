using EcommerceApi.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderViewModel>> GetOrders(bool showAll, int locationId);
        Task<IEnumerable<OrderViewModel>> GetOrdersByCustomer(int customerId);
    }
}
