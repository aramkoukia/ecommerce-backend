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
SELECT * FROM (
SELECT 'TotalSales' AS ProductTypeName, 
       FORMAT(SUM([OrderDetail].Amount * [OrderDetail].UnitPrice), 'N2') AS TotalSales, 
       '' AS LocationName, SUM([OrderDetail].Amount * [OrderDetail].UnitPrice) AS Total
FROM [Order]
INNER JOIN OrderDetail
	ON [Order].OrderId = [OrderDetail].OrderId
INNER JOIN [Product]
	ON [OrderDetail].ProductId = [Product].ProductId
INNER JOIN Location
ON Location.LocationId = [Order].LocationId
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
INNER JOIN (
SELECT OrderId
FROM OrderPayment
WHERE PaymentDate BETWEEN @FromDate AND @ToDate
GROUP BY OrderId
) OrderPayment
ON [Order].OrderId = OrderPayment.OrderId
WHERE [Order].Status IN ('Return', 'Paid')

UNION 

SELECT ProductTypeName, 
       FORMAT(SUM([OrderDetail].Amount * [OrderDetail].UnitPrice), 'N2') AS TotalSales, 
       Location.LocationName, SUM([OrderDetail].Amount * [OrderDetail].UnitPrice) AS Total
FROM [Order]
INNER JOIN OrderDetail
	ON [Order].OrderId = [OrderDetail].OrderId
INNER JOIN [Product]
	ON [OrderDetail].ProductId = [Product].ProductId
INNER JOIN Location
ON Location.LocationId = [Order].LocationId
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
INNER JOIN (
SELECT OrderId
FROM OrderPayment
WHERE PaymentDate BETWEEN @FromDate AND @ToDate
GROUP BY OrderId
) OrderPayment
ON [Order].OrderId = OrderPayment.OrderId
WHERE [Order].Status IN ('Return', 'Paid')
GROUP BY Location.LocationId, LocationName, ProductTypeName 
) t
ORDER BY Total DESC

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
SELECT ProductName, ProductCode, ProductTypeName, FORMAT(ISNULL(TotalSales,0), 'N2') AS TotalSales, ISNULL(Balance,0) AS Balance, ISNULL(Amount, 0) AS Amount, ISNULL(OnHold, 0) AS OnHold, LocationName
FROM 
(Select ProductId, ProductName, LocationId, ProductTypeId, ProductCode, LocationName
 FROM Product, Location
 WHERE LocationId IN @locationIds
 ) Product
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
LEFT JOIN (SELECT ProductId, Balance, LocationId
           FROM ProductInventory
           WHERE LocationId IN @locationIds) Inventoy
     ON Product.ProductId = Inventoy.ProductId
	    AND Product.LocationId = Inventoy.LocationId 
LEFT JOIN (SELECT SUM(OrderDetail.Total) AS TotalSales, SUM(OrderDetail.Amount) AS Amount, ProductId, LocationId
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE [Order].LocationId IN @locationIds
                 AND OrderDate BETWEEN @fromDate AND @toDate
                 AND [Order].Status IN ('Return', 'Paid', 'Account')
		   GROUP BY ProductId, LocationId ) Sales
	ON Product.ProductId = Sales.ProductId
	   AND Product.LocationId = Sales.LocationId 
LEFT JOIN (SELECT SUM(OrderDetail.Amount) AS OnHold, ProductId, LocationId
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE [Order].LocationId IN @locationIds
                 AND [Order].Status = 'OnHold'
                 AND OrderDate BETWEEN @fromDate AND @toDate
		   GROUP BY ProductId, LocationId ) OnHold
	ON Product.ProductId = OnHold.ProductId
	   AND Product.LocationId = OnHold.LocationId 
WHERE ISNULL(TotalSales,0) <> 0 OR ISNULL(OnHold, 0) <> 0
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
SELECT LocationName, ISNULL(Customer.CompanyName, 'WALK-IN') AS CompanyName, ISNULL(Customer.CustomerCode, '') AS CustomerCode, ProductName, ProductCode, OrderId, ProductTypeName, FORMAT(ISNULL(TotalSales,0), 'N2') AS TotalSales, ISNULL(Amount, 0) AS Amount, Sales.[Status]
FROM Product
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
LEFT JOIN (SELECT ProductId, [Order].CustomerId, OrderDetail.OrderId, [Order].[Status], LocationId, SUM(OrderDetail.Total) AS TotalSales, SUM(OrderDetail.Amount) AS Amount
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE OrderDate BETWEEN @fromDate AND @toDate
                 AND [Order].Status IN ('Return', 'Paid', 'Account', 'OnHold')
                 AND [Order].LocationId IN @locationIds
		   GROUP BY ProductId, OrderDetail.OrderId, LocationId, [Order].CustomerId, [Order].[Status]) Sales
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

