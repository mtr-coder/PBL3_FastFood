using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;

namespace PBL3.DataAccess
{
    internal sealed class KhachHangRepository
    {
        public DataTable GetForKhachHangPage()
        {
            const string sql = @"
SELECT kh.MaKH, kh.SDT, ISNULL(kh.DiemTichLuy,0) AS DiemTichLuy,
       ISNULL(kh.DiemTichLuy,0) AS DiemTichLuyTronDoi,
       ISNULL(hv.TenHang, N'Bạc') AS TenHang,
       (ISNULL(kh.DiemTichLuy,0) / 10) * 10000 AS GiamGiaToiDa
FROM dbo.KHACH_HANG kh
OUTER APPLY (
    SELECT TOP 1 hv2.TenHang
    FROM dbo.HANG_THANH_VIEN hv2
    WHERE hv2.DiemToiThieu <= ISNULL(kh.DiemTichLuy,0)
    ORDER BY hv2.DiemToiThieu DESC, hv2.MaHang DESC
) hv
ORDER BY kh.MaKH";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetAll()
        {
            const string sql = @"
SELECT kh.MaKH, kh.SDT,
       ISNULL(kh.DiemTichLuy,0) AS DiemTichLuy,
       ISNULL(kh.DiemTichLuyTronDoi, ISNULL(kh.DiemTichLuy,0)) AS DiemTichLuyTronDoi,
       ISNULL(kh.MaHang, 1) AS MaHang,
       ISNULL(hv.TenHang, N'Bạc') AS TenHang
FROM dbo.KHACH_HANG kh
LEFT JOIN dbo.HANG_THANH_VIEN hv ON hv.MaHang = kh.MaHang
ORDER BY kh.MaKH";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public int UpdateHang(string maKh, int maHang)
        {
            const string sql = @"
UPDATE dbo.KHACH_HANG
SET MaHang = @MaHang
WHERE MaKH = @MaKH";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaKH", SqlDbType.VarChar, 20).Value = maKh;
            cmd.Parameters.Add("@MaHang", SqlDbType.Int).Value = maHang;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public bool IsPhoneExists(bool isInsert, string phone, string? excludeMaKh)
        {
            string query = isInsert
                ? "SELECT COUNT(1) FROM dbo.KHACH_HANG WHERE SDT = @SDT"
                : "SELECT COUNT(1) FROM dbo.KHACH_HANG WHERE SDT = @SDT AND MaKH != @ID";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@SDT", SqlDbType.VarChar).Value = phone;
            if (!isInsert)
            {
                cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = excludeMaKh ?? string.Empty;
            }

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void Insert(string sdt, int diemTichLuy)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            int nextId = 1;
            using (SqlCommand cmdGet = new SqlCommand("SELECT MaKH FROM dbo.KHACH_HANG ORDER BY TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaKH) LIKE 'KH%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaKH), 3, LEN(CONVERT(VARCHAR(20), MaKH)) - 2) ELSE CONVERT(VARCHAR(20), MaKH) END AS INT)", conn))
            using (SqlDataReader reader = cmdGet.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        continue;
                    }

                    string raw = Convert.ToString(reader.GetValue(0)) ?? string.Empty;
                    string numeric = raw.StartsWith("KH", StringComparison.OrdinalIgnoreCase) ? raw.Substring(2) : raw;
                    if (!int.TryParse(numeric, out int v))
                    {
                        continue;
                    }

                    if (v == nextId)
                    {
                        nextId++;
                    }
                    else if (v > nextId)
                    {
                        break;
                    }
                }
            }

            bool hasIdentity;
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.KHACH_HANG'),'MaKH','IsIdentity') = 1 THEN 1 ELSE 0 END", conn))
            {
                hasIdentity = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0) == 1;
            }

            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                if (hasIdentity)
                {
                    using SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT dbo.KHACH_HANG ON; INSERT INTO dbo.KHACH_HANG (MaKH, TenKH, SDT, DiemTichLuy) VALUES (@MaKH, @TenKH, @SDT, @DiemTichLuy); SET IDENTITY_INSERT dbo.KHACH_HANG OFF;", conn, tran);
                    cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = nextId;
                    cmd.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = sdt;
                    cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
                    cmd.Parameters.Add("@DiemTichLuy", SqlDbType.Int).Value = diemTichLuy;
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.KHACH_HANG (MaKH, TenKH, SDT, DiemTichLuy) VALUES (@MaKH, @TenKH, @SDT, @DiemTichLuy)", conn, tran);
                    cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = nextId;
                    cmd.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = sdt;
                    cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
                    cmd.Parameters.Add("@DiemTichLuy", SqlDbType.Int).Value = diemTichLuy;
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public int Update(string maKh, string sdt, int diemTichLuy)
        {
            const string sql = @"
UPDATE dbo.KHACH_HANG
SET TenKH = @TenKH,
    SDT = @SDT,
    DiemTichLuy = @DiemTichLuy
WHERE MaKH = @MaKH";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaKH", SqlDbType.VarChar, 20).Value = maKh;
            cmd.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = sdt;
            cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
            cmd.Parameters.Add("@DiemTichLuy", SqlDbType.Int).Value = diemTichLuy;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Delete(string maKh)
        {
            const string sql = "DELETE FROM dbo.KHACH_HANG WHERE MaKH = @MaKH";
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaKH", SqlDbType.VarChar, 20).Value = maKh;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public string GenerateNextDisplayCode()
        {
            const string sql = @"SELECT MaKH FROM dbo.KHACH_HANG
ORDER BY TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaKH) LIKE 'KH%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaKH), 3, LEN(CONVERT(VARCHAR(20), MaKH)) - 2) ELSE CONVERT(VARCHAR(20), MaKH) END AS INT)";

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            int nextId = 1;
            while (reader.Read())
            {
                if (reader.IsDBNull(0))
                {
                    continue;
                }

                string raw = Convert.ToString(reader.GetValue(0)) ?? string.Empty;
                string numeric = raw.StartsWith("KH", StringComparison.OrdinalIgnoreCase) ? raw.Substring(2) : raw;
                if (!int.TryParse(numeric, out int v))
                {
                    continue;
                }

                if (v == nextId)
                {
                    nextId++;
                }
                else if (v > nextId)
                {
                    break;
                }
            }

            return $"KH{nextId}";
        }

        public void InsertWithName(string sdt, string tenKh)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            bool hasIdentity;
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.KHACH_HANG'),'MaKH','IsIdentity') = 1 THEN 1 ELSE 0 END", conn))
            {
                hasIdentity = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0) == 1;
            }

            if (hasIdentity)
            {
                using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.KHACH_HANG (TenKH, SDT, DiemTichLuy) VALUES (@TenKH, @SDT, 0)", conn);
                cmd.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = tenKh;
                cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
                cmd.ExecuteNonQuery();
                return;
            }

            int next;
            using (SqlCommand cmdNext = new SqlCommand(@"SELECT ISNULL(MAX(TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaKH) LIKE 'KH%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaKH), 3, LEN(CONVERT(VARCHAR(20), MaKH)) - 2) ELSE CONVERT(VARCHAR(20), MaKH) END AS INT)), 0) + 1 FROM dbo.KHACH_HANG", conn))
            {
                next = Convert.ToInt32(cmdNext.ExecuteScalar());
            }

            using SqlCommand cmd2 = new SqlCommand("INSERT INTO dbo.KHACH_HANG (MaKH, TenKH, SDT, DiemTichLuy) VALUES (@MaKH, @TenKH, @SDT, 0)", conn);
            cmd2.Parameters.Add("@MaKH", SqlDbType.Int).Value = next;
            cmd2.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = tenKh;
            cmd2.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
            cmd2.ExecuteNonQuery();
        }
    }
}
