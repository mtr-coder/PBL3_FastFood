using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class KhachHangService
    {
        private readonly KhachHangRepository _repository;

        public KhachHangService()
        {
            _repository = new KhachHangRepository();
        }

        public DataTable GetAll()
        {
            return _repository.GetAll();
        }

        public DataTable GetForKhachHangPage()
        {
            return _repository.GetForKhachHangPage();
        }

        public bool IsPhoneExists(bool isInsert, string phone, string? excludeMaKh)
        {
            return _repository.IsPhoneExists(isInsert, phone.Trim(), excludeMaKh);
        }

        public void Add(string sdt, int diemTichLuy)
        {
            _repository.Insert(sdt.Trim(), diemTichLuy);
        }

        public int Update(string maKh, string sdt, int diemTichLuy)
        {
            return _repository.Update(maKh.Trim(), sdt.Trim(), diemTichLuy);
        }

        public int Delete(string maKh)
        {
            return _repository.Delete(maKh.Trim());
        }

        public string GenerateNextDisplayCode()
        {
            return _repository.GenerateNextDisplayCode();
        }

        public void AddWithName(string sdt, string tenKh)
        {
            _repository.InsertWithName(sdt.Trim(), tenKh.Trim());
        }

        public int UpdateHang(string maKh, int maHang)
        {
            return _repository.UpdateHang(maKh.Trim(), maHang);
        }
    }
}
