using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PBL3.DataAccess
{
    internal sealed class NguyenLieuRepository
    {
        public DataTable GetAll()
        {
            const string sql = @"
SELECT MaNL, TenNL, DonViTinh, GiaNhap, SoLuongTon, NguongToiThieu
FROM dbo.NGUYEN_LIEU
ORDER BY TRY_CAST(MaNL AS INT), MaNL";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void Insert(string tenNl, string donViTinh, decimal giaNhap)
        {
            string nextMaDisplay = GenerateNextDisplayCode();
            string numericPart = nextMaDisplay.StartsWith("NL", StringComparison.OrdinalIgnoreCase)
                ? nextMaDisplay.Substring(2)
                : nextMaDisplay;
            int nextInt = int.TryParse(numericPart, out int tmp) ? tmp : 0;

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            bool hasIdentity;
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.NGUYEN_LIEU'),'MaNL','IsIdentity') = 1 THEN 1 ELSE 0 END", conn))
            {
                hasIdentity = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0) == 1;
            }

            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                if (hasIdentity)
                {
                    using SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT dbo.NGUYEN_LIEU ON; INSERT INTO dbo.NGUYEN_LIEU (MaNL, TenNL, DonViTinh, GiaNhap, SoLuongTon) VALUES (@MaNL, @TenNL, @DonViTinh, @GiaNhap, 0); SET IDENTITY_INSERT dbo.NGUYEN_LIEU OFF;", conn, tran);
                    cmd.Parameters.Add("@MaNL", SqlDbType.Int).Value = nextInt;
                    cmd.Parameters.Add("@TenNL", SqlDbType.NVarChar, 100).Value = tenNl;
                    cmd.Parameters.Add("@DonViTinh", SqlDbType.NVarChar, 20).Value = donViTinh;
                    cmd.Parameters.Add("@GiaNhap", SqlDbType.Decimal).Value = giaNhap;
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.NGUYEN_LIEU (MaNL, TenNL, DonViTinh, GiaNhap, SoLuongTon) VALUES (@MaNL, @TenNL, @DonViTinh, @GiaNhap, 0)", conn, tran);
                    cmd.Parameters.Add("@MaNL", SqlDbType.VarChar, 20).Value = nextMaDisplay;
                    cmd.Parameters.Add("@TenNL", SqlDbType.NVarChar, 100).Value = tenNl;
                    cmd.Parameters.Add("@DonViTinh", SqlDbType.NVarChar, 20).Value = donViTinh;
                    cmd.Parameters.Add("@GiaNhap", SqlDbType.Decimal).Value = giaNhap;
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

        public int Update(string maNl, string tenNl, string donViTinh, decimal giaNhap, decimal nguongToiThieu)
        {
            const string sql = @"
UPDATE dbo.NGUYEN_LIEU
SET TenNL = @TenNL,
    DonViTinh = @DonViTinh,
    GiaNhap = @GiaNhap,
    NguongToiThieu = @NguongToiThieu
WHERE MaNL = @MaNL";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNL", SqlDbType.VarChar, 20).Value = maNl;
            cmd.Parameters.Add("@TenNL", SqlDbType.NVarChar, 100).Value = tenNl;
            cmd.Parameters.Add("@DonViTinh", SqlDbType.NVarChar, 20).Value = donViTinh;
            cmd.Parameters.Add("@GiaNhap", SqlDbType.Decimal).Value = giaNhap;
            cmd.Parameters.Add("@NguongToiThieu", SqlDbType.Decimal).Value = nguongToiThieu;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Delete(string maNl)
        {
            const string sql = "DELETE FROM dbo.NGUYEN_LIEU WHERE MaNL = @MaNL";
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNL", SqlDbType.VarChar, 20).Value = maNl;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public List<(string MaNl, string DonViTinh)> GetDonViTinhPairs()
        {
            List<(string MaNl, string DonViTinh)> result = new List<(string MaNl, string DonViTinh)>();
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            using SqlCommand cmd = new SqlCommand("SELECT MaNL, DonViTinh FROM dbo.NGUYEN_LIEU", conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string maNl = Convert.ToString(reader["MaNL"]) ?? string.Empty;
                string donViTinh = Convert.ToString(reader["DonViTinh"]) ?? string.Empty;
                result.Add((maNl, donViTinh));
            }

            return result;
        }

        public void UpdateDonViTinh(string maNl, string donViTinh)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("UPDATE dbo.NGUYEN_LIEU SET DonViTinh = @DonViTinh WHERE MaNL = @MaNL", conn);
            cmd.Parameters.Add("@DonViTinh", SqlDbType.NVarChar, 20).Value = donViTinh;
            cmd.Parameters.Add("@MaNL", SqlDbType.VarChar, 20).Value = maNl;
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void SeedSampleIfEmpty()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            using SqlCommand countCmd = new SqlCommand("SELECT COUNT(1) FROM dbo.NGUYEN_LIEU", conn);
            int currentCount = Convert.ToInt32(countCmd.ExecuteScalar(), CultureInfo.InvariantCulture);
            if (currentCount > 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO dbo.NGUYEN_LIEU (TenNL, DonViTinh, GiaNhap, SoLuongTon, NguongToiThieu) VALUES
(N'Thịt gà',        N'Kg',   90000,  35, 20),
(N'Thịt bò',        N'Kg',  180000,  18, 15),
(N'Tôm',            N'Kg',  220000,  10, 12),
(N'Khoai tây',      N'Kg',   28000,  40, 25),
(N'Xà lách',        N'Kg',   30000,  12, 10),
(N'Cà chua',        N'Kg',   25000,  14, 10),
(N'Hành tây',       N'Kg',   22000,   9, 10),
(N'Bột chiên giòn', N'Kg',   45000,  22, 15),
(N'Dầu ăn',         N'Lít',  52000,  16, 12),
(N'Nước mắm',       N'Lít',  38000,   8, 10),
(N'Sốt mayonnaise', N'Lít',  70000,   6,  8),
(N'Tương ớt',       N'Lít',  55000,   7,  8),
(N'Phô mai lát',    N'Túi', 120000,   5,  6),
(N'Ly giấy',        N'Thùng',95000,   9,  6),
(N'Hộp giấy',       N'Thùng',85000,   4,  6);";

            using SqlCommand insertCmd = new SqlCommand(sql, conn);
            insertCmd.ExecuteNonQuery();
        }

        public string GenerateNextDisplayCode()
        {
            const string sql = @"SELECT MaNL FROM dbo.NGUYEN_LIEU
ORDER BY TRY_CAST(
    CASE WHEN CONVERT(VARCHAR(20), MaNL) LIKE 'NL%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaNL), 3, LEN(CONVERT(VARCHAR(20), MaNL)) - 2) ELSE CONVERT(VARCHAR(20), MaNL) END AS INT)";

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
                string numeric = raw.StartsWith("NL", StringComparison.OrdinalIgnoreCase) ? raw.Substring(2) : raw;
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

            return $"NL{nextId}";
        }
    }
}
