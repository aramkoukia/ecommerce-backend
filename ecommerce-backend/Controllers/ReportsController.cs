using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceApi.ViewModel;
using EcommerceApi.Repositories;
using System;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Reports")]
    public class ReportsController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IReportRepository _reportRepository;

        public ReportsController(EcommerceContext context, IReportRepository reportRepository)
        {
            _context = context;
            _reportRepository = reportRepository;
        }

        // GET: api/Reports/MonthlySummary
        [HttpGet("MonthlySummary")]
        public async Task<IEnumerable<CurrentMonthSummaryViewModel>> GetMonthlySummary()
        {
            return await _reportRepository.CurrentMonthSummary();
        }

        [HttpGet("MonthlySales")]
        public async Task<IEnumerable<ChartRecordsViewModel>> GetMonthlySales()
        {
            return await _reportRepository.MonthlySales();
        }

        [HttpGet("MonthlyPurchases")]
        public async Task<IEnumerable<ChartRecordsViewModel>> GetMonthlyPurchases()
        {
            return await _reportRepository.MonthlyPurchases();
        }

        [HttpGet("DailySales")]
        public async Task<IEnumerable<ChartRecordsViewModel>> GetDailySales()
        {
            return await _reportRepository.DailySales();
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

            return await _reportRepository.GetProductSalesReport(fromDate, toDate);
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

            return await _reportRepository.GetProductSalesDetailReport(fromDate, toDate);
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

            return await _reportRepository.GetProductTypeSalesReport(fromDate, toDate);
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

            return await _reportRepository.GetSalesReport(fromDate, toDate);
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

            return await _reportRepository.GetPaymentsByPaymentTypeReport(fromDate, toDate);
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

            return await _reportRepository.GetPaymentsTotalReport(fromDate, toDate);
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

            return await _reportRepository.GetPaymentsReport(fromDate, toDate);
        }

        [HttpGet("Purchases")]
        public async Task<IEnumerable<PurchasesReportViewModel>> GetPurchasesReport(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);

            return await _reportRepository.GetPurchasesReport(fromDate, toDate);
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

            return await _reportRepository.GetCustomerUnPaidReport(fromDate, toDate);
        }

        [AllowAnonymous]
        [HttpGet("Ping")]
        public string Ping()
        {
            return "Ping";
        }
    }
}