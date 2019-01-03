﻿using System.Collections.Generic;
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
    }
}