using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace EcommerceApi.Repositories
{
    public class CustomerQueries
    {

        public readonly AppDb Db;
        public CustomerQueries(AppDb db)
        {
            Db = db;
        }

        public async Task<List<MySqlCustomer>> GetAllCustomers()
        {
            var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"
SELECT p.id,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_company_name' limit 1) as _pos_customer_company_name,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_phone' limit 1) as _pos_customer_phone,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_full_name' limit 1) as _pos_customer_full_name,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_first_name' limit 1) as _pos_customer_first_name,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_last_name' limit 1) as _pos_customer_last_name,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_gender' limit 1) as _pos_customer_gender,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_mobile' limit 1) as _pos_customer_mobile,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_fax' limit 1) as _pos_customer_fax,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_email' limit 1) as _pos_customer_email,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_country' limit 1) as _pos_customer_country,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_address' limit 1) as _pos_customer_address,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_city' limit 1) as _pos_customer_city,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_province' limit 1) as _pos_customer_province,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_postal_code' limit 1) as _pos_customer_postal_code,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_pst_number' limit 1) as _pos_customer_pst_number,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_accounttype' limit 1) as _pos_customer_accounttype,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_accountlimit' limit 1) as _pos_customer_accountlimit,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_secemail' limit 1) as _pos_customer_secemail,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_customer_contractorlink' limit 1) as _pos_customer_contractorlink
FROM wp_posts p 
where p.post_type IN ( '_pos_customer')  
";
            return await ReadAllAsync(await cmd.ExecuteReaderAsync());
        }

        private async Task<List<MySqlCustomer>> ReadAllAsync(DbDataReader reader)
        {
            var posts = new List<MySqlCustomer>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var post = new MySqlCustomer(Db)
                    {
                        id = await reader.GetFieldValueAsync<UInt64>(0),
                        _pos_customer_company_name = await reader.IsDBNullAsync(1) ? string.Empty : await reader.GetFieldValueAsync<string>(1),
                        _pos_customer_phone = await reader.IsDBNullAsync(2) ? string.Empty : await reader.GetFieldValueAsync<string>(2),
                        _pos_customer_full_name = await reader.IsDBNullAsync(3) ? string.Empty : await reader.GetFieldValueAsync<string>(3),
                        _pos_customer_first_name = await reader.IsDBNullAsync(4) ? string.Empty : await reader.GetFieldValueAsync<string>(4),
                        _pos_customer_last_name = await reader.IsDBNullAsync(5) ? string.Empty : await reader.GetFieldValueAsync<string>(5),
                        _pos_customer_gender = await reader.IsDBNullAsync(6) ? string.Empty : await reader.GetFieldValueAsync<string>(6),
                        _pos_customer_mobile = await reader.IsDBNullAsync(7) ? string.Empty : await reader.GetFieldValueAsync<string>(7),
                        _pos_customer_fax = await reader.IsDBNullAsync(8) ? string.Empty : await reader.GetFieldValueAsync<string>(8),
                        _pos_customer_email = await reader.IsDBNullAsync(9) ? string.Empty : await reader.GetFieldValueAsync<string>(9),
                        _pos_customer_country = await reader.IsDBNullAsync(10) ? string.Empty : await reader.GetFieldValueAsync<string>(10),
                        _pos_customer_address = await reader.IsDBNullAsync(11) ? string.Empty : await reader.GetFieldValueAsync<string>(11),
                        _pos_customer_city = await reader.IsDBNullAsync(12) ? string.Empty : await reader.GetFieldValueAsync<string>(12),
                        _pos_customer_province = await reader.IsDBNullAsync(13) ? string.Empty : await reader.GetFieldValueAsync<string>(13),
                        _pos_customer_postal_code = await reader.IsDBNullAsync(14) ? string.Empty : await reader.GetFieldValueAsync<string>(14),
                        _pos_customer_pst_number = await reader.IsDBNullAsync(15) ? string.Empty : await reader.GetFieldValueAsync<string>(15),
                        _pos_customer_accounttype = await reader.IsDBNullAsync(16) ? string.Empty : await reader.GetFieldValueAsync<string>(16),
                        _pos_customer_accountlimit = await reader.IsDBNullAsync(17) ? string.Empty : await reader.GetFieldValueAsync<string>(17),
                        _pos_customer_secemail = await reader.IsDBNullAsync(18) ? string.Empty : await reader.GetFieldValueAsync<string>(18),
                        _pos_customer_contractorlink = await reader.IsDBNullAsync(19) ? string.Empty : await reader.GetFieldValueAsync<string>(19)
                    };
                    posts.Add(post);
                }
            }
            return posts;
        }
    }
}
