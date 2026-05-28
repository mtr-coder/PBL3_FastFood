namespace PBL3.Business
{
    internal static class SessionManager
    {
        public static string MaNV { get; private set; } = string.Empty;
        public static string MaCV { get; private set; } = string.Empty;
        public static string HoTen { get; private set; } = string.Empty;
        public static bool IsAdmin { get; private set; }

        public static void Login(string maNv, string maCv, bool isAdmin, string hoTen)
        {
            MaNV = maNv;
            MaCV = maCv;
            IsAdmin = isAdmin;
            HoTen = hoTen;
        }

        public static void Logout()
        {
            MaNV = string.Empty;
            MaCV = string.Empty;
            HoTen = string.Empty;
            IsAdmin = false;
        }
    }
}
