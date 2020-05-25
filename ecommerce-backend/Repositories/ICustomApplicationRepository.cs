using EcommerceApi.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface ICustomApplicationRepository
    {
        Task<IEnumerable<CustomApplicationViewModel>> GetCustomApplicationSteps();
    }
}
