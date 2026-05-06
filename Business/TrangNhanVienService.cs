using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class TrangNhanVienService
    {
        private readonly TrangNhanVienRepository _repository;

        public TrangNhanVienService()
        {
            _repository = new TrangNhanVienRepository();
        }

        public DataTable GetChucVu()
        {
            return _repository.GetChucVu();
        }

        public DataTable GetAllNhanVien()
        {
            return _repository.GetAllNhanVien();
        }

        public DataTable GetNhanVienByMaNv(string maNv)
        {
            return _repository.GetNhanVienByMaNv(maNv.Trim());
        }

        public bool IsPhoneExists(bool isInsert, string phone, string? excludeMaNv)
        {
            return _repository.IsPhoneExists(isInsert, phone.Trim(), excludeMaNv?.Trim());
        }

        public void AddNhanVien(string hoTen, DateTime ngaySinh, string sdt, string email, string diaChi, string matKhau, string trangThai, string maCv)
        {
            _repository.AddNhanVien(hoTen.Trim(), ngaySinh, sdt.Trim(), email.Trim(), diaChi.Trim(), matKhau.Trim(), trangThai.Trim(), maCv.Trim());
        }

        public int UpdateNhanVien(string maNv, string hoTen, DateTime ngaySinh, string sdt, string email, string diaChi, string? matKhau)
        {
            return _repository.UpdateNhanVien(maNv.Trim(), hoTen.Trim(), ngaySinh, sdt.Trim(), email.Trim(), diaChi.Trim(), matKhau?.Trim());
        }

        public int UpdateNhanVienAdmin(string maNv, string hoTen, DateTime ngaySinh, string sdt, string email, string diaChi, string trangThai, string maCv, string? matKhau)
        {
            return _repository.UpdateNhanVienAdmin(
                maNv.Trim(),
                hoTen.Trim(),
                ngaySinh,
                sdt.Trim(),
                email.Trim(),
                diaChi.Trim(),
                trangThai.Trim(),
                maCv.Trim(),
                matKhau?.Trim());
        }

        public int DeleteNhanVien(string maNv)
        {
            return _repository.DeleteNhanVien(maNv.Trim());
        }

        public string GenerateNextDisplayCode()
        {
            return _repository.GenerateNextDisplayCode();
        }

        public void InsertYeuCau(int maNv, string loaiYeuCau, string noiDung, DateTime? tuNgay, DateTime? denNgay)
        {
            _repository.InsertYeuCau(maNv, loaiYeuCau.Trim(), noiDung.Trim(), tuNgay, denNgay);
        }

        public DataTable GetYeuCauHistory(int maNv)
        {
            return _repository.GetYeuCauHistory(maNv);
        }

        public DataTable GetLichTruc(string maNv)
        {
            return _repository.GetLichTruc(maNv.Trim());
        }

        public DataTable GetCaTruc()
        {
            return _repository.GetCaTruc();
        }

        public int GetApprovedLeaveCountForMonth(int maNv, DateTime month)
        {
            return _repository.GetApprovedLeaveCountForMonth(maNv, month);
        }

        public DataTable GetStaffingCounts(DateTime tuNgay, DateTime denNgay)
        {
            return _repository.GetStaffingCounts(tuNgay, denNgay);
        }

        public DataTable GetPhanCongCaRange(string maNv, DateTime tuNgay, DateTime denNgay)
        {
            return _repository.GetPhanCongCaRange(maNv, tuNgay, denNgay);
        }

        public int AddPhanCongCa(string maNv, string maCa, DateTime ngayLam)
        {
            return _repository.AddPhanCongCa(maNv.Trim(), maCa.Trim(), ngayLam);
        }

        public int DeletePhanCongCaByLeaveRange(string maNv, DateTime tuNgay, DateTime denNgay)
        {
            return _repository.DeletePhanCongCaByLeaveRange(maNv.Trim(), tuNgay, denNgay);
        }

        public string GetCurrentPassword(string maNv)
        {
            return _repository.GetCurrentPassword(maNv.Trim());
        }

        public int ResetPassword(string maNv, string newPassword)
        {
            return _repository.ResetPassword(maNv.Trim(), newPassword.Trim());
        }

        public decimal GetLuongCoBanNhanVien(string maNv)
        {
            return _repository.GetLuongCoBanNhanVien(maNv.Trim());
        }

        public int GetPendingYeuCauCount()
        {
            return _repository.GetPendingYeuCauCount();
        }
    }
}
