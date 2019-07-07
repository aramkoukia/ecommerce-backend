using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceApi.Repositories;
using EcommerceApi.ViewModel;
using DinkToPdf.Contracts;
using EcommerceApi.Services;
using DinkToPdf;
using EcommerceApi.Untilities;
using System.IO;
using System;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Customers")]
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly ICustomerRepository _customerRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IConverter _converter;
        private readonly IEmailSender _emailSender;

        public CustomersController(
            EcommerceContext context,
            ICustomerRepository customerRepository,
            IReportRepository reportRepository,
            IConverter converter,
            IEmailSender emailSender)
        {
            _context = context;
            _customerRepository = customerRepository;
            _reportRepository = reportRepository;
            _converter = converter;
            _emailSender = emailSender;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<IEnumerable<CustomerViewModel>> GetCustomer()
        {
            return await _customerRepository.GetCustomers();
        }

        [HttpGet("WithBalance")]
        public async Task<IEnumerable<CustomerViewModel>> GetCustomerWithBalance()
        {
            return await _customerRepository.GetCustomersWithBalance();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _customerRepository.GetCustomer(id);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        // PUT: api/Customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer([FromRoute] int id, [FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != customer.CustomerId)
            {
                return BadRequest();
            }
            customer.CustomerCode = id.ToString();
            _context.Entry(customer).State = EntityState.Modified;

            if (customer.FirstName == null) customer.FirstName = "";
            if (customer.LastName == null) customer.LastName = "";
            if (customer.UserName == null) customer.UserName = "";
            if (customer.CompanyName == null) customer.CompanyName = "";
            if (customer.Email == null) customer.Email = "";
            if (customer.PstNumber == null) customer.PstNumber = "";
            if (customer.PhoneNumber == null) customer.PhoneNumber = "";

            if (customer.Disabled && customer.MergeToCustomerId.HasValue)
            {
                var mergeToCustomer = await _context.Customer.FirstOrDefaultAsync(c => c.MergeToCustomerId == customer.MergeToCustomerId);
                if (mergeToCustomer != null)
                {
                    var customerOrders = _context.Order.Where(o => o.CustomerId == id);
                    foreach (var order in customerOrders)
                    {
                        order.CustomerId = customer.MergeToCustomerId;
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Customers
        [HttpPost]
        public async Task<IActionResult> PostCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // temp, until we turn of the customer sync, then we can use identity
            customer.CustomerId = _context.Customer.Max(m => m.CustomerId) + 1;
            customer.CustomerCode = customer.CustomerId.ToString();
            if (customer.FirstName == null) customer.FirstName = "";
            if (customer.LastName == null) customer.LastName = "";
            if (customer.UserName == null) customer.UserName = "";
            if (customer.CompanyName == null) customer.CompanyName = "";
            if (customer.Email == null) customer.Email = "";
            if (customer.PstNumber == null) customer.PstNumber = "";
            if (customer.PhoneNumber == null) customer.PhoneNumber = "";

            _context.Customer.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomer", new { id = customer.CustomerId }, customer);
        }

        // GET: api/Customers/EmailStatement
        [HttpGet("{id}/EmailStatement")]
        public async Task<IActionResult> EmailCustomerStatement(int id, DateTime fromDate, DateTime toDate)
        {
            var customer = await _customerRepository.GetCustomer(id);
            var file = await GenerateStatementPdf(customer, fromDate, toDate);
            var message = $@"
Dear Customer,


Thank you for choosing LED Lights and Parts. Your e-statement for the month, {toDate.ToString("MMMM")} {toDate.Year} is attached in the email. For any specific invoice information, get back to us to receive a copy. Please contact us at +1(604) 559-5000 for any other queries. 

Regards

Sina Shanaey

3695 East 1st Ave Vancouver, BC V5M 1C2

Tel:(604) 559-5000

Cel:(778) 838-8070

Fax:(604) 559-5008

www.lightsandparts.com | sina@lightsandparts.com
            ";
            var attachment = new MemoryStream(file);
            var attachmentName = $"Monthly Statement {toDate.ToString("MMMM")} {toDate.Year}.pdf";
            var subject = $"Pixel Print Ltd (LED Lights and Parts) Monthly Statement {toDate.ToString("MMMM")} {toDate.Year}";

            if (string.IsNullOrEmpty(customer.Email))
            {
                customer.Email = "aramkoukia@gmail.com";
            }

            _emailSender.SendEmailAsync(customer.Email, subject, message, attachment, attachmentName, true);
            return Ok();
        }

        // GET: api/Customers/EmailStatement
        [AllowAnonymous]
        [HttpGet("{id}/PrintStatement")]
        public async Task<FileResult> PrintCustomerStatement(int id, DateTime fromDate, DateTime toDate)
        {
            var customer = await _customerRepository.GetCustomer(id);
            var file = await GenerateStatementPdf(customer, fromDate, toDate);
            FileContentResult result = new FileContentResult(file, "application/pdf")
            {
                FileDownloadName = $"CustomerStatement-{id}.pdf"
            };
            return result;
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customer.SingleOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(customer);
        }

        private async Task<byte[]> GenerateStatementPdf(CustomerViewModel customer, DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now.AddDays(-30);
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;

            var customerPaidOrders = await _reportRepository.GetCustomerPaidReport(customer.CustomerId, fromDate, toDate);
            var customerUnPaidOrders = await _reportRepository.GetCustomerUnPaidReport(customer.CustomerId, DateTime.MinValue, toDate);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = $"Statement {customer.CompanyName}",
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = CustomerStatementTemplateGenerator.GetHtmlString(customer, customerPaidOrders, customerUnPaidOrders, fromDate, toDate),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            return _converter.Convert(pdf);
        }

        private bool CustomerExists(int id)
        {
            return _context.Customer.Any(e => e.CustomerId == id);
        }
    }
}