using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;

namespace PBL3.DataAccess
{
    internal sealed class LoaiMonRepository
    {
        public DataTable GetAll()
        {
            const string sql = "SELECT MaLoai, TenLoai FROM dbo.LOAI_MON ORDER BY MaLoai";
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public int Insert(string tenLoai)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.LOAI_MON (TenLoai) VALUES (@TenLoai)", conn);
            cmd.Parameters.Add("@TenLoai", SqlDbType.NVarChar, 100).Value = tenLoai;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Update(string maLoai, string tenLoai)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("UPDATE dbo.LOAI_MON SET TenLoai = @TenLoai WHERE MaLoai = @MaLoai", conn);
            cmd.Parameters.Add("@MaLoai", SqlDbType.VarChar, 20).Value = maLoai;
            cmd.Parameters.Add("@TenLoai", SqlDbType.NVarChar, 100).Value = tenLoai;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Delete(string maLoai)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("DELETE FROM dbo.LOAI_MON WHERE MaLoai = @MaLoai", conn);
            cmd.Parameters.Add("@MaLoai", SqlDbType.VarChar, 20).Value = maLoai;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}
