using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace EcommerceApi.Repositories
{
    public class OrderQueries
    {

        public readonly AppDb Db;
        public OrderQueries(AppDb db)
        {
            Db = db;
        }

        public async Task<List<MySqlOrder>> GetAllOrders()
        {
            var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"
SELECT p.id,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_total' limit 1) as _pos_sale_total,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_tax' limit 1) as _pos_sale_tax,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_tax_PST' limit 1) as _pos_sale_tax_PST,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_note' limit 1) as _pos_sale_note,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_status' limit 1) as _pos_status,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_user' limit 1) as _pos_sale_user,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_subtotal' limit 1) as _pos_sale_subtotal,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_tax_GST' limit 1) as _pos_sale_tax_GST,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_location_name' limit 1) as _pos_location_name,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_customer_id' limit 1) as _pos_sale_customer_id,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_po_number' limit 1) as _pos_po_number,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_discount' limit 1) as _pos_sale_discount,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_holdenddate' limit 1) as _pos_holdenddate,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_vc_post_settings' limit 1) as _vc_post_settings,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_tax_GST (AB)' limit 1) as _pos_sale_tax_GST_AB,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = 'nm_members' limit 1) as nm_members,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_tax_No Tax' limit 1) as _pos_sale_tax_No_Tax,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_tax_QST (QC)' limit 1) as _pos_sale_tax__ST_QC,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_tax_GST (QC)' limit 1) as _pos_sale_tax_GST_QC,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_Auth_user' limit 1) as _pos_Auth_user,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_return_phonenumber' limit 1) as _pos_return_phonenumber,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_linkedInvoice' limit 1) as _pos_linkedInvoice,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_pos_sale_tax_0' limit 1) as _pos_sale_tax_0,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_wp_old_date' limit 1) as wp_old_date,
    p.post_date
FROM wp_posts p 
where p.post_type IN ( '_pos_sale')     
";
            return await ReadAllAsync(await cmd.ExecuteReaderAsync());
        }

        private async Task<List<MySqlOrder>> ReadAllAsync(DbDataReader reader)
        {
            var posts = new List<MySqlOrder>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var post = new MySqlOrder(Db)
                    {
                        id = await reader.GetFieldValueAsync<UInt64>(0),
                        _pos_sale_total = await reader.IsDBNullAsync(1) ? string.Empty : await reader.GetFieldValueAsync<string>(1),
                        _pos_sale_tax = await reader.IsDBNullAsync(2) ? string.Empty : await reader.GetFieldValueAsync<string>(2),
                        _pos_sale_tax_PST = await reader.IsDBNullAsync(3) ? string.Empty : await reader.GetFieldValueAsync<string>(3),
                        _pos_sale_note = await reader.IsDBNullAsync(4) ? string.Empty : await reader.GetFieldValueAsync<string>(4),
                        _pos_status = await reader.IsDBNullAsync(5) ? string.Empty : await reader.GetFieldValueAsync<string>(5),
                        _pos_sale_user = await reader.IsDBNullAsync(6) ? string.Empty : await reader.GetFieldValueAsync<string>(6),
                        _pos_sale_subtotal = await reader.IsDBNullAsync(7) ? string.Empty : await reader.GetFieldValueAsync<string>(7),
                        _pos_sale_tax_GST = await reader.IsDBNullAsync(8) ? string.Empty : await reader.GetFieldValueAsync<string>(8),
                        _pos_location_name = await reader.IsDBNullAsync(9) ? string.Empty : await reader.GetFieldValueAsync<string>(9),
                        _pos_sale_customer_id = await reader.IsDBNullAsync(10) ? string.Empty : await reader.GetFieldValueAsync<string>(10),
                        _pos_po_number = await reader.IsDBNullAsync(11) ? string.Empty : await reader.GetFieldValueAsync<string>(11),
                        _pos_sale_discount = await reader.IsDBNullAsync(12) ? string.Empty : await reader.GetFieldValueAsync<string>(12),
                        _pos_holdenddate = await reader.IsDBNullAsync(13) ? string.Empty : await reader.GetFieldValueAsync<string>(13),
                        _vc_post_settings = await reader.IsDBNullAsync(14) ? string.Empty : await reader.GetFieldValueAsync<string>(14),
                        _pos_sale_tax_GST_AB = await reader.IsDBNullAsync(15) ? string.Empty : await reader.GetFieldValueAsync<string>(15),
                        nm_members = await reader.IsDBNullAsync(16) ? string.Empty : await reader.GetFieldValueAsync<string>(16),
                        _pos_sale_tax_No_Tax = await reader.IsDBNullAsync(17) ? string.Empty : await reader.GetFieldValueAsync<string>(17),
                        _pos_sale_tax__ST_QC = await reader.IsDBNullAsync(18) ? string.Empty : await reader.GetFieldValueAsync<string>(18),
                        _pos_sale_tax_GST_QC = await reader.IsDBNullAsync(19) ? string.Empty : await reader.GetFieldValueAsync<string>(19),
                        _pos_Auth_user = await reader.IsDBNullAsync(20) ? string.Empty : await reader.GetFieldValueAsync<string>(20),
                        _pos_return_phonenumber = await reader.IsDBNullAsync(21) ? string.Empty : await reader.GetFieldValueAsync<string>(21),
                        _pos_linkedInvoice = await reader.IsDBNullAsync(22) ? string.Empty : await reader.GetFieldValueAsync<string>(22),
                        _pos_sale_tax_0 = await reader.IsDBNullAsync(23) ? string.Empty : await reader.GetFieldValueAsync<string>(23),
                        wp_old_date = await reader.IsDBNullAsync(24) ? string.Empty : await reader.GetFieldValueAsync<string>(24),
                        post_date = await reader.GetFieldValueAsync<DateTime>(25),
                    };
                    posts.Add(post);
                }
            }
            return posts;
        }
    }
}
