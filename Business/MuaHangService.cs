using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class MuaHangService
    {
        private readonly MuaHangRepository _repository;

        public MuaHangService()
        {
            _repository = new MuaHangRepository();
        }

        public DataTable GetNguyenLieu()
        {
            return _repository.GetNguyenLieu();
        }

        public DataTable GetNhaCungCap(string? maNl)
        {
            return _repository.GetNhaCungCap(maNl);
        }

        public DataTable GetDonViTinh()
        {
            return _repository.GetDonViTinh();
        }

        public void SavePhieuNhap(string maNv, object maNcc, decimal tongTien, DataTable phieuNhapTable)
        {
            _repository.SavePhieuNhap(maNv, maNcc, tongTien, phieuNhapTable);
        }
    }
}
