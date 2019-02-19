using Newtonsoft.Json;
using System;

namespace EcommerceApi.Repositories
{
    public class MySqlOrderLineItem
    {
        [JsonIgnore]
        public AppDb Db { get; set; }

        public MySqlOrderLineItem(AppDb db = null)
        {
            Db = db;
        }

        public UInt64 order_id { get; set; }
        public string _qty { get; set; }
        public string _product_id { get; set; }
        public string _line_price { get; set; }
        public string _line_subtotal { get; set; }
        public string _line_total { get; set; }
        public string _order_product_id { get; set; }
    }
}
    