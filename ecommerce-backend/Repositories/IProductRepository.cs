using EcommerceApi.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductViewModel>> GetProducts();
        Task<IEnumerable<ProductViewModel>> GetAvailableProducts();
        Task<IEnumerable<ProductViewModelV2>> GetAvailableProductsV2(int locationId);
        Task<ProductViewModel> GetProduct(int productId);
        Task<IEnumerable<ProductTransactionViewModel>> GetProductTransactions(int productId, DateTime fromDate, DateTime toDate, string userId, int locationId);
    }
}
