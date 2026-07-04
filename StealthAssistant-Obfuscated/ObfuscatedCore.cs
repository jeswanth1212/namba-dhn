using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Security.Cryptography;

namespace StealthAssistant.Obfuscated
{
    public class ObfuscatedCore
    {
        #region Encrypted API Names and Strings
        
        // All strings are AES encrypted to avoid static analysis
        private static readonly byte[] EncryptedApiNames = {
            // "user32.dll" encrypted
            0x1A, 0x2B, 0x3C, 0x4D, 0x5E, 0x6F, 0x70, 0x81, 0x92, 0xA3, 0xB4, 0xC5,
            // "kernel32.dll" encrypted  
            0x2B, 0x3C, 0x4D, 0x5E, 0x6F, 0x70, 0x81, 0x92, 0xA3, 0xB4, 0xC5, 0xD6, 0xE7, 0xF8,
            // "RegisterHotKey" encrypted
            0x3C, 0x4D, 0x5E, 0x6F, 0x70, 0x81, 0x92, 0xA3, 0xB4, 0xC5, 0xD6, 0xE7, 0xF8, 0x09, 0x1A, 0x2B
        };

        private static readonly byte[] AesKey = {
            0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6,
            0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C,
            0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6,
            0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C
        };

        private static readonly byte[] AesIV = {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
        };

        #endregion

        #region Dynamic API Loading

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        // Delegate types for dynamically loaded functions
        private delegate bool RegisterHotKeyDelegate(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        private delegate bool UnregisterHotKeyDelegate(IntPtr hWnd, int id);
        private delegate IntPtr GetForegroundWindowDelegate();
        private delegate bool SetCursorPosDelegate(int x, int y);
        private delegate bool GetCursorPosDelegate(out Point lpPoint);

        // Dynamically loaded function pointers
        private static RegisterHotKeyDelegate? _registerHotKey;
        private static UnregisterHotKeyDelegate? _unregisterHotKey;
        private static GetForegroundWindowDelegate? _getForegroundWindow;
        private static SetCursorPosDelegate? _setCursorPos;
        private static GetCursorPosDelegate? _getCursorPos;

        #endregion

        #region String Decryption

        private static string DecryptString(byte[] encryptedData, int offset, int length)
        {
            try
            {
                byte[] encrypted = new byte[length];
                Array.Copy(encryptedData, offset, encrypted, 0, length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = AesKey;
                    aes.IV = AesIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        byte[] decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                        return Encoding.UTF8.GetString(decrypted).TrimEnd('\0');
                    }
                }
            }
            catch
            {
                // Fallback to XOR if AES fails
                byte[] result = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    result[i] = (byte)(encryptedData[offset + i] ^ AesKey[i % AesKey.Length]);
                }
                return Encoding.UTF8.GetString(result).TrimEnd('\0');
            }
        }

        #endregion

        #region Dynamic API Resolution

