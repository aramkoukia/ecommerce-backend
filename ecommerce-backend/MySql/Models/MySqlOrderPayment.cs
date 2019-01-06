using Newtonsoft.Json;
using System;

namespace EcommerceApi.Repositories
{
    public class MySqlOrderPayment
    {
        [JsonIgnore]
        public AppDb Db { get; set; }

        public MySqlOrderPayment(AppDb db = null)
        {
            Db = db;
        }

        public UInt64 id { get; set; }
        public DateTime post_date { get; set; }
        public string _pos_payment_amount { get; set; }
        public string _pos_payment_order_id { get; set; }
        public string _pos_payment_paymentType_id { get; set; }
        public UInt64 _pos_payment_customer_id { get; set; }
        public string _pos_payment_lastFour { get; set; }
        public string _pos_payment_AuthCode { get; set; }
        public string _pos_payment_comment { get; set; }
        public string _pos_payment_chequeNo { get; set; }
        public UInt64 post_author { get; set; }
    }
}