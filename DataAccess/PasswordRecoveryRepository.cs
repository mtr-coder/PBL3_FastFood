using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;
using BCrypt.Net;

namespace PBL3.DataAccess
{
    internal sealed class PasswordRecoveryRepository
    {
        public string? GetEmployeeEmail(string account)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            string? emailCol = DetectFirstColumn(conn, "NHAN_VIEN", "Email", "Gmail", "Mail");
            if (string.IsNullOrWhiteSpace(emailCol))
            {
                return null;
            }

            string sql = account.Contains("@")
                ? $"SELECT TOP 1 [{emailCol}] FROM dbo.NHAN_VIEN WHERE [{emailCol}] = @Acc"
                : $"SELECT TOP 1 [{emailCol}] FROM dbo.NHAN_VIEN WHERE MaNV = @Acc";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Acc", SqlDbType.NVarChar, 100).Value = account;
            object? result = cmd.ExecuteScalar();
            return Convert.ToString(result)?.Trim();
        }

        public int UpdatePassword(string account, string newPassword)
        {
            // Hash password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            string? emailCol = DetectFirstColumn(conn, "NHAN_VIEN", "Email", "Gmail", "Mail");
            string sql;
            if (!string.IsNullOrWhiteSpace(emailCol) && account.Contains("@"))
            {
                sql = $"UPDATE dbo.NHAN_VIEN SET MatKhau=@MatKhau WHERE [{emailCol}] = @Account";
            }
            else
            {
                sql = "UPDATE dbo.NHAN_VIEN SET MatKhau=@MatKhau WHERE MaNV = @Account";
            }

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 255).Value = hashedPassword;
            cmd.Parameters.Add("@Account", SqlDbType.NVarChar, 100).Value = account;
            return cmd.ExecuteNonQuery();
        }

        private static string? DetectFirstColumn(SqlConnection conn, string tableName, params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                using SqlCommand cmd = new SqlCommand(@"SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName AND COLUMN_NAME=@Col) THEN 1 ELSE 0 END", conn);
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@Col", candidate);
                object? res = cmd.ExecuteScalar();
                if (res is not null && Convert.ToInt32(res) == 1)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
