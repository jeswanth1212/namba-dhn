using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace StealthAssistant
{
    public class StealthAssistantCore
    {
        #region Windows API Imports
        
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);
        
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();
        
        #endregion
        

        
        #region Fields
        
        private readonly HttpClient _httpClient;
        private readonly List<ChatMessage> _conversationHistory;
        private readonly string _geminiApiKey;
        private ZKeySequenceWindow? _hotkeyWindow;
        private Random _random;
        private readonly List<string> _clipboardHistory;
        
        #endregion
        
        #region Constructor
        
        public StealthAssistantCore()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // 60 second timeout for harder questions
            _conversationHistory = new List<ChatMessage>();
            _geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
            _random = new Random();
            _clipboardHistory = new List<string>();
            
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
        
        // Hotkeys are now registered automatically in SimpleHotkeyWindow
        
        private async void OnHotkeyPressed(object? sender, KeyPressedEventArgs e)
        {
            try
            {
                await HandleHotkey(e.HotkeyId);
            }
            catch (Exception ex)
            {
                // Log error silently or handle appropriately
                Debug.WriteLine($"Hotkey error: {ex.Message}");
            }
        }
        
        private async Task HandleHotkey(int hotkeyId)
        {
            switch (hotkeyId)
            {
                case 1: // Ctrl+Shift+S
                    await HandleNormalQuery();
                    break;
                case 2: // Ctrl+Shift+Q
                    await HandleMCQQuery();
                    break;
                case 3: // Ctrl+Shift+J
                    await HandleJavaQuery();
                    break;
                case 4: // Ctrl+Shift+A
                    await HandleAppendToSelection();
                    break;
                case 5: // Ctrl+Shift+M
                    ShowStatusPopup();
                    break;
            }
        }
        
        #endregion
        
        #region Clipboard Management
        
        private void AddToClipboardHistory(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _clipboardHistory.Add(text);
                if (_clipboardHistory.Count > 10)
                {
                    _clipboardHistory.RemoveAt(0);
                }
            }
        }
        
        #endregion
        
        #region Query Handlers
        
        private async Task HandleNormalQuery()
        {
            var clipboardText = GetClipboardText();
            if (string.IsNullOrEmpty(clipboardText)) return;
            
            // Add input to clipboard history
            AddToClipboardHistory(clipboardText);
            
            var response = await SendToGemini(clipboardText, "normal");
            ShowResponsePopup(response);
            
            // Copy full response to clipboard
            SetClipboardText(response);
            AddToClipboardHistory(response);
        }
        
        private async Task HandleMCQQuery()
        {
            var clipboardText = GetClipboardText();
            if (string.IsNullOrEmpty(clipboardText)) return;
            
            // Add input to clipboard history
            AddToClipboardHistory(clipboardText);
            
            var response = await SendToGemini(clipboardText, "mcq");
            
            // Copy full response to clipboard (no popup)
            SetClipboardText(response);
            AddToClipboardHistory(response);
            
            // Move cursor based on detected answer
            MoveCursorForMCQAnswer(response);
        }
        
        private async Task HandleJavaQuery()
        {
            var clipboardText = GetClipboardText();
            if (string.IsNullOrEmpty(clipboardText)) return;
            
            // Add input to clipboard history
            AddToClipboardHistory(clipboardText);
            
            var response = await SendToGemini(clipboardText, "java");
            
            // Copy entire response to clipboard (should be clean Java code)
            SetClipboardText(response);
            AddToClipboardHistory(response);
            FlickerMouse();
        }
        
        private Task HandleAppendToSelection()
        {
            // Combine last two clipboard entries
            if (_clipboardHistory.Count >= 2)
            {
                var secondRecent = _clipboardHistory[_clipboardHistory.Count - 2];
                var firstRecent = _clipboardHistory[_clipboardHistory.Count - 1];
                
                // Combine: second recent first, then first recent below
                var combinedText = $"{secondRecent}\n{firstRecent}";
                
                // Set as new clipboard content and add to history (no pasting)
                SetClipboardText(combinedText);
                AddToClipboardHistory(combinedText);
            }
            
            return Task.CompletedTask;
        }
        
        #endregion
        
        #region Gemini API Integration
        
        private async Task<string> SendToGemini(string input, string queryType)
        {
            try
            {
                var systemPrompt = GetSystemPrompt(queryType);
                
                _conversationHistory.Add(new ChatMessage("user", input));
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = systemPrompt + "\n\n" + input }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.1
                    }
                };
                
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_geminiApiKey}";
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GeminiResponse>(responseText);
                    
                    var aiResponse = result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "No response from AI";
                    
                    // Check if response is empty or just whitespace
                    if (string.IsNullOrWhiteSpace(aiResponse) || aiResponse == "No response from AI")
                    {
                        return $"Empty response - try simpler question or check API quota";
                    }
                    
                    _conversationHistory.Add(new ChatMessage("assistant", aiResponse));
                    return aiResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"API Error {response.StatusCode}: {errorContent}";
                }
            }
            catch (TaskCanceledException)
            {
                return "Request timeout - question too complex, try breaking it down";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        
        private string GetSystemPrompt(string queryType)
        {
            return queryType switch
            {
                "mcq" => "You are helping with multiple choice questions. Provide clear, concise answers. If this is a multiple choice question, give the correct answer with brief explanation.",
                "java" => "Give only the Java code solution for this problem. Do not include any comments, explanations, or extra text. Only provide the code",
                "normal" => "You are a helpful assistant. Provide clear, concise responses to questions.",
                _ => "You are a helpful assistant."
            };
        }
        
        #endregion
        
        #region Clipboard Management
        
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
            catch
            {
                // Handle silently
            }
        }
        
        #endregion
        
        #region Code Processing
        
        private string ExtractJavaCodeWithoutComments(string response)
        {
            var lines = response.Split('\n');
            var codeLines = new List<string>();
            bool inCodeBlock = false;
            bool foundJavaCode = false;
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Handle code block markers
                if (trimmedLine.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }
                
                // More comprehensive Java code detection
                bool isJavaLine = inCodeBlock || 
                    trimmedLine.StartsWith("public") ||
                    trimmedLine.StartsWith("private") ||
                    trimmedLine.StartsWith("protected") ||
                    trimmedLine.StartsWith("class") ||
                    trimmedLine.StartsWith("interface") ||
                    trimmedLine.StartsWith("import") ||
                    trimmedLine.StartsWith("package") ||
                    trimmedLine.StartsWith("static") ||
                    trimmedLine.StartsWith("final") ||
                    trimmedLine.StartsWith("abstract") ||
                    trimmedLine.StartsWith("@") || // Annotations
                    trimmedLine.Contains("void") ||
                    trimmedLine.Contains("return") ||
                    trimmedLine.Contains("int ") ||
                    trimmedLine.Contains("String") ||
                    trimmedLine.Contains("boolean") ||
                    trimmedLine.Contains("double") ||
                    trimmedLine.Contains("float") ||
                    trimmedLine.Contains("char") ||
                    trimmedLine.Contains("long") ||
                    trimmedLine.Contains("byte") ||
                    trimmedLine.Contains("short") ||
                    trimmedLine.Contains("System.out") ||
                    trimmedLine.Contains("new ") ||
                    trimmedLine.Contains("if (") ||
                    trimmedLine.Contains("for (") ||
                    trimmedLine.Contains("while (") ||
                    trimmedLine.Contains("try {") ||
                    trimmedLine.Contains("catch (") ||
                    trimmedLine.Contains("} else") ||
                    (trimmedLine.Contains("{") && (foundJavaCode || trimmedLine.Contains("("))) ||
                    (trimmedLine.Contains("}") && foundJavaCode) ||
                    (trimmedLine.Contains(";") && foundJavaCode) ||
                    (foundJavaCode && !string.IsNullOrWhiteSpace(trimmedLine));
                
                if (isJavaLine)
                {
                    foundJavaCode = true;
                    var cleanLine = RemoveComments(line);
                    if (!string.IsNullOrWhiteSpace(cleanLine))
                    {
                        codeLines.Add(cleanLine);
                    }
                }
            }
            
            // If no Java code found, return the original response
            return codeLines.Count > 0 ? string.Join("\n", codeLines) : response;
        }
        
        private string RemoveComments(string line)
        {
            // Remove single line comments
            int commentIndex = line.IndexOf("//");
            if (commentIndex >= 0)
            {
                line = line.Substring(0, commentIndex);
            }
            
            // Remove multi-line comments (basic implementation)
            line = System.Text.RegularExpressions.Regex.Replace(line, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);
            
            return line.TrimEnd();
        }
        
        #endregion
        
        #region UI and Notifications
        
        private void ShowResponsePopup(string message)
        {
            var popup = new ResponsePopup(message);
            popup.Show();
        }
        
        private void ShowStatusPopup()
        {
            var popup = new ResponsePopup("Running");
            popup.Show();
        }
        
        private void FlickerMouse()
        {
            GetCursorPos(out Point currentPos);
            
            // Small flicker movement
            SetCursorPos(currentPos.X + 2, currentPos.Y + 2);
            Thread.Sleep(50);
            SetCursorPos(currentPos.X - 2, currentPos.Y - 2);
            Thread.Sleep(50);
            SetCursorPos(currentPos.X, currentPos.Y);
        }
        
        private void MoveCursorForMCQAnswer(string response)
        {
            try
            {
                GetCursorPos(out Point currentPos);
                const int moveDistance = 40; // Visible movement distance
                
                var answer = DetectMCQAnswer(response);
                
                switch (answer.ToUpper())
                {
                    case "A":
                        SetCursorPos(currentPos.X, currentPos.Y - moveDistance); // Up
                        break;
                    case "B":
                        SetCursorPos(currentPos.X + moveDistance, currentPos.Y); // Right
                        break;
                    case "C":
                        SetCursorPos(currentPos.X, currentPos.Y + moveDistance); // Down
                        break;
                    case "D":
                        SetCursorPos(currentPos.X - moveDistance, currentPos.Y); // Left
                        break;
                }
            }
            catch
            {
                // Silently handle any cursor movement errors
            }
        }
        
        private string DetectMCQAnswer(string response)
        {
            try
            {
                // Look for answer patterns like "A)", "B)", "Answer: A", "The answer is B", etc.
                var patterns = new[]
                {
                    @"\b([ABCD])\)",
                    @"answer is ([ABCD])\b",
                    @"Answer: ([ABCD])\b",
                    @"Option ([ABCD])\b",
                    @"([ABCD])\) is correct",
                    @"([ABCD])\) Correct"
                };
                
                foreach (var pattern in patterns)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(response, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        #endregion
        
        #region Cleanup
        
        public void Dispose()
        {
            _hotkeyWindow?.Dispose();
            _httpClient?.Dispose();
        }
        
        #endregion
    }
    
    #region Supporting Classes
    
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
