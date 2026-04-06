using System.Configuration;
using System.Data.SqlClient;

namespace ShopEase.DAL
{
    public class DBHelper
    {
        private static string connectionString =
            ConfigurationManager.ConnectionStrings["ShopEaseDB"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}