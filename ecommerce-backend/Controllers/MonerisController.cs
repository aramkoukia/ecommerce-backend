using System.Threading.Tasks;
using EcommerceApi.Models;
using EcommerceApi.PaymentPlatform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers
{
    // [Authorize]
    [Produces("application/json")]
    [Route("api/Moneris")]
    public class MonerisController : Controller
    {

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
        public async Task<IActionResult> Post([FromBody] ValidationResponse validationResponse)
        {
            return Ok(validationResponse);
        }
    }
}
