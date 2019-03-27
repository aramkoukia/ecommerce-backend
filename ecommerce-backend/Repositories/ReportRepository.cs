using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

        public async Task<IEnumerable<CurrentMonthSummaryViewModel>> CurrentMonthSummary(string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                SELECT 
                                (SELECT 
                                FORMAT(SUM(Total), 'N2') 
                                FROM Purchase
                                WHERE PurchaseDate >= ''
                                AND PurchaseDate BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) AND DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)) AS MonthlyPurchases ,

                                (SELECT 
                                FORMAT(SUM(Total), 'N2')
                                FROM [Order]
                                WHERE [Order].Status IN ('Paid', 'Account') 
                                      AND LocationId IN @locationIds
                                      AND [Order].OrderDate BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) AND DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)) AS MonthlyPaidAccountOrders,


                                (SELECT 
                                FORMAT(SUM(Total), 'N2')
                                FROM [Order]
                                WHERE [Order].Status = 'Paid'
                                      AND LocationId IN @locationIds
                                      AND [Order].OrderDate BETWEEN DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) AND DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)) AS MonthlyPaidOrders
                                 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<CurrentMonthSummaryViewModel>(query, new { locationIds });
            }
        }

        public async Task<IEnumerable<ChartRecordsViewModel>> MonthlySales(string userId)
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
    AND LocationId IN @locationIds
	GROUP BY datepart(month,OrderDate), datepart(year,OrderDate)) B
ON A.month = B.month) t
Order By Year, Label
                                 ";
                var locationIds = (await GetUserLocations(userId)).ToArray();
                conn.Open();
                return await conn.QueryAsync<ChartRecordsViewModel>(query, new { locationIds });
            }
        }

        public async Task<IEnumerable<ChartRecordsViewModel>> MonthlyPurchases(string userId)
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
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<ChartRecordsViewModel>(query);
            }
        }

        public async Task<IEnumerable<ChartRecordsViewModel>> DailySales(string userId)
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
                    AND LocationId IN @locationIds
                    GROUP BY datename(dw,OrderDate), CONVERT(date ,OrderDate)) Sales
                    on AllDays.WeekDayName = Sales.Label
                    ORDER BY Sales.ConvertedOrderDate
                                 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<ChartRecordsViewModel>(query, new { locationIds });
            }
        }

        public async Task<IEnumerable<ProductTypeSalesReportViewModel>> GetProductTypeSalesReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductTypeName, FORMAT(SUM(ISNULL(VanTotalSales,0)), 'N2') AS VanTotalSales, FORMAT(SUM(ISNULL(AbbTotalSales,0)), 'N2') AS AbbTotalSales 
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
WHERE ISNULL(VanTotalSales,0) <> 0 OR ISNULL(AbbTotalSales, 0) <> 0
GROUP BY ProductTypeName
                                 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<ProductTypeSalesReportViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<ProductSalesReportViewModel>> GetProductSalesReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductName, ProductCode, ProductTypeName, FORMAT(ISNULL(VanTotalSales,0), 'N2') AS VanTotalSales, FORMAT(ISNULL(AbbTotalSales, 0), 'N2') AS AbbTotalSales, ISNULL(VanBalance,0) AS VanBalance, ISNULL(AbbBalance, 0) AS AbbBalance, ISNULL(VanAmount, 0) AS VanAmount, ISNULL(AbbAmount, 0) AS AbbAmount
FROM Product
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
LEFT JOIN (SELECT SUM(OrderDetail.Total) AS VanTotalSales, SUM(OrderDetail.Amount) AS VanAmount, ProductId 
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE [Order].LocationId = 1
                 AND OrderDate BETWEEN @fromDate AND @toDate
                 AND [Order].Status IN ('Return', 'Paid', 'Account')
		   GROUP BY ProductId ) VanSales
	ON Product.ProductId = VanSales.ProductId
LEFT JOIN (SELECT SUM(OrderDetail.Total) AS AbbTotalSales,  SUM(OrderDetail.Amount) AS AbbAmount, ProductId 
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE [Order].LocationId = 2
                 AND OrderDate BETWEEN @fromDate AND @toDate
                 AND [Order].Status IN ('Return', 'Paid', 'Account')
		   GROUP BY ProductId ) AbbSales
	ON Product.ProductId = AbbSales.ProductId
