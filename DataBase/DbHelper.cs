using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace PBL3.DataBase
{
    internal static class DbHelper
    {
        private static readonly string _connStr = ResolveConnectionString();

        private static string ResolveConnectionString()
        {
            ConnectionStringSettings? fromDefault = ConfigurationManager.ConnectionStrings["QL_FASTFOOD"];
            if (fromDefault is not null && !string.IsNullOrWhiteSpace(fromDefault.ConnectionString))
            {
                return fromDefault.ConnectionString;
            }

            string dataBaseConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase", "App.config");
            if (File.Exists(dataBaseConfigPath))
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap { ExeConfigFilename = dataBaseConfigPath };
                Configuration cfg = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                ConnectionStringSettings? fromDataBaseConfig = cfg.ConnectionStrings.ConnectionStrings["QL_FASTFOOD"];
                if (fromDataBaseConfig is not null && !string.IsNullOrWhiteSpace(fromDataBaseConfig.ConnectionString))
                {
                    return fromDataBaseConfig.ConnectionString;
                }
            }

            throw new InvalidOperationException("Không tìm thấy connection string 'QL_FASTFOOD'. Vui lòng kiểm tra DataBase/App.config.");
        }

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

        /// <summary>
        /// ExecuteQuery an toan voi parameters, chong SQL injection.
        /// Dung: DbHelper.ExecuteQuery("SELECT * FROM NV WHERE SDT = @sdt", new SqlParameter("@sdt", value));
        /// </summary>
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection conn = GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
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

        /// <summary>
        /// ExecuteNonQuery an toan voi parameters, chong SQL injection.
        /// Dung: DbHelper.ExecuteNonQuery("DELETE FROM NV WHERE MaNV = @ma", new SqlParameter("@ma", value));
        /// </summary>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Tra ve gia tri dau tien (dong 1, cot 1) cua ket qua query.
        /// Dung cho COUNT, MAX, SELECT TOP 1...
        /// </summary>
        public static object? ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteScalar();
        }
    }
}