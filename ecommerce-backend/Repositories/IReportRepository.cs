using EcommerceApi.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IReportRepository
    {
        Task<IEnumerable<CurrentMonthSummaryViewModel>> CurrentMonthSummary(string userId);
        Task<IEnumerable<ChartRecordsViewModel>> MonthlySales(string userId);
        Task<IEnumerable<ChartRecordsViewModel>> MonthlyPurchases(string userId);
        Task<IEnumerable<ChartRecordsViewModel>> DailySales(string userId);
        Task<IEnumerable<ProductTypeSalesReportViewModel>> GetProductTypeSalesReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<ProductSalesReportViewModel>> GetProductSalesReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<ProductSalesDetailReportViewModel>> GetProductSalesDetailReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<SalesReportViewModel>> GetSalesReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<PaymentsReportViewModel>> GetPaymentsReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<PaymentsByPaymentTypeViewModel>> GetPaymentsByPaymentTypeReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<PaymentsTotalViewModel>> GetPaymentsTotalReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<PurchasesReportViewModel>> GetPurchasesReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<PurchasesDetailReportViewModel>> GetPurchasesDetailReport(DateTime fromDate, DateTime toDate, string userId);
        Task<IEnumerable<CustomerPaidOrdersViewModel>> GetCustomerPaidReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnPaidReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<CustomerPaidOrdersViewModel>> GetCustomerPaidReport(int customerId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnPaidReport(int customerId, DateTime fromDate, DateTime toDate);
    }
}
