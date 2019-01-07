using Newtonsoft.Json;

namespace EcommerceApi.Repositories
{
    public class MySqlUser
    {
        [JsonIgnore]
        public AppDb Db { get; set; }

        public MySqlUser(AppDb db = null)
        {
            Db = db;
        }

        public string user_login { get; set; }
        public string user_email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }
}