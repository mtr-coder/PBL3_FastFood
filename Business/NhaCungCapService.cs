using PBL3.DataAccess;
using PBL3.Models;
using System.Data;

namespace PBL3.Business
{
    internal sealed class NhaCungCapService
    {
        private readonly NhaCungCapRepository _repository;

        public NhaCungCapService()
        {
            _repository = new NhaCungCapRepository();
        }

        public NhaCungCapSchemaInfo GetSchemaInfo()
        {
            return _repository.GetSchemaInfo();
        }

        public DataTable GetAll(bool hasEmailColumn, bool hasGhiChuColumn)
        {
            return _repository.GetAll(hasEmailColumn, hasGhiChuColumn);
        }

        public bool IsPhoneExists(bool isInsert, string phone, string? excludeMaNcc)
        {
            return _repository.IsPhoneExists(isInsert, phone.Trim(), excludeMaNcc);
        }

        public void Add(string tenNcc, string sdt, string diaChi, string? email, string? ghiChu, bool hasEmailColumn, bool hasGhiChuColumn, bool hasTrangThaiColumn)
        {
            _repository.Insert(tenNcc.Trim(), sdt.Trim(), diaChi.Trim(), email?.Trim() ?? string.Empty, ghiChu?.Trim() ?? string.Empty, hasEmailColumn, hasGhiChuColumn, hasTrangThaiColumn);
        }

        public int Update(string maNcc, string tenNcc, string sdt, string diaChi, string? email, string? ghiChu, bool hasEmailColumn, bool hasGhiChuColumn)
        {
            return _repository.Update(maNcc.Trim(), tenNcc.Trim(), sdt.Trim(), diaChi.Trim(), email?.Trim() ?? string.Empty, ghiChu?.Trim() ?? string.Empty, hasEmailColumn, hasGhiChuColumn);
        }

        public int GetNhapHistoryCount(string maNcc)
        {
            return _repository.GetNhapHistoryCount(maNcc.Trim());
        }

        public int SoftDeactivate(string maNcc)
        {
            return _repository.SoftDeactivate(maNcc.Trim());
        }

        public int Delete(string maNcc)
        {
            return _repository.Delete(maNcc.Trim());
        }

        public DataTable GetNhapHistory(string maNcc)
        {
            return _repository.GetNhapHistory(maNcc.Trim());
        }

        public DataTable GetNhapDetail(string maHdn)
        {
            return _repository.GetNhapDetail(maHdn.Trim());
        }

        public string GenerateNextDisplayCode()
        {
            return _repository.GenerateNextDisplayCode();
        }
    }
}
