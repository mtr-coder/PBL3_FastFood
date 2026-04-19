using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PBL3.DataAccess
{
    internal sealed class NhapKhoRepository
    {
        public DataTable GetNguyenLieu()
        {
            const string sql = @"
SELECT MaNL, TenNL, ISNULL(GiaNhap, 0) AS GiaNhap
FROM dbo.NGUYEN_LIEU
ORDER BY TenNL";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetNhaCungCapByNguyenLieu(int maNl)
        {
            const string sql = @"
SELECT DISTINCT ncc.MaNCC, ncc.TenNCC
FROM dbo.NHA_CUNG_CAP ncc
JOIN dbo.HOA_DON_NHAP hdn ON hdn.MaNCC = ncc.MaNCC
JOIN dbo.CT_HOA_DON_NHAP ctn ON ctn.MaHDN = hdn.MaHDN
WHERE ctn.MaNL = @MaNL
ORDER BY ncc.TenNCC";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.Add("@MaNL", SqlDbType.Int).Value = maNl;
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public string SaveNhapKho(string maNv, string maNcc, DataTable gioNhapTable)
        {
            decimal tongTien = gioNhapTable.AsEnumerable().Sum(r => r.Field<decimal>("ThanhTien"));

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();

            object maHdn = InsertHoaDonNhap(conn, tran, maNv, maNcc, tongTien);

            foreach (DataRow row in gioNhapTable.Rows)
            {
                object maNl = row["MaNL"];
                decimal soLuong = Convert.ToDecimal(row["SoLuong"], CultureInfo.InvariantCulture);
                decimal donGia = Convert.ToDecimal(row["DonGia"], CultureInfo.InvariantCulture);

                InsertChiTietNhap(conn, tran, maHdn, maNl, soLuong, donGia);
                UpdateSoLuongTon(conn, tran, maNl, soLuong, donGia);
            }

            tran.Commit();
            return Convert.ToString(maHdn) ?? string.Empty;
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

        private static int GenerateNextMaHdnInt(SqlConnection conn, SqlTransaction tran)
        {
            using SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(TRY_CAST(MaHDN AS INT)), 0) + 1 FROM dbo.HOA_DON_NHAP", conn, tran);
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
        }

        private static object InsertHoaDonNhap(SqlConnection conn, SqlTransaction tran, string maNv, string maNcc, decimal tongTien)
        {
            bool maHdnIdentity;
            using (SqlCommand cmdIdentity = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.HOA_DON_NHAP'),'MaHDN','IsIdentity') = 1 THEN 1 ELSE 0 END", conn, tran))
            {
                maHdnIdentity = Convert.ToInt32(cmdIdentity.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture) == 1;
            }

            bool maHdnNumeric = IsColumnNumeric(conn, tran, "HOA_DON_NHAP", "MaHDN");
            bool maNvNumeric = IsColumnNumeric(conn, tran, "HOA_DON_NHAP", "MaNV");
            bool maNccNumeric = IsColumnNumeric(conn, tran, "HOA_DON_NHAP", "MaNCC");

            object maNvValue = maNvNumeric && int.TryParse(maNv, out int nvInt) ? nvInt : maNv;
            object maNccValue = maNccNumeric && int.TryParse(maNcc, out int nccInt) ? nccInt : maNcc;

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
            cmd.Parameters.Add("@MaNL", SqlDbType.Int).Value = Convert.ToInt32(maNlValue, CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@SoLuong", SqlDbType.Decimal).Value = soLuong;
            cmd.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = donGia;
            cmd.Parameters["@SoLuong"].Precision = 18;
            cmd.Parameters["@SoLuong"].Scale = 2;
            cmd.Parameters["@DonGia"].Precision = 18;
            cmd.Parameters["@DonGia"].Scale = 2;
            cmd.ExecuteNonQuery();
        }

        private static void UpdateSoLuongTon(SqlConnection conn, SqlTransaction tran, object maNlValue, decimal soLuong, decimal donGia)
        {
            using SqlCommand cmd = new SqlCommand("UPDATE dbo.NGUYEN_LIEU SET SoLuongTon = ISNULL(SoLuongTon,0) + @SoLuong, GiaNhap = @DonGia WHERE MaNL = @MaNL", conn, tran);
            cmd.Parameters.Add("@MaNL", SqlDbType.Int).Value = Convert.ToInt32(maNlValue, CultureInfo.InvariantCulture);
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
