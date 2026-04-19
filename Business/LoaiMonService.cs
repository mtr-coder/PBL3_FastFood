using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class LoaiMonService
    {
        private readonly LoaiMonRepository _repository;

        public LoaiMonService()
        {
            _repository = new LoaiMonRepository();
        }

        public DataTable GetAll()
        {
            return _repository.GetAll();
        }

        public int Add(string tenLoai)
        {
            return _repository.Insert(tenLoai.Trim());
        }

        public int Update(string maLoai, string tenLoai)
        {
            return _repository.Update(maLoai.Trim(), tenLoai.Trim());
        }

        public int Delete(string maLoai)
        {
            return _repository.Delete(maLoai.Trim());
        }
    }
}
