using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ShopEase.Models;

namespace ShopEase.DAL
{
    public class ProductRepository
    {
        // Get all active products (with optional category filter and search)
        public List<Product> GetAll(int? categoryID = null, string search = null, string sort = null)
        {
            var products = new List<Product>();

            string query = @"
                SELECT p.ProductID, p.CategoryID, c.Name AS CategoryName,
                       p.Name, p.Description, p.Price, p.Stock, p.ImageURL
                FROM Products p
                INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                WHERE p.IsActive = 1";

            if (categoryID.HasValue)
                query += " AND p.CategoryID = @CategoryID";

            if (!string.IsNullOrEmpty(search))
                query += " AND p.Name LIKE @Search";

            // ✅ FIXED SORT LOGIC (no C# 8 switch)
            if (sort == "price_asc")
                query += " ORDER BY p.Price ASC";
            else if (sort == "price_desc")
                query += " ORDER BY p.Price DESC";
            else if (sort == "name")
                query += " ORDER BY p.Name ASC";
            else
                query += " ORDER BY p.ProductID DESC";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (categoryID.HasValue)
                    cmd.Parameters.AddWithValue("@CategoryID", categoryID.Value);

                if (!string.IsNullOrEmpty(search))
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }

            return products;
        }

        // Get single product by ID
        public Product GetByID(int productID)
        {
            Product product = null;

            string query = @"
                SELECT p.ProductID, p.CategoryID, c.Name AS CategoryName,
                       p.Name, p.Description, p.Price, p.Stock, p.ImageURL
                FROM Products p
                INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                WHERE p.ProductID = @ProductID AND p.IsActive = 1";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ProductID", productID);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        product = MapProduct(reader);
                }
            }

            return product;
        }

        // Get featured products for home page (latest 4)
        public List<Product> GetFeatured(int count = 4)
        {
            var products = new List<Product>();

            string query = @"
                SELECT TOP (@Count) p.ProductID, p.CategoryID, c.Name AS CategoryName,
                       p.Name, p.Description, p.Price, p.Stock, p.ImageURL
                FROM Products p
                INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                WHERE p.IsActive = 1
                ORDER BY p.ProductID DESC";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Count", count);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }

            return products;
        }

        // Insert new product
        public void Insert(Product p)
        {
            string query = @"
                INSERT INTO Products (CategoryID, Name, Description, Price, Stock, ImageURL)
                VALUES (@CategoryID, @Name, @Description, @Price, @Stock, @ImageURL)";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CategoryID", p.CategoryID);
                cmd.Parameters.AddWithValue("@Name", p.Name);
                cmd.Parameters.AddWithValue("@Description", p.Description);
                cmd.Parameters.AddWithValue("@Price", p.Price);
                cmd.Parameters.AddWithValue("@Stock", p.Stock);
                cmd.Parameters.AddWithValue("@ImageURL", p.ImageURL ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Update product
        public void Update(Product p)
        {
            string query = @"
                UPDATE Products
                SET CategoryID=@CategoryID,
                    Name=@Name,
                    Description=@Description,
                    Price=@Price,
                    Stock=@Stock,
                    ImageURL=@ImageURL
                WHERE ProductID=@ProductID";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ProductID", p.ProductID);
                cmd.Parameters.AddWithValue("@CategoryID", p.CategoryID);
                cmd.Parameters.AddWithValue("@Name", p.Name);
                cmd.Parameters.AddWithValue("@Description", p.Description);
                cmd.Parameters.AddWithValue("@Price", p.Price);
                cmd.Parameters.AddWithValue("@Stock", p.Stock);
                cmd.Parameters.AddWithValue("@ImageURL", p.ImageURL ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Soft delete
        public void Delete(int productID)
        {
            string query = "UPDATE Products SET IsActive = 0 WHERE ProductID = @ProductID";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ProductID", productID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Map DB row to Product object
        private Product MapProduct(SqlDataReader r)
        {
            return new Product
            {
                ProductID = (int)r["ProductID"],
                CategoryID = (int)r["CategoryID"],
                CategoryName = r["CategoryName"].ToString(),
                Name = r["Name"].ToString(),
                Description = r["Description"].ToString(),
                Price = (decimal)r["Price"],
                Stock = (int)r["Stock"],
                ImageURL = r["ImageURL"].ToString()
            };
        }
    }
}