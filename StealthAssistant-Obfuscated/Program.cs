using System;
using System.Threading;
using System.Windows.Forms;

namespace StealthAssistant.Obfuscated
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
                
                // Create obfuscated core
                var core = new ObfuscatedCore();
                
                // Run message loop
                Application.Run();
                
                // Cleanup
                core.Dispose();
            }
            catch
            {
                // Silent exit on any error
                Environment.Exit(0);
            }
        }
    }
}