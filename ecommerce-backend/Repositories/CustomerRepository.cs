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
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public CustomerRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<CustomerViewModel>> GetCustomersWithBalance(bool showDisabled)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                SELECT Customer.*, CustomerAccount.AccountBalance
                                FROM Customer
                                LEFT JOIN (
	                                SELECT CustomerId, SUM(ISNULL(OrderRemainingTotal, 0)) AS AccountBalance FROM (
		                                SELECT [Order].CustomerId, [Order].OrderId, SUM(Total) - SUM(ISNULL(PaymentAmount,0)) AS OrderRemainingTotal
		                                FROM [Order]
		                                LEFT JOIN (
			                                SELECT OrderId, SUM(PaymentAmount) AS PaymentAmount
			                                FROM OrderPayment
			                                GROUP BY OrderId
		                                ) OrderPayment
		                                ON [Order].OrderId = OrderPayment.OrderId
		                                WHERE CustomerId IS NOT NULL 
			                                    AND (Status = 'Account' OR (Status = 'Return' AND IsAccountReturn = 1))
                                        GROUP BY [Order].OrderId, [Order].CustomerId) AS UnPaidOrders
	                                GROUP BY CustomerId
                                ) CustomerAccount
                                ON CustomerAccount.CustomerId = Customer.CustomerId
                                WHERE (@showDisabled = 1 OR Customer.Disabled = 0)
                                 ";
                conn.Open();
                return await conn.QueryAsync<CustomerViewModel>(query, new { showDisabled });
            }
        }

        public async Task<decimal> GetCustomerBalance(int customerId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"SELECT SUM(ISNULL(OrderRemainingTotal, 0)) AS AccountBalance FROM (
		                                SELECT [Order].CustomerId, [Order].OrderId, SUM(Total) - SUM(ISNULL(PaymentAmount,0)) AS OrderRemainingTotal
		                                FROM [Order]
		                                LEFT JOIN (
			                                SELECT OrderId, SUM(PaymentAmount) AS PaymentAmount
			                                FROM OrderPayment
			                                GROUP BY OrderId
		                                ) OrderPayment
		                                ON [Order].OrderId = OrderPayment.OrderId
		                                WHERE CustomerId = @customerId
			                                  AND (Status = 'Account' OR (Status = 'Return' AND IsAccountReturn = 1)) 
		                                GROUP BY [Order].OrderId, [Order].CustomerId) AS UnPaidOrders
	                                GROUP BY CustomerId
                                 ";
                conn.Open();
                return await conn.QueryFirstOrDefaultAsync<int>(query, new { customerId });
            }
        }

        public async Task<IEnumerable<CustomerViewModel>> GetCustomers()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                SELECT Customer.*
                                FROM Customer
                                ";
                conn.Open();
                return await conn.QueryAsync<CustomerViewModel>(query);
            }
        }

        public async Task<CustomerViewModel> GetCustomer(int customerId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                SELECT Customer.*, CustomerAccount.AccountBalance
                                FROM Customer
                                LEFT JOIN (
	                                SELECT CustomerId, SUM(ISNULL(OrderRemainingTotal, 0)) AS AccountBalance FROM (
		                                SELECT [Order].CustomerId, [Order].OrderId, SUM(Total) - SUM(ISNULL(PaymentAmount,0)) AS OrderRemainingTotal
		                                FROM [Order]
		                                LEFT JOIN (
			                                SELECT OrderId, SUM(PaymentAmount) AS PaymentAmount
			                                FROM OrderPayment
			                                GROUP BY OrderId
		                                ) OrderPayment
		                                ON [Order].OrderId = OrderPayment.OrderId
		                                WHERE CustomerId IS NOT NULL 
			                                  AND (Status = 'Account' OR (Status = 'Return' AND IsAccountReturn = 1))
		                                GROUP BY [Order].OrderId, [Order].CustomerId) AS UnPaidOrders
	                                GROUP BY CustomerId
                                ) CustomerAccount
                                ON CustomerAccount.CustomerId = Customer.CustomerId
                                WHERE Customer.CustomerId = @CustomerId
                                 ";
                conn.Open();
                return await conn.QueryFirstAsync<CustomerViewModel>(query, new { CustomerId = customerId });
            }
        }


        public async Task<IEnumerable<CustomerOrderSummaryViewModel>> GetCustomerOrderSummary(int customerId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
select [Status], 
       CustomerId, 
       FORMAT(Count(OrderId), 'N0') AS OrderCount, 
	   FORMAT(SUM(Total), 'N0') AS OrderTotal,
	   FORMAT(SUM(Subtotal), 'N0') AS OrderSubTotal
from [order]
WHERE CustomerId = @customerId
Group BY status, CustomerId";
                conn.Open();
                return await conn.QueryAsync<CustomerOrderSummaryViewModel>(query, new { customerId });
            }
        }

        public async Task<IEnumerable<CustomerAwaitingPaymentViewModel>> GetCustomerAwaitingPaymentOrders()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT Customer.* , AwaitingOrders.OrderCount, AwaitingOrders.Total
FROM Customer
INNER JOIN  (
	SELECT DISTINCT CustomerId, SUM(Total) AS Total, Count(OrderId) AS OrderCount
	FROM [Order]
	WHERE [Order].[Status] IN ('Account')
	GROUP BY CustomerId
) AwaitingOrders
ON AwaitingOrders.CustomerId = Customer.CustomerId;

SELECT 
[Order].OrderId, PoNumber, 
OrderDate, DateAdd(DAY, 30, OrderDate) As DueDate, 
[Order].Total, 
CASE [Order].[Status] WHEN 'Account' THEN 'Awaiting Payment' END AS [Status], 
Customer.CompanyName, Customer.CustomerCode, Customer.[Address], 
Customer.City, Customer.Province, Customer.PostalCode, Customer.CustomerId
FROM [Order]
INNER JOIN Customer
	ON Customer.CustomerId = [Order].CustomerId
WHERE [Order].[Status] IN ('Account')
";
                conn.Open();

                var result = await conn.QueryMultipleAsync(query);

                var customers = result.Read<CustomerAwaitingPaymentViewModel>().ToList();
                var orders = result.Read<CustomerAwaitingPaymentDetail>().ToList();
                foreach (var customer in customers)
                {
                    customer.CustomerAwaitingPaymentDetail.AddRange(orders.Where(p => p.CustomerId == customer.CustomerId));
                }

                return customers;
            }
        }
    }
}
