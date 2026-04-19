using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;

namespace PBL3.DataAccess
{
    internal sealed class DonViTinhRepository
    {
        public DataTable GetAll()
        {
            const string sql = "SELECT MaDVT, TenDVT FROM dbo.DON_VI_TINH ORDER BY MaDVT";
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void Insert(string tenDvt)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            int nextId = 1;
            using (SqlCommand cmdGet = new SqlCommand("SELECT MaDVT FROM dbo.DON_VI_TINH ORDER BY TRY_CAST(MaDVT AS INT)", conn))
            using (SqlDataReader reader = cmdGet.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        continue;
                    }

                    if (!int.TryParse(Convert.ToString(reader.GetValue(0)), out int v))
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
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.DON_VI_TINH'),'MaDVT','IsIdentity') = 1 THEN 1 ELSE 0 END", conn))
            {
                hasIdentity = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0) == 1;
            }

            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                if (hasIdentity)
                {
                    using SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT dbo.DON_VI_TINH ON; INSERT INTO dbo.DON_VI_TINH (MaDVT, TenDVT) VALUES (@MaDVT, @TenDVT); SET IDENTITY_INSERT dbo.DON_VI_TINH OFF;", conn, tran);
                    cmd.Parameters.Add("@MaDVT", SqlDbType.Int).Value = nextId;
                    cmd.Parameters.Add("@TenDVT", SqlDbType.NVarChar, 100).Value = tenDvt;
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.DON_VI_TINH (MaDVT, TenDVT) VALUES (@MaDVT, @TenDVT)", conn, tran);
                    cmd.Parameters.Add("@MaDVT", SqlDbType.Int).Value = nextId;
                    cmd.Parameters.Add("@TenDVT", SqlDbType.NVarChar, 100).Value = tenDvt;
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

        public int Update(int maDvt, string tenDvt)
        {
            const string sql = "UPDATE dbo.DON_VI_TINH SET TenDVT = @TenDVT WHERE MaDVT = @MaDVT";
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaDVT", SqlDbType.Int).Value = maDvt;
            cmd.Parameters.Add("@TenDVT", SqlDbType.NVarChar, 100).Value = tenDvt;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Delete(int maDvt)
        {
            const string sql = "DELETE FROM dbo.DON_VI_TINH WHERE MaDVT = @MaDVT";
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaDVT", SqlDbType.Int).Value = maDvt;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}
