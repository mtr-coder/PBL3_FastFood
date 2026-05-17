using PBL3.DataAccess;
using PBL3.Models;
using System.Data;

namespace PBL3.Business
{
    internal sealed class LichSuHoaDonService
    {
        private readonly LichSuHoaDonRepository _repository;

        public LichSuHoaDonService()
        {
            _repository = new LichSuHoaDonRepository();
        }

        public LichSuHoaDonSchemaInfo DetectSchema()
        {
            return _repository.DetectSchema();
        }

        public (DateTime TuNgay, DateTime DenNgay) GetDefaultDateRange(bool isAdmin)
        {
            return _repository.GetDefaultDateRange(isAdmin);
        }

        public DataTable GetMasterData(string invoiceType, DateTime fromDate, DateTime toDate, bool isAdmin, string? maNvDangNhap, LichSuHoaDonSchemaInfo schema)
        {
            return _repository.GetMasterData(invoiceType, fromDate, toDate, isAdmin, maNvDangNhap, schema);
        }

        public DataTable GetDetailData(string invoiceType, string maHd)
        {
            return _repository.GetDetailData(invoiceType, maHd);
        }

        public decimal GetTienHangGoc(string maHd)
        {
            return _repository.GetTienHangGoc(maHd);
        }

        public int GetCanceledCount(DateTime fromDate, DateTime toDate, LichSuHoaDonSchemaInfo schema)
        {
            return _repository.GetCanceledCount(fromDate, toDate, schema);
        }

        public int GetPendingCancelCount(DateTime fromDate, DateTime toDate, LichSuHoaDonSchemaInfo schema)
        {
            return _repository.GetPendingCancelCount(fromDate, toDate, schema);
        }

        public DataTable GetPendingCancelList(DateTime fromDate, DateTime toDate, LichSuHoaDonSchemaInfo schema)
        {
            return _repository.GetPendingCancelList(fromDate, toDate, schema);
        }

        public void RejectCancelRequest(string maHd)
        {
            _repository.RejectCancelRequest(maHd);
        }

        public void CancelInvoice(string invoiceType, string maHd, LichSuHoaDonSchemaInfo schema)
        {
            _repository.CancelInvoice(invoiceType, maHd, schema);
        }
    }
}