IF OBJECT_ID('tempdb..#AccountResults') IS NOT NULL
    DROP TABLE #AccountResults

	SELECT LocationName,
	   [Order].[Status],
	   SubTotal,
	   Total,
	   Discount,
	   Transactions,
	   Pst,
	   Gst,
	   OtherTax
INTO #AccountResults FROM (
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
	WHERE [Order].Status IN ('Account')
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
          AND Status IN ('Account')
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
          AND Status IN ('Account')          
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
          AND Status IN ('Account')          
          AND OrderDate BETWEEN @FromDate AND @ToDate
          AND [Order].LocationId IN @locationIds	
          GROUP BY [Order].LocationId, Status
) OtherTax
ON [Order].LocationId = OtherTax.LocationId
   AND [Order].Status = OtherTax.Status

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
	   SUM(SubTotal + RestockingFeeAmount) AS SubTotal, 
       SUM(Total) AS Total,
	   SUM(TotalDiscount) AS Discount,
	   Count([Order].OrderId) AS Transactions,
	   Location.LocationId,
	   Location.LocationName,
	   [Order].Status
	FROM [Order]
	INNER JOIN Location
			ON Location.LocationId = [Order].LocationId
	INNER JOIN (
		SELECT SUM(PaymentAmount) AS PaymentAmount, OrderId
		FROM OrderPayment
		WHERE PaymentDate BETWEEN @FromDate AND @ToDate
		GROUP BY OrderId
	) OrderPayment
	ON [Order].OrderId = OrderPayment.OrderId
	WHERE [Order].Status IN ('Return', 'Paid')
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
	INNER JOIN (
		SELECT SUM(PaymentAmount) AS PaymentAmount, OrderId
		FROM OrderPayment
		WHERE PaymentDate BETWEEN @FromDate AND @ToDate
		GROUP BY OrderId
	) OrderPayment
	ON [Order].OrderId = OrderPayment.OrderId

	WHERE TaxName = 'GST'
          AND Status IN ('Return', 'Paid')
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
	INNER JOIN (
		SELECT SUM(PaymentAmount) AS PaymentAmount, OrderId
		FROM OrderPayment
		WHERE PaymentDate BETWEEN @FromDate AND @ToDate
		GROUP BY OrderId
	) OrderPayment
	ON [Order].OrderId = OrderPayment.OrderId
	WHERE TaxName = 'PST'
          AND Status IN ('Return', 'Paid')          
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
	INNER JOIN (
		SELECT SUM(PaymentAmount) AS PaymentAmount, OrderId
		FROM OrderPayment
		WHERE PaymentDate BETWEEN @FromDate AND @ToDate
		GROUP BY OrderId
	) OrderPayment
	ON [Order].OrderId = OrderPayment.OrderId
	WHERE TaxName NOT IN ('PST', 'GST')
          AND Status IN ('Return', 'Paid')          
          AND [Order].LocationId IN @locationIds	
          GROUP BY [Order].LocationId, Status
) OtherTax
ON [Order].LocationId = OtherTax.LocationId
   AND [Order].Status = OtherTax.Status

SELECT LocationName, [Status], FORMAT(SubTotal, 'N2') AS SubTotal, FORMAT(Total, 'N2') AS Total, FORMAT(Discount, 'N2') AS Discount, Transactions, FORMAT(Pst, 'N2') AS Pst, FORMAT(Gst, 'N2') AS Gst, FORMAT(OtherTax, 'N2') AS OtherTax FROM #Results
UNION 
SELECT LocationName, [Status], FORMAT(SubTotal, 'N2') AS SubTotal, FORMAT(Total, 'N2') AS Total, FORMAT(Discount, 'N2') AS Discount, Transactions, FORMAT(Pst, 'N2') AS Pst, FORMAT(Gst, 'N2') AS Gst, FORMAT(OtherTax, 'N2') AS OtherTax FROM #AccountResults
UNION 
SELECT ' Total Account',  '', FORMAT(SUM(SubTotal), 'N2'), FORMAT(SUM(Total), 'N2'), FORMAT(SUM(Discount), 'N2'), SUM(Transactions), FORMAT(SUM(Pst), 'N2'), FORMAT(SUM(Gst), 'N2'), FORMAT(SUM(OtherTax), 'N2')
FROM #AccountResults
UNION 
SELECT ' Total Without Account',  '', FORMAT(SUM(SubTotal), 'N2'), FORMAT(SUM(Total), 'N2'), FORMAT(SUM(Discount), 'N2'), SUM(Transactions), FORMAT(SUM(Pst), 'N2'), FORMAT(SUM(Gst), 'N2'), FORMAT(SUM(OtherTax), 'N2')
FROM #Results
";
                conn.Open();
                var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<SalesReportViewModel>(query, new { fromDate, toDate, locationIds });
            }
        }

        public async Task<IEnumerable<SalesByPurchasePriceReportViewModel>> GetSalesByPurchasePriceReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
