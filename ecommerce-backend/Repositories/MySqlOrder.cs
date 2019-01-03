using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public class MySqlOrder
    {
        [JsonIgnore]
        public AppDb Db { get; set; }

        public MySqlOrder(AppDb db = null)
        {
            Db = db;
        }

        public UInt64 id { get; set; }
        public string _pos_sale_total { get; set; }
        public string _pos_sale_tax { get; set; }
        public string _pos_sale_tax_PST { get; set; }
        public string _pos_sale_note { get; set; }
        public string _pos_status { get; set; }
        public string _pos_sale_user { get; set; }
        public string _pos_sale_subtotal { get; set; }
        public string _pos_sale_tax_GST { get; set; }
        public string _pos_location_name { get; set; }
        public string _pos_sale_customer_id { get; set; }
        public string _pos_po_number { get; set; }
        public string _pos_sale_discount { get; set; }
        public string _pos_holdenddate { get; set; }
        public string _vc_post_settings { get; set; }
        public string _pos_sale_tax_GST_AB { get; set; }
        public string nm_members { get; set; }
        public string _pos_sale_tax_No_Tax { get; set; }
        public string _pos_sale_tax__ST_QC { get; set; }
        public string _pos_sale_tax_GST_QC { get; set; }
        public string _pos_Auth_user { get; set; }
        public string _pos_return_phonenumber { get; set; }
        public string _pos_linkedInvoice { get; set; }
        public string _pos_sale_tax_0 { get; set; }
        public string wp_old_date { get; set; }
        public DateTime post_date { get; set; }
    }
}