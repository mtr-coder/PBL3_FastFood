using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class BanHangService
    {
        private readonly BanHangRepository _repository;

        public BanHangService()
        {
            _repository = new BanHangRepository();
        }

        public DataTable GetKhachHangOptions()
        {
            return _repository.GetKhachHangOptions();
        }

        public int CreateCustomerByPhone(string sdt)
        {
            return _repository.CreateCustomerByPhone(sdt.Trim());
        }

        public int SaveHoaDonBan(string maNv, int? maKh, decimal tongSauGiam, int diemCong, int diemDung, DataTable hoaDonTable)
        {
            return _repository.SaveHoaDonBan(maNv.Trim(), maKh, tongSauGiam, diemCong, diemDung, hoaDonTable);
        }

        public DataTable GetDanhMucMonAnData()
        {
            return _repository.GetDanhMucMonAnData();
        }

        public DataTable GetSizeOptionsForMon(string maMonRaw, string maMonNumeric, int fallbackMaDvt, string fallbackTenDvt, decimal fallbackGiaMon)
        {
            return _repository.GetSizeOptionsForMon(
                maMonRaw.Trim(),
                maMonNumeric.Trim(),
                fallbackMaDvt,
                fallbackTenDvt,
                fallbackGiaMon);
        }
    }
}
