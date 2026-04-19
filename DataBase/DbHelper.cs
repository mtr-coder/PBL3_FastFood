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