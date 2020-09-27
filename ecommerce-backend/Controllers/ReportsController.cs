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
using DinkToPdf;
using DinkToPdf.Contracts;
using EcommerceApi.Untilities;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Reports")]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepository;
        private readonly IEmailSender _emailSender;
        private readonly IConverter _converter;
        private readonly UserManager<ApplicationUser> _userManager;
        public ReportsController(
            IReportRepository reportRepository,
            IEmailSender emailSender,
            IConverter converter,
            UserManager<ApplicationUser> userManager)
        {
            _emailSender = emailSender;
            _reportRepository = reportRepository;
            _userManager = userManager;
            _converter = converter;
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
            return await GetProductSalesReportData(fromDate, toDate);
        }

        [HttpGet("ProductSalesPdf")]
        public async Task<FileResult> GetProductSalesReportPdf(DateTime fromDate, DateTime toDate)
        {
            var data = await GetProductSalesReportData(fromDate, toDate);
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = $"Product Sales Report From Date:{fromDate.Date.ToShortDateString()} To Date:{toDate.Date.ToShortDateString()}",
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = ProductSalesReportGenerator.GetHtmlString(data, "Product Sales Report", fromDate, toDate),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);

            var result = new FileContentResult(file, "application/pdf")
            {
                FileDownloadName = $"Product Sales Report-From-{fromDate.Date.ToShortDateString().Replace('/', '-')}-To-{toDate.Date.ToShortDateString().Replace('/', '-')}.pdf"
            };
            return result;
        }

        private async Task<IEnumerable<ProductSalesReportViewModel>> GetProductSalesReportData(DateTime fromDate, DateTime toDate)
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
            return await GetProductSalesDetailReportData(fromDate, toDate);
        }

        private async Task<IEnumerable<ProductSalesDetailReportViewModel>> GetProductSalesDetailReportData(DateTime fromDate, DateTime toDate)
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
        public async Task<IEnumerable<SalesReportViewModel>> GetSalesReport(DateTime fromDate, DateTime toDate) =>
            await GetSalesReportData(fromDate, toDate);

        private async Task<IEnumerable<SalesReportViewModel>> GetSalesReportData(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
            {
                fromDate = DateTime.Now;
            }

            if (toDate == DateTime.MinValue)
            {
                toDate = DateTime.Now;
            }
            else
            {
                toDate = toDate.AddDays(1).AddTicks(-1);
            }

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _reportRepository.GetSalesReport(fromDate, toDate, user.Id);
        }

        [HttpGet("SalesPdf")]
        public async Task<FileResult> GetSalesReportPdf(DateTime fromDate, DateTime toDate)
        {
            var data = await GetSalesReportData(fromDate, toDate);
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = $"Sales Report From Date:{fromDate.Date.ToShortDateString()} To Date:{toDate.Date.ToShortDateString()}",
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = SalesReportGenerator.GetHtmlString(data, "Sales Report", fromDate, toDate),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);

            var result = new FileContentResult(file, "application/pdf")
            {
                FileDownloadName = $"Sales Report-From-{fromDate.Date.ToShortDateString().Replace('/', '-')}-To-{toDate.Date.ToShortDateString().Replace('/', '-')}.pdf"
            };
            return result;
        }

        [HttpGet("PaymentsByPaymentType")]
        public async Task<IEnumerable<PaymentsByPaymentTypeViewModel>> GetPaymentsByPaymentTypeReport(DateTime fromDate, DateTime toDate)
        {
            return await GetPaymentsByPaymentTypeData(fromDate, toDate);
        }

        private async Task<IEnumerable<PaymentsByPaymentTypeViewModel>> GetPaymentsByPaymentTypeData(DateTime fromDate, DateTime toDate)
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
            return await GetPaymentsTotalData(fromDate, toDate);
        }

        private async Task<IEnumerable<PaymentsTotalViewModel>> GetPaymentsTotalData(DateTime fromDate, DateTime toDate)
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
            return await GetPaymentReportData(fromDate, toDate);
        }

        [HttpGet("PaymentsPdf")]
        public async Task<FileResult> GetPaymentsReportPdf(DateTime fromDate, DateTime toDate)
        {
            var data3 = await GetPaymentReportData(fromDate, toDate);
            var data1 = await GetPaymentsTotalReport(fromDate, toDate);
            var data2 = await GetPaymentsByPaymentTypeData(fromDate, toDate);
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = $"Sales Report From Date:{fromDate.Date.ToShortDateString()} To Date:{toDate.Date.ToShortDateString()}",
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = PaymentsReportGenerator.GetHtmlString(
                    data1, "Payment Summary",
                    data2, "Payment By Order Status",
                    data3, "Payment Details",
                    fromDate, toDate),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "invoice.css") },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);

            var result = new FileContentResult(file, "application/pdf")
            {
                FileDownloadName = $"Payment Report-From-{fromDate.Date.ToShortDateString().Replace('/', '-')}-To-{toDate.Date.ToShortDateString().Replace('/', '-')}.pdf"
            };
            return result;
        }

        private async Task<IEnumerable<PaymentsReportViewModel>> GetPaymentReportData(DateTime fromDate, DateTime toDate)
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
            return await GetPurchaseReportData(fromDate, toDate);
        }

        private async Task<IEnumerable<PurchasesReportViewModel>> GetPurchaseReportData(DateTime fromDate, DateTime toDate)
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
            return await GetCustomerPaidReportData(fromDate, toDate);
        }

        private async Task<IEnumerable<CustomerPaidOrdersViewModel>> GetCustomerPaidReportData(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            return await _reportRepository.GetCustomerPaidReport(fromDate, toDate);
        }

        [HttpGet("CustomerUnPaid")]
        public async Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnPaidReport(DateTime fromDate, DateTime toDate)
        {
            return await GetCustomerUnpaidReportData(fromDate, toDate);
        }

        private async Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnpaidReportData(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

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