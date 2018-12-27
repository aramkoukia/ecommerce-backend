using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EcommerceApi.ViewModel;
using EcommerceApi.Repositories;
using DinkToPdf.Contracts;
using DinkToPdf;
using EcommerceApi.Untilities;
using System.IO;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers
{
    // [Authorize]
    [Produces("application/json")]
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;
        private readonly IConverter _converter;
        private readonly IEmailSender _emailSender;

        public OrdersController(EcommerceContext context,
                                UserManager<ApplicationUser> userManager,
                                IOrderRepository orderRepository,
                                IConverter converter,
                                IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _orderRepository = orderRepository;
            _converter = converter;
            _emailSender = emailSender;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<IEnumerable<OrderViewModel>> GetOrder()
        {
            return await _orderRepository.GetOrders(null);
        }

        // GET: api/Orders
        [HttpGet("location/{locationId}")]
        public async Task<IEnumerable<OrderViewModel>> GetOrderByLocation([FromRoute] int locationId)
        {
            return await _orderRepository.GetOrders(locationId);
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Order.AsNoTracking()
                .Include(o => o.OrderDetail)
                    .ThenInclude(o =>o.Product)
                .Include(t => t.OrderTax)
                    .ThenInclude(t => t.Tax)
                .Include(o => o.OrderPayment)
                .Include(o => o.Customer)
                .Include(l => l.Location)
                .SingleOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // PUT: api/Orders/5/Status
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> PutOrder([FromRoute] int id, [FromBody] UpdateOrderStatus updateOrderStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updateOrderStatus == null || string.IsNullOrEmpty(updateOrderStatus.OrderStatus))
            {
                return BadRequest();
            }

            var order = await _context.Order.SingleOrDefaultAsync(m => m.OrderId == id);
            order.Status = updateOrderStatus.OrderStatus;
            if (order.Status.Equals(OrderStatus.Paid.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var userId = _userManager.GetUserId(User);
                order.OrderPayment.Add(
                    new OrderPayment
                    {
                        CreatedByUserId = userId,
                        CreatedDate = order.CreatedDate,
                        PaymentAmount = order.Total,
                        PaymentDate = order.CreatedDate,
                        PaymentTypeId = 1 // default credit/debit for now
                    }
                );
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(order);
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder([FromRoute] int id, [FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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

        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            order.CreatedByUserId = userId;
            order.CreatedDate = DateTime.UtcNow;
            order.OrderDate = DateTime.UtcNow;
            order.Customer = null;
            order.Location = null;
            if (order.Status.Equals(OrderStatus.Paid.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                order.OrderPayment.Add(
                    new OrderPayment
                    {
                        CreatedByUserId = userId,
                        CreatedDate = order.CreatedDate,
                        PaymentAmount = order.Total,
                        PaymentDate = order.CreatedDate,
                        PaymentTypeId = 1 // default credit/debit for now
                    }
                );
            }

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        // GET: api/Orders
        [HttpGet("{orderId}/email")]
        public async Task<IActionResult> EmailOrder([FromRoute] int orderId)
        {
            var order = await _context.Order.AsNoTracking()
                .Include(o => o.OrderDetail)
                    .ThenInclude(o => o.Product)
                .Include(t => t.OrderTax)
                    .ThenInclude(t => t.Tax)
                .Include(o => o.OrderPayment)
                .Include(o => o.Customer)
                .Include(l => l.Location)
                .SingleOrDefaultAsync(m => m.OrderId == orderId);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = $"Order {order.OrderId}",
                // Out = @"C:\PDFCreator\Employee_Report.pdf"
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = OrderTemplateGenerator.GetHtmlString(order, false),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            var message = @"
                            Dear Customer,


                            Attached is your current invoice from LED Lights and Parts (Pixel Print Ltd). 

                            If you have a credit account this invoice will be marked Awaiting payment. 

                            If you already paid this invoice will be marked PAID. and no further action is required. 

                            If you requested the quote, the invoice will be marked HOLD. 

                            If you returned or exchanged the invoice will be marked as Return/Exchange or Credit. 

                            Thank you for working with LED Lights and Parts! We\'re happy to work with you to solve any of your lighting challenges. 

                            Sincerely,

                            Shaney

                            3695 East 1st Ave Vancouver, BC V5M 1C2

                            Tel: (604) 559-5000

                            Cel: (778) 839-3352

                            Fax: (604) 559-5008

                            www.lightsandparts.com | essi@lightsandparts.com
            ";
            var attachment = new MemoryStream(file);
            var attachmentName = $"Invoice No {order.OrderId}";
            var subject = $"Pixel Print Ltd (LED Lights and Parts) Invoice No {order.OrderId}";
            await _emailSender.SendEmailAsync(order.Customer.Email, subject, null, message, attachment, attachmentName);
            return Ok();
        }

        // GET: api/Orders
        [HttpGet("{orderId}/print")]
        public async Task<IActionResult> PrintOrder([FromRoute] int orderId)
        {
            var order = await _context.Order.AsNoTracking()
                .Include(o => o.OrderDetail)
                    .ThenInclude(o => o.Product)
                .Include(t => t.OrderTax)
                    .ThenInclude(t => t.Tax)
                .Include(o => o.OrderPayment)
                .Include(o => o.Customer)
                .Include(l => l.Location)
                .SingleOrDefaultAsync(m => m.OrderId == orderId);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = $"Order {order.OrderId}",
                // Out = @"C:\PDFCreator\Employee_Report.pdf"
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = OrderTemplateGenerator.GetHtmlString(order, true),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
                // HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Page [page] of [toPage]" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            // _converter.Convert(pdf);
            var file = _converter.Convert(pdf);

            return File(file, "application/pdf", $"Order-{order.OrderId}.pdf");
        }

        // DELETE: api/Orders/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteOrder([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var order = await _context.Order.SingleOrDefaultAsync(m => m.OrderId == id);
        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Order.Remove(order);
        //    await _context.SaveChangesAsync();

        //    return Ok(order);
        //}

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }
    }
}