IF OBJECT_ID('tempdb..#Results') IS NOT NULL
    DROP TABLE #Results

SELECT LocationName,
  [Order].[Status],
  SubTotal,
  TotalBySalePrice,
  TotalByPurchasePrice,
  Discount,
  Transactions,
  Pst,
  Gst,
  OtherTax
INTO #Results FROM (
SELECT 
  SUM(OrderDetail.SubTotal) AS SubTotal,
  SUM(OrderDetail.DiscountAmount) AS Discount,
  SUM(OrderDetail.Amount * OrderDetail.UnitPrice) AS TotalBySalePrice,
  SUM(OrderDetail.Amount * ISNULL(Product.PurchasePrice,0) * ISNULL(AmountInMainPackage, 1)) AS TotalByPurchasePrice,
  Count([Order].OrderId) AS Transactions,
  Location.LocationId,
  Location.LocationName,
  [Order].Status
FROM [Order]
INNER JOIN [OrderDetail]
	ON [Order].OrderId = [OrderDetail].OrderId
INNER JOIN [Product]
	ON [OrderDetail].ProductId = [Product].ProductId
INNER JOIN Location
ON Location.LocationId = [Order].LocationId
INNER JOIN (
SELECT OrderId
FROM OrderPayment
WHERE PaymentDate BETWEEN @FromDate AND @ToDate
GROUP BY OrderId
) OrderPayment
ON [Order].OrderId = OrderPayment.OrderId
WHERE [Order].Status IN ('Return', 'Paid')
GROUP BY Location.LocationId, LocationName, [Order].Status
) [Order]
LEFT JOIN (
SELECT SUM(TaxAmount) AS GST, LocationId, Status
FROM [Order]
INNER JOIN OrderTax
ON OrderTax.OrderId = [Order].OrderId
INNER JOIN Tax
ON Tax.TaxId = OrderTax.TaxId
INNER JOIN (
SELECT SUM(PaymentAmount) AS PaymentAmount, OrderId
FROM OrderPayment
WHERE PaymentDate BETWEEN @FromDate AND @ToDate
GROUP BY OrderId
) OrderPayment
ON [Order].OrderId = OrderPayment.OrderId
WHERE TaxName = 'GST'
          AND Status IN ('Return', 'Paid')
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
INNER JOIN (
SELECT SUM(PaymentAmount) AS PaymentAmount, OrderId
FROM OrderPayment
WHERE PaymentDate BETWEEN @FromDate AND @ToDate
GROUP BY OrderId
) OrderPayment
ON [Order].OrderId = OrderPayment.OrderId
WHERE TaxName = 'PST'
          AND Status IN ('Return', 'Paid') 
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
INNER JOIN (
SELECT SUM(PaymentAmount) AS PaymentAmount, OrderId
FROM OrderPayment
WHERE PaymentDate BETWEEN @FromDate AND @ToDate
GROUP BY OrderId
) OrderPayment
ON [Order].OrderId = OrderPayment.OrderId
WHERE TaxName NOT IN ('PST', 'GST')
          AND Status IN ('Return', 'Paid') 
          GROUP BY [Order].LocationId, Status
) OtherTax
ON [Order].LocationId = OtherTax.LocationId
   AND [Order].Status = OtherTax.Status

SELECT LocationName, [Status], FORMAT(SubTotal, 'N2') AS SubTotal, FORMAT(TotalBySalePrice, 'N2') AS TotalBySalePrice, FORMAT(TotalByPurchasePrice, 'N2') AS TotalByPurchasePrice, FORMAT(Discount, 'N2') AS Discount, Transactions, FORMAT(Pst, 'N2') AS Pst, FORMAT(Gst, 'N2') AS Gst, FORMAT(OtherTax, 'N2') AS OtherTax FROM #Results
UNION 
SELECT ' Total ',  '', FORMAT(SUM(SubTotal), 'N2'), FORMAT(SUM(TotalBySalePrice), 'N2'), FORMAT(SUM(TotalByPurchasePrice), 'N2') AS TotalByPurchasePrice, FORMAT(SUM(Discount), 'N2'), SUM(Transactions), FORMAT(SUM(Pst), 'N2'), FORMAT(SUM(Gst), 'N2'), FORMAT(SUM(OtherTax), 'N2')
FROM #Results
";
                conn.Open();
                // var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<SalesByPurchasePriceReportViewModel>(query, new { fromDate, toDate });
            }
        }

        public async Task<IEnumerable<SalesByPurchasePriceDetailReportViewModel>> GetSalesByPurchasePriceDetailReport(DateTime fromDate, DateTime toDate, string id)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
