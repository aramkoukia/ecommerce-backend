using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public async Task<IEnumerable<OrderViewModel>> GetOrders(bool showAll, int locationId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                    SELECT [Order].[OrderId]
                                          ,[Order].[CustomerId]
                                          ,[Order].[LocationId]
                                          ,[OrderDate]
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
                                          ISNULL(Customer.CompanyName, 'WALK-IN') AS CompanyName
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
                                    WHERE (@showAll != 0 OR OrderDate >= Dateadd(month, -3, GetDate()))
										  AND ([Order].LocationId = @locationId OR @locationId = 0)
                                    ORDER BY [Order].[OrderId] DESC
                                 ";
                conn.Open();
                return await conn.QueryAsync<OrderViewModel>(query, new { locationId, showAll });
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
                                          ,[OrderDate]
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
    }
}
