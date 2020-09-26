using EcommerceApi.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderViewModel>> GetOrders(DateTime fromDate, DateTime toDate, int locationId, string userId);
        Task<IEnumerable<OrderViewModel>> GetOrdersByCustomer(int customerId);
        Task<InventoryViewModel> GetProductInventoryForValidation(int productId, int locationId);
        Task<IEnumerable<OrderViewModel>> GetCustomerOrdersByDate(int customerId, DateTime fromDate, DateTime toDate);
    }
}
