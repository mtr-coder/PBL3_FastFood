using PBL3.DataBase;
using PBL3.Models;
using System.Data;
using System.Data.SqlClient;

namespace PBL3.DataAccess
{
    internal sealed class NhaCungCapRepository
    {
        public NhaCungCapSchemaInfo GetSchemaInfo()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            return new NhaCungCapSchemaInfo
            {
                HasTrangThaiColumn = TableColumnExists(conn, "NHA_CUNG_CAP", "TrangThai"),
                HasEmailColumn = TableColumnExists(conn, "NHA_CUNG_CAP", "Email"),
                HasGhiChuColumn = TableColumnExists(conn, "NHA_CUNG_CAP", "GhiChu")
            };
        }

        private static bool TableColumnExists(SqlConnection conn, string tableName, string columnName)
        {
            using SqlCommand cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName AND COLUMN_NAME=@ColumnName) THEN 1 ELSE 0 END", conn);
            cmd.Parameters.Add("@TableName", SqlDbType.VarChar, 128).Value = tableName;
            cmd.Parameters.Add("@ColumnName", SqlDbType.VarChar, 128).Value = columnName;
            return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
        }

        public DataTable GetAll(bool hasEmailColumn, bool hasGhiChuColumn)
        {
            string emailSelect = hasEmailColumn ? "ncc.Email AS Email" : "CAST(NULL AS NVARCHAR(200)) AS Email";
            string ghiChuSelect = hasGhiChuColumn ? "ncc.GhiChu AS GhiChu" : "CAST(NULL AS NVARCHAR(300)) AS GhiChu";

            string sql = $@"
SELECT ncc.MaNCC, ncc.TenNCC, ncc.SDT,
       {emailSelect},
       ncc.DiaChi,
       ISNULL(hang.MatHangChuYeu, N'') AS MatHangChuYeu,
       {ghiChuSelect}
FROM dbo.NHA_CUNG_CAP ncc
OUTER APPLY (
    SELECT STUFF((
        SELECT DISTINCT N', ' + nl.TenNL
        FROM dbo.HOA_DON_NHAP hdn
        JOIN dbo.CT_HOA_DON_NHAP cthdn ON hdn.MaHDN = cthdn.MaHDN
        JOIN dbo.NGUYEN_LIEU nl ON cthdn.MaNL = nl.MaNL
        WHERE hdn.MaNCC = ncc.MaNCC
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 2, N'') AS MatHangChuYeu
) hang
ORDER BY TRY_CAST(ncc.MaNCC AS INT), ncc.MaNCC";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public bool IsPhoneExists(bool isInsert, string phone, string? excludeMaNcc)
        {
            string query = isInsert
                ? "SELECT COUNT(1) FROM dbo.NHA_CUNG_CAP WHERE SDT = @SDT"
                : "SELECT COUNT(1) FROM dbo.NHA_CUNG_CAP WHERE SDT = @SDT AND MaNCC != @ID";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@SDT", SqlDbType.VarChar).Value = phone;
            if (!isInsert)
            {
                cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = excludeMaNcc ?? string.Empty;
            }

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void Insert(string tenNcc, string sdt, string diaChi, string email, string ghiChu, bool hasEmailColumn, bool hasGhiChuColumn, bool hasTrangThaiColumn)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            int nextId = 1;
            using (SqlCommand cmdGet = new SqlCommand("SELECT MaNCC FROM dbo.NHA_CUNG_CAP ORDER BY TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaNCC) LIKE 'NCC%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaNCC), 4, LEN(CONVERT(VARCHAR(20), MaNCC)) - 3) ELSE CONVERT(VARCHAR(20), MaNCC) END AS INT)", conn))
            using (SqlDataReader reader = cmdGet.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        continue;
                    }

                    string raw = Convert.ToString(reader.GetValue(0)) ?? string.Empty;
                    string numeric = raw.StartsWith("NCC", StringComparison.OrdinalIgnoreCase) ? raw.Substring(3) : raw;
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
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.NHA_CUNG_CAP'),'MaNCC','IsIdentity') = 1 THEN 1 ELSE 0 END", conn))
            {
                hasIdentity = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0) == 1;
            }

            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                string insertColumns = "MaNCC, TenNCC, SDT, DiaChi";
                string insertValues = "@MaNCC, @TenNCC, @SDT, @DiaChi";
                if (hasEmailColumn)
                {
                    insertColumns += ", Email";
                    insertValues += ", @Email";
                }

                if (hasGhiChuColumn)
                {
                    insertColumns += ", GhiChu";
                    insertValues += ", @GhiChu";
                }

                if (hasTrangThaiColumn)
                {
                    insertColumns += ", TrangThai";
                    insertValues += ", @TrangThai";
                }

                string sqlInsert = $"INSERT INTO dbo.NHA_CUNG_CAP ({insertColumns}) VALUES ({insertValues})";

                if (hasIdentity)
                {
                    using SqlCommand cmd = new SqlCommand($"SET IDENTITY_INSERT dbo.NHA_CUNG_CAP ON; {sqlInsert}; SET IDENTITY_INSERT dbo.NHA_CUNG_CAP OFF;", conn, tran);
                    cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value = nextId;
                    cmd.Parameters.Add("@TenNCC", SqlDbType.NVarChar, 100).Value = tenNcc;
                    cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
                    cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 200).Value = diaChi;
                    if (hasEmailColumn)
                    {
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = email;
                    }

                    if (hasGhiChuColumn)
                    {
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 300).Value = ghiChu;
                    }

                    if (hasTrangThaiColumn)
                    {
                        cmd.Parameters.Add("@TrangThai", SqlDbType.Bit).Value = true;
                    }

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    using SqlCommand cmd = new SqlCommand(sqlInsert, conn, tran);
                    cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value = nextId;
                    cmd.Parameters.Add("@TenNCC", SqlDbType.NVarChar, 100).Value = tenNcc;
                    cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
                    cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 200).Value = diaChi;
                    if (hasEmailColumn)
                    {
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = email;
                    }

                    if (hasGhiChuColumn)
                    {
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 300).Value = ghiChu;
                    }

                    if (hasTrangThaiColumn)
                    {
                        cmd.Parameters.Add("@TrangThai", SqlDbType.Bit).Value = true;
                    }

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

        public int Update(string maNcc, string tenNcc, string sdt, string diaChi, string email, string ghiChu, bool hasEmailColumn, bool hasGhiChuColumn)
        {
            string sql = @"
UPDATE dbo.NHA_CUNG_CAP
SET TenNCC = @TenNCC,
    SDT = @SDT,
    DiaChi = @DiaChi";

            if (hasEmailColumn)
            {
                sql += ", Email = @Email";
            }

            if (hasGhiChuColumn)
            {
                sql += ", GhiChu = @GhiChu";
            }

            sql += " WHERE MaNCC = @MaNCC";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNCC", SqlDbType.VarChar, 20).Value = maNcc;
            cmd.Parameters.Add("@TenNCC", SqlDbType.NVarChar, 100).Value = tenNcc;
            cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
            cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 200).Value = diaChi;
            if (hasEmailColumn)
            {
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = email;
            }

            if (hasGhiChuColumn)
            {
                cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 300).Value = ghiChu;
            }

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int GetNhapHistoryCount(string maNcc)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.HOA_DON_NHAP WHERE MaNCC = @MaNCC", conn);
            cmd.Parameters.Add("@MaNCC", SqlDbType.VarChar, 20).Value = maNcc;
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        public int SoftDeactivate(string maNcc)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("UPDATE dbo.NHA_CUNG_CAP SET TrangThai = 0 WHERE MaNCC = @MaNCC", conn);
            cmd.Parameters.Add("@MaNCC", SqlDbType.VarChar, 20).Value = maNcc;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Delete(string maNcc)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("DELETE FROM dbo.NHA_CUNG_CAP WHERE MaNCC = @MaNCC", conn);
            cmd.Parameters.Add("@MaNCC", SqlDbType.VarChar, 20).Value = maNcc;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public DataTable GetNhapHistory(string maNcc)
        {
            string sql = @"SELECT hdn.MaHDN,
       hdn.NgayNhap,
       CAST(COALESCE(NULLIF(hdn.TongTien, 0), ct.TongTienChiTiet, 0) AS DECIMAL(18, 0)) AS TongTien
FROM dbo.HOA_DON_NHAP hdn
OUTER APPLY (
    SELECT SUM(ISNULL(cthdn.SoLuong, 0) * ISNULL(cthdn.DonGia, 0)) AS TongTienChiTiet
    FROM dbo.CT_HOA_DON_NHAP cthdn
    WHERE CONVERT(NVARCHAR(50), cthdn.MaHDN) = CONVERT(NVARCHAR(50), hdn.MaHDN)
) ct
WHERE hdn.MaNCC = @MaNCC
ORDER BY hdn.NgayNhap DESC, hdn.MaHDN DESC";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNCC", SqlDbType.VarChar, 20).Value = maNcc;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetNhapDetail(string maHdn)
        {
            const string sql = @"
SELECT ISNULL(nl.TenNL, CONVERT(NVARCHAR(100), ct.MaNL)) AS TenNguyenLieu,
       ISNULL(ct.SoLuong, 0) AS SoLuong,
       CAST(ISNULL(ct.DonGia, 0) AS DECIMAL(18, 0)) AS DonGia,
       CAST(ISNULL(ct.SoLuong, 0) * ISNULL(ct.DonGia, 0) AS DECIMAL(18, 0)) AS ThanhTien
FROM dbo.CT_HOA_DON_NHAP ct
LEFT JOIN dbo.NGUYEN_LIEU nl ON nl.MaNL = ct.MaNL
WHERE ct.MaHDN = @MaHDN";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaHDN", SqlDbType.VarChar, 20).Value = maHdn;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public string GenerateNextDisplayCode()
        {
            const string sql = @"SELECT MaNCC FROM dbo.NHA_CUNG_CAP
ORDER BY TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaNCC) LIKE 'NCC%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaNCC), 4, LEN(CONVERT(VARCHAR(20), MaNCC)) - 3) ELSE CONVERT(VARCHAR(20), MaNCC) END AS INT)";

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
                string numeric = raw.StartsWith("NCC", StringComparison.OrdinalIgnoreCase) ? raw.Substring(3) : raw;
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

            return $"NCC{nextId}";
        }
    }
}