IF OBJECT_ID('tempdb..#Results') IS NOT NULL
    DROP TABLE #Results

SELECT LocationName,
  ProductCode, 
  ProductName,
  Amount,
  PurchasePrice,
  SalesPrice,
  TotalBySalePrice,
  TotalByPurchasePrice
INTO #Results FROM (
SELECT 
  SUM(OrderDetail.Amount * OrderDetail.UnitPrice) AS TotalBySalePrice,
  SUM(OrderDetail.Amount * ISNULL(Product.PurchasePrice,0) * ISNULL(AmountInMainPackage, 1)) AS TotalByPurchasePrice,
  SUM(OrderDetail.Amount) As Amount,
  Product.PurchasePrice,
  AVG(UnitPrice) AS SalesPrice,
  Location.LocationId,
  Location.LocationName,
  Product.ProductCode,
  Product.ProductName
FROM [Order]
INNER JOIN [OrderDetail]
	ON [Order].OrderId = [OrderDetail].OrderId
INNER JOIN [Product]
	ON [OrderDetail].ProductId = [Product].ProductId
INNER JOIN Location
ON Location.LocationId = [Order].LocationId
INNER JOIN (
SELECT OrderId
FROM OrderPayment
WHERE PaymentDate BETWEEN @FromDate AND @ToDate
GROUP BY OrderId
) OrderPayment
ON [Order].OrderId = OrderPayment.OrderId
WHERE [Order].Status IN ('Return', 'Paid')
GROUP BY Location.LocationId, LocationName, [Order].Status, ProductCode, ProductName, Product.PurchasePrice
) [Order]

SELECT LocationName, ProductCode, ProductName, FORMAT(Amount, 'N2') AS Amount, FORMAT(SalesPrice, 'N2') AS SalesPrice, FORMAT(PurchasePrice, 'N2') AS PurchasePrice,  FORMAT(TotalBySalePrice, 'N2') AS TotalBySalePrice, FORMAT(TotalByPurchasePrice, 'N2') AS TotalByPurchasePrice FROM #Results
UNION 
SELECT ' Total ',  '', '', '', '', '',FORMAT(SUM(TotalBySalePrice), 'N2'), FORMAT(SUM(TotalByPurchasePrice), 'N2') AS TotalByPurchasePrice
FROM #Results
";
                conn.Open();
                // var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<SalesByPurchasePriceDetailReportViewModel>(query, new { fromDate, toDate });
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
WHERE PaymentDate BETWEEN @fromDate AND @toDate
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
WHERE PaymentDate BETWEEN @fromDate AND @toDate
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
WHERE PaymentDate BETWEEN @fromDate AND @toDate
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
SELECT CASE Product.ProductCode WHEN '' THEN '_' ELSE Product.ProductCode END AS ProductCode ,
       Product.ProductName, 
       FORMAT(PlannedPurchase.Amount, 'N2') AS PlannedAmount, 
	   FORMAT(PlannedPurchase.TotalPrice, 'N2') AS PlannedTotalPrice,
       FORMAT(PaidPurchase.Amount, 'N2') AS PaidAmount, 
	   FORMAT(PaidPurchase.TotalPrice, 'N2') AS PaidTotalPrice,
       FORMAT(OnDeliveryPurchase.Amount, 'N2') AS OnDeliveryAmount, 
	   FORMAT(OnDeliveryPurchase.TotalPrice, 'N2') AS OnDeliveryTotalPrice,
       FORMAT(CustomClearancePurchase.Amount, 'N2') AS CustomClearanceAmount, 
	   FORMAT(CustomClearancePurchase.TotalPrice, 'N2') AS CustomClearanceTotalPrice,
       FORMAT(ArrivedPurchase.Amount, 'N2') AS ArrivedAmount, 
	   FORMAT(ArrivedPurchase.TotalPrice, 'N2') AS ArrivedTotalPrice
