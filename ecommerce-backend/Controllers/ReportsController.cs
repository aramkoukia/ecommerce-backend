using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceApi.ViewModel;
using EcommerceApi.Repositories;
using System;
using Microsoft.AspNetCore.Identity;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Reports")]
    public class ReportsController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IReportRepository _reportRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public ReportsController(
            EcommerceContext context, 
            IReportRepository reportRepository,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
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
        public async Task<IEnumerable<ProductProfitReportViewModel>> GetProductProfitReport(DateTime salesFromDate,
                                                                                            DateTime salesToDate,
                                                                                            DateTime purchaseFromDate,
                                                                                            DateTime purchaseToDate)
        {
            if (salesFromDate == DateTime.MinValue)
                salesFromDate = DateTime.Now.AddYears(-1);
            if (salesToDate == DateTime.MinValue)
                salesToDate = DateTime.Now;
            else
                salesToDate = salesToDate.AddDays(1).AddTicks(-1);

            if (purchaseFromDate == DateTime.MinValue)
                purchaseFromDate = DateTime.Now.AddYears(-1);
            if (purchaseToDate == DateTime.MinValue)
                purchaseToDate = DateTime.Now;
            else
                purchaseToDate = purchaseToDate.AddDays(1).AddTicks(-1);


            return await _reportRepository.GetProductProfitReport(salesFromDate, salesToDate, purchaseFromDate, purchaseToDate);
        }

        [HttpGet("InventoryValueProfit")]
        public async Task<IEnumerable<InventoryValueReportViewModel>> GetInventoryValueProfit()
        {
            return await _reportRepository.GetInventoryValueProfit();
        }
    }
}