using System;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace StealthAssistant
{
    internal static class Program
    {
        private static Mutex? _mutex;
        
        [STAThread]
        static void Main()
        {
            // Ensure only one instance runs
            bool createdNew;
            _mutex = new Mutex(true, "WindowsServiceHostMutex", out createdNew);
            
            if (!createdNew)
            {
                return; // Another instance is already running
            }

            try
            {
                // Load environment variables
                DotNetEnv.Env.Load();
                
                // Initialize application with stealth
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Hide from Alt+Tab
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                
                // Start the stealth assistant
                var assistant = new StealthAssistantCore();
                
                // Run without showing any window
                Application.Run();
            }
            finally
            {
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
        }
    }
}




