using PBL3.DataBase;
using PBL3.Models;
using System.Data.SqlClient;

namespace PBL3.DataAccess
{
    internal sealed class AuthRepository
    {
        public void CheckConnection()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
        }

        public NhanVienDangNhapInfo? GetByPhoneAndPassword(string soDienThoai, string matKhau)
        {
            const string query = @"
SELECT nv.MaNV, nv.MaCV
FROM dbo.NHAN_VIEN nv
WHERE nv.SDT = @sdt AND nv.MatKhau = @mk";

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@sdt", soDienThoai);
            cmd.Parameters.AddWithValue("@mk", matKhau);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new NhanVienDangNhapInfo
            {
                MaNV = reader["MaNV"]?.ToString()?.Trim() ?? string.Empty,
                MaCV = reader["MaCV"]?.ToString()?.Trim() ?? string.Empty
            };
        }
    }
}
