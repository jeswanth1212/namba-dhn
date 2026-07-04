using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

public class StealthAssistantCore
{
    #region Windows API Imports
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);
    
    [DllImport("kernel32.dll")]
    private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
    
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();
    
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
    
    #endregion
    
    #region Fields
    
    private readonly HttpClient _httpClient;
    private readonly List<ChatMessage> _conversationHistory;
    private readonly string _geminiApiKey;
    private ZKeySequenceWindow? _hotkeyWindow;
    private Random _random;
    
    #endregion
    
    #region Constructor
    
    public StealthAssistantCore()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(180);
        _conversationHistory = new List<ChatMessage>();
        _geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
        _random = new Random();
        
        if (string.IsNullOrEmpty(_geminiApiKey))
        {
            throw new InvalidOperationException("GEMINI_API_KEY environment variable not set");
        }
        
        InitializeStealth();
        CreateHiddenWindow();
        OptimizeMemoryFootprint();
    }
    
    #endregion
    
    #region Stealth Implementation
    
    private void InitializeStealth()
    {
        try
        {
            // Minimize memory footprint
            SetProcessWorkingSetSize(GetCurrentProcess(), -1, -1);
            
            // Randomize process behavior to avoid detection patterns
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
            
            // Anti-debugging measures
            if (Debugger.IsAttached)
            {
                Environment.Exit(0);
            }
            
            // Memory obfuscation - randomize unused memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch
        {
            // Silently handle any stealth initialization errors
        }
    }
    
    private void OptimizeMemoryFootprint()
    {
        // Periodic memory optimization to avoid detection
        var timer = new System.Threading.Timer((_) =>
        {
            try
            {
                GC.Collect(2, GCCollectionMode.Optimized);
                SetProcessWorkingSetSize(GetCurrentProcess(), -1, -1);
            }
            catch { }
        }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    
    #endregion
    
    #region Hidden Window Management
    
    private void CreateHiddenWindow()
    {
        _hotkeyWindow = new ZKeySequenceWindow();
        _hotkeyWindow.HotkeyPressed += OnHotkeyPressed;
    }
    
    #endregion
    
    #region Hotkey Management
    
    private async void OnHotkeyPressed(object? sender, KeyPressedEventArgs e)
    {
        try
        {
            await HandleHotkey(e.HotkeyId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hotkey error: {ex.Message}");
        }
    }
    
    private async Task HandleHotkey(int hotkeyId)
    {
        switch (hotkeyId)
        {
            case 1: // Z+M - Status
                Console.WriteLine("Stealth Assistant Active");
                break;
            case 2: // Z+W - Extract text and get AI response
                await HandleExtractAndQuery();
                break;
            case 11: // Z+1 - Self Destruct
                HandleSelfDestruct();
                break;
        }
    }
    
    #endregion
    
    #region Core Handlers
    
    private async Task HandleExtractAndQuery()
    {
        try
        {
            var clipboardText = GetClipboardText();
            if (string.IsNullOrEmpty(clipboardText)) return;
            
            var response = await SendToGemini(clipboardText);
            Console.WriteLine($"AI Response: {response}");
            
            SetClipboardText(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    private void HandleSelfDestruct()
    {
        try
        {
            // Permanent deletion of only the exe and .env files
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            string envPath = Path.Combine(appDir, ".env");
            string batchFile = Path.Combine(Path.GetTempPath(), "kill_and_wipe.bat");
            
            // Create a batch file that waits for this process to exit, then deletes only exe and .env
            string batchContent = $@"
@echo off
timeout /t 1 /nobreak > nul
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
            Console.WriteLine($"Self-destruct failed: {ex.Message}");
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
            return $"Error: {ex.Message}";
        }
    }
    
    #endregion
}

public class ChatMessage
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
}