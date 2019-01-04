using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public class MySqlProduct
    {
        [JsonIgnore]
        public AppDb Db { get; set; }

        public MySqlProduct(AppDb db = null)
        {
            Db = db;
        }

        public UInt64 id { get; set; }
        public string post_title { get; set; }
        public string _price { get; set; }
        public string _sku { get; set; }
        public string _cat_id { get; set; }
        public string _category { get; set; }
    }
}