FROM (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'Plan'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) PlannedPurchase
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'Paid'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) PaidPurchase
ON PlannedPurchase.ProductId = PaidPurchase.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'OnDelivery'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) OnDeliveryPurchase
ON PlannedPurchase.ProductId = OnDeliveryPurchase.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'CustomClearance'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) CustomClearancePurchase
ON PlannedPurchase.ProductId = CustomClearancePurchase.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'Arrived'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) ArrivedPurchase
ON PlannedPurchase.ProductId = ArrivedPurchase.ProductId
INNER JOIN Product
	ON PlannedPurchase.ProductId = Product.ProductId

UNION 

SELECT ' Total ' AS ProductCode,
       ''  AS ProductName, 
       '' AS PlannedAmount, 
	   FORMAT(SUM(PlannedPurchase.TotalPrice), 'N2') AS PlannedTotalPrice,
       '' AS PaidAmount, 
	   FORMAT(SUM(PaidPurchase.TotalPrice), 'N2') AS PaidTotalPrice,
       '' AS OnDeliveryAmount, 
	   FORMAT(SUM(OnDeliveryPurchase.TotalPrice), 'N2') AS OnDeliveryTotalPrice,
       '' AS CustomClearanceAmount, 
	   FORMAT(SUM(CustomClearancePurchase.TotalPrice), 'N2') AS CustomClearanceTotalPrice,
       '' AS ArrivedAmount, 
	   FORMAT(SUM(ArrivedPurchase.TotalPrice), 'N2') AS ArrivedTotalPrice
FROM (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'Plan'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) PlannedPurchase
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'Paid'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) PaidPurchase
ON PlannedPurchase.ProductId = PaidPurchase.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'OnDelivery'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) OnDeliveryPurchase
ON PlannedPurchase.ProductId = OnDeliveryPurchase.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'CustomClearance'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) CustomClearancePurchase
ON PlannedPurchase.ProductId = CustomClearancePurchase.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Amount, SUM(ToTalPrice) AS TotalPrice
	FROM PurchaseDetail
	WHERE Status = 'Arrived'
		  AND CreatedDate BETWEEN @FromDate AND @ToDate 
    GROUP BY ProductId) ArrivedPurchase
ON PlannedPurchase.ProductId = ArrivedPurchase.ProductId
INNER JOIN Product
	ON PlannedPurchase.ProductId = Product.ProductId
                                 ";
                conn.Open();
                // var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<PurchasesReportViewModel>(query, new { fromDate, toDate });
            }
        }
        
        public async Task<IEnumerable<PurchasesDetailReportViewModel>> GetPurchasesDetailReport(DateTime fromDate, DateTime toDate, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductCode, ProductName, PurchaseDetail.PurchaseId, Amount, TotalPrice, PaidDate, PurchaseDetail.EstimatedDelivery, ArrivedDate, PurchaseDetail.PoNumber, Supplier, LocationName
FROM PurchaseDetail
INNER JOIN Purchase
	ON Purchase.PurchaseId = PurchaseDetail.PurchaseId 
		AND PurchaseDetail.CreatedDate BETWEEN @FromDate AND @ToDate 
INNER JOIN Product
	ON Product.ProductId = PurchaseDetail.ProductId
LEFT JOIN Location
	ON Location.LocationId = PurchaseDetail.ArrivedAtLocationId
                                 ";
                conn.Open();
                // var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<PurchasesDetailReportViewModel>(query, new { fromDate, toDate });
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
SELECT [Order].OrderId, PoNumber, OrderDate, [Order].Total, OrderPayment.PaymentAmount, [Order].[Status], 
       ISNULL(PaymentType.PaymentTypeName, '') AS PaymentTypeName, Customer.CompanyName, Customer.CustomerCode, Customer.[Address], Customer.City, Customer.Province, Customer.PostalCode
FROM [Order]
INNER JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
LEFT JOIN 
( 
	SELECT OrderId, PaymentDate, PaymentTypeId, SUM(PaymentAmount) AS PaymentAmount
	FROM OrderPayment
	GROUP BY OrderId, PaymentDate, PaymentTypeId
) OrderPayment
	ON [Order].OrderId = OrderPayment.OrderId
LEFT JOIN PaymentType
	ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId
WHERE [Order].CustomerId = @customerId
      AND [Order].[Status] IN ('Paid', 'Return')
      AND OrderDate BETWEEN @FromDate AND @ToDate
	  AND ISNULL(OrderPayment.PaymentAmount, 0) <> 0
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
SELECT [Order].OrderId, PoNumber, OrderDate, DateAdd(DAY, 30, OrderDate) As DueDate, [Order].Total, 
       CASE [Order].[Status] 
	   WHEN 'Account' THEN 'Awaiting Payment'
	   WHEN 'Return' THEN 'Return Awaiting Payment'
	   END AS [Status], 
       Customer.CompanyName, Customer.CustomerCode, Customer.[Address], Customer.City, Customer.Province, Customer.PostalCode
FROM [Order]
INNER JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
LEFT JOIN 
( 
	SELECT OrderId, PaymentDate, PaymentTypeId, SUM(PaymentAmount) AS PaymentAmount
	FROM OrderPayment
	GROUP BY OrderId, PaymentDate, PaymentTypeId
) OrderPayment
ON [Order].OrderId = OrderPayment.OrderId
WHERE [Order].CustomerId = @customerId 
      AND ([Order].[Status] = 'Account' OR ([Order].[Status] = 'Return' AND  ISNULL(OrderPayment.PaymentAmount, 0) = 0))
      AND OrderDate <= @ToDate";
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

        public async Task<IEnumerable<SalesForecastReportViewModel>> GetSalesForecastReport(DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductCode, ProductName, Last1Month, Last3Month, Last6Month, Last12Month AS Last12Month , CAST(Last12Month / 12 AS DECIMAL(12,0)) AS Last12MonthAverage, Inventory.Balance,
       CASE WHEN Inventory.Balance < Last6Month THEN 'Yes' ELSE 'No' END AS NeedsPurchase  
FROM Product
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Last1Month 
	FROM [Order]
	INNER JOIN OrderDetail
		ON OrderDetail.OrderId = [Order].OrderId
	WHERE Status IN ('Paid', 'Account')
		  AND OrderDate >= DATEADD(MONTH, -1, GETDATE()) 
	GROUP BY ProductId) Last1Month
ON Last1Month.ProductId = Product.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Last3Month 
	FROM [Order]
	INNER JOIN OrderDetail
		ON OrderDetail.OrderId = [Order].OrderId
	WHERE Status IN ('Paid', 'Account')
		  AND OrderDate >= DATEADD(MONTH, -3, GETDATE()) 
	GROUP BY ProductId) Last3Month
