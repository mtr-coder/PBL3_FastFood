using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class TrangHoaDonService
    {
        private readonly TrangHoaDonRepository _repository;

        public TrangHoaDonService()
        {
            _repository = new TrangHoaDonRepository();
        }

        public DataTable GetHoaDonMaster(string loaiHoaDon, string maNv)
        {
            return _repository.GetHoaDonMaster(loaiHoaDon, maNv);
        }

        public DataTable GetHoaDonDetail(string loaiHoaDon, string maHd)
        {
            return _repository.GetHoaDonDetail(loaiHoaDon, maHd);
        }

        public void DeleteHoaDon(string loaiHoaDon, string maHd)
        {
            _repository.DeleteHoaDon(loaiHoaDon, maHd);
        }

        public bool RequestCancelBanInvoice(string maHd, string lyDoHuy)
        {
            return _repository.RequestCancelBanInvoice(maHd, lyDoHuy);
        }

        public bool ValidateManagerPassword(string password)
        {
            return _repository.ValidateManagerPassword(password);
        }

        public decimal GetTienHangGoc(string maHd)
        {
            return _repository.GetTienHangGoc(maHd);
        }
    }
}
