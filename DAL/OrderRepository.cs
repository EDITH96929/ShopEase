using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ShopEase.Models;

namespace ShopEase.DAL
{
    public class OrderRepository
    {
        // Place a new order using a transaction (atomic — either all saves or none)
        public int PlaceOrder(Order order, List<CartItem> cartItems)
        {
            int newOrderID = 0;

            using (var conn = DBHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Insert the order header
                    string orderQuery = @"
                        INSERT INTO Orders (UserID, TotalAmount, Status, ShipAddress)
                        VALUES (@UserID, @TotalAmount, 'Pending', @ShipAddress);
                        SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(orderQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@UserID", order.UserID);
                        cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                        cmd.Parameters.AddWithValue("@ShipAddress", order.ShipAddress);
                        newOrderID = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 2. Insert each order item + reduce stock
                    foreach (var item in cartItems)
                    {
                        string itemQuery = @"
                            INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice)
                            VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice)";

                        using (var cmd = new SqlCommand(itemQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", newOrderID);
                            cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@UnitPrice", item.Price);
                            cmd.ExecuteNonQuery();
                        }

                        // Reduce stock
                        string stockQuery = @"
                            UPDATE Products
                            SET Stock = Stock - @Quantity
                            WHERE ProductID = @ProductID AND Stock >= @Quantity";

                        using (var cmd = new SqlCommand(stockQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3. Clear the cart
                    string clearCart = "DELETE FROM CartItems WHERE UserID = @UserID";
                    using (var cmd = new SqlCommand(clearCart, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@UserID", order.UserID);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return newOrderID;
        }

        // Get all orders for a specific user (order history)
        public List<Order> GetOrdersByUser(int userID)
        {
            var orders = new List<Order>();
            string query = @"
                SELECT OrderID, UserID, TotalAmount, Status, ShipAddress, CreatedAt
                FROM Orders
                WHERE UserID = @UserID
                ORDER BY CreatedAt DESC";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        orders.Add(MapOrder(reader));
                }
            }
            return orders;
        }

        // Get single order with its items (for confirmation + detail page)
        public Order GetOrderByID(int orderID, int userID)
        {
            Order order = null;

            string orderQuery = @"
                SELECT OrderID, UserID, TotalAmount, Status, ShipAddress, CreatedAt
                FROM Orders
                WHERE OrderID = @OrderID AND UserID = @UserID";

            using (var conn = DBHelper.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) order = MapOrder(reader);
                    }
                }

                if (order == null) return null;

                // Get order items
                string itemsQuery = @"
                    SELECT oi.OrderItemID, oi.OrderID, oi.ProductID,
                           p.Name AS ProductName, p.ImageURL,
                           oi.Quantity, oi.UnitPrice
                    FROM OrderItems oi
                    INNER JOIN Products p ON oi.ProductID = p.ProductID
                    WHERE oi.OrderID = @OrderID";

                order.Items = new List<OrderItem>();
                using (var cmd = new SqlCommand(itemsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            order.Items.Add(new OrderItem
                            {
                                OrderItemID = (int)reader["OrderItemID"],
                                OrderID = (int)reader["OrderID"],
                                ProductID = (int)reader["ProductID"],
                                ProductName = reader["ProductName"].ToString(),
                                ImageURL = reader["ImageURL"].ToString(),
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (decimal)reader["UnitPrice"]
                            });
                        }
                    }
                }
            }
            return order;
        }

        // Admin: get all orders
        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();
            string query = @"
        SELECT o.OrderID, o.UserID, u.FullName,
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
                        orders.Add(new Order
                        {
                            OrderID = (int)reader["OrderID"],
                            UserID = (int)reader["UserID"],
                            TotalAmount = (decimal)reader["TotalAmount"],
                            Status = reader["Status"].ToString(),
                            ShipAddress = reader["FullName"].ToString()
                                        + "||" + reader["ShipAddress"].ToString(),
                            CreatedAt = (System.DateTime)reader["CreatedAt"]
                        });
                    }
                }
            }
            return orders;
        }

        // Admin: update order status
        public void UpdateStatus(int orderID, string status)
        {
            string query = "UPDATE Orders SET Status = @Status WHERE OrderID = @OrderID";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private Order MapOrder(SqlDataReader r)
        {
            return new Order
            {
                OrderID = (int)r["OrderID"],
                UserID = (int)r["UserID"],
                TotalAmount = (decimal)r["TotalAmount"],
                Status = r["Status"].ToString(),
                ShipAddress = r["ShipAddress"].ToString(),
                CreatedAt = (DateTime)r["CreatedAt"]
            };
        }

        
    }
}