using PBL3.DataAccess;

namespace PBL3.Business
{
    internal sealed class PasswordRecoveryService
    {
        private readonly PasswordRecoveryRepository _repository;

        public PasswordRecoveryService()
        {
            _repository = new PasswordRecoveryRepository();
        }

        public (bool IsSuccess, string Email, string Error) TryResolveEmployeeEmail(string account)
        {
            try
            {
                string? email = _repository.GetEmployeeEmail(account.Trim());
                if (string.IsNullOrWhiteSpace(email))
                {
                    return (false, string.Empty, "Không tìm thấy email của tài khoản này.");
                }

                return (true, email, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, "Lỗi kiểm tra tài khoản: " + ex.Message);
            }
        }

        public int ResetPassword(string account, string newPassword)
        {
            return _repository.UpdatePassword(account.Trim(), newPassword);
        }
    }
}
