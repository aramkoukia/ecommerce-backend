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
            return await _reportRepository.GetProductSalesReport(fromDate, toDate);
        }

        [HttpGet("ProductTypeSales")]
        public async Task<IEnumerable<ProductTypeSalesReportViewModel>> GetProductTypeSalesReport(DateTime fromDate, DateTime toDate)
        {
            return await _reportRepository.GetProductTypeSalesReport(fromDate, toDate);
        }

        [HttpGet("Sales")]
        public async Task<IEnumerable<ProductTypeSalesReportViewModel>> GetSalesReport(DateTime fromDate, DateTime toDate)
        {
            return await _reportRepository.GetProductTypeSalesReport(fromDate, toDate);
        }

        [HttpGet("Payments")]
        public async Task<IEnumerable<ProductTypeSalesReportViewModel>> GetPaymentsReport(DateTime fromDate, DateTime toDate)
        {
            return await _reportRepository.GetProductTypeSalesReport(fromDate, toDate);
        }

        [HttpGet("Purchases")]
        public async Task<IEnumerable<ProductTypeSalesReportViewModel>> GetPurchasesReport(DateTime fromDate, DateTime toDate)
        {
            return await _reportRepository.GetProductTypeSalesReport(fromDate, toDate);
        }

        [AllowAnonymous]
        [HttpGet("Ping")]
        public string Ping()
        {
            return "Ping";
        }
    }
}