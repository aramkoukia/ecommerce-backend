using EcommerceApi.ViewModel;
using EcommerceApi.ViewModel.Website;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface ICustomApplicationRepository
    {
        Task<IEnumerable<CustomApplicationViewModel>> GetCustomApplicationSteps();
        Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetCustomApplicationResult(string[] selectedOptions);
    }
}
