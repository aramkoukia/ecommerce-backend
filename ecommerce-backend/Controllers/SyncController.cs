using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceApi.Models;
using EcommerceApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using EcommerceApi.Services;
using System.Diagnostics;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    public class SyncController : Controller
    {
        private AppDb _db;
        private readonly EcommerceContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public SyncController(EcommerceContext context, AppDb db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _db = db;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet("Products")]
        public async Task<IActionResult> SyncProducts()
        {
            var timeElapped = 0;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var errorList = new List<string>();
            var productsCreated = 0;
            var productsUpdated = 0;
            try
            {
                if (_db.Connection.State == System.Data.ConnectionState.Closed)
                {
                    await _db.Connection.OpenAsync();
                }

                var query = new ProductQueries(_db);
                var products = await query.GetAllProducts();
                foreach (var product in products)
                {
                    try
                    {
                        decimal price = 0;
                        if (!string.IsNullOrEmpty(product._price))
                        {
                            decimal.TryParse(product._price, out price);
                        }
                        int typeId = 0;
                        if (!string.IsNullOrEmpty(product._cat_id))
                        {
                            int.TryParse(product._cat_id, out typeId);
                        }

                        var found = await _context.Product.FindAsync(int.Parse(product.id.ToString()));
                        if (found == null)
                        {
                            productsCreated++;
                            var newProduct = new Product
                            {
                                AllowOutOfStockPurchase = true,
                                Barcode = product._sku,
                                ChargeTaxes = true,
                                ModifiedDate = DateTime.Now,
                                ProductCode = product._sku,
                                ProductDescription = "",
                                ProductId = int.Parse(product.id.ToString()),
                                ProductName = product.post_title,
                                ProductTypeId = typeId,
                                PurchasePrice = 0,
                                Sku = product._sku,
                                SalesPrice = price
                            };

                            if (typeId > 0)
                            {
                                var existingProductType = await _context.ProductType.FindAsync(typeId);
                                if (existingProductType == null)
                                {
                                    newProduct.ProductType = new ProductType
                                    {
                                        ProductTypeId = typeId,
                                        ModifiedDate = DateTime.Now,
                                        ProductTypeName = product._category
                                    };
                                }
                            }
                            else
                            {
                                newProduct.ProductType = null;
                            }
                            await _context.Product.AddAsync(newProduct);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            productsUpdated++;
                            found.ModifiedDate = DateTime.Now;
                            found.ProductName = product.post_title;
                            found.ProductTypeId = typeId;
                            found.SalesPrice = price;
                            await _context.SaveChangesAsync();
                        }

                    }
                    catch (Exception e)
                    {
                        errorList.Add("products error:" + e.ToString());
                    }
                }
                _db.Connection.Close();
            }
            catch (Exception ex)
            {
                errorList.Add("products error:" + ex.ToString());
            }
            stopWatch.Stop();
            var timeTook = $"Products Sync Took: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).Minutes} minutes.";
            var message = $"Products Sync Finished. \n Products Created: {productsCreated}. \n Products Updated: {productsUpdated}. {timeTook}\n Errors: {string.Join(",", errorList)}";
            await _emailSender.SendEmailAsync("aramkoukia@gmail.com", "Sync Finished: Products", message, null, null, null, true);
            
            return Ok(message);
        }

        //[HttpGet("ProductsInventory")]
        //public async Task<IActionResult> SyncProductsInventory()
        //{
        //    var errorList = new List<string>();
        //    try
        //    {
        //        if (_db.Connection.State == System.Data.ConnectionState.Closed)
        //        {
        //            await _db.Connection.OpenAsync();
        //        }

        //        var query = new ProductQueries(_db);
        //        var products = await query.GetAllProductInventories();
        //        foreach (var product in products)
        //        {
        //            var found = _context.ProductInventory.FirstOrDefault(p => p.LocationId == int.Parse(product.warehouse_id.ToString()) && p.ProductId == int.Parse(product.product_id.ToString()));
        //            if (found == null)
        //            {
        //                var productExists = await _context.Product.FindAsync(int.Parse(product.product_id.ToString()));
        //                if (productExists != null)
        //                {
        //                    var newProduct = new ProductInventory
        //                    {
        //                        ProductId = int.Parse(product.product_id.ToString()),
        //                        Balance = string.IsNullOrEmpty(product.stock) ? 0 : decimal.Parse(product.stock),
        //                        BinCode = "",
        //                        LocationId = int.Parse(product.warehouse_id.ToString()),
        //                        ModifiedDate = DateTime.Now,
        //                    };
        //                    await _context.ProductInventory.AddAsync(newProduct);
        //                    await _context.SaveChangesAsync();
        //                }
        //            }
        //            else
        //            {
        //                found.Balance = string.IsNullOrEmpty(product.stock) ? 0 : decimal.Parse(product.stock);
        //                await _context.SaveChangesAsync();
        //            }
        //        }
        //        _db.Connection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        errorList.Add("order taxes:" + ex.ToString());
        //    }

        //    await _emailSender.SendEmailAsync("aramkoukia@gmail.com", "Sync Finished: Products Inventory", $"Sync Finished: Products Inventory. {string.Join(",", errorList)}");
        //    return Ok(errorList);
        //}

        //[HttpGet("Customers")]
        //public async Task<IActionResult> SyncCustomers()
        //{
        //    var errorList = new List<string>();
        //    var customersCreated = 0;
        //    var customersUpdated = 0;
        //    try
        //    {
        //        if (_db.Connection.State == System.Data.ConnectionState.Closed)
        //        {
        //            await _db.Connection.OpenAsync();
        //        }

        //        var query = new CustomerQueries(_db);
        //        var customers = await query.GetAllCustomers();
        //        foreach (var customer in customers)
        //        {
        //            var limit = 0;
        //            int.TryParse(customer._pos_customer_accountlimit, out limit);

        //            var found = await _context.Customer.FindAsync(int.Parse(customer.id.ToString()));
        //            if (found == null)
        //            {
        //                customersCreated++;
        //                var newCustomer = new Customer
        //                {
        //                    Address = customer._pos_customer_address,
        //                    City = customer._pos_customer_city,
        //                    CompanyName = customer._pos_customer_company_name,
        //                    Country = customer._pos_customer_country,
        //                    CreditLimit = limit,
        //                    CustomerCode = customer.id.ToString(),
        //                    CustomerId = int.Parse(customer.id.ToString()),
        //                    Email = customer._pos_customer_email,
        //                    FirstName = customer._pos_customer_first_name,
        //                    LastName = customer._pos_customer_last_name,
        //                    Mobile = customer._pos_customer_mobile,
        //                    PhoneNumber = customer._pos_customer_phone,
        //                    PostalCode = customer._pos_customer_postal_code,
        //                    Province = customer._pos_customer_province,
        //                    PstNumber = customer._pos_customer_pst_number,
        //                    Status = "",
        //                    UserName = customer._pos_customer_email,
        //                    Website = customer._pos_customer_contractorlink
        //                };
        //                await _context.Customer.AddAsync(newCustomer);
        //                await _context.SaveChangesAsync();
        //            }
        //            else
        //            {
        //                customersUpdated++;
        //                found.Address = customer._pos_customer_address;
        //                found.City = customer._pos_customer_city;
        //                found.CompanyName = customer._pos_customer_company_name;
        //                found.Country = customer._pos_customer_country;
        //                found.CreditLimit = limit;
        //                found.CustomerCode = customer.id.ToString();
        //                // found.CustomerId = int.Parse(customer.id.ToString());
        //                found.Email = customer._pos_customer_email;
        //                found.FirstName = customer._pos_customer_first_name;
        //                found.LastName = customer._pos_customer_last_name;
        //                found.Mobile = customer._pos_customer_mobile;
        //                found.PhoneNumber = customer._pos_customer_phone;
        //                found.PostalCode = customer._pos_customer_postal_code;
        //                found.Province = customer._pos_customer_province;
        //                found.PstNumber = customer._pos_customer_pst_number;
        //                found.Status = "";
        //                found.UserName = customer._pos_customer_email;
        //                found.Website = customer._pos_customer_contractorlink;
        //                await _context.SaveChangesAsync();
        //            }
        //        }
        //        _db.Connection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        errorList.Add("order taxes:" + ex.ToString());
        //    }

        //    await _emailSender.SendEmailAsync("aramkoukia@gmail.com", "Sync Finished: Customers", $"Sync Finished: Customers.  \n Customers Created: {customersCreated}. \n Customers Updated: {customersUpdated}. \n Errors: {string.Join(",", errorList)}");

        //    return Ok(errorList);
        //}

        [HttpGet("Orders")]
        public async Task<IActionResult> SyncOrders()
        {
            var errorList = new List<string>();
            try
            {
                if (_db.Connection.State == System.Data.ConnectionState.Closed)
                {
                    await _db.Connection.OpenAsync();
                }

                var query = new OrderQueries(_db);
                var orders = await query.GetAllOrders();
                foreach (var order in orders)
                {
                    //var limit = 0;
                    //int.TryParse(customer._pos_customer_accountlimit, out limit);
                    int customerId = 0;
                    if (!string.IsNullOrEmpty(order._pos_sale_customer_id))
                    {
                        int.TryParse(order._pos_sale_customer_id, out customerId);
                    }
                    decimal discount = 0;
                    if (!string.IsNullOrEmpty(order._pos_sale_discount))
                    {
                        decimal.TryParse(order._pos_sale_discount, out discount);
                    }
                    decimal total = 0;
                    if (!string.IsNullOrEmpty(order._pos_sale_total))
                    {
                        decimal.TryParse(order._pos_sale_total, out total);
                    }
                    decimal subtotal = 0;
                    if (!string.IsNullOrEmpty(order._pos_sale_subtotal))
                    {
                        decimal.TryParse(order._pos_sale_subtotal, out subtotal);
                    }
                    var location = 1;
                    if (!string.IsNullOrEmpty(order._pos_location_name))
                    {
                        location = order._pos_location_name.Equals("abbotsford", StringComparison.InvariantCultureIgnoreCase) ? 2 : 1;
                    }

                    var found = await _context.Order.FindAsync(int.Parse(order.id.ToString()));
                    if (found == null)
                    {

                        var newOrder = new Order
                        {
                            CreatedByUserId = order._pos_sale_user,
                            CreatedDate = order.post_date,
                            CustomerId = customerId,
                            TotalDiscount = discount,
                            LocationId = location,
                            Notes = order._pos_sale_note,
                            OrderDate = order.post_date,
                            OriginalOrderId = 0,// order._pos_linkedInvoice,
                            PoNumber = order._pos_po_number,
                            PstNumber = "",
                            Status = order._pos_status,
                            SubTotal = subtotal,
                            OrderId = int.Parse(order.id.ToString()),
                            Total = total
                        };
                        newOrder.Customer = null;
                        newOrder.Location = null;
                        await _context.Order.AddAsync(newOrder);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // await _context.SaveChangesAsync();
                    }
                }
                _db.Connection.Close();
            }
            catch (Exception ex)
            {
                errorList.Add("order taxes:" + ex.ToString());
            }

            return Ok(errorList);
        }

        [HttpGet("Users")]
        public async Task<IActionResult> SyncUsers()
        {
            var errorList = new List<string>();
            try
            {
                if (_db.Connection.State == System.Data.ConnectionState.Closed)
                {
                    await _db.Connection.OpenAsync();
                }

                var query = new UserQueries(_db);
                var users = await query.GetAllUsers();
                foreach (var user in users)
                {
                    var givenName = string.IsNullOrWhiteSpace(user.first_name + " " + user.last_name) ? user.user_login : user.first_name + " " + user.last_name;
                    if (await _userManager.FindByEmailAsync(user.user_email) == null)
                    {
                        var u = new ApplicationUser
                        {
                            UserName = user.user_login,
                            Email = user.user_email,
                            EmailConfirmed = true,
                            GivenName = givenName
                        };

                        await _userManager.CreateAsync(u, "P2ssw0rd!");
                    }
                }
                _db.Connection.Close();
            }
            catch (Exception ex)
            {
                errorList.Add("order taxes:" + ex.ToString());
            }

            return Ok(errorList);
        }

        [HttpGet("OrderTaxes")]
        public async Task<IActionResult> SyncOrderTaxes()
        {
            var errorList = new List<string>();
            try
            {
                if (_db.Connection.State == System.Data.ConnectionState.Closed)
                {
                    await _db.Connection.OpenAsync();
                }

                var query = new OrderQueries(_db);
                var orders = await query.GetAllOrders();
                foreach (var order in orders)
                {
                    try
                    {
                        var orderId = int.Parse(order.id.ToString());
                        if (orderId >= 75746)
                        {
                            continue;
                        }

                        decimal taxGst = 0;
                        if (!string.IsNullOrEmpty(order._pos_sale_tax_GST))
                        {
                            decimal.TryParse(order._pos_sale_tax_GST, out taxGst);
                        }
                        decimal taxPst = 0;
                        if (!string.IsNullOrEmpty(order._pos_sale_tax_PST))
                        {
                            decimal.TryParse(order._pos_sale_tax_PST, out taxPst);
                        }

                        var found = await _context.OrderTax.FindAsync(int.Parse(order.id.ToString()));
                        if (found == null)
                        {
                            if (taxPst > 0)
                            {
                                var orderTaxGst = new OrderTax
                                {
                                    OrderId = int.Parse(order.id.ToString()),
                                    TaxAmount = taxGst,
                                    TaxId = 1
                                };
                                await _context.OrderTax.AddAsync(orderTaxGst);
                            }
                            if (taxPst > 0)
                            {
                                var orderTaxPst = new OrderTax
                                {
                                    OrderId = int.Parse(order.id.ToString()),
                                    TaxAmount = taxPst,
                                    TaxId = 2
                                };
                                await _context.OrderTax.AddAsync(orderTaxPst);
                            }
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            // await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        errorList.Add("order taxes:" + e.ToString());
                    }

                }
                _db.Connection.Close();
            }
            catch (Exception ex)
            {
                errorList.Add("order taxes:" + ex.ToString());
            }

            return Ok(errorList);
        }

        [HttpGet("OrderPayments")]
        public async Task<IActionResult> SyncOrderPayments()
        {
            var errorList = new List<string>();
            try
            {
                if (_db.Connection.State == System.Data.ConnectionState.Closed)
                {
                    await _db.Connection.OpenAsync();
                }

                var query = new OrderQueries(_db);
                var orders = await query.GetAllOrderPayments();
                foreach (var order in orders)
                {
                    try
                    {
                        var orderId = int.Parse(order._pos_payment_order_id.ToString());
                        if (orderId >= 75746)
                        {
                            continue;
                        }

                        //var found = _context.OrderPayment.FirstOrDefault(o => o.OrderId == int.Parse(order._pos_payment_order_id.ToString())
                        //                                          && o.PaymentAmount == decimal.Parse(order._pos_payment_amount.ToString())
                        //                                          && o.AuthCode == order._pos_payment_AuthCode.ToString());
                        //if (found == null)
                        //{
                        var newOrder = new OrderPayment
                        {
                            CreatedByUserId = order.post_author.ToString(),
                            CreatedDate = order.post_date,
                            CreditCard = order._pos_payment_lastFour,
                            OrderId = orderId,
                            PaymentAmount = decimal.Parse(order._pos_payment_amount),
                            PaymentDate = order.post_date,
                            PaymentTypeId = int.Parse(order._pos_payment_paymentType_id.ToString()),
                            AuthCode = order._pos_payment_AuthCode,
                            Notes = order._pos_payment_comment,
                            ChequeNo = order._pos_payment_chequeNo
                        };
                        newOrder.PaymentType = null;
                        await _context.OrderPayment.AddAsync(newOrder);
                        await _context.SaveChangesAsync();
                        //}
                        //else
                        //{
                        //    // await _context.SaveChangesAsync();
                        //}
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                _db.Connection.Close();
            }
            catch (Exception ex)
            {
                errorList.Add("order taxes:" + ex.ToString());
            }

            return Ok(errorList);
        }

        [HttpGet("OrderDetails")]
        public async Task<IActionResult> SyncOrderLineItems()
        {
            var errorList = new List<string>();
            try
            {
                if (_db.Connection.State == System.Data.ConnectionState.Closed)
                {
                    await _db.Connection.OpenAsync();
                }

                var query = new OrderQueries(_db);
                var orders = await query.GetAllOrderDetails();
                foreach (var order in orders)
                {
                    try
                    {
                        var orderId = int.Parse(order.order_id.ToString());
                        var productId = int.Parse(order._product_id);
                        if (orderId >= 75746
                            || !_context.Product.Where(o => o.ProductId == productId).Any()
                            || !_context.Order.Where(o=>o.OrderId == orderId).Any())
                        {
                            continue;
                        }

                        var newOrder = new OrderDetail
                        {
                            Amount = decimal.Parse(order._qty),
                            ProductId = productId,
                            SubTotal = string.IsNullOrEmpty(order._line_subtotal) ? 0 : decimal.Parse(order._line_subtotal),
                            Total = string.IsNullOrEmpty(order._line_total) ? 0 : decimal.Parse(order._line_total),
                            UnitPrice = string.IsNullOrEmpty(order._line_price) ? 0 : decimal.Parse(order._line_price),
                            OrderId = orderId
                        };
                        newOrder.Product = null;
                        newOrder.DiscountAmount = 0;
                        newOrder.TotalDiscount = 0;
                        await _context.OrderDetail.AddAsync(newOrder);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        errorList.Add("order line items:" + ex.ToString());
                    }
                }
                _db.Connection.Close();
            }
            catch (Exception ex)
            {
                errorList.Add("order line items:" + ex.ToString());
            }

            return Ok(errorList);
        }
    }
}
