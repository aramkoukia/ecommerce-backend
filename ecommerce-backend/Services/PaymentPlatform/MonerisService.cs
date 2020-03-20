using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using EcommerceApi.Models;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using EcommerceApi.Models.Moneris;
using EcommerceApi.ViewModel.Moneris;
using EcommerceApi.ViewModel.Moneris.EcommerceApi.ViewModel.Moneris;

namespace EcommerceApi.Services.PaymentPlatform
{
    public class MonerisService : IMonerisService
    {
        public HttpClient Client { get; }
        private readonly IConfiguration _config;
        private readonly EcommerceContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private static readonly TelemetryClient _telemetryClient;

        static MonerisService()
        {
            _telemetryClient = new TelemetryClient();
        }
        public MonerisService(IHttpClientFactory clientFactory,
                              IConfiguration config,
                              EcommerceContext context)
        {
            _config = config;
            _context = context;
            _clientFactory = clientFactory;
        }

        public async Task<ValidationResponse> TransactionRequestAsync(TransactionRequest transactionRequest)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var clientPosSettings = _context.ClientPosSettings.FirstOrDefault(c => c.ClientIp == transactionRequest.ClientIp);
                if (clientPosSettings == null)
                {
                    _telemetryClient.TrackException(new Exception($"ClientPosSettings missing for client IP: {transactionRequest.ClientIp}"));
                    return null; // log error, and don't return null dude!
                }

                var monerisRequest = new MonerisRequest
                {
                    apiToken = _config["Moneris:apiToken"],
                    postbackUrl = _config["Moneris:postbackUrl"],
                    storeId = clientPosSettings.StoreId,
                    terminalId = clientPosSettings.TerminalId,
                    txnType = transactionRequest.TransactionType,
                    request = new Request
                    {
                        amount = transactionRequest.Amount.ToString(),
                        orderId = transactionRequest.OrderId.ToString()
                    },
                };
                var requestString = JsonConvert.SerializeObject(monerisRequest);
                var response = await client.PostAsync(
                    _config["Moneris:baseUrl"],
                    new StringContent(
                        requestString,
                        Encoding.UTF8,
                        "application/json"));

                var result = await response.Content
                    .ReadAsAsync<ValidationResponse>();

                var monerisLog = new MonerisTransactionLog
                {
                    ClientIp = transactionRequest.ClientIp,
                    OrderId = transactionRequest.OrderId,
                    Amount = transactionRequest.Amount,
                    CreatedDate = transactionRequest.CreatedDate,
                    Request = requestString,
                    Response = JsonConvert.SerializeObject(result),
                    ResponseCode = result.Receipt.ResponseCode,
                    ResponseMessage = result.Receipt.Message,
                    StoreId = monerisRequest.storeId,
                    TerminalId = monerisRequest.terminalId,
                    TransactionType = monerisRequest.txnType,
                    UserId = transactionRequest.UserId,
                };

