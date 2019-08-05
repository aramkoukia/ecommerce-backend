using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceApi.ViewModel.Moneris;
using EcommerceApi.ViewModel.Moneris.EcommerceApi.ViewModel.Moneris;

namespace EcommerceApi.Services.PaymentPlatform
{
    public interface IMonerisService
    {
        Task<ValidationResponse> TransactionRequestAsync(TransactionRequest transactionRequest);
        Task<object> BatchClose(BatchCloseRequest batchCloseRequest);
        Task<object> UnPair(UnPairRequest unPairRequest);
        Task<object> Pair(PairRequest pairRequest);
        Task<object> Initialize(InitializeRequest initializeRequest);
    }
}
