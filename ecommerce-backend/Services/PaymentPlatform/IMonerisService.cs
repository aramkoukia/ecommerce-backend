using EcommerceApi.ViewModel.Moneris;
using System.Threading.Tasks;

namespace EcommerceApi.Services.PaymentPlatform
{
    public interface IMonerisService
    {
        Task<ValidationResponse> TransactionRequestAsync(TransactionRequest transactionRequest);
        Task<ValidationResponse> BatchClose(MonerisAdminRequest monerisAdminRequest);
        Task<ValidationResponse> UnPair(MonerisAdminRequest monerisAdminRequest);
        Task<ValidationResponse> Pair(MonerisAdminRequest monerisAdminRequest);
        Task<ValidationResponse> Initialize(MonerisAdminRequest monerisAdminRequest);
    }
}
