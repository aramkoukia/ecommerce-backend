using EcommerceApi.ViewModel.Moneris;
using System.Threading.Tasks;

namespace EcommerceApi.Services.PaymentPlatform
{
    public interface IMonerisService
    {
        Task<ValidationResponse> TransactionRequestAsync(TransactionRequest transactionRequest);
        Task<object> BatchClose(MonerisAdminRequest monerisAdminRequest);
        Task<object> UnPair(MonerisAdminRequest monerisAdminRequest);
        Task<object> Pair(MonerisAdminRequest monerisAdminRequest);
        Task<object> Initialize(MonerisAdminRequest monerisAdminRequest);
    }
}