ON Last3Month.ProductId = Product.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Last6Month 
	FROM [Order]
	INNER JOIN OrderDetail
		ON OrderDetail.OrderId = [Order].OrderId
	WHERE Status IN ('Paid', 'Account')
		  AND OrderDate >= DATEADD(MONTH, -6, GETDATE()) 
	GROUP BY ProductId) Last6Month
ON Last6Month.ProductId = Product.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Amount) AS Last12Month 
	FROM [Order]
	INNER JOIN OrderDetail
		ON OrderDetail.OrderId = [Order].OrderId
	WHERE Status IN ('Paid', 'Account')
		  AND OrderDate >= DATEADD(MONTH, -12, GETDATE())
	GROUP BY ProductId) Last12Month
ON Last12Month.ProductId = Product.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(ISNULL(Balance, 0)) AS Balance
	FROM ProductInventory
	GROUP BY ProductId
) AS Inventory
ON Product.ProductId = Inventory.ProductId 
WHERE Last3Month IS NOT NULL 
      OR Last6Month IS NOT NULL
	  OR Last12Month IS NOT NULL
ORDER BY Last12Month DESC
                                 ";
                conn.Open();
                // var locationIds = (await GetUserLocations(userId)).ToArray();
                return await conn.QueryAsync<SalesForecastReportViewModel>(query);
            }
        }

        public async Task<IEnumerable<ProductProfitReportViewModel>> GetProductProfitReport(DateTime salesFromDate,
                                                                                            DateTime salesToDate,
                                                                                            DateTime purchaseFromDate,
                                                                                            DateTime purchaseToDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductName, 
       ProductCode, 
       ProductTypeName,
	   FORMAT(ISNULL(PurchaseAmount, 0), 'N2') AS PurchaseAmount, 
	   FORMAT(ISNULL(AvgPurchasePrice, 0), 'N2') AS AvgPurchasePrice, 
	   FORMAT(ISNULL(AvgOverheadCost, 0), 'N2') AS AvgOverheadCost, 
	   FORMAT(ISNULL(AvgTotalCost, 0), 'N2') AS AvgTotalCost, 
	   FORMAT(ISNULL(SalesAmount, 0), 'N2') AS SalesAmount, 
       FORMAT(ISNULL(SalesAmount,0) * ISNULL(PurchasePrice,0), 'N2') AS TotalSalesByPurchasePrice, 
	   FORMAT(ISNULL(TotalSales, 0), 'N2') AS TotalSales, 
	   FORMAT(ISNULL(AvgSalesPrice, 0), 'N2') AS AvgSalesPrice, 
	   FORMAT(ISNULL(SalesAmount * AvgTotalCost, 0), 'N2') AS TotalCost,
	   FORMAT(ISNULL(AvgSalesPrice - AvgTotalCost, 0), 'N2') AS AvgProfitPerItem,
	   FORMAT(ISNULL(TotalSales - (SalesAmount * AvgTotalCost), 0), 'N2') AS TotalProfitByAvgCost,
       FORMAT(ISNULL(TotalSales - (SalesAmount * ISNULL(PurchasePrice,0)), 0), 'N2') AS TotalProfitByPurchasePrice
FROM Product
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
INNER JOIN (
SELECT ProductId, 
       SUM(Amount) AS PurchaseAmount, 
	   Avg(UnitPrice) AS AvgPurchasePrice, 
	   Avg(ISNULL(OverheadCost, 0)/ Amount) AS AvgOverheadCost,
	   Avg(UnitPrice + (ISNULL(OverheadCost, 0)/ Amount)) AS AvgTotalCost  
FROM purchasedetail
WHERE PaidDate BETWEEN @purchaseFromDate AND @purchaseToDate
GROUP BY ProductId) Purchase
	ON Purchase.ProductId = Product.ProductId
LEFT JOIN (SELECT ProductId, SUM(OrderDetail.UnitPrice * OrderDetail.Amount) AS TotalSales, SUM(OrderDetail.Amount) AS SalesAmount, AVG(OrderDetail.UnitPrice) AS AvgSalesPrice
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE OrderDate BETWEEN @salesFromDate AND @salesToDate
                 AND [Order].Status IN ('Return', 'Paid', 'Account')
		   GROUP BY ProductId) Sales
ON Product.ProductId = Sales.ProductId
-- ORDER BY ISNULL(TotalSales - (SalesAmount * AvgTotalCost), 0) DESC

UNION

SELECT ' Total ' AS ProductName,
       '' AS ProductCode,
	   '' AS ProductTypeName , 
       FORMAT(SUM(PurchaseAmount), 'N2') AS PurchaseAmount, 
       '' AS AvgPurchasePrice,
       '' AS AvgOverheadCost,
	   '' AS AvgTotalCost,
	   '' AS SalesAmount,
	   FORMAT(SUM(TotalSalesByPurchasePrice), 'N2') AS TotalSalesByPurchasePrice,
	   FORMAT(SUM(TotalSales), 'N2') AS TotalSales,
	   '' AS AvgSalesPrice,
	   FORMAT(SUM(TotalCost), 'N2') AS TotalCost,
	   '' AS AvgProfitPerItem,
	   '' AS TotalProfitByAvgCost,
	   FORMAT(SUM(TotalProfitByPurchasePrice), 'N2') AS TotalProfitByPurchasePrice
FROM (
SELECT ProductName, 
       ProductCode, 
       ProductTypeName,
	   ISNULL(PurchaseAmount, 0) AS PurchaseAmount, 
	   ISNULL(AvgPurchasePrice, 0) AS AvgPurchasePrice, 
	   ISNULL(AvgOverheadCost, 0) AS AvgOverheadCost, 
	   ISNULL(AvgTotalCost, 0) AS AvgTotalCost, 
	   ISNULL(SalesAmount, 0) AS SalesAmount, 
       ISNULL(SalesAmount,0) * ISNULL(PurchasePrice,0) AS TotalSalesByPurchasePrice, 
	   ISNULL(TotalSales, 0) AS TotalSales, 
	   ISNULL(AvgSalesPrice, 0) AS AvgSalesPrice, 
	   ISNULL(SalesAmount * AvgTotalCost, 0) AS TotalCost,
	   ISNULL(AvgSalesPrice - AvgTotalCost, 0) AS AvgProfitPerItem,
	   ISNULL(TotalSales - (SalesAmount * AvgTotalCost), 0) AS TotalProfitByAvgCost,
       ISNULL(TotalSales - (SalesAmount * ISNULL(PurchasePrice,0)), 0) AS TotalProfitByPurchasePrice
FROM Product
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
INNER JOIN (
SELECT ProductId, 
       SUM(Amount) AS PurchaseAmount, 
	   Avg(UnitPrice) AS AvgPurchasePrice, 
	   Avg(ISNULL(OverheadCost, 0)/ Amount) AS AvgOverheadCost,
	   Avg(UnitPrice + (ISNULL(OverheadCost, 0)/ Amount)) AS AvgTotalCost  
FROM purchasedetail
WHERE PaidDate BETWEEN @purchaseFromDate AND @purchaseToDate
GROUP BY ProductId) Purchase
	ON Purchase.ProductId = Product.ProductId
LEFT JOIN (SELECT ProductId, SUM(OrderDetail.UnitPrice * OrderDetail.Amount) AS TotalSales, SUM(OrderDetail.Amount) AS SalesAmount, AVG(OrderDetail.UnitPrice) AS AvgSalesPrice
		   FROM [Order]
		   INNER JOIN OrderDetail
			 ON OrderDetail.OrderId = [Order].OrderId
		   WHERE OrderDate BETWEEN @salesFromDate AND @salesToDate
                 AND [Order].Status IN ('Return', 'Paid', 'Account')
		   GROUP BY ProductId) Sales
ON Product.ProductId = Sales.ProductId) t
";
                conn.Open();
                return await conn.QueryAsync<ProductProfitReportViewModel>(query, new { salesFromDate, salesToDate, purchaseFromDate, purchaseToDate });
            }
        }

        public async Task<IEnumerable<InventoryValueReportViewModel>> GetInventoryValue()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT Product.ProductId, 
        ProductCode, 
	    ProductName, 
	    SalesPrice, 
	    PurchasePrice, 
	    Product.ModifiedDate, 
	    Product.ProductTypeId, 
	    ProductType.ProductTypeName,
	    ISNULL(Balance,0) As Balance,
		ISNULL(OnHoldItems.OnHoldAmount, 0) AS OnHoldAmount,
        FORMAT(ISNULL(Balance,0) * ISNULL(PurchasePrice, 0), 'N2') AS TotalValue
