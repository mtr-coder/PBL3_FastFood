namespace PBL3.Models
{
    internal sealed class LoginResult
    {
        public bool IsSuccess { get; private set; }
        public bool IsAdmin { get; private set; }
        public string MaNV { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;

        public static LoginResult Success(string maNv, bool isAdmin)
        {
            return new LoginResult
            {
                IsSuccess = true,
                IsAdmin = isAdmin,
                MaNV = maNv
            };
        }

        public static LoginResult Fail(string message)
        {
            return new LoginResult
            {
                IsSuccess = false,
                ErrorMessage = message
            };
        }
    }
}
