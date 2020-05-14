using EcommerceApi.ViewModel;
using EcommerceApi.ViewModel.Website;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IProductTypeRepository
    {
        Task<IEnumerable<WebsiteProductTypeViewModel>> GetWebsiteProductTypes();
        Task<IEnumerable<ProductTypeViewModel>> GetProductTypes();
        Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetWebsiteProductsByProductType(string id);
    }
}
