using System.Collections.Generic;
using System.Data.SqlClient;
using ShopEase.Models;

namespace ShopEase.DAL
{
    public class AdminRepository
    {
        // Dashboard stats
        public int GetTotalProducts()
        {
            string query = "SELECT COUNT(*) FROM Products WHERE IsActive = 1";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetTotalOrders()
        {
            string query = "SELECT COUNT(*) FROM Orders";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetTotalUsers()
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Role = 'Customer'";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public decimal GetTotalRevenue()
        {
            string query = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders WHERE Status != 'Cancelled'";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                return (decimal)cmd.ExecuteScalar();
            }
        }

        // Recent 5 orders for dashboard
        public List<Order> GetRecentOrders()
        {
            var orders = new List<Order>();
            string query = @"
                SELECT TOP 5 o.OrderID, o.UserID, u.FullName,
                       o.TotalAmount, o.Status, o.ShipAddress, o.CreatedAt
                FROM Orders o
                INNER JOIN Users u ON o.UserID = u.UserID
                ORDER BY o.CreatedAt DESC";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new Order
                        {
                            OrderID = (int)reader["OrderID"],
                            UserID = (int)reader["UserID"],
                            TotalAmount = (decimal)reader["TotalAmount"],
                            Status = reader["Status"].ToString(),
                            ShipAddress = reader["ShipAddress"].ToString(),
                            CreatedAt = (System.DateTime)reader["CreatedAt"]
                        };
                        // Temporarily store customer name in a tag
                        order.ShipAddress = reader["FullName"].ToString()
                                          + "||" + order.ShipAddress;
                        orders.Add(order);
                    }
                }
            }
            return orders;
        }
    }
}