LEFT JOIN (SELECT ProductId, Balance AS VanBalance 
           FROM ProductInventory
           WHERE LocationId = 1) VanInventoy
     ON Product.ProductId = VanInventoy.ProductId
LEFT JOIN (SELECT ProductId, Balance AS AbbBalance 
           FROM ProductInventory
           WHERE LocationId = 2) AbbInventoy
     ON Product.ProductId = AbbInventoy.ProductId
WHERE ISNULL(VanTotalSales,0) <> 0 OR ISNULL(AbbTotalSales, 0) <> 0
                                 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<ProductSalesReportViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<ProductSalesDetailReportViewModel>> GetProductSalesDetailReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT LocationName, ISNULL(Customer.CompanyName, 'WALK-IN') AS CompanyName, ISNULL(Customer.CustomerCode, '') AS CustomerCode, ProductName, ProductCode, OrderId, ProductTypeName, FORMAT(ISNULL(TotalSales,0), 'N2') AS TotalSales, ISNULL(Amount, 0) AS Amount
FROM Product
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
LEFT JOIN (SELECT ProductId, [Order].CustomerId, OrderDetail.OrderId, LocationId, SUM(OrderDetail.Total) AS TotalSales, SUM(OrderDetail.Amount) AS Amount
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE OrderDate BETWEEN @fromDate AND @toDate
                 AND [Order].Status IN ('Return', 'Paid', 'Account')
                 AND [Order].LocationId IN @locationIds
		   GROUP BY ProductId, OrderDetail.OrderId, LocationId, [Order].CustomerId) Sales
	ON Product.ProductId = Sales.ProductId
INNER JOIN [Location]
	ON Sales.LocationId = [Location].LocationId
LEFT JOIN Customer
	ON Customer.CustomerId = Sales.CustomerId
WHERE [Location].LocationId in @locationIds AND ISNULL(TotalSales,0) <> 0 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<ProductSalesDetailReportViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<SalesReportViewModel>> GetSalesReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
IF OBJECT_ID('tempdb..#Results') IS NOT NULL
    DROP TABLE #Results

SELECT LocationName,
	   [Order].[Status],
	   SubTotal,
	   Total,
	   Discount,
	   Transactions,
	   Pst,
	   Gst,
	   OtherTax
INTO #Results FROM (
	SELECT 
	   SUM(SubTotal) AS SubTotal, 
       SUM(Total) AS Total,
	   SUM(TotalDiscount) AS Discount,
	   Count([Order].OrderId) AS Transactions,
	   Location.LocationId,
	   Location.LocationName,
	   [Order].Status
	FROM [Order]
	INNER JOIN Location
		ON Location.LocationId = [Order].LocationId
	WHERE [Order].Status IN ('Return', 'Paid', 'Account')
		  AND OrderDate BETWEEN @FromDate AND @ToDate
          AND [Order].LocationId IN @locationIds
	GROUP BY Location.LocationId, LocationName, [Order].Status
) [Order]
LEFT JOIN (
	SELECT SUM(TaxAmount) AS GST, LocationId, Status
	FROM [Order]
	INNER JOIN OrderTax
		ON OrderTax.OrderId = [Order].OrderId
	INNER JOIN Tax
		ON Tax.TaxId = OrderTax.TaxId
	WHERE TaxName = 'GST'
          AND Status IN ('Return', 'Paid', 'Account')
          AND OrderDate BETWEEN @FromDate AND @ToDate
          AND [Order].LocationId IN @locationIds	
    GROUP BY [Order].LocationId, Status
) GST
	ON [Order].LocationId = GST.LocationId
       AND [Order].Status = GST.Status
LEFT JOIN (
	SELECT SUM(TaxAmount) AS Pst, LocationId, Status 
	FROM [Order]
	INNER JOIN OrderTax
		ON OrderTax.OrderId = [Order].OrderId
	INNER JOIN Tax
		ON Tax.TaxId = OrderTax.TaxId
	WHERE TaxName = 'PST'
          AND Status IN ('Return', 'Paid', 'Account')          
          AND OrderDate BETWEEN @FromDate AND @ToDate
          AND [Order].LocationId IN @locationIds	
    GROUP BY [Order].LocationId, [Status]
) PST
	ON [Order].LocationId = PST.LocationId
       AND [Order].Status = PST.Status
