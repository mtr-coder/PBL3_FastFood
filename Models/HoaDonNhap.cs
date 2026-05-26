namespace PBL3.Models
{
    internal class HoaDonNhap
    {
        public int MaHDN { get; set; }
        public DateTime? NgayNhap { get; set; }
        public int? MaNV { get; set; }
        public int? MaNCC { get; set; }
        public decimal? TongTien { get; set; }
        public bool TrangThai { get; set; }
        public string? LyDoHuy { get; set; }
    }
}
