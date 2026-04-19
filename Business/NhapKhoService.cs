using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class NhapKhoService
    {
        private readonly NhapKhoRepository _repository;

        public NhapKhoService()
        {
            _repository = new NhapKhoRepository();
        }

        public DataTable GetNguyenLieu()
        {
            return _repository.GetNguyenLieu();
        }

        public DataTable GetNhaCungCapByNguyenLieu(int maNl)
        {
            return _repository.GetNhaCungCapByNguyenLieu(maNl);
        }

        public string SaveNhapKho(string maNv, string maNcc, DataTable gioNhapTable)
        {
            return _repository.SaveNhapKho(maNv, maNcc, gioNhapTable);
        }
    }
}
