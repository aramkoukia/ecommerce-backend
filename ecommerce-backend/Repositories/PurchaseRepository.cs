using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using EcommerceApi.ViewModel;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public PurchaseRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<PurchaseViewModel>> GetPurchases()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                    SELECT [Purchase].[PurchaseId]
                                          ,[Supplier]
                                          ,[PurchaseDate]
                                          ,[Total]
                                          ,[SubTotal]
                                          ,[Notes]
                                          ,[Status]
                                          ,[PoNumber]
                                          ,[DeliveryDate]
                                          ,[CreatedByUserId]
                                    FROM [Purchase]
                                    ORDER BY CreatedDate DESC
                                 ";
                conn.Open();
                return await conn.QueryAsync<PurchaseViewModel>(query);
            }
        }

        public async Task<IEnumerable<PurchaseDetailViewModel>> GetPurchaseDetails()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT Purchase.PurchaseId, Purchase.Supplier, Purchase.PONumber,
       PurchaseDetailPlan.ProductId, ProductCode, ProductName, 
	   PurchaseDetailPlan.Amount AS PlanAmount, PurchaseDetailPlan.UnitPrice AS PlanPrice, PurchaseDetailPlan.OverheadCost AS PlanOverheadCost,
	   PurchaseDetailPaid.Amount AS PaidAmount, PurchaseDetailPaid.UnitPrice AS PaidPrice, PurchaseDetailPaid.OverheadCost AS PaidOverheadCost, 
	   PurchaseDetailPlan.Amount - ISNULL(PurchaseDetailPaid.Amount, 0) AS RemainToPay,
	   PurchaseDetailOnDelivery.Amount AS OnDeliveryAmount, PurchaseDetailOnDelivery.UnitPrice As OnDelivertPrice, PurchaseDetailOnDelivery.OverheadCost AS OnDeliveryOverheadCost,
	   PurchaseDetailCustomClearance.Amount AS CustomClearanceAmount, PurchaseDetailCustomClearance.UnitPrice AS CustomClearancePrice, PurchaseDetailCustomClearance.OverheadCost AS CustomClearanceOverheadCost,
	   PurchaseDetailArrived.Amount AS ArrivedAmount, PurchaseDetailArrived.UnitPrice AS ArrivedPrice, PurchaseDetailArrived.OverheadCost AS ArrivedOverheadCost, PurchaseDetailArrived.ArrivedDate,
	   PurchaseDetailPlan.Amount - ISNULL(PurchaseDetailArrived.Amount,0) AS RemainToArrive
FROM Purchase
INNER JOIN (
  SELECT PurchaseId, ProductId, SUM(Amount) AS Amount, AVG(UnitPrice) AS UnitPrice, AVG(OverheadCost) AS OverheadCost
  FROM PurchaseDetail
  WHERE [Status] = 'Plan'
  GROUP BY PurchaseId, ProductId) PurchaseDetailPlan
ON Purchase.PurchaseId = PurchaseDetailPlan.PurchaseId
INNER JOIN Product
	ON Product.ProductId = PurchaseDetailPlan.ProductId 
LEFT JOIN (
  SELECT PurchaseId, ProductId, SUM(Amount) AS Amount, AVG(UnitPrice) AS UnitPrice, AVG(OverheadCost) AS OverheadCost 
  FROM PurchaseDetail
  WHERE [Status] = 'Paid'
  GROUP BY PurchaseId, ProductId) PurchaseDetailPaid
ON Purchase.PurchaseId = PurchaseDetailPaid.PurchaseId
   AND PurchaseDetailPlan.ProductId = PurchaseDetailPaid.ProductId
LEFT JOIN (
  SELECT PurchaseId, ProductId, SUM(Amount) AS Amount, AVG(UnitPrice) AS UnitPrice, AVG(OverheadCost) AS OverheadCost
  FROM PurchaseDetail
  WHERE [Status] = 'OnDelivery'
  GROUP BY PurchaseId, ProductId) PurchaseDetailOnDelivery
ON Purchase.PurchaseId = PurchaseDetailOnDelivery.PurchaseId
   AND PurchaseDetailPlan.ProductId = PurchaseDetailOnDelivery.ProductId
LEFT JOIN (
  SELECT PurchaseId, ProductId, SUM(Amount) AS Amount, AVG(UnitPrice) AS UnitPrice, AVG(OverheadCost) AS OverheadCost
  FROM PurchaseDetail
  WHERE [Status] = 'CustomClearance'
  GROUP BY PurchaseId, ProductId) PurchaseDetailCustomClearance
ON Purchase.PurchaseId = PurchaseDetailCustomClearance.PurchaseId
   AND PurchaseDetailPlan.ProductId = PurchaseDetailCustomClearance.ProductId
LEFT JOIN (
  SELECT PurchaseId, ProductId, SUM(Amount) AS Amount, AVG(UnitPrice) AS UnitPrice, AVG(OverheadCost) AS OverheadCost, MIN(ArrivedDate) AS ArrivedDate
  FROM PurchaseDetail
  WHERE [Status] = 'Arrived'
  GROUP BY PurchaseId, ProductId) PurchaseDetailArrived
ON Purchase.PurchaseId = PurchaseDetailArrived.PurchaseId
   AND PurchaseDetailPlan.ProductId = PurchaseDetailArrived.ProductId
ORDER BY Purchase.PoNumber
";
                conn.Open();
                return await conn.QueryAsync<PurchaseDetailViewModel>(query);
            }
        }
    }
}
