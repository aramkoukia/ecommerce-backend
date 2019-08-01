using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Payments")]
    public class PaymentController : Controller
    {
        private readonly EcommerceContext _context;

        public PaymentController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/CustomerStoreCredits
        [HttpPost("/Pair")]
        public IActionResult Pair([FromBody] PairRequest pairRequest)
        {
            return Ok();
        }

        [HttpPost("/Initialize")]
        public IActionResult Initialize([FromBody] InitializeRequest initializeRequest)
        {
            return Ok();
        }
    }
}