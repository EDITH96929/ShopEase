using System.Collections.Generic;
using System.Data.SqlClient;
using ShopEase.Models;

namespace ShopEase.DAL
{
    public class CartRepository
    {
        // Get all cart items for a user (joined with product info)
        public List<CartItem> GetCartItems(int userID)
        {
            var list = new List<CartItem>();
            string query = @"
                SELECT ci.CartItemID, ci.UserID, ci.ProductID, ci.Quantity,
                       p.Name AS ProductName, p.Price, p.ImageURL
                FROM CartItems ci
                INNER JOIN Products p ON ci.ProductID = p.ProductID
                WHERE ci.UserID = @UserID";

            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CartItem
                        {
                            CartItemID = (int)reader["CartItemID"],
                            UserID = (int)reader["UserID"],
                            ProductID = (int)reader["ProductID"],
                            ProductName = reader["ProductName"].ToString(),
                            Price = (decimal)reader["Price"],
                            ImageURL = reader["ImageURL"].ToString(),
                            Quantity = (int)reader["Quantity"]
                        });
                    }
                }
            }
            return list;
        }

        // Add item — if already in cart, increase quantity
        public void AddItem(int userID, int productID, int quantity)
        {
            string checkQuery = @"SELECT CartItemID, Quantity FROM CartItems
                                  WHERE UserID = @UserID AND ProductID = @ProductID";
            using (var conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Already exists — update quantity
                            int existingID = (int)reader["CartItemID"];
                            int existingQty = (int)reader["Quantity"];
                            reader.Close();

                            string updateQuery = "UPDATE CartItems SET Quantity = @Qty WHERE CartItemID = @ID";
                            using (var upCmd = new SqlCommand(updateQuery, conn))
                            {
                                upCmd.Parameters.AddWithValue("@Qty", existingQty + quantity);
                                upCmd.Parameters.AddWithValue("@ID", existingID);
                                upCmd.ExecuteNonQuery();
                            }
                            return;
                        }
                    }
                }

                // New item
                string insertQuery = @"INSERT INTO CartItems (UserID, ProductID, Quantity)
                                       VALUES (@UserID, @ProductID, @Quantity)";
                using (var cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Update quantity of a specific cart item
        public void UpdateQuantity(int cartItemID, int quantity)
        {
            string query = "UPDATE CartItems SET Quantity = @Quantity WHERE CartItemID = @CartItemID";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@CartItemID", cartItemID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Remove one item from cart
        public void RemoveItem(int cartItemID)
        {
            string query = "DELETE FROM CartItems WHERE CartItemID = @CartItemID";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CartItemID", cartItemID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Clear entire cart after order is placed
        public void ClearCart(int userID)
        {
            string query = "DELETE FROM CartItems WHERE UserID = @UserID";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Cart item count (for navbar badge)
        public int GetCartCount(int userID)
        {
            string query = "SELECT ISNULL(SUM(Quantity), 0) FROM CartItems WHERE UserID = @UserID";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }
    }
}