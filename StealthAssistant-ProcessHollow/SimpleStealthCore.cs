using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using DotNetEnv;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace StealthAssistant.ProcessHollow
{
    public class SimpleStealthCore
    {
        private readonly HttpClient _httpClient;
        private readonly string _openRouterApiKey;
        private readonly string _geminiApiKey;
        private ZKeySequenceWindow? _hotkeyWindow;
        private StealthClipboardViewer? _clipboardViewer;
        
        // Conversation history
        private readonly System.Collections.Generic.List<ChatMessage> _conversationHistory;
        
        // Theme
        private bool _isDarkTheme = true; // Start with dark theme
        
        // Model selection
        private bool _useOpenRouter = true; // Start with OpenRouter
        
        // Auto-typing fields
        private System.Threading.Timer? _autoTypeTimer;
        private string _autoTypeText = "";
        private int _autoTypePosition = 0;
        private readonly object _autoTypeLock = new object();
        
        // SendInput API declarations for hardware-level keyboard simulation
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION u;
        }
        
        [StructLayout(LayoutKind.Explicit, Size = 28)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        
        // Screenshot capture API imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int width, int height);
        
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hGDIObj);
        
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, uint dwRop);
        
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hDC);
        
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        public SimpleStealthCore()
        {
            // Load .env file from the same directory as the executable
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string envPath = Path.Combine(appDir, ".env");
                if (File.Exists(envPath))
                {
                    Env.Load(envPath);
                }
            }
            catch { }
            
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            _openRouterApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "";
            _geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
            _conversationHistory = new System.Collections.Generic.List<ChatMessage>();
        }

        public void Start()
        {
            try
            {
                _hotkeyWindow = new ZKeySequenceWindow();
                _hotkeyWindow.HotkeyPressed += OnHotkeyPressed;
                
                // Create clipboard viewer (initially hidden)
                _clipboardViewer = new StealthClipboardViewer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnHotkeyPressed(object? sender, KeyPressedEventArgs e)
        {
            try
            {
                await HandleHotkey(e.HotkeyId);
            }
            catch (Exception ex)
            {
                ShowError($"Hotkey error: {ex.Message}");
            }
        }

        private async Task HandleHotkey(int hotkeyId)
        {
            switch (hotkeyId)
            {
                case 1: // Z+M - Status
                    ShowStatus();
                    break;
                case 2: // Z+W - AI Query
                    await HandleAIQuery();
                    break;
                case 3: // Z+J - Java Code
                    await HandleGenerateCode("java");
                    break;
                case 4: // Z+P - Python Code
                    await HandleGenerateCode("python");
                    break;
                case 5: // Z+C - C++ Code
                    await HandleGenerateCode("cpp");
                    break;
                case 6: // Z+E - Clipboard Viewer
                    HandleToggleClipboardViewer();
                    break;
                case 7: // Z+V - Auto-type
                    ShowNotImplemented("Auto-type");
                    break;
                case 8: // ` - Pause/Resume
                    ShowNotImplemented("Pause/Resume");
                    break;
                case 10: // Z+B - Compiler Auto-Type
                    await HandleCompilerAutoType();
                    break;
                case 11: // Z+1 - Self Destruct
                    HandleSelfDestruct();
                    break;
                case 12: // Z+R - Reset History
                    HandleResetHistory();
                    break;
                case 13: // Z+T - Toggle Theme
                    HandleToggleTheme();
                    break;
                case 14: // Z+L - Toggle Model
                    HandleToggleModel();
                    break;
            }
        }

        private void ShowStatus()
        {
            var modelName = _useOpenRouter ? "OpenRouter" : "Gemini";
            var popup = new ResponsePopup($"Active ({modelName})\nZ+M:Status Z+W:AI Z+L:Model", _isDarkTheme);
            popup.Show();
        }

        private async Task HandleAIQuery()
        {
            try
            {
                // Capture screenshot
                var screenshotBase64 = CaptureScreenshotAsBase64();
                if (string.IsNullOrEmpty(screenshotBase64))
                {
                    ShowError("Screenshot capture failed");
                    return;
                }
                
                // Extract text using Windows OCR
                var extractedText = await ExtractTextFromImageAsync(screenshotBase64);
                
                if (string.IsNullOrEmpty(extractedText))
                {
                    ShowError("OCR failed - no text found");
                    return;
                }

                if (string.IsNullOrEmpty(_openRouterApiKey))
                {
                    ShowError("OPENROUTER_API_KEY not set");
                    return;
                }

                // Smart prompt processing with default summarization
                string processedPrompt = ProcessSmartPromptWithSummarization(extractedText);
                
                // Add user message to history
                _conversationHistory.Add(new ChatMessage { Role = "user", Content = processedPrompt });
                
                // Send to AI with history
                var response = _useOpenRouter 
                    ? await SendToOpenRouterWithHistory() 
                    : await SendToGeminiWithHistory();
                
                // Add assistant response to history
                _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = response });
                
                // Copy to clipboard
                SetClipboardText(response);
                
                // Show first 100 characters in fixed-size popup
                var previewText = response.Length > 100 ? response.Substring(0, 100) + "..." : response;
                
                var popup = new ResponsePopup(previewText, _isDarkTheme);
                popup.Show();
            }
            catch (Exception ex)
            {
                ShowError($"AI query failed: {ex.Message}");
            }
        }
        
        private string ProcessSmartPromptWithSummarization(string extractedText)
        {
            // Check for compiler error patterns
            if (IsCompilerError(extractedText))
            {
                // Get last code from history
                var lastCode = GetLastCodeFromHistory();
                if (!string.IsNullOrEmpty(lastCode))
                {
                    return $"Fix this compiler error in the code:\n\nPrevious Code:\n{lastCode}\n\nCompiler Error:\n{extractedText}\n\nProvide only the corrected code.";
                }
            }
            
            // Check for prompt() pattern
            var promptMatch = System.Text.RegularExpressions.Regex.Match(extractedText, @"prompt\s*\((.*?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (promptMatch.Success)
            {
                var promptText = promptMatch.Groups[1].Value.Trim();
                return $"{promptText}\n\nContext:\n{extractedText}\n\nProvide a concise summary in bullet points with only the most important information.";
            }
            
            // Default: Add summarization instruction
            return $"{extractedText}\n\nProvide a concise summary in bullet points with only the most important information.";
        }
        
        private bool IsCompilerError(string text)
        {
            var errorKeywords = new[] { "error:", "exception", "failed", "cannot find symbol", "syntax error", "undefined reference", "compilation error" };
            var lowerText = text.ToLower();
            return errorKeywords.Any(keyword => lowerText.Contains(keyword));
        }
        
        private string GetLastCodeFromHistory()
        {
            // Look for last assistant message that contains code
            for (int i = _conversationHistory.Count - 1; i >= 0; i--)
            {
                if (_conversationHistory[i].Role == "assistant")
                {
                    var content = _conversationHistory[i].Content;
                    // Check if it looks like code (has common code patterns)
                    if (content.Contains("{") || content.Contains("class ") || content.Contains("def ") || content.Contains("function"))
                    {
                        return content;
                    }
                }
            }
            return string.Empty;
        }

        private async Task<string> SendToOpenRouter(string input)
        {
            try
            {
                var requestBody = new
                {
                    model = "openrouter/owl-alpha",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = input
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Add OpenRouter API key to headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openRouterApiKey}");
                
                var url = "https://openrouter.ai/api/v1/chat/completions";
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseText);
                    
                    return result?.Choices?[0]?.Message?.Content ?? "No response";
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return $"API Error: {response.StatusCode} - {errorBody.Substring(0, Math.Min(200, errorBody.Length))}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        
        private async Task<string> SendToOpenRouterWithHistory()
        {
            try
            {
                // Build messages array from conversation history
                var messages = _conversationHistory.Select(msg => new
                {
                    role = msg.Role,
                    content = msg.Content
                }).ToArray();

                var requestBody = new
                {
                    model = "openrouter/owl-alpha",
                    messages = messages
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Add OpenRouter API key to headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openRouterApiKey}");
                
                var url = "https://openrouter.ai/api/v1/chat/completions";
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseText);
                    
                    return result?.Choices?[0]?.Message?.Content ?? "No response";
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return $"API Error: {response.StatusCode} - {errorBody.Substring(0, Math.Min(200, errorBody.Length))}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        
        private async Task<string> SendToGemini(string input)
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
                                new { text = input }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite-preview:generateContent?key={_geminiApiKey}";
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GeminiResponse>(responseText);
                    
                    return result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "No response";
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return $"API Error: {response.StatusCode} - {errorBody.Substring(0, Math.Min(200, errorBody.Length))}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        
        private async Task<string> SendToGeminiWithHistory()
        {
            try
            {
                // Build contents array from conversation history
                var contents = _conversationHistory.Select(msg => new
                {
                    role = msg.Role == "assistant" ? "model" : "user",
                    parts = new[]
                    {
                        new { text = msg.Content }
                    }
                }).ToArray();

                var requestBody = new
                {
                    contents = contents
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite-preview:generateContent?key={_geminiApiKey}";
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GeminiResponse>(responseText);
                    
                    return result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "No response";
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return $"API Error: {response.StatusCode} - {errorBody.Substring(0, Math.Min(200, errorBody.Length))}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task HandleGenerateCode(string language)
        {
            try
            {
                // Capture screenshot
                var screenshotBase64 = CaptureScreenshotAsBase64();
                if (string.IsNullOrEmpty(screenshotBase64))
                {
                    ShowError("Screenshot capture failed");
                    return;
                }
                
                // Extract text using Windows OCR
                var extractedText = await ExtractTextFromImageAsync(screenshotBase64);
                
                if (string.IsNullOrEmpty(extractedText))
                {
                    ShowError("OCR failed - no text found");
                    return;
                }

                if (string.IsNullOrEmpty(_openRouterApiKey))
                {
                    ShowError("OPENROUTER_API_KEY not set");
                    return;
                }

                string prompt = language.ToLower() switch
                {
                    "java" => $"Generate clean Java code for: {extractedText}\n\nProvide only the code, no explanations.",
                    "python" => $"Generate clean Python code for: {extractedText}\n\nProvide only the code, no explanations.",
                    "cpp" => $"Generate clean C++ code for: {extractedText}\n\nProvide only the code, no explanations.",
                    _ => extractedText
                };

                var response = _useOpenRouter 
                    ? await SendToOpenRouter(prompt) 
                    : await SendToGemini(prompt);
                    
                SetClipboardText(response);
                
                var popup = new ResponsePopup($"{language.ToUpper()} code copied", _isDarkTheme);
                popup.Show();
            }
            catch (Exception ex)
            {
                ShowError($"Code generation failed: {ex.Message}");
            }
        }

        private void HandleToggleClipboardViewer()
        {
            try
            {
                var clipboardText = GetClipboardText();
                _clipboardViewer?.ToggleVisibility(clipboardText);
            }
            catch (Exception ex)
            {
                ShowError($"Clipboard viewer error: {ex.Message}");
            }
        }

        private async Task HandleCompilerAutoType()
        {
            try
            {
                string clipboardText = GetClipboardText();
                if (string.IsNullOrEmpty(clipboardText))
                {
                    ShowError("Clipboard is empty");
                    return;
                }

                // Strip indentation from each line
                var lines = clipboardText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var processedLines = new System.Collections.Generic.List<string>();
                
                foreach (var line in lines)
                {
                    string trimmedLine = line.TrimStart(' ', '\t');
                    processedLines.Add(string.IsNullOrWhiteSpace(trimmedLine) ? "" : trimmedLine);
                }
                
                var processedText = string.Join("\n", processedLines);
                
                lock (_autoTypeLock)
                {
                    // Stop any existing typing
                    _autoTypeTimer?.Dispose();
                    
                    var popup = new ResponsePopup($"Auto-typing {processedText.Length} chars", _isDarkTheme);
                    popup.Show();
                    
                    // Reset and start from beginning
                    _autoTypeText = processedText;
                    _autoTypePosition = 0;
                    
                    // Start typing at 10,000 chars/sec (10 chars per 1ms)
                    _autoTypeTimer = new System.Threading.Timer(AutoTypeCallback, null, 100, 1);
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ShowError($"Auto-type failed: {ex.Message}");
            }
        }
        
        private void AutoTypeCallback(object? state)
        {
            lock (_autoTypeLock)
            {
                if (_autoTypePosition >= _autoTypeText.Length)
                {
                    // Finished typing
                    _autoTypeTimer?.Dispose();
                    _autoTypeTimer = null;
                    return;
                }
                
                // Type 10 characters per callback (10,000 chars/sec)
                int charsToType = Math.Min(10, _autoTypeText.Length - _autoTypePosition);
                
                for (int i = 0; i < charsToType; i++)
                {
                    char c = _autoTypeText[_autoTypePosition + i];
                    SendCharacterUsingSendInput(c);
                }
                
                _autoTypePosition += charsToType;
            }
        }
        
        private void SendCharacterUsingSendInput(char c)
        {
            // Handle special characters
            if (c == '\r')
            {
                return; // Skip carriage return
            }
            else if (c == '\n')
            {
                SendKeyPress(0x0D); // VK_RETURN
                return;
            }
            else if (c == '\t')
            {
                SendKeyPress(0x09); // VK_TAB
                return;
            }
            
            // Create input for key down and key up
            INPUT[] inputs = new INPUT[2];
            
            // Key down
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            
            // Key up
            inputs[1] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
        
        private void SendKeyPress(ushort vkCode)
        {
            INPUT[] inputs = new INPUT[2];
            
            // Key down
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vkCode,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            
            // Key up
            inputs[1] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vkCode,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void HandleToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            var themeName = _isDarkTheme ? "Dark" : "Light";
            var popup = new ResponsePopup($"Theme: {themeName}", _isDarkTheme);
            popup.Show();
            
            // Update clipboard viewer theme if it exists
            _clipboardViewer?.UpdateTheme(_isDarkTheme);
        }
        
        private void HandleToggleModel()
        {
            _useOpenRouter = !_useOpenRouter;
            var modelName = _useOpenRouter ? "OpenRouter" : "Gemini 3.1";
            var popup = new ResponsePopup($"Model: {modelName}", _isDarkTheme);
            popup.Show();
        }

        private void HandleResetHistory()
        {
            try
            {
                _conversationHistory.Clear();
                var popup = new ResponsePopup("History cleared!", _isDarkTheme);
                popup.Show();
            }
            catch (Exception ex)
            {
                ShowError($"Reset failed: {ex.Message}");
            }
        }

        private void ShowNotImplemented(string feature)
        {
            var popup = new ResponsePopup($"{feature} - Not implemented yet", _isDarkTheme);
            popup.Show();
        }

        private void HandleSelfDestruct()
        {
            try
            {
                var popup = new ResponsePopup("Self-destruct...", _isDarkTheme);
                popup.Show();
                
                // Get current exe path and directory
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                string exeDir = Path.GetDirectoryName(exePath) ?? "";
                string envPath = Path.Combine(exeDir, ".env");
                
                // Create a batch file to delete the exe and .env after the process exits
                string batchPath = Path.Combine(Path.GetTempPath(), $"cleanup_{Guid.NewGuid()}.bat");
                
                var batchContent = new StringBuilder();
                batchContent.AppendLine("@echo off");
                batchContent.AppendLine("timeout /t 2 /nobreak >nul");
                
                // Delete .env file permanently (no recycle bin)
                if (File.Exists(envPath))
                {
                    batchContent.AppendLine($"del /f /q \"{envPath}\"");
                }
                
                // Delete exe file permanently (no recycle bin)
                batchContent.AppendLine($"del /f /q \"{exePath}\"");
                
                // Delete the batch file itself
                batchContent.AppendLine($"del /f /q \"{batchPath}\"");
                
                File.WriteAllText(batchPath, batchContent.ToString());
                
                // Start the batch file in hidden mode
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = batchPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                
                System.Diagnostics.Process.Start(startInfo);
                
                // Exit immediately
                Environment.Exit(0);
            }
            catch
            {
                // If self-destruct fails, just exit
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

        private void ShowError(string message)
        {
            // Truncate error messages to fit fixed popup
            var shortMessage = message.Length > 80 ? message.Substring(0, 80) + "..." : message;
            var popup = new ResponsePopup($"Error: {shortMessage}", _isDarkTheme);
            popup.Show();
        }

        private string CaptureScreenshotAsBase64()
        {
            try
            {
                // Get screen dimensions
                int screenWidth = GetSystemMetrics(0); // SM_CXSCREEN
                int screenHeight = GetSystemMetrics(1); // SM_CYSCREEN
                
                // Get desktop device context
                IntPtr desktopDC = GetDC(IntPtr.Zero);
                
                // Create compatible DC and bitmap
                IntPtr memoryDC = CreateCompatibleDC(desktopDC);
                IntPtr bitmap = CreateCompatibleBitmap(desktopDC, screenWidth, screenHeight);
                
                // Select bitmap into memory DC
                IntPtr oldBitmap = SelectObject(memoryDC, bitmap);
                
                // Copy desktop to memory DC
                bool success = BitBlt(memoryDC, 0, 0, screenWidth, screenHeight, desktopDC, 0, 0, 0x00CC0020); // SRCCOPY
                
                if (!success)
                {
                    // Clean up and return empty
                    SelectObject(memoryDC, oldBitmap);
                    DeleteObject(bitmap);
                    DeleteDC(memoryDC);
                    ReleaseDC(IntPtr.Zero, desktopDC);
                    return string.Empty;
                }
                
                // Convert to managed Bitmap
                var managedBitmap = Bitmap.FromHbitmap(bitmap);
                
                // Convert to Base64
                string base64String;
                using (var memoryStream = new MemoryStream())
                {
                    managedBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imageBytes = memoryStream.ToArray();
                    base64String = Convert.ToBase64String(imageBytes);
                }
                
                // Clean up
                managedBitmap.Dispose();
                SelectObject(memoryDC, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDC);
                ReleaseDC(IntPtr.Zero, desktopDC);
                
                return base64String;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        private async Task<string> ExtractTextFromImageAsync(string base64Image)
        {
            try
            {
                // Convert base64 to byte array
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                
                // Create InMemoryRandomAccessStream
                using var stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(imageBytes.AsBuffer());
                stream.Seek(0);
                
                // Create BitmapDecoder
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                
                // Get OCR engine for English
                var ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
                if (ocrEngine == null)
                {
                    return string.Empty;
                }
                
                // Perform OCR
                var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
                
                // Extract text
                var extractedText = new StringBuilder();
                foreach (var line in ocrResult.Lines)
                {
                    extractedText.AppendLine(line.Text);
                }
                
                return extractedText.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        public void Dispose()
        {
            _autoTypeTimer?.Dispose();
            _httpClient?.Dispose();
            _hotkeyWindow?.Dispose();
            _clipboardViewer?.Dispose();
        }
    }

    // OpenRouter API response classes
    public class OpenRouterResponse
    {
        public OpenRouterChoice[]? Choices { get; set; }
    }

    public class OpenRouterChoice
    {
        public OpenRouterMessage? Message { get; set; }
    }

    public class OpenRouterMessage
    {
        public string? Content { get; set; }
    }
    
    // Gemini API response classes
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
    
    // Chat message for conversation history
    public class ChatMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }
}