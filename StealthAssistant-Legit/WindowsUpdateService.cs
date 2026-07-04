using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Diagnostics;

public class WindowsUpdateService
{
    #region Windows API Imports (Minimal and legitimate)
    
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    
    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);
    
    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);
    
    [DllImport("user32.dll")]
    private static extern bool SetClipboardData(uint uFormat, IntPtr hMem);
    
    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();
    
    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
    
    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);
    
    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);
    
    private const uint CF_TEXT = 1;
    private const uint GMEM_MOVEABLE = 0x0002;
    
    // Virtual key codes
    private const int VK_Z = 0x5A;
    private const int VK_M = 0x4D;
    private const int VK_W = 0x57;
    private const int VK_1 = 0x31;
    
    #endregion
    
    #region Fields
    
    private readonly HttpClient _httpClient;
    private readonly string _geminiApiKey;
    private bool _zPressed = false;
    private DateTime _lastZPress = DateTime.MinValue;
    private readonly System.Threading.Timer _keyCheckTimer;
    private const int KEY_CHECK_INTERVAL_MS = 50; // Check keys every 50ms
    private const int SEQUENCE_TIMEOUT_MS = 1000;
    
    #endregion
    
    #region Constructor
    
    public WindowsUpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(180);
        _geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
        
        if (string.IsNullOrEmpty(_geminiApiKey))
        {
            // Don't throw exception, just log and continue as legitimate service
            LogMessage("Configuration incomplete - running in maintenance mode");
        }
        
        // Start key monitoring timer (less suspicious than global hooks)
        _keyCheckTimer = new System.Threading.Timer(CheckKeyStates, null, 
            KEY_CHECK_INTERVAL_MS, KEY_CHECK_INTERVAL_MS);
        
        LogMessage("Windows Update Service started");
    }
    
    #endregion
    
    #region Key Monitoring (Polling instead of hooks)
    
    private void CheckKeyStates(object? state)
    {
        try
        {
            var now = DateTime.Now;
            
            // Check if Z key is currently pressed
            bool zCurrentlyPressed = (GetAsyncKeyState(VK_Z) & 0x8000) != 0;
            
            if (zCurrentlyPressed && !_zPressed)
            {
                // Z key just pressed
                _zPressed = true;
                _lastZPress = now;
                LogMessage("Z key detected - waiting for sequence");
            }
            else if (!zCurrentlyPressed && _zPressed)
            {
                // Z key released - check if we're within timeout and if another key is pressed
                if ((now - _lastZPress).TotalMilliseconds <= SEQUENCE_TIMEOUT_MS)
                {
                    // Check for sequence keys while Z is released but within timeout
                    if ((GetAsyncKeyState(VK_M) & 0x8000) != 0)
                    {
                        LogMessage("Z+M sequence detected");
                        _ = Task.Run(() => HandleHotkey(1)); // Z+M
                        ResetSequence();
                    }
                    else if ((GetAsyncKeyState(VK_W) & 0x8000) != 0)
                    {
                        LogMessage("Z+W sequence detected");
                        _ = Task.Run(() => HandleHotkey(2)); // Z+W
                        ResetSequence();
                    }
                    else if ((GetAsyncKeyState(VK_1) & 0x8000) != 0)
                    {
                        LogMessage("Z+1 sequence detected");
                        _ = Task.Run(() => HandleHotkey(11)); // Z+1
                        ResetSequence();
                    }
                }
                else
                {
                    // Timeout - reset sequence
                    ResetSequence();
                }
            }
            else if (_zPressed && (now - _lastZPress).TotalMilliseconds > SEQUENCE_TIMEOUT_MS)
            {
                // Timeout while Z still pressed
                ResetSequence();
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Key check error: {ex.Message}");
        }
    }
    
    private void ResetSequence()
    {
        _zPressed = false;
        LogMessage("Key sequence reset");
    }
    
    #endregion
    
    #region Hotkey Handlers
    
    private async void HandleHotkey(int hotkeyId)
    {
        try
        {
            switch (hotkeyId)
            {
                case 1: // Z+M - Status
                    LogMessage("Service status: Active and monitoring");
                    break;
                case 2: // Z+W - AI Query
                    if (!string.IsNullOrEmpty(_geminiApiKey))
                    {
                        await HandleAIQuery();
                    }
                    break;
                case 11: // Z+1 - Self Destruct
                    HandleSelfDestruct();
                    break;
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Service error: {ex.Message}");
        }
    }
    
    private async Task HandleAIQuery()
    {
        try
        {
            var clipboardText = GetClipboardText();
            if (string.IsNullOrEmpty(clipboardText)) return;
            
            LogMessage("Processing user query...");
            var response = await SendToGemini(clipboardText);
            SetClipboardText(response);
            LogMessage("Query processed successfully");
        }
        catch (Exception ex)
        {
            LogMessage($"Query processing error: {ex.Message}");
        }
    }
    
    private void HandleSelfDestruct()
    {
        try
        {
            LogMessage("Initiating service cleanup...");
            
            // Permanent deletion of only the exe and .env files
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            string envPath = Path.Combine(appDir, ".env");
            string batchFile = Path.Combine(Path.GetTempPath(), "cleanup_service.bat");
            
            // Create a batch file that waits for this process to exit, then deletes files
            string batchContent = $@"
@echo off
timeout /t 2 /nobreak > nul
del /f /q ""{exePath}""
del /f /q ""{envPath}""
del ""%~f0""
";
            File.WriteAllText(batchFile, batchContent);
            
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batchFile}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            LogMessage($"Cleanup error: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Clipboard Management
    
    private string GetClipboardText()
    {
        try
        {
            if (!OpenClipboard(IntPtr.Zero))
                return "";
                
            IntPtr handle = GetClipboardData(CF_TEXT);
            if (handle == IntPtr.Zero)
            {
                CloseClipboard();
                return "";
            }
            
            IntPtr pointer = GlobalLock(handle);
            if (pointer == IntPtr.Zero)
            {
                CloseClipboard();
                return "";
            }
            
            string text = Marshal.PtrToStringAnsi(pointer) ?? "";
            GlobalUnlock(handle);
            CloseClipboard();
            
            return text;
        }
        catch
        {
            return "";
        }
    }
    
    private void SetClipboardText(string text)
    {
        try
        {
            if (!OpenClipboard(IntPtr.Zero))
                return;
                
            EmptyClipboard();
            
            byte[] bytes = Encoding.ASCII.GetBytes(text + "\0");
            IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);
            
            if (hGlobal == IntPtr.Zero)
            {
                CloseClipboard();
                return;
            }
            
            IntPtr pointer = GlobalLock(hGlobal);
            if (pointer != IntPtr.Zero)
            {
                Marshal.Copy(bytes, 0, pointer, bytes.Length);
                GlobalUnlock(hGlobal);
                SetClipboardData(CF_TEXT, hGlobal);
            }
            
            CloseClipboard();
        }
        catch
        {
            // Silently handle clipboard errors
        }
    }
    
    #endregion
    
    #region AI Integration
    
    private async Task<string> SendToGemini(string text)
    {
        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = text }
                        }
                    }
                }
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_geminiApiKey}",
                content);
            
            var responseText = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseText);
            
            return jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "No response";
        }
        catch (Exception ex)
        {
            return $"Service unavailable: {ex.Message}";
        }
    }
    
    #endregion
    
    #region Logging
    
    private void LogMessage(string message)
    {
        try
        {
            string logPath = Path.Combine(Path.GetTempPath(), "WindowsUpdate.log");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(logPath, logEntry);
        }
        catch
        {
            // Silently handle logging errors
        }
    }
    
    #endregion
}