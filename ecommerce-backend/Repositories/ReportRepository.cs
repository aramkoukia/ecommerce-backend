using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using EcommerceApi.ViewModel;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public ReportRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<CurrentMonthSummaryViewModel>> CurrentMonthSummary()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                SELECT 
                                (SELECT 
                                SUM(Total) 
                                FROM Purchase
                                WHERE PurchaseDate >= ''
                                AND PurchaseDate BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) AND DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)) AS MonthlyPurchases ,

                                (SELECT 
                                SUM(Total) 
                                FROM [Order]
                                WHERE [Order].Status IN ('Paid', 'Account') 
                                      AND [Order].OrderDate BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) AND DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)) AS MonthlyPaidAccountOrders,


                                (SELECT 
                                SUM(Total)
                                FROM [Order]
                                WHERE [Order].Status = 'Paid'
                                      AND [Order].OrderDate BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) AND DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)) AS MonthlyPaidOrders
                                 ";
                conn.Open();
                return await conn.QueryAsync<CurrentMonthSummaryViewModel>(query);
            }
        }

        public async Task<IEnumerable<ChartRecordsViewModel>> MonthlySales()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT * FROM (
SELECT A.Month AS Label, ISNULL(B.OrderTotal,0) / 1000 AS Value, Year
FROM (SELECT 1 AS MONTH
UNION 
SELECT 2 
UNION 
SELECT 3 
UNION 
SELECT 4 
UNION 
SELECT 5 
UNION 
SELECT 6 
UNION 
SELECT 7 
UNION 
SELECT 8 
UNION 
SELECT 9 
UNION 
SELECT 10 
UNION 
SELECT 11 
UNION 
SELECT 12 ) A 
LEFT JOIN (
	SELECT datepart(month,OrderDate) AS Month, datepart(year,OrderDate) AS Year, Sum(Total) as OrderTotal 
	FROM [Order] 
	WHERE Status IN ('Account', 'Paid')
	GROUP BY datepart(month,OrderDate), datepart(year,OrderDate)) B
ON A.month = B.month) t
Order By Year, Label
                                 ";
                conn.Open();
                return await conn.QueryAsync<ChartRecordsViewModel>(query);
            }
        }

        public async Task<IEnumerable<ChartRecordsViewModel>> MonthlyPurchases()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT * FROM (
SELECT A.Month AS Label, ISNULL(B.PurchaseTotal,0) / 1000 AS Value, Year
FROM (SELECT 1 AS MONTH
UNION 
SELECT 2 
UNION 
SELECT 3 
UNION 
SELECT 4 
UNION 
SELECT 5 
UNION 
SELECT 6 
UNION 
SELECT 7 
UNION 
SELECT 8 
UNION 
SELECT 9 
UNION 
SELECT 10 
UNION 
SELECT 11 
UNION 
SELECT 12 ) A 
LEFT JOIN (
	SELECT datepart(month,PurchaseDate) AS Month, datepart(year,PurchaseDate) AS Year, Sum(Total) as PurchaseTotal 
	FROM [Purchase] 
	GROUP BY datepart(month,PurchaseDate), datepart(year,PurchaseDate)) B
ON A.month = B.month) t
Order By Year, Label
                                 ";
                conn.Open();
                return await conn.QueryAsync<ChartRecordsViewModel>(query);
            }
        }

        public async Task<IEnumerable<ChartRecordsViewModel>> DailySales()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                    SELECT LEFT(WeekDayName, 2) As Label, ISNULL(Value, 0) / 1000 AS Value FROM (
                    SELECT 'Monday' AS WeekDayName, 1 AS SortOrder
                    UNION
                    SELECT 'Tuesday', 2 AS SortOrder
                    UNION
                    SELECT 'Wednesday', 3 AS SortOrder
                    UNION
                    SELECT 'Thursday', 4 AS SortOrder
                    UNION
                    SELECT 'Friday', 5 AS SortOrder
                    UNION
                    SELECT 'Saturday', 6 AS SortOrder
                    UNION
                    SELECT 'Sunday', 7 AS SortOrder) AS AllDays
                    LEFT JOIN (
                    SELECT DATENAME(dw,OrderDate) AS Label, SUM(Total) AS Value, CONVERT(date, OrderDate) AS ConvertedOrderDate
                    FROM [Order]
                    WHERE [Order].OrderDate > DATEADD(DAY, -7, GETDATE())
                    AND Status IN ('Paid', 'Account')
                    GROUP BY datename(dw,OrderDate), CONVERT(date ,OrderDate)) Sales
                    on AllDays.WeekDayName = Sales.Label
                    ORDER BY Sales.ConvertedOrderDate
                                 ";
                conn.Open();
                return await conn.QueryAsync<ChartRecordsViewModel>(query);
            }
        }

        public async Task<IEnumerable<ProductTypeSalesReportViewModel>> GetProductTypeSalesReport(DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductTypeName, SUM(ISNULL(VanTotalSales,0)) AS VanTotalSales, SUM(ISNULL(AbbTotalSales,0)) AS AbbTotalSales 
FROM Product
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
LEFT JOIN (SELECT SUM(OrderDetail.Total) AS VanTotalSales, ProductId 
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE [Order].LocationId = 1
                 AND OrderDate BETWEEN @fromDate AND @toDate
		   GROUP BY ProductId ) VanSales
	ON Product.ProductId = VanSales.ProductId
LEFT JOIN (SELECT SUM(OrderDetail.Total) AS AbbTotalSales, ProductId 
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE [Order].LocationId = 2
                  AND OrderDate BETWEEN @fromDate AND @toDate
		   GROUP BY ProductId ) AbbSales
ON Product.ProductId = AbbSales.ProductId
GROUP BY ProductTypeName
                                 ";
                conn.Open();
                return await conn.QueryAsync<ProductTypeSalesReportViewModel>(query, new { fromDate, toDate });
            }
        }

        public async Task<IEnumerable<ProductSalesReportViewModel>> GetProductSalesReport(DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductName, ProductCode, ProductTypeName, VanTotalSales, AbbTotalSales 
FROM Product
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
LEFT JOIN (SELECT SUM(OrderDetail.Total) AS VanTotalSales, ProductId 
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE [Order].LocationId = 1
                 AND OrderDate BETWEEN @fromDate AND @toDate
		   GROUP BY ProductId ) VanSales
	ON Product.ProductId = VanSales.ProductId
LEFT JOIN (SELECT SUM(OrderDetail.Total) AS AbbTotalSales, ProductId 
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE [Order].LocationId = 2
                 AND OrderDate BETWEEN @fromDate AND @toDate
		   GROUP BY ProductId ) AbbSales
ON Product.ProductId = AbbSales.ProductId
                                 ";
                conn.Open();
                return await conn.QueryAsync<ProductSalesReportViewModel>(query, new { fromDate, toDate });
            }
        }

        public async Task<IEnumerable<SalesReportViewModel>> GetSalesReport(DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                 ";
                conn.Open();
                return await conn.QueryAsync<SalesReportViewModel>(query, new { fromDate, toDate });
            }
        }

        public async Task<IEnumerable<PaymentsReportViewModel>> GetPaymentsReport(DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT Users.GivenName, PaymentTypeName, [Order].OrderId, Customer.CompanyName, [Order].Status, SUM(PaymentAmount) AS PaymentAmount
FROM OrderPayment
INNER JOIN PaymentType
	ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId 
INNER JOIN [Order]
	ON [Order].OrderId = OrderPayment.OrderId
LEFT JOIN Users
	ON Users.Id = OrderPayment.CreatedByUserId
WHERE OrderDate BETWEEN @fromDate AND @toDate
LEFT JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
GROUP BY PaymentTypeName, Users.GivenName, [Order].OrderId, Customer.CompanyName, [Order].Status
                                 ";
                conn.Open();
                return await conn.QueryAsync<PaymentsReportViewModel>(query, new { fromDate, toDate });
            }
        }

        public async Task<IEnumerable<PurchasesReportViewModel>> GetPurchasesReport(DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                 ";
                conn.Open();
                return await conn.QueryAsync<PurchasesReportViewModel>(query, new { fromDate, toDate });
            }
        }
    }
}
