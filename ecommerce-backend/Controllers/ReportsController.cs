using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceApi.ViewModel;
using EcommerceApi.Repositories;
using System;
using Microsoft.AspNetCore.Identity;
using EcommerceApi.Services;
using CsvHelper;
using System.IO;
using System.Globalization;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Reports")]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepository;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        public ReportsController(
            IReportRepository reportRepository,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager)
        {
            _emailSender = emailSender;
            _reportRepository = reportRepository;
            _userManager = userManager;
        }

        // GET: api/Reports/MonthlySummary
        [HttpGet("MonthlySummary")]
        public async Task<IEnumerable<CurrentMonthSummaryViewModel>> GetMonthlySummary()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);
            return await _reportRepository.CurrentMonthSummary(user.Id);
        }

        [HttpGet("MonthlySales")]
        public async Task<IEnumerable<ChartRecordsViewModel>> GetMonthlySales()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);
            return await _reportRepository.MonthlySales(user.Id);
        }

        [HttpGet("MonthlyPurchases")]
        public async Task<IEnumerable<ChartRecordsViewModel>> GetMonthlyPurchases()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);
            return await _reportRepository.MonthlyPurchases(user.Id);
        }

        [HttpGet("DailySales")]
        public async Task<IEnumerable<ChartRecordsViewModel>> GetDailySales()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);
            return await _reportRepository.DailySales(user.Id);
        }

        [HttpGet("ProductSales")]
        public async Task<IEnumerable<ProductSalesReportViewModel>> GetProductSalesReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else 
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetProductSalesReport(fromDate, toDate, user.Id);
        }

        [HttpGet("ProductSalesDetail")]
        public async Task<IEnumerable<ProductSalesDetailReportViewModel>> GetProductSalesDetailReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetProductSalesDetailReport(fromDate, toDate, user.Id);
        }

        [HttpGet("ProductTypeSales")]
        public async Task<IEnumerable<ProductTypeSalesReportViewModel>> GetProductTypeSalesReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetProductTypeSalesReport(fromDate, toDate, user.Id);
        }

        [HttpGet("Sales")]
        public async Task<IEnumerable<SalesReportViewModel>> GetSalesReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetSalesReport(fromDate, toDate, user.Id);
        }

        [HttpGet("PaymentsByPaymentType")]
        public async Task<IEnumerable<PaymentsByPaymentTypeViewModel>> GetPaymentsByPaymentTypeReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetPaymentsByPaymentTypeReport(fromDate, toDate, user.Id);
        }

        [HttpGet("PaymentsTotal")]
        public async Task<IEnumerable<PaymentsTotalViewModel>> GetPaymentsTotalReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetPaymentsTotalReport(fromDate, toDate, user.Id);
        }

        [HttpGet("Payments")]
        public async Task<IEnumerable<PaymentsReportViewModel>> GetPaymentsReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetPaymentsReport(fromDate, toDate, user.Id);
        }

        [HttpGet("PurchaseSummary")]
        public async Task<IEnumerable<PurchasesReportViewModel>> GetPurchaseReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            // System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            // var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetPurchasesReport(fromDate, toDate, "");
        }

        [HttpGet("PurchaseDetail")]
        public async Task<IEnumerable<PurchasesDetailReportViewModel>> GetPurchasesDetailReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            // System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            // var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetPurchasesDetailReport(fromDate, toDate, "");
        }

        [HttpGet("CustomerPaid")]
        public async Task<IEnumerable<CustomerPaidOrdersViewModel>> GetCustomerPaidReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);

            return await _reportRepository.GetCustomerPaidReport(fromDate, toDate);
        }

        [HttpGet("CustomerUnPaid")]
        public async Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnPaidReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var userId = _userManager.GetUserId(User);
            return await _reportRepository.GetCustomerUnPaidReport(fromDate, toDate);
        }

        [HttpGet("SalesForcast")]
        public async Task<IEnumerable<SalesForecastReportViewModel>> GetSalesForecastReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            return await _reportRepository.GetSalesForecastReport(fromDate, toDate);
        }

        [HttpGet("PurchaseProfit")]
        public async Task<IEnumerable<ProductProfitReportViewModel>> GetProductProfitReport(DateTime fromSalesDate,
                                                                                            DateTime toSalesDate,
                                                                                            DateTime fromPurchaseDate,
                                                                                            DateTime toPurchaseDate)
        {
            if (fromSalesDate == DateTime.MinValue)
                fromSalesDate = DateTime.Now.AddYears(-1);
            if (toSalesDate == DateTime.MinValue)
                toSalesDate = DateTime.Now;
            else
                toSalesDate = toSalesDate.AddDays(1).AddTicks(-1);

            if (fromPurchaseDate == DateTime.MinValue)
                fromPurchaseDate = DateTime.Now.AddYears(-1);
            if (toPurchaseDate == DateTime.MinValue)
                toPurchaseDate = DateTime.Now;
            else
                toPurchaseDate = toPurchaseDate.AddDays(1).AddTicks(-1);


            return await _reportRepository.GetProductProfitReport(fromSalesDate, toSalesDate, fromPurchaseDate, toPurchaseDate);
        }

        [HttpGet("SalesByPurchasePrice")]
        public async Task<IEnumerable<SalesByPurchasePriceReportViewModel>> GetSalesByPurchasePriceReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetSalesByPurchasePriceReport(fromDate, toDate, user.Id);
        }

        [HttpGet("SalesByPurchasePriceDetail")]
        public async Task<IEnumerable<SalesByPurchasePriceDetailReportViewModel>> GetSalesByPurchasePriceDetailReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetSalesByPurchasePriceDetailReport(fromDate, toDate, user.Id);
        }

        [HttpGet("InventoryValue")]
        public async Task<IEnumerable<InventoryValueReportViewModel>> GetInventoryValue()
        {
            return await _reportRepository.GetInventoryValue();
        }

        [HttpGet("InventoryValueTotal")]
        public async Task<IEnumerable<InventoryValueTotalReportViewModel>> GetInventoryValueTotal()
        {
            return await _reportRepository.GetInventoryValueTotal();
        }

        [HttpGet("InventoryValueTotalByCategory")]
        public async Task<IEnumerable<InventoryValueTotalByCategoryReportViewModel>> GetInventoryValueTotalByCategory()
        {
            return await _reportRepository.GetInventoryValueTotalByCategory();
        }

        [HttpGet("MonthlyInventoryValue")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMonthlyInventoryValue()
        {
            var date = DateTime.Now;
            var result1 = await _reportRepository.GetInventoryValueTotal();
            var result2 = await _reportRepository.GetInventoryValueTotalByCategory();
            var result3 = await _reportRepository.GetInventoryValue(); ;
            MemoryStream attachment1;
            MemoryStream attachment2;
            MemoryStream attachment3;
            var attachmentName1 = $"Total Inventory Value - {date.ToString("MMMM")} {date.Year}.csv";
            var attachmentName2 = $"Inventory Value By Category - {date.ToString("MMMM")} {date.Year}.csv";
            var attachmentName3 = $"Inventory Value By Product - {date.ToString("MMMM")} {date.Year}.csv";

            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(result1);
                writer.Flush();
                attachment1 = new MemoryStream(mem.ToArray());
            }

            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(result2);
                writer.Flush();
                attachment2 = new MemoryStream(mem.ToArray());
            }

            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(result3);
                attachment3 = new MemoryStream(mem.ToArray());
            }

            var subject = $"LED Lights and Parts - Monthly Inventory Value: {date.ToString("MMMM")} {date.Year}";
            var message = $"Monthly Inventory Value Report {date.ToString("MMMM")} {date.Year}";

            _emailSender.SendEmailAsync(
                "aramkoukia@gmail.com",
                subject,
                message,
                new[] { attachment1, attachment2, attachment3 },
                new[] { attachmentName1, attachmentName2, attachmentName3 },
                true);
            return Ok();
        }
    }
}