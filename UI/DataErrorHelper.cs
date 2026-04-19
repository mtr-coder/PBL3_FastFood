namespace PBL3
{
    internal static class DataErrorHelper
    {
        public static bool IsForeignKeyViolation(Exception ex)
        {
            Exception? current = ex;
            while (current is not null)
            {
                string message = current.Message;
                if (message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("The DELETE statement conflicted", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("The UPDATE statement conflicted", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }
    }
}
