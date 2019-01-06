using Newtonsoft.Json;
using System;

namespace EcommerceApi.Repositories
{
    public class MySqlProductInventories
    {
        [JsonIgnore]
        public AppDb Db { get; set; }

        public MySqlProductInventories(AppDb db = null)
        {
            Db = db;
        }

        public int id { get; set; }
        public int product_id { get; set; }
        public int warehouse_id { get; set; }
        public string stock { get; set; }
    }
}