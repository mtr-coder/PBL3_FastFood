using PBL3.UI;

namespace PBL3
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // Start the application with the login form.
            Application.Run(new QuanLiNhanVien());
        }
    }
}