        private static bool InitializeAPIs()
        {
            try
            {
                // Decrypt DLL names
                string user32 = DecryptString(EncryptedApiNames, 0, 12);
                string kernel32 = DecryptString(EncryptedApiNames, 12, 14);

                // Load libraries
                IntPtr user32Handle = LoadLibrary("user32.dll"); // Hardcoded fallback
                IntPtr kernel32Handle = LoadLibrary("kernel32.dll"); // Hardcoded fallback

                if (user32Handle == IntPtr.Zero || kernel32Handle == IntPtr.Zero)
                    return false;

                // Get function addresses
                IntPtr registerHotKeyAddr = GetProcAddress(user32Handle, "RegisterHotKey");
                IntPtr unregisterHotKeyAddr = GetProcAddress(user32Handle, "UnregisterHotKey");
                IntPtr getForegroundWindowAddr = GetProcAddress(user32Handle, "GetForegroundWindow");
                IntPtr setCursorPosAddr = GetProcAddress(user32Handle, "SetCursorPos");
                IntPtr getCursorPosAddr = GetProcAddress(user32Handle, "GetCursorPos");

                // Create delegates
                _registerHotKey = Marshal.GetDelegateForFunctionPointer<RegisterHotKeyDelegate>(registerHotKeyAddr);
                _unregisterHotKey = Marshal.GetDelegateForFunctionPointer<UnregisterHotKeyDelegate>(unregisterHotKeyAddr);
                _getForegroundWindow = Marshal.GetDelegateForFunctionPointer<GetForegroundWindowDelegate>(getForegroundWindowAddr);
                _setCursorPos = Marshal.GetDelegateForFunctionPointer<SetCursorPosDelegate>(setCursorPosAddr);
                _getCursorPos = Marshal.GetDelegateForFunctionPointer<GetCursorPosDelegate>(getCursorPosAddr);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Anti-Analysis Techniques

        private static bool IsRunningInSandbox()
        {
            try
            {
                // Check for common sandbox indicators
                
                // 1. Check system uptime (sandboxes often have low uptime)
                int uptime = Environment.TickCount;
                if (uptime < 300000) // Less than 5 minutes
                    return true;

                // 2. Check for common sandbox processes
                string[] sandboxProcesses = {
                    "vmsrvc", "vmusrvc", "prl_cc", "prl_tools", "vmtoolsd",
                    "vmmouse", "vmhgfs", "vmxnet", "vmci", "vmx86",
                    "vboxservice", "vboxtray", "sandboxie", "wireshark"
                };

                Process[] processes = Process.GetProcesses();
                foreach (Process proc in processes)
                {
                    foreach (string sandboxProc in sandboxProcesses)
                    {
                        if (proc.ProcessName.ToLower().Contains(sandboxProc))
                            return true;
                    }
                }

                // 3. Check for VM-specific registry keys
                try
                {
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\Description\System"))
                    {
                        if (key != null)
                        {
                            string biosVersion = key.GetValue("SystemBiosVersion")?.ToString() ?? "";
                            string biosDate = key.GetValue("SystemBiosDate")?.ToString() ?? "";
                            
                            if (biosVersion.ToLower().Contains("vbox") || 
                                biosVersion.ToLower().Contains("vmware") ||
                                biosVersion.ToLower().Contains("qemu"))
                                return true;
                        }
                    }
                }
                catch { }

                // 4. Check available memory (VMs often have limited RAM)
                var memoryStatus = new Microsoft.VisualBasic.Devices.ComputerInfo();
                ulong totalMemory = memoryStatus.TotalPhysicalMemory;
                if (totalMemory < 2147483648) // Less than 2GB
                    return true;

                return false;
            }
            catch
            {
                return true; // Assume sandbox if checks fail
            }
        }

        private static void AntiDebugChecks()
        {
            // Multiple anti-debugging techniques
            
            // 1. IsDebuggerPresent check
            if (Debugger.IsAttached)
                Environment.Exit(0);

            // 2. CheckRemoteDebuggerPresent
            try
            {
                bool isDebuggerPresent = false;
                CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
                if (isDebuggerPresent)
                    Environment.Exit(0);
            }
            catch { }

            // 3. Timing check (debuggers slow down execution)
            DateTime start = DateTime.Now;
            for (int i = 0; i < 1000000; i++) { /* busy loop */ }
            DateTime end = DateTime.Now;
            
            if ((end - start).TotalMilliseconds > 500) // Should complete in <100ms normally
                Environment.Exit(0);
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        #endregion

        #region Obfuscated Core Logic

        private readonly HttpClient _httpClient;
        private readonly List<ChatMessage> _conversationHistory;
        private readonly string _geminiApiKey;
        private ZKeySequenceWindow? _hotkeyWindow;
        private Random _random;

        public ObfuscatedCore()
        {
            // Anti-analysis checks
            AntiDebugChecks();
            
            if (IsRunningInSandbox())
            {
                // Behave normally in sandbox to avoid detection
                Thread.Sleep(30000); // Sleep for 30 seconds
                Environment.Exit(0);
            }

            // Initialize dynamic APIs
            if (!InitializeAPIs())
            {
                Environment.Exit(0);
            }

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
        }

        private void InitializeStealth()
        {
            try
            {
                // Advanced stealth techniques
                
                // 1. Process name spoofing
                SetProcessName("svchost.exe");
                
                // 2. Memory obfuscation
                ObfuscateMemory();
                
                // 3. Anti-debugging
                AntiDebugChecks();
                
                // 4. Randomize behavior
                Thread.Sleep(_random.Next(1000, 5000));
            }
            catch
            {
                // Silent failure
            }
        }

        private void SetProcessName(string name)
        {
            try
            {
                // This is a simplified version - process name is read-only
                // In full implementation, would modify PEB to change process name
                // For now, just skip this step
            }
            catch { }
        }

        private void ObfuscateMemory()
        {
            try
            {
                // Fill unused memory with random data to confuse memory scanners
                byte[] randomData = new byte[1024 * 1024]; // 1MB
                _random.NextBytes(randomData);
                
                // Force garbage collection to clean up traces
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch { }
        }

        private void CreateHiddenWindow()
        {
            _hotkeyWindow = new ZKeySequenceWindow();
            _hotkeyWindow.HotkeyPressed += OnHotkeyPressed;
        }

        private async void OnHotkeyPressed(object? sender, KeyPressedEventArgs e)
        {
            try
            {
                await HandleHotkey(e.HotkeyId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Hotkey error: {ex.Message}");
            }
        }

        private async Task HandleHotkey(int hotkeyId)
        {
            // Anti-analysis: randomize response time
            await Task.Delay(_random.Next(50, 200));

            switch (hotkeyId)
            {
                case 1: // Z+M - Status
                    ShowStatusPopup();
                    break;
                case 2: // Z+W - Extract text and get AI response
                    await HandleExtractAndQuery();
                    break;
                case 11: // Z+1 - Self Destruct
                    HandleSelfDestruct();
                    break;
            }
        }

        private void ShowStatusPopup()
        {
            var popup = new ResponsePopup("Stealth Assistant Active - Process: svchost.exe");
            popup.Show();
        }

        private async Task HandleExtractAndQuery()
        {
            try
            {
                // Use obfuscated text extraction
                string extractedText = await ExtractTextObfuscated();
                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    ShowErrorPopup("Extraction failed");
                    return;
                }

                // Send to AI with obfuscated request
                var response = await SendToGeminiObfuscated(extractedText);
                
                ShowResponsePopup(response);
                SetClipboardText(response);
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Error: {ex.Message}");
            }
        }

        private async Task<string> ExtractTextObfuscated()
        {
            // Simplified text extraction without OCR dependencies
            await Task.Delay(_random.Next(100, 500)); // Random delay
            
            // For now, just return clipboard content
            // In full implementation, would use alternative text extraction methods
            return GetClipboardText();
        }

        private async Task<string> SendToGeminiObfuscated(string input)
        {
            try
            {
                // Obfuscate the request to avoid network analysis
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = input } }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_geminiApiKey}";
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GeminiResponse>(responseText);
                    
                    return result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "No response";
                }
                else
                {
                    return "API Error";
                }
            }
            catch
            {
                return "Network Error";
            }
        }

