using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;

namespace PBL3.DataAccess
{
    internal sealed class MuaHangRepository
    {
        public DataTable GetNguyenLieu()
        {
            const string sql = @"
SELECT MaNL, TenNL, DonViTinh, ISNULL(GiaNhap, 0) AS GiaNhap, ISNULL(SoLuongTon, 0) AS SoLuongTon
FROM dbo.NGUYEN_LIEU
ORDER BY MaNL";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetNhaCungCap()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            string sql = "SELECT MaNCC, TenNCC FROM dbo.NHA_CUNG_CAP ORDER BY MaNCC";

            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        public string? GetSuggestedNhaCungCap(string maNl)
        {
            if (string.IsNullOrWhiteSpace(maNl))
            {
                return null;
            }

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            const string sql = @"SELECT TOP 1 hdn.MaNCC
FROM dbo.HOA_DON_NHAP hdn
INNER JOIN dbo.CT_HOA_DON_NHAP ctn ON ctn.MaHDN = hdn.MaHDN
WHERE ctn.MaNL = @MaNL
ORDER BY hdn.NgayNhap DESC, hdn.MaHDN DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNL", SqlDbType.VarChar, 20).Value = maNl;
            object? result = cmd.ExecuteScalar();
            return result is null || result == DBNull.Value ? null : Convert.ToString(result);
        }

        public DataTable GetDonViTinh()
        {
            const string sql = @"SELECT DISTINCT DonViTinh
FROM dbo.NGUYEN_LIEU
WHERE DonViTinh IS NOT NULL AND LTRIM(RTRIM(DonViTinh)) <> ''
ORDER BY DonViTinh";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public object SavePhieuNhap(string maNv, object maNcc, decimal tongTien, DataTable phieuNhapTable)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();

            object maHdn = InsertHoaDonNhap(conn, tran, maNv, maNcc, tongTien);

            foreach (DataRow row in phieuNhapTable.Rows)
            {
                object maNlValue = row["MaNL"];
                if (string.IsNullOrWhiteSpace(Convert.ToString(maNlValue)))
                {
                    string tenNlMoi = Convert.ToString(row["Tên nguyên liệu"]) ?? string.Empty;
                    string dvtMoi = Convert.ToString(row["Đơn vị tính"]) ?? string.Empty;
                    decimal donGiaMoi = Convert.ToDecimal(row["Đơn giá"]);
                    maNlValue = EnsureNguyenLieuExists(conn, tran, tenNlMoi, dvtMoi, donGiaMoi);
                }

                decimal soLuong = Convert.ToDecimal(row["Số lượng"]);
                decimal donGia = Convert.ToDecimal(row["Đơn giá"]);

                InsertChiTietNhap(conn, tran, maHdn, maNlValue, soLuong, donGia);
                UpdateSoLuongTon(conn, tran, maNlValue, soLuong, donGia);
            }

            tran.Commit();
            return maHdn;
        }

        private static int EnsureNguyenLieuExists(SqlConnection conn, SqlTransaction tran, string tenNl, string donViTinh, decimal donGia)
        {
            using (SqlCommand cmdFind = new SqlCommand("SELECT TOP 1 MaNL FROM dbo.NGUYEN_LIEU WHERE TenNL = @TenNL AND DonViTinh = @DonViTinh ORDER BY MaNL", conn, tran))
            {
                cmdFind.Parameters.Add("@TenNL", SqlDbType.NVarChar, 100).Value = tenNl;
                cmdFind.Parameters.Add("@DonViTinh", SqlDbType.NVarChar, 30).Value = donViTinh;
                object? exists = cmdFind.ExecuteScalar();
                if (exists is not null && exists != DBNull.Value)
                {
                    return Convert.ToInt32(exists);
                }
            }

            bool hasNguongColumn;
            using (SqlCommand cmdCol = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='NGUYEN_LIEU' AND COLUMN_NAME='NguongToiThieu') THEN 1 ELSE 0 END", conn, tran))
            {
                hasNguongColumn = Convert.ToInt32(cmdCol.ExecuteScalar()) == 1;
            }

            string insertSql = hasNguongColumn
                ? "INSERT INTO dbo.NGUYEN_LIEU (TenNL, DonViTinh, SoLuongTon, GiaNhap, NguongToiThieu) OUTPUT INSERTED.MaNL VALUES (@TenNL, @DonViTinh, 0, @GiaNhap, 0)"
                : "INSERT INTO dbo.NGUYEN_LIEU (TenNL, DonViTinh, SoLuongTon, GiaNhap) OUTPUT INSERTED.MaNL VALUES (@TenNL, @DonViTinh, 0, @GiaNhap)";

            using SqlCommand cmdInsert = new SqlCommand(insertSql, conn, tran);
            cmdInsert.Parameters.Add("@TenNL", SqlDbType.NVarChar, 100).Value = tenNl;
            cmdInsert.Parameters.Add("@DonViTinh", SqlDbType.NVarChar, 30).Value = donViTinh;
            cmdInsert.Parameters.Add("@GiaNhap", SqlDbType.Decimal).Value = donGia;
            cmdInsert.Parameters["@GiaNhap"].Precision = 18;
            cmdInsert.Parameters["@GiaNhap"].Scale = 2;
            object? newId = cmdInsert.ExecuteScalar();
            return Convert.ToInt32(newId);
        }

        private static int GenerateNextMaHdnInt(SqlConnection conn, SqlTransaction tran)
        {
            using SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(TRY_CAST(MaHDN AS INT)), 0) + 1 FROM dbo.HOA_DON_NHAP", conn, tran);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static object InsertHoaDonNhap(SqlConnection conn, SqlTransaction tran, string maNv, object maNcc, decimal tongTien)
        {
            bool maHdnIdentity;
            using (SqlCommand cmdIdentity = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.HOA_DON_NHAP'),'MaHDN','IsIdentity') = 1 THEN 1 ELSE 0 END", conn, tran))
            {
                maHdnIdentity = Convert.ToInt32(cmdIdentity.ExecuteScalar() ?? 0) == 1;
            }

            bool maHdnNumeric = IsColumnNumeric(conn, tran, "HOA_DON_NHAP", "MaHDN");
            bool maNvNumeric = IsColumnNumeric(conn, tran, "HOA_DON_NHAP", "MaNV");
            bool maNccNumeric = IsColumnNumeric(conn, tran, "HOA_DON_NHAP", "MaNCC");

            object maNvValue = maNvNumeric && int.TryParse(maNv, out int nvInt) ? nvInt : maNv;
            object maNccValue = maNccNumeric ? Convert.ToInt32(maNcc) : maNcc;

            if (maHdnIdentity)
            {
                using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.HOA_DON_NHAP (NgayNhap, MaNV, MaNCC, TongTien) OUTPUT INSERTED.MaHDN VALUES (@NgayNhap, @MaNV, @MaNCC, @TongTien)", conn, tran);
                cmd.Parameters.Add("@NgayNhap", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@MaNV", maNvNumeric ? SqlDbType.Int : SqlDbType.VarChar, 20).Value = maNvValue;
                cmd.Parameters.Add("@MaNCC", maNccNumeric ? SqlDbType.Int : SqlDbType.VarChar, 20).Value = maNccValue;
                cmd.Parameters.Add("@TongTien", SqlDbType.Decimal).Value = tongTien;
                cmd.Parameters["@TongTien"].Precision = 18;
                cmd.Parameters["@TongTien"].Scale = 2;
                return cmd.ExecuteScalar() ?? 0;
            }

            object maHdnValue = maHdnNumeric ? GenerateNextMaHdnInt(conn, tran) : $"HDN{GenerateNextMaHdnInt(conn, tran)}";
            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.HOA_DON_NHAP (MaHDN, NgayNhap, MaNV, MaNCC, TongTien) VALUES (@MaHDN, @NgayNhap, @MaNV, @MaNCC, @TongTien)", conn, tran))
            {
                cmd.Parameters.Add("@MaHDN", maHdnNumeric ? SqlDbType.Int : SqlDbType.VarChar, 20).Value = maHdnValue;
                cmd.Parameters.Add("@NgayNhap", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@MaNV", maNvNumeric ? SqlDbType.Int : SqlDbType.VarChar, 20).Value = maNvValue;
                cmd.Parameters.Add("@MaNCC", maNccNumeric ? SqlDbType.Int : SqlDbType.VarChar, 20).Value = maNccValue;
                cmd.Parameters.Add("@TongTien", SqlDbType.Decimal).Value = tongTien;
                cmd.Parameters["@TongTien"].Precision = 18;
                cmd.Parameters["@TongTien"].Scale = 2;
                cmd.ExecuteNonQuery();
            }

            return maHdnValue;
        }

        private static void InsertChiTietNhap(SqlConnection conn, SqlTransaction tran, object maHdn, object maNlValue, decimal soLuong, decimal donGia)
        {
            bool maHdnNumeric = IsColumnNumeric(conn, tran, "CT_HOA_DON_NHAP", "MaHDN");
            using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.CT_HOA_DON_NHAP (MaHDN, MaNL, SoLuong, DonGia) VALUES (@MaHDN, @MaNL, @SoLuong, @DonGia)", conn, tran);
            cmd.Parameters.Add("@MaHDN", maHdnNumeric ? SqlDbType.Int : SqlDbType.VarChar, 20).Value = maHdn;
            cmd.Parameters.Add("@MaNL", SqlDbType.Int).Value = Convert.ToInt32(maNlValue);
            cmd.Parameters.Add("@SoLuong", SqlDbType.Decimal).Value = soLuong;
            cmd.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = donGia;
            cmd.Parameters["@SoLuong"].Precision = 18;
            cmd.Parameters["@SoLuong"].Scale = 2;
            cmd.Parameters["@DonGia"].Precision = 18;
            cmd.Parameters["@DonGia"].Scale = 2;
            cmd.ExecuteNonQuery();
        }

        private static bool IsColumnNumeric(SqlConnection conn, SqlTransaction tran, string tableName, string columnName)
        {
            using SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName AND COLUMN_NAME=@ColumnName", conn, tran);
            cmd.Parameters.Add("@TableName", SqlDbType.VarChar, 128).Value = tableName;
            cmd.Parameters.Add("@ColumnName", SqlDbType.VarChar, 128).Value = columnName;
            string type = (Convert.ToString(cmd.ExecuteScalar()) ?? string.Empty).ToLowerInvariant();
            return type is "int" or "bigint" or "smallint" or "tinyint";
        }

        private static void UpdateSoLuongTon(SqlConnection conn, SqlTransaction tran, object maNlValue, decimal soLuong, decimal donGia)
        {
            using SqlCommand cmd = new SqlCommand("UPDATE dbo.NGUYEN_LIEU SET SoLuongTon = ISNULL(SoLuongTon,0) + @SoLuong, GiaNhap = @DonGia WHERE MaNL = @MaNL", conn, tran);
            cmd.Parameters.Add("@MaNL", SqlDbType.Int).Value = Convert.ToInt32(maNlValue);
            cmd.Parameters.Add("@SoLuong", SqlDbType.Decimal).Value = soLuong;
            cmd.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = donGia;
            cmd.Parameters["@SoLuong"].Precision = 18;
            cmd.Parameters["@SoLuong"].Scale = 2;
            cmd.Parameters["@DonGia"].Precision = 18;
            cmd.Parameters["@DonGia"].Scale = 2;
            cmd.ExecuteNonQuery();
        }
    }
}
