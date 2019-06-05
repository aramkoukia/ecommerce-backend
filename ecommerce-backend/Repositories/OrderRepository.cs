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
    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public OrderRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<OrderViewModel>> GetOrders(DateTime fromDate, DateTime toDate, int locationId, string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT [Order].[OrderId]
	,[Order].[CustomerId]
	,[Order].[LocationId]
	,FORMAT ([OrderDate], 'dd/MM/yyyy hh:mm tt') AS OrderDate
	,[Total]
	,[SubTotal]
	,[TotalDiscount]
	,[Order].[PstNumber]
	,[Notes]
	,[PoNumber]
	,[Order].[Status]
	,Users.GivenName
	,ISNULL(OrderPayment.PaidAmount, 0) AS PaidAmount
	,Location.LocationName,
	PaymentTypeName,
	ISNULL(Customer.CompanyName, 'WALK-IN') AS CompanyName,
	CASE WHEN [Order].[Status] = 'Account' THEN FORMAT(DATEADD(DAY, 40, [OrderDate]), 'dd/MM/yyyy hh:mm tt') ELSE NULL END AS DueDate,
	CASE WHEN [Order].[Status] = 'Account' THEN CASE WHEN DATEADD(DAY, 40, [OrderDate]) <= GETDATE() THEN 'Yes' ELSE 'No' END ELSE NULL END AS OverDue,
	TaxAmount AS PstAmount,
	CASE WHEN TaxAmount IS NULL THEN 'No' ELSE 'Yes' END AS PstCharged 
FROM [Order]
INNER JOIN Location
	ON Location.LocationId = [Order].LocationId
LEFT JOIN Users
	ON Users.Id = [Order].CreatedByUserId
LEFT JOIN Customer
    ON Customer.CustomerId = [Order].CustomerId
LEFT JOIN 
	( 
        SELECT OrderId, SUM(PaymentAmount) AS PaidAmount , STRING_AGG(PaymentTypeName, ', ') AS PaymentTypeName  
        FROM OrderPayment
        INNER JOIN PaymentType
	        ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId 
        GROUP BY OrderId
    ) AS OrderPayment
	ON OrderPayment.OrderId = [Order].OrderId
LEFT JOIN 
	( 
        SELECT OrderId, TaxAmount
        FROM OrderTax
		INNER JOIN Tax
			ON OrderTax.TaxId = Tax.TaxId
		WHERE TaxName like '%PST%'
    ) AS OrderTax
	ON OrderTax.OrderId = [Order].OrderId
WHERE OrderDate BETWEEN @fromDate AND @toDate
      AND [Order].LocationId IN @locIds
ORDER BY [Order].[OrderId] DESC";
                conn.Open();
                var locationIds = new List<int>();
                if (locationId == 0)
                {
                    locationIds = (await GetUserLocations(userId)).ToList();
                }
                else
                {
                    locationIds.Add(locationId);
                }

                var locIds = locationIds.ToArray();
                return await conn.QueryAsync<OrderViewModel>(query, new { locIds, fromDate, toDate });
            }
        }

        public async Task<IEnumerable<OrderViewModel>> GetOrdersByCustomer(int customerId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                    SELECT [Order].[OrderId]
                                          ,[CustomerId]
                                          ,[Order].[LocationId]
                                          ,FORMAT ([OrderDate], 'dd/MM/yyyy hh:mm tt') AS OrderDate
                                          ,[Total]
                                          ,[SubTotal]
                                          ,[TotalDiscount]
                                          ,[PstNumber]
                                          ,[Notes]
                                          ,[PoNumber]
                                          ,[Status]
                                          ,Users.GivenName
	                                      ,ISNULL(OrderPayment.PaidAmount, 0) AS PaidAmount,
	                                      Location.LocationName,
                                          PaymentTypeName
                                    FROM [Order]
                                    INNER JOIN Location
	                                    ON Location.LocationId = [Order].LocationId
                                    Left JOIN Users
	                                    ON Users.Id = [Order].CreatedByUserId
                                    LEFT JOIN 
	                                    ( 
                                            SELECT OrderId, SUM(PaymentAmount) AS PaidAmount , STRING_AGG(PaymentTypeName, ', ') AS PaymentTypeName  
                                            FROM OrderPayment
                                            INNER JOIN PaymentType
	                                            ON PaymentType.PaymentTypeId = OrderPayment.PaymentTypeId 
                                            GROUP BY OrderId
                                        ) AS OrderPayment
	                                    ON OrderPayment.OrderId = [Order].OrderId
                                    WHERE [Order].CustomerId = @CustomerId
                                    ORDER BY [Order].[OrderId] DESC
                                 ";
                conn.Open();
                return await conn.QueryAsync<OrderViewModel>(query, new { CustomerId = customerId });
            }
        }

        public async Task<InventoryViewModel> GetProductInventoryForValidation(int productId, int locationId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT Product.ProductId, ISNULL(Balance, 0) AS Balance, Product.ProductCode, Product.ProductName, [Location].LocationName, ISNULL(OnHold, 0) AS OnHold 
FROM Product
LEFT JOIN 
( SELECT ProductId, Balance, @locationId as LocationId
  FROM ProductInventory
  WHERE ProductInventory.ProductId = @productId
         AND ProductInventory.LocationId = @locationId) ProductInventory
	ON Product.ProductId = ProductInventory.ProductId
LEFT JOIN [Location]
	ON [Location].LocationId = ProductInventory.LocationId
LEFT JOIN (
	SELECT ProductId, LocationId, SUM(Amount) AS OnHold 
	FROM [Order]
	INNER JOIN OrderDetail
		ON [Order].OrderId = OrderDetail.OrderId
	WHERE LocationId = @locationId
		AND OrderDetail.ProductId = @productId
		AND [Status] = 'OnHold'
	GROUP BY ProductId, LocationId
) OnHoldOrders
ON OnHoldOrders.ProductId = ProductInventory.ProductId
AND OnHoldOrders.LocationId = ProductInventory.LocationId 
WHERE Product.ProductId = @productId
                                 ";
                conn.Open();
                return await conn.QueryFirstAsync<InventoryViewModel>(query, new { productId , locationId });
            }
        }

        private async Task<IEnumerable<int>> GetUserLocations(string userId)
        {
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
