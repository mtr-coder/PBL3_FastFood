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

            bool laAdmin = nhanVien.MaCV == "1";
            return LoginResult.Success(nhanVien.MaNV, laAdmin);
        }

        public void CheckDatabaseConnection()
        {
            _authRepository.CheckConnection();
        }
    }
}
