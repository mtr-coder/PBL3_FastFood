using PBL3.DataBase;
using PBL3.Models;
using System.Data;
using System.Data.SqlClient;

namespace PBL3.DataAccess
{
    internal sealed class HopThuYeuCauRepository
    {
        public List<HopThuYeuCauItem> GetRequests(string loai, int trangThai)
        {
            List<HopThuYeuCauItem> items = new List<HopThuYeuCauItem>();

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT yc.MaYeuCau, yc.MaNV, yc.LoaiYeuCau, yc.NoiDung, yc.NgayGui, yc.TuNgay, yc.DenNgay, yc.TrangThai, yc.PhanHoiAdmin,
       ISNULL(nv.HoTen, N'Không rõ') AS HoTen, ISNULL(cv.TenCV, N'') AS TenCV
FROM dbo.YEU_CAU yc
LEFT JOIN dbo.NHAN_VIEN nv ON TRY_CAST(nv.MaNV AS INT) = yc.MaNV
LEFT JOIN dbo.CHUC_VU cv ON cv.MaCV = nv.MaCV
WHERE (@Loai = N'Tất cả' OR yc.LoaiYeuCau LIKE @LoaiLike)
  AND (@TrangThai = -1 OR yc.TrangThai = @TrangThai)
ORDER BY CASE WHEN yc.TrangThai = 0 THEN 0 ELSE 1 END, yc.NgayGui DESC";

            cmd.Parameters.AddWithValue("@Loai", loai);
            cmd.Parameters.AddWithValue("@LoaiLike", loai == "Tất cả" ? "%" : $"%{loai}%");
            cmd.Parameters.AddWithValue("@TrangThai", trangThai);

            conn.Open();
            using SqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                items.Add(new HopThuYeuCauItem
                {
                    MaYeuCau = rd.GetInt32(0),
                    MaNV = rd.GetInt32(1),
                    LoaiYeuCau = Convert.ToString(rd[2]) ?? string.Empty,
                    NoiDung = Convert.ToString(rd[3]) ?? string.Empty,
                    NgayGui = rd.IsDBNull(4) ? DateTime.Now : Convert.ToDateTime(rd[4]),
                    TuNgay = rd.IsDBNull(5) ? null : Convert.ToDateTime(rd[5]),
                    DenNgay = rd.IsDBNull(6) ? null : Convert.ToDateTime(rd[6]),
                    TrangThai = rd.IsDBNull(7) ? 0 : Convert.ToInt32(rd[7]),
                    PhanHoiAdmin = Convert.ToString(rd[8]) ?? string.Empty,
                    HoTen = Convert.ToString(rd[9]) ?? string.Empty,
                    TenCV = Convert.ToString(rd[10]) ?? string.Empty
                });
            }

            return items;
        }

        public string? GetLatestShift(int maNv)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT TOP 1 ct.TenCa, pc.NgayLam
FROM dbo.PHAN_CONG_CA pc
JOIN dbo.CA_TRUC ct ON ct.MaCa = pc.MaCa
WHERE TRY_CAST(pc.MaNV AS INT) = @MaNV
ORDER BY pc.NgayLam DESC";
            cmd.Parameters.AddWithValue("@MaNV", maNv);
            conn.Open();
            using SqlDataReader rd = cmd.ExecuteReader();
            if (!rd.Read())
            {
                return null;
            }

            string tenCa = Convert.ToString(rd[0]) ?? string.Empty;
            DateTime ngay = rd.IsDBNull(1) ? DateTime.MinValue : Convert.ToDateTime(rd[1]);
            return $"{tenCa} - {ngay:dd/MM/yyyy}";
        }

        public List<string> GetHistory(int maNv)
        {
            List<string> history = new List<string>();
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT TOP 20 LoaiYeuCau, NgayGui, TrangThai
FROM dbo.YEU_CAU
WHERE MaNV = @MaNV
ORDER BY NgayGui DESC";
            cmd.Parameters.AddWithValue("@MaNV", maNv);
            conn.Open();
            using SqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                string loai = Convert.ToString(rd[0]) ?? string.Empty;
                DateTime ngay = rd.IsDBNull(1) ? DateTime.Now : Convert.ToDateTime(rd[1]);
                int st = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
                history.Add($"{ngay:dd/MM/yyyy HH:mm}|{loai}|{st}");
            }

            return history;
        }

        public void ApproveRequest(int maYeuCau, string phanHoi, bool setNghiViec, int maNv)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();

            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.YEU_CAU SET TrangThai = 1, PhanHoiAdmin = @PhanHoi WHERE MaYeuCau = @Id", conn, tran))
            {
                cmd.Parameters.AddWithValue("@PhanHoi", phanHoi);
                cmd.Parameters.AddWithValue("@Id", maYeuCau);
                cmd.ExecuteNonQuery();
            }

            if (setNghiViec)
            {
                using SqlCommand cmdNv = new SqlCommand("UPDATE dbo.NHAN_VIEN SET TrangThai = N'Nghỉ việc' WHERE TRY_CAST(MaNV AS INT) = @MaNV", conn, tran);
                cmdNv.Parameters.AddWithValue("@MaNV", maNv);
                cmdNv.ExecuteNonQuery();
            }

            tran.Commit();
        }

        public void RejectRequest(int maYeuCau, string phanHoi)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE dbo.YEU_CAU SET TrangThai = 2, PhanHoiAdmin = @PhanHoi WHERE MaYeuCau = @Id";
            cmd.Parameters.AddWithValue("@PhanHoi", phanHoi);
            cmd.Parameters.AddWithValue("@Id", maYeuCau);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void DeleteRequest(int maYeuCau)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM dbo.YEU_CAU WHERE MaYeuCau = @Id";
            cmd.Parameters.AddWithValue("@Id", maYeuCau);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
