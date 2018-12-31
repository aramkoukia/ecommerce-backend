using EcommerceApi.ViewModel;
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

    }
}
