namespace PBL3.Models
{
    internal sealed class LichSuHoaDonSchemaInfo
    {
        public bool HasTrangThaiHdb { get; set; }
        public bool HasTrangThaiHdn { get; set; }
        public string TrangThaiHdbType { get; set; } = string.Empty;
        public string TrangThaiHdnType { get; set; } = string.Empty;
    }
}
