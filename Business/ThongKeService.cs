using PBL3.DataAccess;
using PBL3.Models;

namespace PBL3.Business
{
    internal sealed class ThongKeService
    {
        private readonly ThongKeRepository _repository;

        public ThongKeService()
        {
            _repository = new ThongKeRepository();
        }

        public ThongKeDashboardData GetDashboardData(DateTime from, DateTime to)
        {
            return _repository.GetDashboardData(from, to);
        }
    }
}
