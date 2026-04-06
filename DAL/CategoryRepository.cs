using System.Collections.Generic;
using System.Data.SqlClient;
using ShopEase.Models;

namespace ShopEase.DAL
{
    public class CategoryRepository
    {
        public List<Category> GetAll()
        {
            var list = new List<Category>();
            string query = "SELECT CategoryID, Name, Description FROM Categories WHERE IsActive = 1";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Category
                        {
                            CategoryID = (int)reader["CategoryID"],
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString()
                        });
                    }
                }
            }
            return list;
        }
    }
}