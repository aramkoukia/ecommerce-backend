using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace EcommerceApi.Repositories
{
    public class ProductQueries
    {

        public readonly AppDb Db;
        public ProductQueries(AppDb db)
        {
            Db = db;
        }

        public async Task<List<MySqlProduct>> GetAllProducts()
        {
            var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"
SELECT p.id, p.post_title,
    (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_price' limit 1) as _price,
	(select meta_value from wp_postmeta where post_id = p.id and meta_key = '_sku' limit 1) as _sku,
	(select meta_value from wp_postmeta where post_id = p.id and meta_key = '_yoast_wpseo_primary_product_cat' limit 1) as _cat_id,
	(SELECT name FROM wp_terms WHERE term_id = (select meta_value from wp_postmeta where post_id = p.id and meta_key = '_yoast_wpseo_primary_product_cat' limit 1) limit 1) as _category
	FROM wp_posts p 
where p.post_type IN ( 'product')
";
            return await ReadAllProductsAsync(await cmd.ExecuteReaderAsync());
        }

        public async Task<List<MySqlProductInventories>> GetAllProductInventories()
        {
            var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"
                select id, product_id, warehouse_id, stock from wp_inventory_manager_product_warehouse
            ";
            return await ReadAllProductInventoriesAsync(await cmd.ExecuteReaderAsync());
        }

        private async Task<List<MySqlProduct>> ReadAllProductsAsync(DbDataReader reader)
        {
            var posts = new List<MySqlProduct>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var post = new MySqlProduct(Db)
                    {
                        id = await reader.GetFieldValueAsync<UInt64>(0),
                        post_title = await reader.IsDBNullAsync(1) ? string.Empty : await reader.GetFieldValueAsync<string>(1),
                        _price = await reader.IsDBNullAsync(2) ? string.Empty : await reader.GetFieldValueAsync<string>(2),
                        _sku = await reader.IsDBNullAsync(3) ? string.Empty : await reader.GetFieldValueAsync<string>(3),
                        _cat_id = await reader.IsDBNullAsync(4) ? string.Empty : await reader.GetFieldValueAsync<string>(4),
                        _category = await reader.IsDBNullAsync(5) ? string.Empty : await reader.GetFieldValueAsync<string>(5)
                    };
                    posts.Add(post);
                }
            }
            return posts;
        }

        private async Task<List<MySqlProductInventories>> ReadAllProductInventoriesAsync(DbDataReader reader)
        {
            var posts = new List<MySqlProductInventories>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var post = new MySqlProductInventories(Db)
                    {
                        id = await reader.GetFieldValueAsync<int>(0),
                        product_id = await reader.GetFieldValueAsync<int>(1),
                        warehouse_id = await reader.GetFieldValueAsync<int>(2),
                        stock = await reader.IsDBNullAsync(3) ? string.Empty : await reader.GetFieldValueAsync<string>(3)
                    };
                    posts.Add(post);
                }
            }
            return posts;
        }
    }
}