LEFT JOIN (
	SELECT SUM(TaxAmount) AS OtherTax, LocationId, Status
	FROM [Order]
	INNER JOIN OrderTax
		ON OrderTax.OrderId = [Order].OrderId
	INNER JOIN Tax
		ON Tax.TaxId = OrderTax.TaxId
	WHERE TaxName NOT IN ('PST', 'GST')
          AND Status IN ('Return', 'Paid', 'Account')          
          AND OrderDate BETWEEN @FromDate AND @ToDate
          AND [Order].LocationId IN @locationIds	
          GROUP BY [Order].LocationId, Status
) OtherTax
ON [Order].LocationId = OtherTax.LocationId
   AND [Order].Status = OtherTax.Status

SELECT LocationName, [Status], FORMAT(SubTotal, 'N2') AS SubTotal, FORMAT(Total, 'N2') AS Total, FORMAT(Discount, 'N2') AS Discount, Transactions, FORMAT(Pst, 'N2') AS Pst, FORMAT(Gst, 'N2') AS Gst, FORMAT(OtherTax, 'N2') AS OtherTax FROM #Results
UNION 
SELECT ' Total Account',  '', FORMAT(SUM(SubTotal), 'N2'), FORMAT(SUM(Total), 'N2'), FORMAT(SUM(Discount), 'N2'), SUM(Transactions), FORMAT(SUM(Pst), 'N2'), FORMAT(SUM(Gst), 'N2'), FORMAT(SUM(OtherTax), 'N2')
FROM #Results
WHERE [Status] = 'Account'
UNION 
SELECT ' Total Without Account',  '', FORMAT(SUM(SubTotal), 'N2'), FORMAT(SUM(Total), 'N2'), FORMAT(SUM(Discount), 'N2'), SUM(Transactions), FORMAT(SUM(Pst), 'N2'), FORMAT(SUM(Gst), 'N2'), FORMAT(SUM(OtherTax), 'N2')
FROM #Results
WHERE [Status] <> 'Account'
";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<SalesReportViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<PaymentsTotalViewModel>> GetPaymentsTotalReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ISNULL(PaymentTypeName, 'Total') AS PaymentTypeName, FORMAT(SUM(PaymentAmount), 'N2') AS PaymentAmount, ISNULL(Location.LocationName, 'All Locations') AS LocationName
FROM OrderPayment
INNER JOIN PaymentType
	ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId 
INNER JOIN [Order]
	ON [Order].OrderId = OrderPayment.OrderId
LEFT JOIN Users
	ON Users.Id = OrderPayment.CreatedByUserId
LEFT JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
INNER JOIN Location
    ON [Order].LocationId = Location.LocationId
WHERE OrderDate BETWEEN @fromDate AND @toDate
      AND [Location].LocationId IN @locationIds
GROUP BY PaymentTypeName, Location.LocationName
WITH ROLLUP                                 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<PaymentsTotalViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<PaymentsReportViewModel>> GetPaymentsReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT Users.GivenName, PaymentTypeName, [Order].OrderId, Customer.CompanyName, [Order].Status, FORMAT(SUM(PaymentAmount), 'N2') AS PaymentAmount, Location.LocationName
FROM OrderPayment
INNER JOIN PaymentType
	ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId 
INNER JOIN [Order]
	ON [Order].OrderId = OrderPayment.OrderId
LEFT JOIN Users
	ON Users.Id = OrderPayment.CreatedByUserId
LEFT JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
INNER JOIN Location
    ON [Order].LocationId = Location.LocationId
WHERE OrderDate BETWEEN @fromDate AND @toDate
      AND [Location].LocationId IN @locationIds
GROUP BY PaymentTypeName, Users.GivenName, [Order].OrderId, Customer.CompanyName, [Order].Status, Location.LocationName
                                 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<PaymentsReportViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<PaymentsByPaymentTypeViewModel>> GetPaymentsByPaymentTypeReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT PaymentTypeName, [Order].Status, FORMAT(SUM(PaymentAmount), 'N2') AS PaymentAmount, Location.LocationName
FROM OrderPayment
INNER JOIN PaymentType
	ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId 
INNER JOIN [Order]
	ON [Order].OrderId = OrderPayment.OrderId
LEFT JOIN Users
	ON Users.Id = OrderPayment.CreatedByUserId
LEFT JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
INNER JOIN Location
    ON [Order].LocationId = Location.LocationId
WHERE OrderDate BETWEEN @fromDate AND @toDate
      AND [Location].LocationId IN @locationIds
