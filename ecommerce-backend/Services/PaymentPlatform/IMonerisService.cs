using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceApi.Services.PaymentPlatform
{
    public interface IMonerisService
    {
        Task<ValidationResponse> TransactionRequestAsync(TransactionRequest transactionRequest);
    }
}
