using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class NguyenLieuService
    {
        private readonly NguyenLieuRepository _repository;

        public NguyenLieuService()
        {
            _repository = new NguyenLieuRepository();
        }

        public DataTable GetAll()
        {
            return _repository.GetAll();
        }

        public void Add(string tenNl, string donViTinh, decimal giaNhap)
        {
            _repository.Insert(tenNl.Trim(), donViTinh.Trim(), giaNhap);
        }

        public int Update(string maNl, string tenNl, string donViTinh, decimal giaNhap, decimal nguongToiThieu)
        {
            return _repository.Update(maNl.Trim(), tenNl.Trim(), donViTinh.Trim(), giaNhap, nguongToiThieu);
        }

        public int Delete(string maNl)
        {
            return _repository.Delete(maNl.Trim());
        }

        public List<(string MaNl, string DonViTinh)> GetDonViTinhPairs()
        {
            return _repository.GetDonViTinhPairs();
        }

        public void UpdateDonViTinh(string maNl, string donViTinh)
        {
            _repository.UpdateDonViTinh(maNl.Trim(), donViTinh.Trim());
        }

        public void SeedSampleIfEmpty()
        {
            _repository.SeedSampleIfEmpty();
        }

        public string GenerateNextDisplayCode()
        {
            return _repository.GenerateNextDisplayCode();
        }
    }
}
