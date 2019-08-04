using System;
using System.Threading.Tasks;
using EcommerceApi.Models;
using EcommerceApi.Models.Moneris;
using EcommerceApi.PaymentPlatform;
using EcommerceApi.ViewModel.Moneris;
using EcommerceApi.ViewModel.Moneris.EcommerceApi.ViewModel.Moneris;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EcommerceApi.Controllers
{
    // [Authorize]
    [Produces("application/json")]
    [Route("api/Moneris")]
    public class MonerisController : Controller
    {
        private readonly EcommerceContext _context;

        public MonerisController(EcommerceContext context)
        {
            _context = context;
        }

        [HttpPost("/Pair")]
        public IActionResult Pair([FromBody] PairRequest pairRequest)
        {
            return Ok(pairRequest);
        }

        [HttpPost("/Initialize")]
        public IActionResult Initialize([FromBody] InitializeRequest initializeRequest)
        {
            return Ok(initializeRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] object validationResponse)
        {
            _context.MonerisCallbackLogs.Add(
                new MonerisCallbackLog
                {
                    CreatedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time"),
                    Response = JsonConvert.SerializeObject(validationResponse)
                });
            await _context.SaveChangesAsync();
            return Ok(validationResponse);
        }
    }
}
