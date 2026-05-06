using PBL3.DataAccess;
using PBL3.Models;
using System.Data;

namespace PBL3.Business
{
    internal sealed class HopThuYeuCauService
    {
        private readonly HopThuYeuCauRepository _repository;

        public HopThuYeuCauService()
        {
            _repository = new HopThuYeuCauRepository();
        }

        public List<HopThuYeuCauItem> GetRequests(string loai, int trangThai)
        {
            return _repository.GetRequests(loai, trangThai);
        }

        public string? GetLatestShift(int maNv)
        {
            return _repository.GetLatestShift(maNv);
        }

        public List<string> GetHistory(int maNv)
        {
            return _repository.GetHistory(maNv);
        }

        public int GetApprovedLeaveCountForMonth(int maNv, DateTime month)
        {
            return _repository.GetApprovedLeaveCountForMonth(maNv, month);
        }

        public DataTable GetStaffingCounts(DateTime tuNgay, DateTime denNgay)
        {
            return _repository.GetStaffingCounts(tuNgay, denNgay);
        }

        public DataTable GetCaTruc()
        {
            return _repository.GetCaTruc();
        }

        public void ApproveRequest(HopThuYeuCauItem item, string phanHoi)
        {
            bool nghiHan = item.LoaiYeuCau.Contains("hẳn", StringComparison.OrdinalIgnoreCase)
                || item.LoaiYeuCau.Contains("han", StringComparison.OrdinalIgnoreCase);
            _repository.ApproveRequest(item.MaYeuCau, phanHoi.Trim(), nghiHan, item.MaNV, item.TuNgay, item.DenNgay);
        }

        public void RejectRequest(int maYeuCau, string phanHoi)
        {
            _repository.RejectRequest(maYeuCau, phanHoi.Trim());
        }

        public void DeleteRequest(int maYeuCau)
        {
            _repository.DeleteRequest(maYeuCau);
        }
    }
}
