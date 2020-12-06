using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/LoginHistory")]
    [Authorize]
    public class LoginHistoryController : Controller
    {
        private readonly EcommerceContext _context;

        public LoginHistoryController(
            EcommerceContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<LoginHistory> GetLoginHistory()
        {
            return _context.LoginHistory.OrderByDescending(l => l.CreatedDate).Take(1000);
        }
    }
}