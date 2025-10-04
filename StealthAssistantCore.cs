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
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

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
        
        #endregion
        

        
        #region Fields
        
        private readonly HttpClient _httpClient;
        private readonly List<ChatMessage> _conversationHistory;
        private readonly string _geminiApiKey;
        private ZKeySequenceWindow? _hotkeyWindow;
        private Random _random;
        private readonly List<string> _clipboardHistory;
        private StealthClipboardViewer? _clipboardViewer;
        
        #endregion
        
        #region Constructor
        
        public StealthAssistantCore()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(180); // 180 second timeout for complex OCR questions
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
            
            // Create clipboard viewer (initially hidden)
            _clipboardViewer = new StealthClipboardViewer();
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
                case 1: // Z+S
                    await HandleNormalQuery();
                    break;
                case 2: // Z+Q
                    await HandleMCQQuery();
                    break;
                case 3: // Z+J
                    await HandleJavaQuery();
                    break;
                case 4: // Z+A
                    await HandleAppendToSelection();
                    break;
                case 5: // Z+M
                    ShowStatusPopup();
                    break;
                case 6: // Z+X (old screenshot)
                    await HandleScreenshotMCQQuery();
                    break;
                case 7: // Z+E - Toggle clipboard viewer
                    HandleToggleClipboardViewer();
                    break;
                case 8: // Z+Up - Scroll up
                    HandleScrollUp();
                    break;
                case 9: // Z+Down - Scroll down
                    HandleScrollDown();
                    break;
                case 10: // Z+R+S - Screenshot OCR Normal
                    await HandleScreenshotOCRNormal();
                    break;
                case 11: // Z+R+Q - Screenshot OCR MCQ
                    await HandleScreenshotOCRMCQ();
                    break;
                case 12: // Z+R+J - Screenshot OCR Java
                    await HandleScreenshotOCRJava();
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
        
        private void HandleToggleClipboardViewer()
        {
            var clipboardText = GetClipboardText();
            _clipboardViewer?.ToggleVisibility(clipboardText);
        }
        
        private void HandleScrollUp()
        {
            _clipboardViewer?.ScrollUp();
        }
        
        private void HandleScrollDown()
        {
            _clipboardViewer?.ScrollDown();
        }
        
        private async Task HandleScreenshotOCRNormal()
        {
            try
            {
                // Capture screenshot
                var screenshotBase64 = CaptureScreenshotAsBase64();
                if (string.IsNullOrEmpty(screenshotBase64))
                {
                    ShowErrorPopup("Screenshot capture failed");
                    return;
                }
                
                // Extract text using Windows OCR
                var extractedText = await ExtractTextFromImageAsync(screenshotBase64);
                
                if (string.IsNullOrEmpty(extractedText))
                {
                    ShowErrorPopup("OCR failed - no text found");
                    return;
                }
                
                // Add to clipboard history
                AddToClipboardHistory(extractedText);
                
                // Send to Gemini
                var response = await SendToGemini(extractedText, "normal");
                ShowResponsePopup(response);
                
                // Copy response to clipboard
                SetClipboardText(response);
                AddToClipboardHistory(response);
                FlickerMouse();
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Screenshot OCR Normal failed: {ex.Message}");
            }
        }
        
        private async Task HandleScreenshotOCRMCQ()
        {
            try
            {
                // Capture screenshot
                var screenshotBase64 = CaptureScreenshotAsBase64();
                if (string.IsNullOrEmpty(screenshotBase64))
                {
                    ShowErrorPopup("Screenshot capture failed");
                    return;
                }
                
                // Extract text using Windows OCR
                var extractedText = await ExtractTextFromImageAsync(screenshotBase64);
                
                if (string.IsNullOrEmpty(extractedText))
                {
                    ShowErrorPopup("OCR failed - no text found");
                    return;
                }
                
                // Add to clipboard history
                AddToClipboardHistory(extractedText);
                
                // Send to Gemini as MCQ
                var response = await SendToGemini(extractedText, "mcq");
                
                // Copy response to clipboard
                SetClipboardText(response);
                AddToClipboardHistory(response);
                
                // Detect answer
                var detectedAnswer = DetectMCQAnswer(response);
                if (!string.IsNullOrEmpty(detectedAnswer))
                {
                    ShowAnswerPopup(detectedAnswer.ToUpper());
                }
                
                // Move cursor based on detected answer
                MoveCursorForMCQAnswer(response);
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Screenshot OCR MCQ failed: {ex.Message}");
            }
        }
        
        private async Task HandleScreenshotOCRJava()
        {
            try
            {
                // Capture screenshot
                var screenshotBase64 = CaptureScreenshotAsBase64();
                if (string.IsNullOrEmpty(screenshotBase64))
                {
                    ShowErrorPopup("Screenshot capture failed");
                    return;
                }
                
                // Extract text using Windows OCR
                var extractedText = await ExtractTextFromImageAsync(screenshotBase64);
                
                if (string.IsNullOrEmpty(extractedText))
                {
                    ShowErrorPopup("OCR failed - no text found");
                    return;
                }
                
                // Add to clipboard history
                AddToClipboardHistory(extractedText);
                
                // Send to Gemini as Java code
                var response = await SendToGemini(extractedText, "java");
                
                // Copy code to clipboard
                SetClipboardText(response);
                AddToClipboardHistory(response);
                FlickerMouse();
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Screenshot OCR Java failed: {ex.Message}");
            }
        }
        
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
            
            // Detect and show answer popup
            var detectedAnswer = DetectMCQAnswer(response);
            if (!string.IsNullOrEmpty(detectedAnswer))
            {
                ShowAnswerPopup(detectedAnswer.ToUpper());
            }
            
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
        
        private async Task HandleScreenshotMCQQuery()
        {
            var errors = new List<string>();
            
            try
            {
                // Capture screenshot
                var screenshotBase64 = CaptureScreenshotAsBase64();
                if (string.IsNullOrEmpty(screenshotBase64))
                {
                    errors.Add("Screenshot capture failed");
                }
                
                string? response = null;
                
                if (!string.IsNullOrEmpty(screenshotBase64))
                {
                    // Try vision model first
                    try
                    {
                        response = await SendToGeminiVision(screenshotBase64, "screenshot_mcq");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Vision API failed: {ex.Message}");
                        
                        // Fallback to OCR + regular model
                        try
                        {
                            var extractedText = ExtractTextFromImage(screenshotBase64);
                            if (!string.IsNullOrEmpty(extractedText))
                            {
                                response = await SendToGemini(extractedText, "mcq");
                            }
                            else
                            {
                                errors.Add("Text extraction failed");
                            }
                        }
                        catch (Exception ocrEx)
                        {
                            errors.Add($"OCR fallback failed: {ocrEx.Message}");
                        }
                    }
                }
                
                // Handle response or errors
                if (!string.IsNullOrEmpty(response))
                {
                    // Copy response to clipboard
                    SetClipboardText(response);
                    AddToClipboardHistory(response);
                    
                    // Detect answer letter for popup and cursor movement
                    var detectedAnswer = DetectMCQAnswer(response);
                    
                    // Show answer popup in bottom right
                    if (!string.IsNullOrEmpty(detectedAnswer))
                    {
                        ShowAnswerPopup(detectedAnswer.ToUpper());
                    }
                    
                    // Move cursor based on detected answer
                    MoveCursorForMCQAnswer(response);
                }
                
                // Show errors if any occurred
                if (errors.Count > 0)
                {
                    ShowErrorPopup(string.Join("; ", errors));
                }
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Screenshot MCQ failed: {ex.Message}");
            }
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
        
        private async Task<string> SendToGemini(string input, string queryType, bool useFastModel = false)
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
                
                // Use Flash model for faster responses, Pro model for complex reasoning
                var modelName = useFastModel ? "gemini-2.0-flash-exp" : "gemini-2.0-flash-exp";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_geminiApiKey}";
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
                return "Request timeout (180s) - API took too long. Check internet connection or try again.";
            }
            catch (HttpRequestException ex)
            {
                return $"Network error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        
        private async Task<string> SendToGeminiVision(string imageBase64, string queryType)
        {
            try
            {
                var systemPrompt = GetSystemPrompt(queryType);
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = systemPrompt },
                                new 
                                { 
                                    inline_data = new
                                    {
                                        mime_type = "image/png",
                                        data = imageBase64
                                    }
                                }
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
                
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={_geminiApiKey}";
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
                    
                    return aiResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Error {response.StatusCode}: {errorContent}");
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request timeout (180s) - image processing took too long");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Vision API Error: {ex.Message}");
            }
        }
        
        private string GetSystemPrompt(string queryType)
        {
            return queryType switch
            {
                "mcq" => "You are helping with multiple choice questions. Provide clear, concise answers. If this is a multiple choice question, give the correct answer with brief explanation.",
                "java" => "Give only the Java code solution for this problem. Do not include any comments, explanations, or extra text. Only provide the code",
                "normal" => "You are a helpful assistant. Provide clear, concise responses to questions.",
                "screenshot_mcq" => "Analyze this screenshot for multiple choice questions. Return only the letter (A, B, C, or D) of the correct answer.",
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
        
        private void ShowErrorPopup(string message)
        {
            var popup = new ResponsePopup(message);
            popup.Show();
            
            // Auto-close after 2 seconds
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000;
            timer.Tick += (s, e) => 
            {
                timer.Stop();
                popup.Close();
            };
            timer.Start();
        }
        
        private void ShowAnswerPopup(string answer)
        {
            var popup = new ResponsePopup($"Answer: {answer}");
            popup.Show();
            
            // Auto-close after 3 seconds (slightly longer to see the answer)
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000;
            timer.Tick += (s, e) => 
            {
                timer.Stop();
                popup.Close();
            };
            timer.Start();
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
            catch (Exception)
            {
                // Log error but don't show popup here (handled by caller)
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
            catch (Exception ex)
            {
                Debug.WriteLine($"OCR Error: {ex.Message}");
                return string.Empty;
            }
        }
        
        private string ExtractTextFromImage(string base64Image)
        {
            // Synchronous wrapper for compatibility
            return ExtractTextFromImageAsync(base64Image).GetAwaiter().GetResult();
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
            _clipboardViewer?.Dispose();
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
