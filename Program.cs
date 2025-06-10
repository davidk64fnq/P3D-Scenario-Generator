using System.Text;

namespace P3D_Scenario_Generator
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Register the CodePages encoding provider
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Your existing code that uses Windows-1252 encoding
            _ = Encoding.GetEncoding("Windows-1252");

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form());
        }
    }
}
