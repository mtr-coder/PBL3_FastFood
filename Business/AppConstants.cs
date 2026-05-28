namespace PBL3.Business
{
    /// <summary>
    /// Tap trung cac hang so dung chung trong toan bo project.
    /// Thay vi dung magic numbers/strings rai rac, su dung AppConstants.XYZ.
    /// </summary>
    internal static class AppConstants
    {
        // === Chuc vu ===
        /// <summary>Ma chuc vu Admin/Quan ly.</summary>
        public const string MaCvAdmin = "1";

        // === Luong & Ca truc ===
        /// <summary>Don gia co ban cho 1 ca truc (VND).</summary>
        public const decimal DonGiaCaTruc = 176000m;

        // === Diem khach hang ===
        /// <summary>So diem can de doi 1 moc giam gia.</summary>
        public const int DiemMoiMocGiam = 10;

        /// <summary>So tien giam (VND) cho moi moc diem.</summary>
        public const int TienGiamMoiMoc = 10000;

        /// <summary>Nguong tong tien hoa don de cong diem (VND).</summary>
        public const int NguongCongDiem = 100000;

        /// <summary>So diem cong cho moi nguong dat duoc.</summary>
        public const int DiemCongMoiNguong = 10;

        // === Nghi phep ===
        /// <summary>So ngay nghi phep toi da trong 1 thang.</summary>
        public const int SoNgayNghiPhepToiDa = 3;
    }
}
