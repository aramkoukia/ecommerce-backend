using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private AppDb _db;
        public ValuesController(AppDb db)
        {
            _db = db;
        }
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            await _db.Connection.OpenAsync();
            var query = new CustomerQueries(_db);
            var result = await query.GetAllCustomers();

            return new OkObjectResult(result);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
