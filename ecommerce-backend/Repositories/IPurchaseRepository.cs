using EcommerceApi.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<PurchaseViewModel>> GetPurchases();
        Task<IEnumerable<PurchaseDetailViewModel>> GetPurchaseDetails(bool showPending, bool showOnDelivery, bool showCustomClearance, bool showCompletes);
        
    }
}