FROM Product
LEFT JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN 
(SELECT ProductId, Sum(ISNULL(Balance,0)) AS Balance
 FROM ProductInventory
 GROUP BY ProductId) ProductInventory
  ON ProductInventory.ProductId = Product.ProductId
LEFT JOIN (
  SELECT ProductId, SUM(Amount) As OnHoldAmount
  FROM [Order]
  INNER JOIN OrderDetail
	ON [Order].OrderId = OrderDetail.OrderId
  WHERE [Order].Status = 'OnHold'
  GROUP BY ProductId
) AS OnHoldItems
  ON OnHoldItems.ProductId = Product.ProductId
  WHERE Balance <> 0
";
                conn.Open();
                return await conn.QueryAsync<InventoryValueReportViewModel>(query);
            }
        }

        public async Task<IEnumerable<InventoryValueTotalReportViewModel>> GetInventoryValueTotal()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT LocationName, 
       FORMAT(SUM(Balance * SalesPrice), 'N2') AS ValueBySalePrice, 
       FORMAT(SUM(Balance * PurchasePrice), 'N2') AS ValueByPurchasePrice
FROM ProductInventory
INNER JOIN Product
	ON Product.ProductId = ProductInventory.ProductId
INNER JOIN Location
	ON Location.LocationId = ProductInventory.LocationId
