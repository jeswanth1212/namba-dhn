using System;
using System.Threading;
using System.IO;

internal static class Program
{
    private static Mutex? _mutex;
    
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
            // Load environment variables manually (no DotNetEnv dependency)
            LoadEnvironmentVariables();
            
            // Start the stealth assistant
            var assistant = new StealthAssistantCore();
            
            // Keep running in background
            Console.WriteLine("Service started. Press Ctrl+C to exit.");
            
            // Handle Ctrl+C gracefully
            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
                Environment.Exit(0);
            };
            
            // Keep the application running
            while (true)
            {
                Thread.Sleep(1000);
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
}