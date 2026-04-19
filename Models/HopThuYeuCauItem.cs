namespace PBL3.Models
{
    internal sealed class HopThuYeuCauItem
    {
        public int MaYeuCau { get; set; }
        public int MaNV { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenCV { get; set; } = string.Empty;
        public string LoaiYeuCau { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayGui { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public int TrangThai { get; set; }
        public string PhanHoiAdmin { get; set; } = string.Empty;
    }
}
