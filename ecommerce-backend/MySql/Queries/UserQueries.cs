using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public class UserQueries
    {

        public readonly AppDb Db;
        public UserQueries(AppDb db)
        {
            Db = db;
        }

        public async Task<List<MySqlUser>> GetAllUsers()
        {
            var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"
select user_login, user_email,
(select meta_value from wp_usermeta where user_id = wp_users.id and meta_key = 'first_Name' limit 1) as first_Name,
(select meta_value from wp_usermeta where user_id = wp_users.id and meta_key = 'last_name' limit 1) as last_name
from wp_users where ID in (3,4,5,6,7,25,655,1102,1199,1232,1231,1440,1614)
";
            return await ReadAllProductsAsync(await cmd.ExecuteReaderAsync());
        }

        private async Task<List<MySqlUser>> ReadAllProductsAsync(DbDataReader reader)
        {
            var posts = new List<MySqlUser>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var post = new MySqlUser(Db)
                    {
                        user_login = await reader.IsDBNullAsync(0) ? string.Empty : await reader.GetFieldValueAsync<string>(0),
                        user_email = await reader.IsDBNullAsync(1) ? string.Empty : await reader.GetFieldValueAsync<string>(1),
                        first_name = await reader.IsDBNullAsync(2) ? string.Empty : await reader.GetFieldValueAsync<string>(2),
                        last_name = await reader.IsDBNullAsync(3) ? string.Empty : await reader.GetFieldValueAsync<string>(3),
                    };
                    posts.Add(post);
                }
            }
            return posts;
        }
    }
}