GROUP BY LocationName

UNION 

SELECT ' Total ', 
       FORMAT(SUM(Balance * SalesPrice), 'N2') AS ValueBySalePrice, 
       FORMAT(SUM(Balance * PurchasePrice), 'N2') AS ValueByPurchasePrice
FROM ProductInventory
INNER JOIN Product
	ON Product.ProductId = ProductInventory.ProductId
INNER JOIN Location
	ON Location.LocationId = ProductInventory.LocationId
";
                conn.Open();
                return await conn.QueryAsync<InventoryValueTotalReportViewModel>(query);
            }
        }

        public async Task<IEnumerable<InventoryValueTotalByCategoryReportViewModel>> GetInventoryValueTotalByCategory()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT LocationName, 
       ProductTypeName AS CategoryName,
       FORMAT(SUM(Balance * SalesPrice), 'N2') AS ValueBySalePrice, 
       FORMAT(SUM(Balance * PurchasePrice), 'N2') AS ValueByPurchasePrice,
	   SUM(Balance * SalesPrice) AS ValueBySalePriceValue
FROM ProductInventory
INNER JOIN Product
	ON Product.ProductId = ProductInventory.ProductId
INNER JOIN ProductType
	ON ProductType.ProductTypeId = Product.ProductTypeId
INNER JOIN Location
	ON Location.LocationId = ProductInventory.LocationId
GROUP BY LocationName, ProductTypeName
ORDER BY ValueBySalePriceValue DESC
";
                conn.Open();
                return await conn.QueryAsync<InventoryValueTotalByCategoryReportViewModel>(query);
            }
        }
    }
}
