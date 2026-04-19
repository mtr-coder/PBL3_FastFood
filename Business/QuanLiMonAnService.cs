using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class QuanLiMonAnService
    {
        private readonly QuanLiMonAnRepository _repository;

        public QuanLiMonAnService()
        {
            _repository = new QuanLiMonAnRepository();
        }

        public DataTable GetMonAn()
        {
            return _repository.GetMonAn();
        }

        public DataTable GetLoaiMonOptions()
        {
            return _repository.GetLoaiMonOptions();
        }

        public DataTable GetDonViTinhOptions()
        {
            return _repository.GetDonViTinhOptions();
        }

        public DataTable GetDonViPhucVuOptions()
        {
            return _repository.GetDonViPhucVuOptions();
        }

        public DataTable GetNguyenLieuOptions()
        {
            return _repository.GetNguyenLieuOptions();
        }

        public (DataTable sizeTable, DataTable dinhMucTable) GetMonDetails(string maMon)
        {
            return _repository.GetMonDetails(maMon.Trim());
        }

        public string InsertMonAn(string tenMon, string maLoai, string maDvt, string trangThai, DataTable sizeTable, DataTable dinhMucTable)
        {
            return _repository.InsertMonAn(tenMon.Trim(), maLoai.Trim(), maDvt.Trim(), trangThai.Trim(), sizeTable, dinhMucTable);
        }

        public int UpdateMonAn(string maMon, string tenMon, string maLoai, string maDvt, string trangThai, DataTable sizeTable, DataTable dinhMucTable)
        {
            return _repository.UpdateMonAn(maMon.Trim(), tenMon.Trim(), maLoai.Trim(), maDvt.Trim(), trangThai.Trim(), sizeTable, dinhMucTable);
        }

        public int DeleteMonAn(string maMon)
        {
            return _repository.DeleteMonAn(maMon.Trim());
        }

        public void DeleteSizeAndDinhMuc(string maMon, string maDvpv)
        {
            _repository.DeleteSizeAndDinhMuc(maMon.Trim(), maDvpv.Trim());
        }

        public void DeleteDinhMuc(string maMon, string maDvpv, string maNl)
        {
            _repository.DeleteDinhMuc(maMon.Trim(), maDvpv.Trim(), maNl.Trim());
        }

        public bool ForeignKeysExist(string maLoai, string maDvt)
        {
            return _repository.ForeignKeysExist(maLoai.Trim(), maDvt.Trim());
        }

        public string GenerateNextMaMon()
        {
            return _repository.GenerateNextMaMon();
        }
    }
}
