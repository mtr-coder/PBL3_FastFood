using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace PBL3.DataBase
{
    internal static class DbHelper
    {
        private static readonly string _connStr =
            ConfigurationManager.ConnectionStrings["QL_FASTFOOD"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connStr);
        }

        public static DataTable ExecuteQuery(string sql)
        {
            using (SqlConnection conn = GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public static int ExecuteNonQuery(string sql)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                return cmd.ExecuteNonQuery();
            }
        }
    }
}