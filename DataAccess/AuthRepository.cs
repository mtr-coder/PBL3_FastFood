using PBL3.DataBase;
using PBL3.Models;
using PBL3.Business;
using System.Data.SqlClient;
using BCrypt.Net;
using System.Linq;

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
SELECT nv.MaNV, nv.MaCV, nv.HoTen, nv.MatKhau
FROM dbo.NHAN_VIEN nv
WHERE nv.SDT = @sdt";

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@sdt", soDienThoai);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Không tìm th?y user v?i SDT: {soDienThoai}");
                return null;
            }

            return new NhanVienDangNhapInfo
            {
                MaNV = reader["MaNV"]?.ToString()?.Trim() ?? string.Empty,
                MaCV = reader["MaCV"]?.ToString()?.Trim() ?? string.Empty,
                HoTen = reader["HoTen"]?.ToString()?.Trim() ?? string.Empty,
                MatKhau = reader["MatKhau"]?.ToString()?.Trim() ?? string.Empty
            };
        }
    }
}