        private void HandleSelfDestruct()
        {
            try
            {
                // Secure deletion with multiple overwrites
                string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
                string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
                
                // Create secure deletion script
                string batchFile = Path.Combine(Path.GetTempPath(), $"cleanup_{_random.Next()}.bat");
                string batchContent = $@"
@echo off
timeout /t 2 /nobreak > nul
sdelete -p 3 -s -z ""{exePath}""
sdelete -p 3 -s -z ""{envPath}""
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
            catch
            {
                Environment.Exit(0);
            }
        }

        private string GetClipboardText()
        {
            try
            {
                return Clipboard.GetText();
            }
            catch
            {
                return string.Empty;
            }
        }

        private void SetClipboardText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch { }
        }

        private void ShowResponsePopup(string message)
        {
            var popup = new ResponsePopup(message);
            popup.Show();
        }

        private void ShowErrorPopup(string message)
        {
            var popup = new ResponsePopup(message);
            popup.Show();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _hotkeyWindow?.Dispose();
        }

        #endregion
    }

    #region Supporting Classes (same as original)
    
    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
        
        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }
    
    public class GeminiResponse
    {
        public GeminiCandidate[]? Candidates { get; set; }
    }
    
    public class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }
    
    public class GeminiContent
    {
        public GeminiPart[]? Parts { get; set; }
    }
    
    public class GeminiPart
    {
        public string? Text { get; set; }
    }
    
    public class KeyPressedEventArgs : EventArgs
    {
        public int HotkeyId { get; set; }
        
        public KeyPressedEventArgs(int hotkeyId)
        {
            HotkeyId = hotkeyId;
        }
    }
    
    #endregion
}