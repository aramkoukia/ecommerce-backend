using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public class MySqlCustomer
    {
        [JsonIgnore]
        public AppDb Db { get; set; }

        public MySqlCustomer(AppDb db = null)
        {
            Db = db;
        }

        public UInt64 id { get; set; }
        public string _pos_customer_company_name { get; set; }
        public string _pos_customer_phone { get; set; }
        public string _pos_customer_full_name { get; set; }
        public string _pos_customer_first_name { get; set; }
        public string _pos_customer_last_name { get; set; }
        public string _pos_customer_gender { get; set; }
        public string _pos_customer_mobile { get; set; }
        public string _pos_customer_fax { get; set; }
        public string _pos_customer_email { get; set; }
        public string _pos_customer_country { get; set; }
        public string _pos_customer_address { get; set; }
        public string _pos_customer_city { get; set; }
        public string _pos_customer_province { get; set; }
        public string _pos_customer_postal_code { get; set; }
        public string _pos_customer_pst_number { get; set; }
        public string _pos_customer_accounttype { get; set; }
        public string _pos_customer_accountlimit { get; set; }
        public string _pos_customer_secemail { get; set; }
        public string _pos_customer_contractorlink { get; set; }
    }
}