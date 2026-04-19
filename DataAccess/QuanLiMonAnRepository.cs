using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PBL3.DataAccess
{
    internal sealed class QuanLiMonAnRepository
    {
        public DataTable GetMonAn()
        {
            const string sql = @"
SELECT mb.MaMon, mb.TenMon, mb.MaLoai, lm.TenLoai, mb.MaDVT, dvt.TenDVT, mb.TrangThai
FROM dbo.MON_BAN mb
LEFT JOIN dbo.LOAI_MON lm ON lm.MaLoai = mb.MaLoai
LEFT JOIN dbo.DON_VI_TINH dvt ON dvt.MaDVT = mb.MaDVT
ORDER BY MaMon";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetLoaiMonOptions()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT MaLoai, TenLoai FROM dbo.LOAI_MON ORDER BY MaLoai", conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DataRow row = dt.NewRow();
            row["MaLoai"] = -1;
            row["TenLoai"] = "-Chỉnh sửa-";
            dt.Rows.Add(row);
            return dt;
        }

        public DataTable GetDonViTinhOptions()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT MaDVT, TenDVT FROM dbo.DON_VI_TINH ORDER BY MaDVT", conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DataRow row = dt.NewRow();
            row["MaDVT"] = -1;
            row["TenDVT"] = "-Chỉnh sửa-";
            dt.Rows.Add(row);
            return dt;
        }

        public DataTable GetDonViPhucVuOptions()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT MaDVPV, TenDVPV FROM dbo.DON_VI_PHUC_VU ORDER BY MaDVPV", conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetNguyenLieuOptions()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT MaNL, TenNL, DonViTinh, GiaNhap, SoLuongTon FROM dbo.NGUYEN_LIEU ORDER BY MaNL", conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public (DataTable sizeTable, DataTable dinhMucTable) GetMonDetails(string maMon)
        {
            DataTable sizeTable = new DataTable();
            sizeTable.Columns.Add("MaDVPV", typeof(string));
            sizeTable.Columns.Add("TenDVPV", typeof(string));
            sizeTable.Columns.Add("DonGia", typeof(decimal));
            sizeTable.Columns.Add("TrangThai", typeof(string));

            DataTable dinhMucTable = new DataTable();
            dinhMucTable.Columns.Add("MaDVPV", typeof(string));
            dinhMucTable.Columns.Add("MaNL", typeof(string));
            dinhMucTable.Columns.Add("TenNL", typeof(string));
            dinhMucTable.Columns.Add("SoLuongSuDung", typeof(decimal));
            dinhMucTable.Columns.Add("DonViTinh", typeof(string));
            dinhMucTable.Columns.Add("GiaNhap", typeof(decimal));
            dinhMucTable.Columns.Add("SoLuongTon", typeof(decimal));
            dinhMucTable.Columns.Add("ThanhTien", typeof(decimal), "SoLuongSuDung * GiaNhap");

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            const string sqlDinhMuc = @"SELECT dm.MaDVPV, dm.MaNL, nl.TenNL, dm.SoLuongSuDung, nl.DonViTinh, nl.GiaNhap, nl.SoLuongTon
FROM dbo.DINH_MUC_MON dm
LEFT JOIN dbo.NGUYEN_LIEU nl ON nl.MaNL = dm.MaNL
WHERE dm.MaMon = @MaMon";

            string giaColumn = ResolveMonDvpvGiaColumn(conn);
            bool hasTrangThaiColumn = HasMonDvpvTrangThaiColumn(conn);
            string trangThaiSelect = hasTrangThaiColumn ? "mdv.TrangThai" : "N'Đang bán' AS TrangThai";

            string sqlSize = $@"SELECT mdv.MaDVPV, dv.TenDVPV, mdv.{giaColumn} AS DonGia, {trangThaiSelect}
FROM dbo.MON_DON_VI_PHUC_VU mdv
LEFT JOIN dbo.DON_VI_PHUC_VU dv ON dv.MaDVPV = mdv.MaDVPV
WHERE mdv.MaMon = @MaMon";

            using (SqlCommand cmd = new SqlCommand(sqlSize, conn))
            {
                cmd.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sizeTable.Rows.Add(reader["MaDVPV"], reader["TenDVPV"], reader["DonGia"], reader["TrangThai"]);
                }
            }

            using (SqlCommand cmd = new SqlCommand(sqlDinhMuc, conn))
            {
                cmd.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    dinhMucTable.Rows.Add(reader["MaDVPV"], reader["MaNL"], reader["TenNL"], reader["SoLuongSuDung"], reader["DonViTinh"], reader["GiaNhap"], reader["SoLuongTon"]);
                }
            }

            return (sizeTable, dinhMucTable);
        }

        public string InsertMonAn(string tenMon, string maLoai, string maDvt, string trangThai, DataTable sizeTable, DataTable dinhMucTable)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            int nextId = 1;
            using (SqlCommand cmdGet = new SqlCommand("SELECT MaMon FROM dbo.MON_BAN ORDER BY TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaMon) LIKE 'MA%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaMon), 3, LEN(CONVERT(VARCHAR(20), MaMon)) - 2) ELSE CONVERT(VARCHAR(20), MaMon) END AS INT)", conn))
            using (SqlDataReader reader = cmdGet.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0)) continue;
                    string raw = Convert.ToString(reader.GetValue(0)) ?? string.Empty;
                    string numericPart = raw.StartsWith("MA", StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
                    if (!int.TryParse(numericPart, out int v)) continue;
                    if (v == nextId) nextId++; else if (v > nextId) break;
                }
            }

            bool hasIdentity;
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.MON_BAN'),'MaMon','IsIdentity') = 1 THEN 1 ELSE 0 END", conn))
            {
                hasIdentity = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture) == 1;
            }

            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                string maMonDbValue;
                if (hasIdentity)
                {
                    using SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT dbo.MON_BAN ON; INSERT INTO dbo.MON_BAN (MaMon, TenMon, MaLoai, MaDVT, TrangThai) VALUES (@MaMon, @TenMon, @MaLoai, @MaDVT, @TrangThai); SET IDENTITY_INSERT dbo.MON_BAN OFF;", conn, tran);
                    cmd.Parameters.Add("@MaMon", SqlDbType.Int).Value = nextId;
                    cmd.Parameters.Add("@TenMon", SqlDbType.NVarChar, 100).Value = tenMon;
                    cmd.Parameters.Add("@MaLoai", SqlDbType.VarChar, 20).Value = maLoai;
                    cmd.Parameters.Add("@MaDVT", SqlDbType.VarChar, 20).Value = maDvt;
                    cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = trangThai;
                    cmd.ExecuteNonQuery();
                    maMonDbValue = nextId.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    string maMonValue = $"MA{nextId}";
                    using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.MON_BAN (MaMon, TenMon, MaLoai, MaDVT, TrangThai) VALUES (@MaMon, @TenMon, @MaLoai, @MaDVT, @TrangThai)", conn, tran);
                    cmd.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMonValue;
                    cmd.Parameters.Add("@TenMon", SqlDbType.NVarChar, 100).Value = tenMon;
                    cmd.Parameters.Add("@MaLoai", SqlDbType.VarChar, 20).Value = maLoai;
                    cmd.Parameters.Add("@MaDVT", SqlDbType.VarChar, 20).Value = maDvt;
                    cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = trangThai;
                    cmd.ExecuteNonQuery();
                    maMonDbValue = maMonValue;
                }

                SaveSizeAndDinhMuc(conn, tran, maMonDbValue, sizeTable, dinhMucTable);
                tran.Commit();
                return maMonDbValue;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public int UpdateMonAn(string maMon, string tenMon, string maLoai, string maDvt, string trangThai, DataTable sizeTable, DataTable dinhMucTable)
        {
            const string sql = @"
UPDATE dbo.MON_BAN
SET TenMon = @TenMon,
    MaLoai = @MaLoai,
    MaDVT = @MaDVT,
    TrangThai = @TrangThai
WHERE MaMon = @MaMon";

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                int rows;
                using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                {
                    cmd.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                    cmd.Parameters.Add("@TenMon", SqlDbType.NVarChar, 100).Value = tenMon;
                    cmd.Parameters.Add("@MaLoai", SqlDbType.VarChar, 20).Value = maLoai;
                    cmd.Parameters.Add("@MaDVT", SqlDbType.VarChar, 20).Value = maDvt;
                    cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = trangThai;
                    rows = cmd.ExecuteNonQuery();
                }

                if (rows == 0)
                {
                    tran.Rollback();
                    return 0;
                }

                bool hasHoaDonBan = HasHoaDonBanReferences(conn, tran, maMon);
                if (!hasHoaDonBan)
                {
                    using SqlCommand clearDinhMuc = new SqlCommand("DELETE FROM dbo.DINH_MUC_MON WHERE MaMon = @MaMon", conn, tran);
                    clearDinhMuc.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                    clearDinhMuc.ExecuteNonQuery();

                    using SqlCommand clearSize = new SqlCommand("DELETE FROM dbo.MON_DON_VI_PHUC_VU WHERE MaMon = @MaMon", conn, tran);
                    clearSize.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                    clearSize.ExecuteNonQuery();

                    SaveSizeAndDinhMuc(conn, tran, maMon, sizeTable, dinhMucTable);
                }

                tran.Commit();
                return rows;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public int DeleteMonAn(string maMon)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("DELETE FROM dbo.MON_BAN WHERE MaMon = @MaMon", conn);
            cmd.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public void DeleteSizeAndDinhMuc(string maMon, string maDvpv)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                using SqlCommand cmdDelDm = new SqlCommand("DELETE FROM dbo.DINH_MUC_MON WHERE MaMon = @MaMon AND MaDVPV = @MaDVPV", conn, tran);
                cmdDelDm.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                cmdDelDm.Parameters.Add("@MaDVPV", SqlDbType.VarChar, 20).Value = maDvpv;
                cmdDelDm.ExecuteNonQuery();

                using SqlCommand cmdDelSize = new SqlCommand("DELETE FROM dbo.MON_DON_VI_PHUC_VU WHERE MaMon = @MaMon AND MaDVPV = @MaDVPV", conn, tran);
                cmdDelSize.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                cmdDelSize.Parameters.Add("@MaDVPV", SqlDbType.VarChar, 20).Value = maDvpv;
                cmdDelSize.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void DeleteDinhMuc(string maMon, string maDvpv, string maNl)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand("DELETE FROM dbo.DINH_MUC_MON WHERE MaMon = @MaMon AND MaDVPV = @MaDVPV AND MaNL = @MaNL", conn);
            cmd.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
            cmd.Parameters.Add("@MaDVPV", SqlDbType.VarChar, 20).Value = maDvpv;
            cmd.Parameters.Add("@MaNL", SqlDbType.VarChar, 20).Value = maNl;
            cmd.ExecuteNonQuery();
        }

        public bool ForeignKeysExist(string maLoai, string maDvt)
        {
            const string sql = @"
SELECT
    CASE WHEN EXISTS (SELECT 1 FROM dbo.LOAI_MON WHERE MaLoai = @MaLoai) THEN 1 ELSE 0 END AS HasLoai,
    CASE WHEN EXISTS (SELECT 1 FROM dbo.DON_VI_TINH WHERE MaDVT = @MaDVT) THEN 1 ELSE 0 END AS HasDvt;";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaLoai", SqlDbType.VarChar, 20).Value = maLoai;
            cmd.Parameters.Add("@MaDVT", SqlDbType.VarChar, 20).Value = maDvt;
            conn.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read()) return false;
            return reader.GetInt32(0) == 1 && reader.GetInt32(1) == 1;
        }

        public string GenerateNextMaMon()
        {
            const string sql = @"SELECT MaMon FROM dbo.MON_BAN
ORDER BY TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaMon) LIKE 'MA%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaMon), 3, LEN(CONVERT(VARCHAR(20), MaMon)) - 2) ELSE CONVERT(VARCHAR(20), MaMon) END AS INT)";

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            int nextId = 1;
            while (reader.Read())
            {
                if (reader.IsDBNull(0)) continue;
                string raw = Convert.ToString(reader.GetValue(0)) ?? string.Empty;
                string numeric = raw.StartsWith("MA", StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
                if (!int.TryParse(numeric, out int v)) continue;
                if (v == nextId) nextId++; else if (v > nextId) break;
            }
            return $"MA{nextId}";
        }

        private static void SaveSizeAndDinhMuc(SqlConnection conn, SqlTransaction tran, string maMon, DataTable sizeTable, DataTable dinhMucTable)
        {
            string giaColumn = ResolveMonDvpvGiaColumn(conn, tran);
            bool hasTrangThaiColumn = HasMonDvpvTrangThaiColumn(conn, tran);

            string insertSizeSql = hasTrangThaiColumn
                ? $@"INSERT INTO dbo.MON_DON_VI_PHUC_VU (MaMon, MaDVPV, {giaColumn}, TrangThai)
VALUES (@MaMon, @MaDVPV, @DonGia, @TrangThai)"
                : $@"INSERT INTO dbo.MON_DON_VI_PHUC_VU (MaMon, MaDVPV, {giaColumn})
VALUES (@MaMon, @MaDVPV, @DonGia)";

            foreach (DataRow size in sizeTable.Rows)
            {
                using SqlCommand cmdSize = new SqlCommand(insertSizeSql, conn, tran);
                cmdSize.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                cmdSize.Parameters.Add("@MaDVPV", SqlDbType.VarChar, 20).Value = Convert.ToString(size["MaDVPV"]) ?? string.Empty;
                cmdSize.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = Convert.ToDecimal(size["DonGia"], CultureInfo.InvariantCulture);
                if (hasTrangThaiColumn)
                {
                    cmdSize.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = Convert.ToString(size["TrangThai"]) ?? "Đang bán";
                }
                cmdSize.ExecuteNonQuery();
            }

            const string insertDinhMucSql = @"INSERT INTO dbo.DINH_MUC_MON (MaMon, MaDVPV, MaNL, SoLuongSuDung)
VALUES (@MaMon, @MaDVPV, @MaNL, @SoLuongSuDung)";

            foreach (DataRow dm in dinhMucTable.Rows)
            {
                using SqlCommand cmdDm = new SqlCommand(insertDinhMucSql, conn, tran);
                cmdDm.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
                cmdDm.Parameters.Add("@MaDVPV", SqlDbType.VarChar, 20).Value = Convert.ToString(dm["MaDVPV"]) ?? string.Empty;
                cmdDm.Parameters.Add("@MaNL", SqlDbType.VarChar, 20).Value = Convert.ToString(dm["MaNL"]) ?? string.Empty;
                cmdDm.Parameters.Add("@SoLuongSuDung", SqlDbType.Decimal).Value = Convert.ToDecimal(dm["SoLuongSuDung"], CultureInfo.InvariantCulture);
                cmdDm.ExecuteNonQuery();
            }
        }

        private static bool HasHoaDonBanReferences(SqlConnection conn, SqlTransaction tran, string maMon)
        {
            const string sql = @"SELECT CASE WHEN EXISTS (
SELECT 1 FROM dbo.CT_HOA_DON_BAN WHERE MaMon = @MaMon
) THEN 1 ELSE 0 END";

            using SqlCommand cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.Add("@MaMon", SqlDbType.VarChar, 20).Value = maMon;
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
        }

        private static string ResolveMonDvpvGiaColumn(SqlConnection conn, SqlTransaction? tran = null)
        {
            const string sql = @"SELECT TOP 1 COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'MON_DON_VI_PHUC_VU'
  AND COLUMN_NAME IN ('DonGia', 'GiaBan', 'Gia')
ORDER BY CASE COLUMN_NAME WHEN 'DonGia' THEN 1 WHEN 'GiaBan' THEN 2 WHEN 'Gia' THEN 3 ELSE 99 END";

            using SqlCommand cmd = new SqlCommand(sql, conn, tran);
            string? columnName = Convert.ToString(cmd.ExecuteScalar());
            return string.IsNullOrWhiteSpace(columnName) ? "DonGia" : columnName;
        }

        private static bool HasMonDvpvTrangThaiColumn(SqlConnection conn, SqlTransaction? tran = null)
        {
            const string sql = @"SELECT CASE WHEN EXISTS (
SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'MON_DON_VI_PHUC_VU'
  AND COLUMN_NAME = 'TrangThai')
THEN 1 ELSE 0 END";

            using SqlCommand cmd = new SqlCommand(sql, conn, tran);
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
        }
    }
}