                await _context.MonerisTransactionLog.AddAsync(monerisLog);
                await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                return null;
            }
        }

        public async Task<object> BatchClose(MonerisAdminRequest monerisAdminRequest)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var monerisRequest = new MonerisRequest
                {
                    apiToken = _config["Moneris:apiToken"],
                    postbackUrl = _config["Moneris:postbackUrl"],
                    storeId = monerisAdminRequest.StoreId,
                    terminalId = monerisAdminRequest.TerminalId,
                    txnType = "batchClose"
                };

                var requestString = JsonConvert.SerializeObject(monerisRequest);
                var response = await client.PostAsync(
                    _config["Moneris:baseUrl"],
                    new StringContent(
                        requestString,
                        Encoding.UTF8,
                        "application/json"));

                var result = await response.Content
                    .ReadAsAsync<ValidationResponse>();

                var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

                var monerisLog = new MonerisTransactionLog
                {
                    ClientIp = monerisAdminRequest.ClientIp,
                    OrderId = null,
                    Amount = 0,
                    CreatedDate = date,
                    Request = requestString,
                    Response = JsonConvert.SerializeObject(result),
                    ResponseCode = result.Receipt.ResponseCode,
                    ResponseMessage = result.Receipt.Message,
                    StoreId = monerisRequest.storeId,
                    TerminalId = monerisRequest.terminalId,
                    TransactionType = monerisRequest.txnType,
                    UserId = monerisAdminRequest.UserId,
                };

                await _context.MonerisTransactionLog.AddAsync(monerisLog);
                await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                return null;
            }
        }

        public async Task<object> UnPair(MonerisAdminRequest monerisAdminRequest)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var monerisRequest = new MonerisRequest
                {
                    apiToken = _config["Moneris:apiToken"],
                    postbackUrl = _config["Moneris:postbackUrl"],
                    storeId = monerisAdminRequest.StoreId,
                    terminalId = monerisAdminRequest.TerminalId,
                    txnType = "unpair"
                };

                var requestString = JsonConvert.SerializeObject(monerisRequest);
                var response = await client.PostAsync(
                    _config["Moneris:baseUrl"],
                    new StringContent(
                        requestString,
                        Encoding.UTF8,
                        "application/json"));

                var result = await response.Content
                    .ReadAsAsync<ValidationResponse>();

                var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

                var monerisLog = new MonerisTransactionLog
                {
                    ClientIp = monerisAdminRequest.ClientIp,
                    OrderId = null,
                    Amount = 0,
                    CreatedDate = date,
                    Request = requestString,
                    Response = JsonConvert.SerializeObject(result),
                    ResponseCode = result.Receipt.ResponseCode,
                    ResponseMessage = result.Receipt.Message,
                    StoreId = monerisRequest.storeId,
                    TerminalId = monerisRequest.terminalId,
                    TransactionType = monerisRequest.txnType,
                    UserId = monerisAdminRequest.UserId,
                };

                await _context.MonerisTransactionLog.AddAsync(monerisLog);
                await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                return null;
            }
        }

        public async Task<object> Pair(MonerisAdminRequest monerisAdminRequest)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var monerisRequest = new MonerisRequest
                {
                    apiToken = _config["Moneris:apiToken"],
                    postbackUrl = _config["Moneris:postbackUrl"],
                    storeId = monerisAdminRequest.StoreId,
                    terminalId = monerisAdminRequest.TerminalId,
                    txnType = "pair",
                    request = new Request { 
                        pairingToken = monerisAdminRequest.PairingToken
                    }
                };

                var requestString = JsonConvert.SerializeObject(monerisRequest);
                var response = await client.PostAsync(
                    _config["Moneris:baseUrl"],
                    new StringContent(
                        requestString,
                        Encoding.UTF8,
                        "application/json"));

                var result = await response.Content
                    .ReadAsAsync<ValidationResponse>();

                var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

                var monerisLog = new MonerisTransactionLog
                {
                    ClientIp = monerisAdminRequest.ClientIp,
                    OrderId = null,
                    Amount = 0,
                    CreatedDate = date,
                    Request = requestString,
                    Response = JsonConvert.SerializeObject(result),
                    ResponseCode = result.Receipt.ResponseCode,
                    ResponseMessage = result.Receipt.Message,
                    StoreId = monerisRequest.storeId,
                    TerminalId = monerisRequest.terminalId,
                    TransactionType = monerisRequest.txnType,
                    UserId = monerisAdminRequest.UserId,
                };

                await _context.MonerisTransactionLog.AddAsync(monerisLog);
                await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                return null;
            }
        }

        public async Task<object> Initialize(MonerisAdminRequest monerisAdminRequest)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var monerisRequest = new MonerisRequest
                {
                    apiToken = _config["Moneris:apiToken"],
                    postbackUrl = _config["Moneris:postbackUrl"],
                    storeId = monerisAdminRequest.StoreId,
                    terminalId = monerisAdminRequest.TerminalId,
                    txnType = "initialization"
                };

                var requestString = JsonConvert.SerializeObject(monerisRequest);
                var response = await client.PostAsync(
                    _config["Moneris:baseUrl"],
                    new StringContent(
                        requestString,
                        Encoding.UTF8,
                        "application/json"));

                var result = await response.Content
                    .ReadAsAsync<ValidationResponse>();

                var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

                var monerisLog = new MonerisTransactionLog
                {
                    ClientIp = monerisAdminRequest.ClientIp,
                    OrderId = null,
                    Amount = 0,
                    CreatedDate = date,
                    Request = requestString,
                    Response = JsonConvert.SerializeObject(result),
                    ResponseCode = result.Receipt.ResponseCode,
                    ResponseMessage = result.Receipt.Message,
                    StoreId = monerisRequest.storeId,
                    TerminalId = monerisRequest.terminalId,
                    TransactionType = monerisRequest.txnType,
                    UserId = monerisAdminRequest.UserId,
                };

                await _context.MonerisTransactionLog.AddAsync(monerisLog);
                await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                return null;
            }
        }
    }
}
