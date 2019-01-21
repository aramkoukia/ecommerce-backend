using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public async Task<IEnumerable<CustomerViewModel>> GetCustomersWithBalance()
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
			                                    AND Status Like '%Paid%' 
		                                GROUP BY [Order].OrderId, [Order].CustomerId) AS UnPaidOrders
	                                GROUP BY CustomerId
                                ) CustomerAccount
                                ON CustomerAccount.CustomerId = Customer.CustomerId
                                 ";
                conn.Open();
                return await conn.QueryAsync<CustomerViewModel>(query);
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
			                                    AND Status Like '%Paid%' 
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
    }
}
