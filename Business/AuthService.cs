using PBL3.DataAccess;
using PBL3.Models;

namespace PBL3.Business
{
    internal sealed class AuthService
    {
        private readonly AuthRepository _authRepository;

        public AuthService()
        {
            _authRepository = new AuthRepository();
        }

        public LoginResult Authenticate(string soDienThoai, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(soDienThoai) || string.IsNullOrWhiteSpace(matKhau))
            {
                return LoginResult.Fail("Vui lòng nhập đầy đủ số điện thoại và mật khẩu!");
            }

            NhanVienDangNhapInfo? nhanVien = _authRepository.GetByPhoneAndPassword(soDienThoai.Trim(), matKhau.Trim());
            if (nhanVien is null)
            {
                return LoginResult.Fail("Sai số điện thoại hoặc mật khẩu!");
            }

            // Kiểm tra mật khẩu bằng BCrypt
            System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] MatKhau nhap: '{matKhau.Trim()}'");
            System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] MatKhau DB: '{nhanVien.MatKhau}'");
            System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] MatKhau DB length: {nhanVien.MatKhau.Length}");
            System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Hash dung cua '123': '{BCrypt.Net.BCrypt.HashPassword("123")}'");
            if (!BCrypt.Net.BCrypt.Verify(matKhau.Trim(), nhanVien.MatKhau))
            {
                return LoginResult.Fail("Sai số điện thoại hoặc mật khẩu!");
            }

            bool laAdmin = nhanVien.MaCV == AppConstants.MaCvAdmin;
            SessionManager.Login(nhanVien.MaNV, nhanVien.MaCV, laAdmin, nhanVien.HoTen);
            return LoginResult.Success(nhanVien.MaNV, laAdmin);
        }

        public void CheckDatabaseConnection()
        {
            _authRepository.CheckConnection();
        }
    }
}
