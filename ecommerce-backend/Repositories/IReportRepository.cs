using EcommerceApi.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public interface IReportRepository
    {
        Task<IEnumerable<CurrentMonthSummaryViewModel>> CurrentMonthSummary();
        Task<IEnumerable<ChartRecordsViewModel>> MonthlySales();
        Task<IEnumerable<ChartRecordsViewModel>> MonthlyPurchases();
        Task<IEnumerable<ChartRecordsViewModel>> DailySales();
        Task<IEnumerable<ProductTypeSalesReportViewModel>> GetProductTypeSalesReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<ProductSalesReportViewModel>> GetProductSalesReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<SalesReportViewModel>> GetSalesReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<PaymentsReportViewModel>> GetPaymentsReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<PaymentsByPaymentTypeViewModel>> GetPaymentsByPaymentTypeReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<PaymentsTotalViewModel>> GetPaymentsTotalReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<PurchasesReportViewModel>> GetPurchasesReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<CustomerPaidOrdersViewModel>> GetCustomerPaidReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnPaidReport(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<CustomerPaidOrdersViewModel>> GetCustomerPaidReport(int customerId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnPaidReport(int customerId, DateTime fromDate, DateTime toDate);
    }
}
