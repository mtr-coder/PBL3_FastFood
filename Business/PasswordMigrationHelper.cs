namespace PBL3.Business
{
    internal sealed class PasswordMigrationHelper
    {
        /// <param name="defaultPassword"
        public static string GenerateDefaultPasswordHash(string defaultPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(defaultPassword);
        }

        /// <param name="password">Password cần kiểm tra</param>
        public static bool IsBCryptHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Hash BCrypt luôn bắt đầu với $2a$, $2b$, $2x$, hoặc $2y$
            return password.StartsWith("$2a$", StringComparison.Ordinal)
                || password.StartsWith("$2b$", StringComparison.Ordinal)
                || password.StartsWith("$2x$", StringComparison.Ordinal)
                || password.StartsWith("$2y$", StringComparison.Ordinal);
        }
    }
}