GROUP BY PaymentTypeName, [Order].Status, Location.LocationName
                                 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<PaymentsByPaymentTypeViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<PurchasesReportViewModel>> GetPurchasesReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                 ";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<PurchasesReportViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<CustomerPaidOrdersViewModel>> GetCustomerPaidReport(DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT [Order].OrderId, PoNumber, OrderDate, FORMAT([Order].Total, 'N2') AS Total, FORMAT(OrderPayment.PaymentAmount, 'N2') AS PaymentAmount, [Order].[Status], PaymentType.PaymentTypeName, Customer.CompanyName, Customer.CustomerCode, Customer.[Address], Customer.City, Customer.Province, Customer.PostalCode
FROM [Order]
INNER JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
INNER JOIN 
( 
	SELECT OrderId, PaymentDate, PaymentTypeId, SUM(PaymentAmount) AS PaymentAmount
	FROM OrderPayment
	WHERE PaymentDate BETWEEN @FromDate AND @ToDate
	GROUP BY OrderId, PaymentDate, PaymentTypeId
) OrderPayment
	ON [Order].OrderId = OrderPayment.OrderId
INNER JOIN PaymentType
	ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId
WHERE [Order].[Status] IN ('Paid', 'Return')
      AND OrderDate BETWEEN @FromDate AND @ToDate
                                 ";
                conn.Open();
                return await conn.QueryAsync<CustomerPaidOrdersViewModel>(query, new { fromDate, toDate });
            }
        }

        public async Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnPaidReport(DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT [Order].OrderId, PoNumber, OrderDate, DateAdd(DAY, 30, OrderDate) As DueDate, [Order].Total, CASE [Order].[Status] WHEN 'Account' THEN 'Awaiting Payment' END AS [Status], Customer.CompanyName, Customer.CustomerCode, Customer.[Address], Customer.City, Customer.Province, Customer.PostalCode
FROM [Order]
INNER JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
WHERE [Order].[Status] IN ('Account')
      AND OrderDate <= @ToDate
                                 ";
                conn.Open();
                return await conn.QueryAsync<CustomerUnPaidOrdersViewModel>(query, new { toDate });
            }
        }

        public async Task<IEnumerable<CustomerPaidOrdersViewModel>> GetCustomerPaidReport(int customerId, DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT [Order].OrderId, PoNumber, OrderDate, [Order].Total, OrderPayment.PaymentAmount, [Order].[Status], PaymentType.PaymentTypeName, Customer.CompanyName, Customer.CustomerCode, Customer.[Address], Customer.City, Customer.Province, Customer.PostalCode
FROM [Order]
INNER JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
INNER JOIN 
( 
	SELECT OrderId, PaymentDate, PaymentTypeId, SUM(PaymentAmount) AS PaymentAmount
	FROM OrderPayment
	WHERE PaymentDate BETWEEN @FromDate AND @ToDate
	GROUP BY OrderId, PaymentDate, PaymentTypeId
) OrderPayment
	ON [Order].OrderId = OrderPayment.OrderId
INNER JOIN PaymentType
	ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId
WHERE [Order].CustomerId = @customerId
      AND [Order].[Status] IN ('Paid', 'Return')
      AND OrderDate BETWEEN @FromDate AND @ToDate
                                 ";
                conn.Open();
                return await conn.QueryAsync<CustomerPaidOrdersViewModel>(query, new { customerId, fromDate, toDate });
            }
        }

        public async Task<IEnumerable<CustomerUnPaidOrdersViewModel>> GetCustomerUnPaidReport(int customerId, DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT [Order].OrderId, PoNumber, OrderDate, DateAdd(DAY, 30, OrderDate) As DueDate, [Order].Total, CASE [Order].[Status] WHEN 'Account' THEN 'Awaiting Payment' END AS [Status], Customer.CompanyName, Customer.CustomerCode, Customer.[Address], Customer.City, Customer.Province, Customer.PostalCode
FROM [Order]
INNER JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
WHERE [Order].CustomerId = @customerId 
      AND [Order].[Status] IN ('Account')
      AND OrderDate <= @ToDate
                                 ";
                conn.Open();
                return await conn.QueryAsync<CustomerUnPaidOrdersViewModel>(query, new { customerId, toDate });
            }
        }

        private async Task<IEnumerable<int>> GetUserLocations(string userId) {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT LocationId FROM UserLocation
WHERE UserId = @userId";
                conn.Open();
                return await conn.QueryAsync<int>(query, new { userId });
            }
        }
    }
}
