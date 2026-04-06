using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using ShopEase.Models;        // ← THIS is what was missing

namespace ShopEase.DAL
{
    public class UserRepository
    {
        public bool Register(User user)
        {
            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(checkQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Email", user.Email);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                if (count > 0) return false;
            }

            string query = @"INSERT INTO Users (FullName, Email, PasswordHash, Phone, Address, Role)
                             VALUES (@FullName, @Email, @PasswordHash, @Phone, @Address, 'Customer')";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(user.PasswordHash));
                cmd.Parameters.AddWithValue("@Phone", user.Phone ?? "");
                cmd.Parameters.AddWithValue("@Address", user.Address ?? "");
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }

        public User Login(string email, string password)
        {
            string query = @"SELECT UserID, FullName, Email, Role
                             FROM Users
                             WHERE Email = @Email AND PasswordHash = @PasswordHash";
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            UserID = (int)reader["UserID"],
                            FullName = reader["FullName"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}