using System.Threading.Tasks;
using EcommerceApi.PaymentPlatform;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EcommerceApi
{
    [Route("api/[controller]")]
    public class MonerisController : Controller
    {
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ValidationResponse validationResponse)
        {
            return Ok(validationResponse);
        }
    }
}
