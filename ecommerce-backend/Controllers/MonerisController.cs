using System;
using System.Threading.Tasks;
using EcommerceApi.Models;
using EcommerceApi.Models.Moneris;
using EcommerceApi.Services.PaymentPlatform;
using EcommerceApi.ViewModel.Moneris;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Moneris")]
    public class MonerisController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IMonerisService _monerisService;
        private readonly IHttpContextAccessor _accessor;

        public MonerisController(EcommerceContext context,
                                 IMonerisService monerisService,
                                 IHttpContextAccessor accessor)
        {
            _context = context;
            _monerisService = monerisService;
            _accessor = accessor;
        }

        [HttpPost("pair")]
        public async Task<IActionResult> Pair([FromBody] MonerisAdminRequest monerisAdminRequest)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            var clientIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString().Replace("{", "").Replace("}", "");
            monerisAdminRequest.ClientIp = clientIp;
            monerisAdminRequest.UserId = currentUser.Identity.Name;
            var result = await _monerisService.Pair(monerisAdminRequest);
            if (result != null && result.Receipt.Error.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return Ok();
            }
            return BadRequest(result?.Receipt?.Message);
        }

        [HttpPost("unpair")]
        public async Task<IActionResult> UnPair([FromBody] MonerisAdminRequest monerisAdminRequest)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            var clientIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString().Replace("{", "").Replace("}", "");
            monerisAdminRequest.ClientIp = clientIp;
            monerisAdminRequest.UserId = currentUser.Identity.Name;
            var result = await _monerisService.UnPair(monerisAdminRequest);
            if (result != null && result.Receipt.Error.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return Ok();
            }
            return BadRequest(result?.Receipt?.Message);
        }

        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize([FromBody] MonerisAdminRequest monerisAdminRequest)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            var clientIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString().Replace("{", "").Replace("}", "");
            monerisAdminRequest.ClientIp = clientIp;
            monerisAdminRequest.UserId = currentUser.Identity.Name;
            var result = await _monerisService.Initialize(monerisAdminRequest);
            if (result != null && result.Receipt.Error.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return Ok();
            }
            return BadRequest(result.Receipt.Message);
        }

        [HttpPost("batchclose")]
        public async Task<IActionResult> BatchClose([FromBody] MonerisAdminRequest monerisAdminRequest)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            var clientIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString().Replace("{", "").Replace("}", "");
            monerisAdminRequest.ClientIp = clientIp;
            monerisAdminRequest.UserId = currentUser.Identity.Name;
            var result = await _monerisService.BatchClose(monerisAdminRequest);
            if (result != null && result.Receipt.Error.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return Ok();
            }
            return BadRequest(result?.Receipt?.Message);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] ValidationResponse validationResponse)
        {
            SaveCallbackLog(validationResponse);
            await _context.SaveChangesAsync();
            if (validationResponse.Receipt.TxnName.Equals(TransactionType.purchase.ToString(), StringComparison.InvariantCultureIgnoreCase)) {
                UpdateOrderAuthCode(validationResponse);
            }
            return Ok(validationResponse);
        }

        private void UpdateOrderAuthCode(ValidationResponse validationResponse)
        {
            var valiatedOrderId = 0;
            var result = int.TryParse(validationResponse.Receipt.ReceiptId, out valiatedOrderId);
            if (!result) {
                return;
            }
            var order = _context.Order.Find(valiatedOrderId);
            if (order != null)
            {
                order.AuthCode = validationResponse.Receipt.AuthCode;
                order.CardLastFourDigits = validationResponse.Receipt.Pan;
            }
        }

        private void SaveCallbackLog(ValidationResponse validationResponse)
        {
            _context.MonerisCallbackLog.Add(
                new MonerisCallbackLog
                {
                    AccountType = validationResponse.Receipt.AccountType,
                    // OrderId = validationResponse.Receipt.ReceiptId == "null" ? (int?)null : int.Parse(validationResponse.Receipt.ReceiptId),
                    Aid = validationResponse.Receipt.Aid,
                    Amount = validationResponse.Receipt.Amount,
                    AppLabel = validationResponse.Receipt.AppLabel,
                    AppPreferredName = validationResponse.Receipt.AppPreferredName,
                    Arqc = validationResponse.Receipt.Arqc,
                    AuthCode = validationResponse.Receipt.AuthCode,
                    AvailableBalance = validationResponse.Receipt.AvailableBalance,
                    BaseRate = validationResponse.Receipt.BaseRate,
                    CardName = validationResponse.Receipt.CardName,
                    CardType = validationResponse.Receipt.CardType,
                    CloudTicket = validationResponse.Receipt.CloudTicket,
                    Completed = validationResponse.Receipt.Completed,
                    CvmIndicator = validationResponse.Receipt.CvmIndicator,
                    EMVCashbackAmount = validationResponse.Receipt.EMVCashbackAmount,
                    EMVEchoData = validationResponse.Receipt.EMVEchoData,
                    EncryptedCardInfo = validationResponse.Receipt.EncryptedCardInfo,
                    Error = validationResponse.Receipt.Error,
                    ExchangeRate = validationResponse.Receipt.ExchangeRate,
                    ForeignCurrencyAmount = validationResponse.Receipt.ForeignCurrencyAmount,
                    ForeignCurrencyCode = validationResponse.Receipt.ForeignCurrencyCode,
                    FormFactor = validationResponse.Receipt.FormFactor,
                    InitRequired = validationResponse.Receipt.InitRequired,
                    ISO = validationResponse.Receipt.ISO,
                    LanguageCode = validationResponse.Receipt.LanguageCode,
                    LogonRequired = validationResponse.Receipt.LogonRequired,
                    Pan = validationResponse.Receipt.Pan,
                    PartialAuthAmount = validationResponse.Receipt.PartialAuthAmount,
                    ReceiptId = validationResponse.Receipt.ReceiptId,
                    ReferenceNumber = validationResponse.Receipt.ReferenceNumber,
                    ReservedField1 = validationResponse.Receipt.ReservedField1,
                    ReservedField2 = validationResponse.Receipt.ReservedField2,
                    ReservedField3 = validationResponse.Receipt.ReservedField3,
                    ReservedField4 = validationResponse.Receipt.ReservedField4,
                    ResponseCode = validationResponse.Receipt.ResponseCode,
                    SafIndicator = validationResponse.Receipt.SafIndicator,
                    SurchargeAmount = validationResponse.Receipt.SurchargeAmount,
                    SwipeIndicator = validationResponse.Receipt.SwipeIndicator,
                    Tcacc = validationResponse.Receipt.Tcacc,
                    Timeout = validationResponse.Receipt.Timeout,
                    TipAmount = validationResponse.Receipt.TipAmount,
                    Token = validationResponse.Receipt.Token,
                    TokenResponseCode = validationResponse.Receipt.TokenResponseCode,
                    TransDate = validationResponse.Receipt.TransDate,
                    TransId = validationResponse.Receipt.TransId,
                    TransTime = validationResponse.Receipt.TransTime,
                    TransType = validationResponse.Receipt.TransType,
                    Tsi = validationResponse.Receipt.Tsi,
                    TvrArqc = validationResponse.Receipt.TvrArqc,
                    TvrTcacc = validationResponse.Receipt.TvrTcacc,
                    TxnName = validationResponse.Receipt.TxnName,
                    CreatedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time"),
                    Response = JsonConvert.SerializeObject(validationResponse)
                });
        }
    }
}
