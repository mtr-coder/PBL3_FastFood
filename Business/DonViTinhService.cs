using PBL3.DataAccess;
using System.Data;

namespace PBL3.Business
{
    internal sealed class DonViTinhService
    {
        private readonly DonViTinhRepository _repository;

        public DonViTinhService()
        {
            _repository = new DonViTinhRepository();
        }

        public DataTable GetAll()
        {
            return _repository.GetAll();
        }

        public void Add(string tenDvt)
        {
            _repository.Insert(tenDvt.Trim());
        }

        public int Update(int maDvt, string tenDvt)
        {
            return _repository.Update(maDvt, tenDvt.Trim());
        }

        public int Delete(int maDvt)
        {
            return _repository.Delete(maDvt);
        }
    }
}
