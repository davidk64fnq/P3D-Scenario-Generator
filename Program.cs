using P3D_Scenario_Generator.Services;
using System.Text;

namespace P3D_Scenario_Generator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Create a logger instance that can be used throughout the application.
            Logger logger = new();

            // Set up a global exception handler for non-UI thread exceptions.
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception? ex = e.ExceptionObject as Exception;
                Task logTask = logger.ErrorAsync($"An unhandled exception occurred in a background thread.", ex);

                if (Application.OpenForms.Count > 0 && Application.OpenForms[0] is Form mainForm)
                {
                    // Set the flag to prevent the Form_FormClosing event from looping.
                    mainForm.SetIsShuttingDown(true);

                    // Report the error to the UI before exiting.
                    mainForm.GetProgressReporter()?.Report($"ERROR: Unhandled exception. See log for details.");
                }

                // Exit the application gracefully.
                Application.Exit();
            };

            // Set up a global exception handler for UI thread exceptions.
            Application.ThreadException += (sender, e) =>
            {
                Task logTask = logger.ErrorAsync($"A UI thread exception occurred.", e.Exception);

                if (Application.OpenForms.Count > 0 && Application.OpenForms[0] is Form mainForm)
                {
                    // Set the flag to prevent the Form_FormClosing event from looping.
                    mainForm.SetIsShuttingDown(true);

                    // Report the error to the UI before exiting.
                    mainForm.GetProgressReporter()?.Report($"ERROR: Unhandled exception. See log for details.");
                }

                // Exit the application gracefully.
                Application.Exit();
            };

            // Your existing encoding configuration
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _ = Encoding.GetEncoding("Windows-1252");

            // Your existing application configuration
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form());
        }
    }
}