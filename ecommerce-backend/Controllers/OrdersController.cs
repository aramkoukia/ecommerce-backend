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
    [Authorize]
    [Produces("application/json")]
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IConverter _converter;
        private readonly IEmailSender _emailSender;

        public OrdersController(EcommerceContext context,
                                UserManager<ApplicationUser> userManager,
                                IOrderRepository orderRepository,
                                ICustomerRepository customerRepository,
                                IConverter converter,
                                IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _converter = converter;
            _emailSender = emailSender;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<IEnumerable<OrderViewModel>> GetOrder(bool showAllOrders)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _orderRepository.GetOrders(showAllOrders, 0, user.Id);
        }

        // GET: api/Orders/Location/{locationId}
        [HttpGet("location/{locationId}")]
        public async Task<IEnumerable<OrderViewModel>> GetOrderByLocation([FromRoute] int locationId, bool showAllOrders)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _orderRepository.GetOrders(showAllOrders, locationId, user.Id);
        }

        // GET: api/Orders/Customer/{customerId}
        [HttpGet("customer/{customerId}")]
        public async Task<IEnumerable<OrderViewModel>> GetOrderByCustomer([FromRoute] int customerId)
        {
            return await _orderRepository.GetOrdersByCustomer(customerId);
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
                    .ThenInclude(p => p.PaymentType)
                .Include(o => o.Customer)
                .Include(l => l.Location)
                .SingleOrDefaultAsync(m => m.OrderId == id);

            if (order.CustomerId != null && order.Customer != null)
            {
                order.Customer.AccountBalance = await _customerRepository.GetCustomerBalance(order.CustomerId.Value);
            }

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // GET: api/cancelonholdorders
        [HttpGet("cancelonholdorders")]
        [AllowAnonymous]
        public async Task<IActionResult> CancelOnHoldOrders()
        {
            var orders = _context.Order
                .Where(o => o.Status.Equals(OrderStatus.OnHold.ToString(), StringComparison.InvariantCultureIgnoreCase)
                       && o.OrderDate < DateTime.Now.AddDays(-14));
            var updateOrderStatus = new UpdateOrderStatus
            {
                OrderStatus = OrderStatus.Draft.ToString()
            };

            foreach (var order in orders)
            {
                var done = await AddToInventory(order, updateOrderStatus);
                order.Status = OrderStatus.Draft.ToString();
                order.Notes = $"{order.Notes} - Marked as Draft from OnHold after 14 days by system.";
                _emailSender.SendAdminReportAsync("OnHold Order Cancelled", $"OnHold Order Cancelled. \n Order Id: {order.OrderId}");
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.AuthCode != null && u.AuthCode.Equals(order.AuthCode, StringComparison.InvariantCultureIgnoreCase));
            order.CreatedByUserId = user.Id;
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
            order.CreatedDate = date;
            order.OrderDate = date;
            if (order.Status != OrderStatus.Return.ToString() && order.Total < 0)
            {
                order.Status = OrderStatus.Return.ToString();
            }

            if (order.Status == OrderStatus.Return.ToString()
                && order.CustomerId != null
                && (order.OrderPayment == null || !order.OrderPayment.Any()))
            {
                order.IsAccountReturn = true;
            }

            order.OrderId = _context.Order.Max(o => o.OrderId) + 1;
            if (order.OrderPayment != null && order.OrderPayment.Any())
            {
                var orderPayments = order.OrderPayment.Select(m => new { m.PaymentTypeId, m.PaymentAmount, m.ChequeNo }).Distinct().ToList();
                order.OrderPayment.Clear();
                foreach (var payment in orderPayments)
                {
                    order.OrderPayment.Add(new OrderPayment
                    {
                        CreatedByUserId = user.Id,
                        CreatedDate = date,
                        PaymentAmount = payment.PaymentAmount,
                        PaymentDate = date,
                        PaymentTypeId = payment.PaymentTypeId,
                        ChequeNo = payment.ChequeNo
                    });

                    // Paid by Store Credit. Upating customer store credit and add to history
                    if (payment.PaymentTypeId == 26 && order.CustomerId != null)
                    {
                        var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == order.CustomerId);
                        if (customer != null)
                        {
                            var storeCreditNote = $"Used to pay Order: {order.OrderId}";
                            if (order.Status == OrderStatus.Return.ToString())
                            {
                                storeCreditNote = $"Store Credit added for Refund of Order: {order.OrderId}";
                            }

                            customer.StoreCredit = customer.StoreCredit + decimal.Multiply(payment.PaymentAmount, decimal.MinusOne);
                            _context.CustomerStoreCredit.Add(
                                new CustomerStoreCredit
                                {
                                    Amount = decimal.Multiply(payment.PaymentAmount, decimal.MinusOne),
                                    CreatedByUserId = user.Email,
                                    CreatedDate = date,
                                    CustomerId = customer.CustomerId,
                                    Notes = storeCreditNote
                                }
                            );
                        }
                    }
                }
            }

            order.Customer = null;
            order.Location = null;
            var done = await NewOrderUpdateInventory(order);
            _context.Order.Add(order);

            _emailSender.SendAdminReportAsync("New Order", $"New Order Created. \n Order Id: {order.OrderId}. \n Status: {order.Status} \n Total: ${order.Total} \n User: {user.GivenName}");

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder([FromRoute] int id, [FromBody] Order order)
        {
            // only supports draft orders
            if (!ModelState.IsValid || order.Status != OrderStatus.Draft.ToString())
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.AuthCode != null && u.AuthCode.Equals(order.AuthCode, StringComparison.InvariantCultureIgnoreCase));

            var existingOrder = _context.Order
                .Include(o => o.OrderDetail)
                .Include(o => o.OrderTax)
                .FirstOrDefault(o => o.OrderId == id);
            if (existingOrder == null)
            {
                return BadRequest($"Order Id {id} not found");
            }

            existingOrder.CreatedByUserId = user.Id;
            existingOrder.CustomerId = order.CustomerId;
            existingOrder.Email = order.Email;
            existingOrder.LocationId = order.LocationId;
            existingOrder.Notes = order.Notes;
            existingOrder.PoNumber = order.PoNumber;
            existingOrder.PstNumber = order.PstNumber;
            existingOrder.SubTotal = order.SubTotal;
            existingOrder.Total = order.Total;
            existingOrder.TotalDiscount = order.TotalDiscount;

            foreach (var detail in existingOrder.OrderDetail)
            {
                _context.OrderDetail.Remove(detail);
            }

            foreach (var tax in existingOrder.OrderTax)
            {
                _context.OrderTax.Remove(tax);
            }

            foreach (var detail in order.OrderDetail)
            {
                existingOrder.OrderDetail.Add(detail);
            }

            foreach (var tax in order.OrderTax)
            {
                existingOrder.OrderTax.Add(tax);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = existingOrder.OrderId }, existingOrder);
        }

        // PUT: api/Orders/5/Status
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> PutOrderStatus([FromRoute] int id, [FromBody] UpdateOrderStatus updateOrderStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updateOrderStatus == null || string.IsNullOrEmpty(updateOrderStatus.OrderStatus))
            {
                return BadRequest();
            }
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

            var order = await _context.Order
                .Include(o => o.OrderDetail)
                .SingleOrDefaultAsync(m => m.OrderId == id);

            var originalOrderStatus = order.Status;
            if (updateOrderStatus.OrderStatus.Equals(OrderStatus.Paid.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var userId = _userManager.GetUserId(User);
                if (updateOrderStatus.OrderPayment != null && updateOrderStatus.OrderPayment.Any())
                {
                    var orderPayments = updateOrderStatus.OrderPayment.Select(m=> new { m.PaymentTypeId, m.PaymentAmount, m.ChequeNo }).Distinct().ToList();
                    foreach (var payment in orderPayments)
                    {
                        order.OrderPayment.Add(new OrderPayment
                        {
                            CreatedByUserId = userId,
                            CreatedDate = date,
                            PaymentAmount = payment.PaymentAmount,
                            PaymentDate = date,
                            PaymentTypeId = payment.PaymentTypeId,
                            ChequeNo = payment.ChequeNo
                        });

                        // Paid by Store Credit. Upating customer store credit and add to history
                        if (payment.PaymentTypeId == 26 && order.CustomerId != null)
                        {
                            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == order.CustomerId);
                            if (customer != null)
                            {
                                var storeCreditNote = $"Used to pay Order: {order.OrderId}";
                                if (order.Status == OrderStatus.Return.ToString())
                                {
                                    storeCreditNote = $"Store Credit added for Refund of Order: {order.OrderId}";
                                }

                                customer.StoreCredit = customer.StoreCredit + decimal.Multiply(payment.PaymentAmount, decimal.MinusOne);
                                _context.CustomerStoreCredit.Add(
                                    new CustomerStoreCredit
                                    {
                                        Amount = decimal.Multiply(payment.PaymentAmount, decimal.MinusOne),
                                        CreatedByUserId = userId,
                                        CreatedDate = date,
                                        CustomerId = customer.CustomerId,
                                        Notes = storeCreditNote
                                    }
                                );
                            }
                        }
                    }
                }
            }

            // When order is marked as Draft from OnHold we should add them to inventory
            if (updateOrderStatus.OrderStatus == OrderStatus.Draft.ToString() &&
               order.Status == OrderStatus.OnHold.ToString())
            {
                var done = await AddToInventory(order, updateOrderStatus);
            }
            else
            {
                var done = await ExistingOrderUpdateInventory(order, updateOrderStatus);
            }

            order.Status = updateOrderStatus.OrderStatus;
            if (updateOrderStatus.OrderStatus == OrderStatus.Paid.ToString())
            {
                order.OrderDate = date;
            }

            _emailSender.SendAdminReportAsync("Order Status Changed", $"Order Status changed. \n Order Id: {id}. \n From: {originalOrderStatus} To: {updateOrderStatus.OrderStatus.ToString()}");

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

        [HttpPut("{id}/Move/{locationId}")]
        public async Task<IActionResult> PutOrderLocation([FromRoute] int id, [FromRoute] int locationId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

            var order = await _context.Order
                .Include(o => o.OrderDetail)
                .Include(o => o.Location)
                .SingleOrDefaultAsync(m => m.OrderId == id);

            if (order.LocationId == locationId)
            {
                return BadRequest("You need to change a different location for transfer");
            }

            var originalLocation = order.Location.LocationName;
            var newLocation = _context.Location.FirstOrDefault(l => l.LocationId == locationId).LocationName;

            var done = await TransferOrderInventory(order, locationId);

            order.LocationId = locationId;

            _emailSender.SendAdminReportAsync("Order Location Changed", $"Order Location changed. \n Order Id: {id}. \n From: {originalLocation} To: {newLocation}");

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

        [HttpPut("{id}/Info")]
        public async Task<IActionResult> PutOrderInfo([FromRoute] int id, [FromBody] UpdateOrderInfo updateOrderInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Order.SingleOrDefaultAsync(m => m.OrderId == id);
            order.Notes = updateOrderInfo.Notes;
            order.PoNumber = updateOrderInfo.PoNumber;
            order.CardAuthCode = updateOrderInfo.CardAuthCode;
            order.CardLastFourDigits = updateOrderInfo.CardLastFourDigits;
            order.AuthorizedBy = updateOrderInfo.AuthorizedBy;
            order.PhoneNumber = updateOrderInfo.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok(order);
        }

        [HttpPut("{id}/Payment")]
        public async Task<IActionResult> PutOrderPayment([FromRoute] int id, [FromBody] UpdateOrderPayment updateOrderPayment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updateOrderPayment == null || updateOrderPayment.OrderPayment == null || !updateOrderPayment.OrderPayment.Any())
            {
                return BadRequest();
            }

            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

            var order = await _context.Order
                .Include(o => o.OrderPayment)
                  .ThenInclude(o => o.PaymentType)
                .SingleOrDefaultAsync(m => m.OrderId == id);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            var orderPayments = updateOrderPayment.OrderPayment.Select(m => new { m.PaymentTypeId, m.PaymentAmount, m.ChequeNo }).Distinct().ToList();
            var originalPaymentTypes = string.Join(",", order.OrderPayment.Select(m => m.PaymentType.PaymentTypeName).ToArray());
            var originalPaymentAmount = string.Join(",", order.OrderPayment.Select(m => m.PaymentAmount).ToArray());
            var originalAmountTotal = order.OrderPayment.Sum(m => m.PaymentAmount);
            var newAmountTotal = updateOrderPayment.OrderPayment.Sum(m => m.PaymentAmount);
            if (newAmountTotal != originalAmountTotal)
            {
                return BadRequest("Original Amount is not the same as New Amount");
            }

            var paymentTypeIds = updateOrderPayment.OrderPayment.Select(o => o.PaymentTypeId);
            var newPaymentTypes = string.Join(",", _context.PaymentType.Where(p =>  paymentTypeIds.Contains(p.PaymentTypeId)).Select(m => m.PaymentTypeName).ToArray());
            var newPaymentAmount = string.Join(",", updateOrderPayment.OrderPayment.Select(m => m.PaymentAmount).ToArray());

            foreach (var payment in order.OrderPayment)
            { 
                _context.OrderPayment.Remove(payment);
            }

            foreach (var payment in orderPayments)
            {
                order.OrderPayment.Add(new OrderPayment
                {
                    CreatedByUserId = userId,
                    CreatedDate = date,
                    PaymentAmount = payment.PaymentAmount,
                    PaymentDate = date,
                    PaymentTypeId = payment.PaymentTypeId,
                    ChequeNo = payment.ChequeNo,
                    OrderId = id,
                });

                // Paid by Store Credit. Upating customer store credit and add to history
                if (payment.PaymentTypeId == 26 && order.CustomerId != null)
                {
                    var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == order.CustomerId);
                    if (customer != null)
                    {
                        var storeCreditNote = $"Used to pay Order: {order.OrderId}";
                        if (order.Status == OrderStatus.Return.ToString())
                        {
                            storeCreditNote = $"Store Credit added for Refund of Order: {order.OrderId}";
                        }

                        customer.StoreCredit = customer.StoreCredit + decimal.Multiply(payment.PaymentAmount, decimal.MinusOne);
                        _context.CustomerStoreCredit.Add(
                            new CustomerStoreCredit
                            {
                                Amount = decimal.Multiply(payment.PaymentAmount, decimal.MinusOne),
                                CreatedByUserId = userId,
                                CreatedDate = date,
                                CustomerId = customer.CustomerId,
                                Notes = storeCreditNote
                            }
                        );
                    }

                }
            }

            _emailSender.SendAdminReportAsync("Order Payment Type Changed", $"Order Payment Type changed. \n Order Id: {id}. \n From Types: {originalPaymentTypes}, Amounts: {originalPaymentAmount}. \n\n To Types: {newPaymentTypes}, Amounts: {newPaymentAmount}");

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

        [HttpPut("{id}/Customer")]
        public async Task<IActionResult> PutOrderCustomer([FromRoute] int id, [FromBody] UpdateOrderCustomer updateOrderCustomer)
        {
            var order = await _context.Order.SingleOrDefaultAsync(m => m.OrderId == id);
            if (!ModelState.IsValid || !order.Status.Equals(OrderStatus.Draft.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest(ModelState);
            }

            order.CustomerId = updateOrderCustomer.CustomerId;
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        [HttpGet("{orderId}/email")]
        public async Task<IActionResult> EmailOrder([FromRoute] int orderId, [FromQuery] string email, string authCode)
        {
            var order = await _context.Order.AsNoTracking()
                .Include(o => o.OrderDetail)
                    .ThenInclude(o => o.Product)
                .Include(t => t.OrderTax)
                    .ThenInclude(t => t.Tax)
                .Include(o => o.OrderPayment)
                    .ThenInclude(t1 => t1.PaymentType)
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
            };

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == order.CreatedByUserId);
            order.CreatedByUserName = user.UserName;

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = OrderTemplateGenerator.GetHtmlString(order, false),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            var message = $@"
Dear Customer,


Attached is your current invoice from LED Lights and Parts (Pixel Print Ltd). 

If you have a credit account, the invoice will be marked as ACCOUNT.

If you requested to hold on products, the invoice will be marked as HOLD.

If you already paid, the invoice will be marked as PAID and no further action is required. 

If you requested the quote, the invoice will be marked as DRAFT. 

If you returned or exchanged the invoice will be marked as Return/Exchange or Credit. 

Thank you for working with LED Lights and Parts! We are happy to work with you to solve any of your lighting challenges. 

Sincerely,

{user.UserName}

3695 East 1st Ave Vancouver, BC V5M 1C2

Tel: (604) 559-5000

Cel: (778) 839-3352

Fax: (604) 559-5008

www.lightsandparts.com | {user.Email}
            ";
            var attachment = new MemoryStream(file);
            var attachmentName = $"Invoice No {order.OrderId}.pdf";
            var subject = $"Pixel Print Ltd (LED Lights and Parts) Invoice No {order.OrderId}";

            if (string.IsNullOrEmpty(email))
            {
                email = order.Customer.Email;
            }

            var orderToUpdateEmail = _context.Order.FirstOrDefault(o => o.OrderId == orderId);
            orderToUpdateEmail.Email = email;
            await _context.SaveChangesAsync();

            _emailSender.SendEmailAsync(email, subject, null, message, attachment, attachmentName, true);
            return Ok();
        }

        [HttpGet("{orderId}/print")]
        [AllowAnonymous]
        public async Task<FileResult> PrintOrder([FromRoute] int orderId)
        {
            var order = await _context.Order.AsNoTracking()
                .Include(o => o.OrderDetail)
                    .ThenInclude(o => o.Product)
                .Include(t => t.OrderTax)
                    .ThenInclude(t => t.Tax)
                .Include(o => o.OrderPayment)
                    .ThenInclude(t1 => t1.PaymentType)
                .Include(o => o.Customer)
                .Include(l => l.Location)
                .SingleOrDefaultAsync(m => m.OrderId == orderId);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == order.CreatedByUserId);
            order.CreatedByUserName = user?.UserName;

            var includeMerchantCopy = false;
            if (order.Status == OrderStatus.Draft.ToString() || order.Status == OrderStatus.OnHold.ToString())
            {
                includeMerchantCopy = false;
            }
            else
            {
                includeMerchantCopy = true;
            }

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = $"Order {order.OrderId}",
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = OrderTemplateGenerator.GetHtmlString(order, includeMerchantCopy),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            // _converter.Convert(pdf);
            var file = _converter.Convert(pdf);
            FileContentResult result = new FileContentResult(file, "application/pdf")
            {
                FileDownloadName = $"Order-{order.OrderId}.pdf"
            };

            return result;
        }

        [HttpGet("{orderId}/packingslip")]
        [AllowAnonymous]
        public async Task<FileResult> PackingSlipOrder([FromRoute] int orderId)
        {
            var order = await _context.Order.AsNoTracking()
                .Include(o => o.OrderDetail)
                    .ThenInclude(o => o.Product)
                .Include(t => t.OrderTax)
                    .ThenInclude(t => t.Tax)
                .Include(o => o.OrderPayment)
                    .ThenInclude(t1 => t1.PaymentType)
                .Include(o => o.Customer)
                .Include(l => l.Location)
                .SingleOrDefaultAsync(m => m.OrderId == orderId);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == order.CreatedByUserId);
            order.CreatedByUserName = user?.UserName;

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = $"Packing Slip - Order {order.OrderId}",
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = ShipmentSlipTemplateGenerator.GetHtmlString(order),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            FileContentResult result = new FileContentResult(file, "application/pdf")
            {
                FileDownloadName = $"Order-{order.OrderId}.pdf"
            };

            return result;
        }

        [HttpPut("validateinventory")]
        public async Task<IActionResult> ValidateInventory([FromBody] InventoryValidationRequest inventoryValidationRequest)
        {
            var result = new List<InventoryValidationResponse>();
            if (inventoryValidationRequest == null
               || inventoryValidationRequest.OrderItems == null
               || !inventoryValidationRequest.OrderItems.Any())
            {
                return Ok(result);
            }

            foreach (var item in inventoryValidationRequest.OrderItems)
            {
                var inventory = await _orderRepository.GetProductInventoryForValidation(item.ProductId, item.LocationId);
                if (inventory.Balance < item.Amount)
                {
                    result.Add(new InventoryValidationResponse
                    {
                        ProductCode = inventory.ProductCode,
                        Amount = inventory.Balance,
                        AmountRequested = item.Amount,
                        AmountShort = item.Amount - inventory.Balance,
                        LocationName = inventory.LocationName,
                        OnHold = inventory.OnHold,
                        ProductId = item.ProductId,
                        ProductName = inventory.ProductName,
                    });
                }
            }

            return Ok(result);
        }

        private async Task<bool> OriginalOrderWasPaid(int? originalOrderId)
        {
            if (!originalOrderId.HasValue)
            {
                return false;
            }

            var originalOrder = await _context.Order.SingleOrDefaultAsync(m => m.OrderId == originalOrderId.Value);
            if (originalOrder != null && originalOrder.Status.Equals(OrderStatus.Paid.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private async Task<bool> NewOrderUpdateInventory(Order order)
        {
            if (order.Status == OrderStatus.Draft.ToString())
            {
                return true;
            }
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

            foreach (var item in order.OrderDetail)
            {
                var productInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
                    m.ProductId == item.ProductId &&
                    m.LocationId == order.LocationId);

                // if order is refund we add to inventory
                var addOrUpdate = -1;
                var amount = Math.Abs(item.Amount);
                if (order.Status == OrderStatus.Return.ToString() || item.Amount < 0)
                {
                    addOrUpdate = 1;
                }

                if (productInventory != null)
                {
                    productInventory.Balance = productInventory.Balance + (addOrUpdate * amount);
                    productInventory.ModifiedDate = date;
                }
                else
                {
                    _context.ProductInventory.Add(
                        new ProductInventory
                        {
                            Balance = addOrUpdate * amount,
                            BinCode = "",
                            LocationId = order.LocationId,
                            ModifiedDate = date,
                            ProductId = item.ProductId
                        });
                }
            }
            return true;
        }

        private async Task<bool> ExistingOrderUpdateInventory(Order order, UpdateOrderStatus updateOrderStatus)
        {
            if (updateOrderStatus.OrderStatus == OrderStatus.Draft.ToString())
            {
                return true;
            }
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

            foreach (var item in order.OrderDetail)
            {
                var productInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
                    m.ProductId == item.ProductId &&
                    m.LocationId == order.LocationId);

                // if order is refund we add to inventory
                var addOrUpdate = -1;
                var amount = Math.Abs(item.Amount);
                if (updateOrderStatus.OrderStatus == OrderStatus.Return.ToString() || item.Amount < 0)
                {
                    addOrUpdate = 1;
                }

                if (productInventory != null)
                {
                    productInventory.Balance = productInventory.Balance + (addOrUpdate * amount);
                    productInventory.ModifiedDate = date;
                }
                else
                {
                    _context.ProductInventory.Add(
                        new ProductInventory
                        {
                            Balance = addOrUpdate * amount,
                            BinCode = "",
                            LocationId = order.LocationId,
                            ModifiedDate = date,
                            ProductId = item.ProductId
                        });
                }

            }
            return true;
        }

        private async Task<bool> TransferOrderInventory(Order order, int locationId)
        {
            if (order.Status == OrderStatus.Draft.ToString())
            {
                return true;
            }

            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");

            foreach (var item in order.OrderDetail)
            {
                // Updating Source Location Inventory
                var sourceLocationProductInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
                    m.ProductId == item.ProductId &&
                    m.LocationId == order.LocationId);

                // if order is refund we add to inventory
                var addOrUpdate = 1;
                var amount = Math.Abs(item.Amount);
                if (order.Status == OrderStatus.Return.ToString() || item.Amount < 0)
                {
                    addOrUpdate = -1;
                }

                if (sourceLocationProductInventory != null)
                {
                    sourceLocationProductInventory.Balance = sourceLocationProductInventory.Balance + (addOrUpdate * amount);
                    sourceLocationProductInventory.ModifiedDate = date;
                }
                else
                {
                    _context.ProductInventory.Add(
                        new ProductInventory
                        {
                            Balance = addOrUpdate * amount,
                            BinCode = "",
                            LocationId = order.LocationId,
                            ModifiedDate = date,
                            ProductId = item.ProductId
                        });
                }

                // Updating Destination Location Product Inventory
                var destinationLocationProductInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
                    m.ProductId == item.ProductId &&
                    m.LocationId == locationId);

                // if order is refund we add to inventory
                addOrUpdate = -1;
                amount = Math.Abs(item.Amount);
                if (order.Status == OrderStatus.Return.ToString() || item.Amount < 0)
                {
                    addOrUpdate = 1;
                }

                if (destinationLocationProductInventory != null)
                {
                    destinationLocationProductInventory.Balance = destinationLocationProductInventory.Balance + (addOrUpdate * amount);
                    destinationLocationProductInventory.ModifiedDate = date;
                }
                else
                {
                    _context.ProductInventory.Add(
                        new ProductInventory
                        {
                            Balance = addOrUpdate * amount,
                            BinCode = "",
                            LocationId = locationId,
                            ModifiedDate = date,
                            ProductId = item.ProductId
                        });
                }
            }
            return true;
        }

        private async Task<bool> AddToInventory(Order order, UpdateOrderStatus updateOrderStatus)
        {
            if (updateOrderStatus.OrderStatus != OrderStatus.Draft.ToString())
            {
                return true;
            }
            var date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
            foreach (var item in order.OrderDetail)
            {
                var productInventory = await _context.ProductInventory.FirstOrDefaultAsync(m =>
                    m.ProductId == item.ProductId &&
                    m.LocationId == order.LocationId);

                if (productInventory != null)
                {
                    productInventory.Balance = productInventory.Balance + item.Amount;
                    productInventory.ModifiedDate = date;
                }
                else
                {
                    _context.ProductInventory.Add(
                        new ProductInventory
                        {
                             Balance = item.Amount,
                             BinCode = "",
                             LocationId = order.LocationId,
                             ModifiedDate = date,
                             ProductId = item.ProductId                                 
                        });
                }
            }
            return true;
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }
    }
}