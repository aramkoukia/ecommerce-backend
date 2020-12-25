using EcommerceApi.ViewModel;
using EcommerceApi.ViewModel.Website;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IProductTypeRepository
    {
        Task<IEnumerable<WebsiteProductTypeViewModel>> GetWebsiteProductTypes();
        Task<WebsiteProductTypeViewModel> GetWebsiteProductType(string slug);
        Task<IEnumerable<ProductTypeViewModel>> GetProductTypes();
        Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetWebsiteProductsByProductType(string id);
        Task<IEnumerable<string>> GetWebsiteProductSlugs();
        Task<WebsiteProductViewModel> GetWebsiteProduct(string id);
        Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetWebsiteProducts();
        Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetWebsiteProductDetails();
        Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetNewWebsiteProducts();
        Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetPopularWebsiteProducts();

    }
}
