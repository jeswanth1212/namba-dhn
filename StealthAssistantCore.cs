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
        
        // Auto-typing fields
        private System.Threading.Timer? _autoTypeTimer;
        private string _autoTypeText = "";
        private int _autoTypePosition = 0;
        private bool _autoTypePaused = false;
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
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
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
        
        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
        
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        
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
                case 1: // Z+M - Status
                    ShowStatusPopup();
                    break;
                case 2: // Z+W - Extract text and get AI response
                    await HandleExtractAndQuery();
                    break;
                case 3: // Z+J - Generate Java code
                    await HandleExtractAndGenerateCode("java");
                    break;
                case 4: // Z+P - Generate Python code
                    await HandleExtractAndGenerateCode("python");
                    break;
                case 5: // Z+C - Generate C++ code
                    await HandleExtractAndGenerateCode("cpp");
                    break;
                case 6: // Z+E - Toggle clipboard viewer
                    HandleToggleClipboardViewer();
                    break;
                case 7: // Z+V - Auto-type clipboard
                    HandleAutoTypeStart();
                    break;
                case 8: // ` - Pause/Resume auto-typing
                    HandleAutoTypePauseResume();
                    break;
                case 10: // Z+B - Auto-type for compiler (strips indentation)
                    HandleAutoTypeCompilerMode();
                    break;
                case 12: // Z+R - Reset conversation history
                    HandleResetConversationHistory();
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
        
        private void HandleResetConversationHistory()
        {
            try
            {
                _conversationHistory.Clear();
                ShowResponsePopup("Conversation history cleared!");
                Debug.WriteLine("Conversation history reset");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Reset history error: {ex.Message}");
                ShowErrorPopup($"Error: {ex.Message}");
            }
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
            
            // Extract clean Java code (remove comments if any)
            var cleanCode = ExtractJavaCodeWithoutComments(response);
            
            // Copy entire response to clipboard (should be clean Java code)
            SetClipboardText(cleanCode);
            AddToClipboardHistory(cleanCode);
            FlickerMouse();
        }
        
        private async Task HandleJavaCodeFromScreen()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    ShowErrorPopup("No foreground window found");
                    return;
                }
                
                var allExtractedText = new StringBuilder();
                
                // Strategy 1: Try PrintWindow first (might bypass screenshot detection)
                try
                {
                    string printWindowBase64 = AlternativeTextExtractor.CaptureWindowUsingPrintWindow(foregroundWindow);
                    
                    if (!string.IsNullOrEmpty(printWindowBase64))
                    {
                        var ocrText = await ExtractTextFromImageAsync(printWindowBase64);
                        
                        if (!string.IsNullOrWhiteSpace(ocrText))
                        {
                            allExtractedText.AppendLine(ocrText);
                        }
                    }
                }
                catch { }
                
                // Strategy 2: IAccessible (Accessibility API)
                try
                {
                    string accessibleText = AlternativeTextExtractor.ExtractTextUsingIAccessible(foregroundWindow);
                    if (!string.IsNullOrWhiteSpace(accessibleText))
                    {
                        allExtractedText.AppendLine(accessibleText);
                    }
                }
                catch { }
                
                // Strategy 3: Window enumeration
                try
                {
                    string windowText = AlternativeTextExtractor.ExtractTextFromWindowTree(foregroundWindow);
                    if (!string.IsNullOrWhiteSpace(windowText))
                    {
                        allExtractedText.AppendLine(windowText);
                    }
                }
                catch { }
                
                string extractedText = allExtractedText.ToString().Trim();
                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    ShowErrorPopup("Screen extraction failed - try other methods");
                    return;
                }
                
                // Add extracted text to clipboard history
                AddToClipboardHistory(extractedText);
                
                // Send to Gemini for Java code generation
                var response = await SendToGemini(extractedText, "java");
                
                // Extract clean Java code (remove comments if any)
                var cleanCode = ExtractJavaCodeWithoutComments(response);
                
                // Copy code to clipboard
                SetClipboardText(cleanCode);
                AddToClipboardHistory(cleanCode);
                FlickerMouse();
                
                // Show success popup
                ShowResponsePopup("Java code generated and copied to clipboard");
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Java code generation failed: {ex.Message}");
            }
        }
        
        private async Task HandleScreenshotMCQQuery()
        {
            var errors = new List<string>();
            
            try
            {
                string? screenshotBase64 = null;
                
                // Try BitBlt first (standard method)
                screenshotBase64 = CaptureScreenshotAsBase64();
                
                // If BitBlt fails, try PrintWindow (alternative method that might bypass detection)
                if (string.IsNullOrEmpty(screenshotBase64))
                {
                    try
                    {
                        IntPtr foregroundWindow = GetForegroundWindow();
                        if (foregroundWindow != IntPtr.Zero)
                        {
                            screenshotBase64 = AlternativeTextExtractor.CaptureWindowUsingPrintWindow(foregroundWindow);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"PrintWindow fallback failed: {ex.Message}");
                    }
                }
                
                if (string.IsNullOrEmpty(screenshotBase64))
                {
                    errors.Add("All screenshot methods failed");
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
                            var extractedText = await ExtractTextFromImageAsync(screenshotBase64);
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
        
        private async Task HandleAlternativeTextExtraction()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    ShowErrorPopup("No foreground window found");
                    return;
                }
                
                var allExtractedText = new StringBuilder();
                
                // Strategy 1: Try PrintWindow first (might bypass screenshot detection)
                try
                {
                    string printWindowBase64 = AlternativeTextExtractor.CaptureWindowUsingPrintWindow(foregroundWindow);
                    if (!string.IsNullOrEmpty(printWindowBase64))
                    {
                        var ocrText = await ExtractTextFromImageAsync(printWindowBase64);
                        if (!string.IsNullOrWhiteSpace(ocrText))
                        {
                            allExtractedText.AppendLine("=== PrintWindow OCR ===");
                            allExtractedText.AppendLine(ocrText);
                        }
                    }
                }
                catch { }
                
                // Strategy 2: IAccessible (Accessibility API)
                try
                {
                    string accessibleText = AlternativeTextExtractor.ExtractTextUsingIAccessible(foregroundWindow);
                    if (!string.IsNullOrWhiteSpace(accessibleText))
                    {
                        allExtractedText.AppendLine("=== Accessibility API ===");
                        allExtractedText.AppendLine(accessibleText);
                    }
                }
                catch { }
                
                // Strategy 3: Window enumeration
                try
                {
                    string windowText = AlternativeTextExtractor.ExtractTextFromWindowTree(foregroundWindow);
                    if (!string.IsNullOrWhiteSpace(windowText))
                    {
                        allExtractedText.AppendLine("=== Window Text ===");
                        allExtractedText.AppendLine(windowText);
                    }
                }
                catch { }
                
                string extractedText = allExtractedText.ToString().Trim();
                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    ShowErrorPopup("All extraction methods failed - try other hotkeys");
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
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Alternative extraction failed: {ex.Message}");
            }
        }
        
        private async Task HandleWindowEnumerationExtraction()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    ShowErrorPopup("No foreground window found");
                    return;
                }
                
                // Try PrintWindow capture first (might bypass screenshot detection)
                string printWindowBase64 = AlternativeTextExtractor.CaptureWindowUsingPrintWindow(foregroundWindow);
                
                if (!string.IsNullOrEmpty(printWindowBase64))
                {
                    // Apply OCR to PrintWindow capture
                    var extractedText = await ExtractTextFromImageAsync(printWindowBase64);
                    
                    if (!string.IsNullOrEmpty(extractedText))
                    {
                        AddToClipboardHistory(extractedText);
                        var response = await SendToGemini(extractedText, "normal");
                        ShowResponsePopup(response);
                        SetClipboardText(response);
                        AddToClipboardHistory(response);
                        return;
                    }
                }
                
                // Fallback to window enumeration
                string windowText = AlternativeTextExtractor.ExtractTextFromWindowTree(foregroundWindow);
                
                if (string.IsNullOrWhiteSpace(windowText))
                {
                    ShowErrorPopup("Window enumeration failed - no text found");
                    return;
                }
                
                AddToClipboardHistory(windowText);
                var enumResponse = await SendToGemini(windowText, "normal");
                ShowResponsePopup(enumResponse);
                SetClipboardText(enumResponse);
                AddToClipboardHistory(enumResponse);
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Window enumeration failed: {ex.Message}");
            }
        }
        
        private async Task HandleBrowserTextExtraction()
        {
            try
            {
                // Extract text specifically from browser windows
                string extractedText = await AlternativeTextExtractor.ExtractTextFromBrowser();
                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    ShowErrorPopup("Browser extraction failed - ensure browser is active");
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
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Browser extraction failed: {ex.Message}");
            }
        }
        
        // NEW METHOD: Z+W - Extract text and get AI response (always normal, shows summary)
        private async Task HandleExtractAndQuery()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    ShowErrorPopup("No foreground window found");
                    return;
                }
                
                var allExtractedText = new StringBuilder();
                
                // Use all bypass techniques
                // Strategy 1: PrintWindow
                try
                {
                    string printWindowBase64 = AlternativeTextExtractor.CaptureWindowUsingPrintWindow(foregroundWindow);
                    if (!string.IsNullOrEmpty(printWindowBase64))
                    {
                        var ocrText = await ExtractTextFromImageAsync(printWindowBase64);
                        if (!string.IsNullOrWhiteSpace(ocrText))
                        {
                            allExtractedText.AppendLine(ocrText);
                        }
                    }
                }
                catch { }
                
                // Strategy 2: IAccessible
                try
                {
                    string accessibleText = AlternativeTextExtractor.ExtractTextUsingIAccessible(foregroundWindow);
                    if (!string.IsNullOrWhiteSpace(accessibleText))
                    {
                        allExtractedText.AppendLine(accessibleText);
                    }
                }
                catch { }
                
                // Strategy 3: Window enumeration
                try
                {
                    string windowText = AlternativeTextExtractor.ExtractTextFromWindowTree(foregroundWindow);
                    if (!string.IsNullOrWhiteSpace(windowText))
                    {
                        allExtractedText.AppendLine(windowText);
                    }
                }
                catch { }
                
                string extractedText = allExtractedText.ToString().Trim();
                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    ShowErrorPopup("All extraction methods failed");
                    return;
                }
                
                // Add to clipboard history
                AddToClipboardHistory(extractedText);
                
                // Always use normal query type - AI will detect MCQ automatically
                // Send to Gemini with smart prompt
                var response = await SendToGemini(extractedText, "normal");
                
                // Show 1-2 line summary in popup
                string summary = ExtractSummary(response, 2);
                ShowResponsePopup(summary);
                
                // Full response to clipboard
                SetClipboardText(response);
                AddToClipboardHistory(response);
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Extraction failed: {ex.Message}");
            }
        }
        
        // NEW METHOD: Z+J/Q/P - Extract text and generate code
        private async Task HandleExtractAndGenerateCode(string language)
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    ShowErrorPopup("No foreground window found");
                    return;
                }
                
                var allExtractedText = new StringBuilder();
                
                // Use all bypass techniques (same as HandleExtractAndQuery)
                try
                {
                    string printWindowBase64 = AlternativeTextExtractor.CaptureWindowUsingPrintWindow(foregroundWindow);
                    if (!string.IsNullOrEmpty(printWindowBase64))
                    {
                        var ocrText = await ExtractTextFromImageAsync(printWindowBase64);
                        if (!string.IsNullOrWhiteSpace(ocrText))
                        {
                            allExtractedText.AppendLine(ocrText);
                        }
                    }
                }
                catch { }
                
                try
                {
                    string accessibleText = AlternativeTextExtractor.ExtractTextUsingIAccessible(foregroundWindow);
                    if (!string.IsNullOrWhiteSpace(accessibleText))
                    {
                        allExtractedText.AppendLine(accessibleText);
                    }
                }
                catch { }
                
                try
                {
                    string windowText = AlternativeTextExtractor.ExtractTextFromWindowTree(foregroundWindow);
                    if (!string.IsNullOrWhiteSpace(windowText))
                    {
                        allExtractedText.AppendLine(windowText);
                    }
                }
                catch { }
                
                string extractedText = allExtractedText.ToString().Trim();
                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    ShowErrorPopup("All extraction methods failed");
                    return;
                }
                
                // Add to clipboard history
                AddToClipboardHistory(extractedText);
                
                // Enhance extracted text with PROMPT and ERROR detection
                string enhancedText = EnhanceExtractedTextForCodeGeneration(extractedText);
                
                // Generate code based on language
                string queryType = language; // "java", "python", or "cpp"
                var response = await SendToGemini(enhancedText, queryType);
                
                // Extract code without comments (keep minimal comments)
                string code = ExtractCodeWithMinimalComments(response, language);
                
                // Show "Generated" popup
                ShowResponsePopup("Generated");
                
                // Code to clipboard
                SetClipboardText(code);
                AddToClipboardHistory(code);
            }
            catch (Exception ex)
            {
                ShowErrorPopup($"Code generation failed: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Gemini API Integration
        
        private async Task<string> SendToGemini(string input, string queryType, bool useFastModel = false)
        {
            try
            {
                var systemPrompt = GetSystemPrompt(queryType);
                
                _conversationHistory.Add(new ChatMessage("user", input));
                
                // Build contents array with conversation history
                var contentsList = new List<object>();
                
                // Add system prompt as first message
                contentsList.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = systemPrompt } }
                });
                
                // Add conversation history
                foreach (var message in _conversationHistory)
                {
                    contentsList.Add(new
                    {
                        role = message.Role == "user" ? "user" : "model",
                        parts = new[] { new { text = message.Content } }
                    });
                }
                
                var requestBody = new
                {
                    contents = contentsList.ToArray(),
                    generationConfig = new
                    {
                        temperature = 0.1
                    }
                };
                
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Select model based on query type for optimal free tier usage
                // Using gemini-3.1-flash-lite-preview for all queries
                string modelName = queryType switch
                {
                    "mcq" => "gemini-3.1-flash-lite-preview",  // Best free tier for MCQ
                    "java" => "gemini-3.1-flash-lite-preview",      // Good for code generation
                    "normal" => "gemini-3.1-flash-lite-preview",    // Balanced for general queries
                    _ => "gemini-3.1-flash-lite-preview"            // Default fallback
                };
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
                
                // Using gemini-3.1-flash-lite-preview for vision/screenshot processing
                var modelName = "gemini-3.1-flash-lite-preview";
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
                "mcq" => "You are helping with multiple choice questions. Analyze the question and provide: 1) The correct option letter/number, 2) A brief one-line explanation why it's correct. Format: 'Option: X - Brief reason'",
                "java" => "Generate complete, runnable Java code that:\n1. Includes ALL necessary import statements (java.util.*, java.io.*, etc.)\n2. Has minimal comments (only for main method and class purpose)\n3. Uses simple data structures (ArrayList, HashMap, HashSet, StringBuilder, arrays, etc)\n4. Includes proper input handling (Scanner or BufferedReader)\n5. If PROMPT is provided, implement that specific requirement\n6. If compiler ERROR is present, fix the code to resolve the error\n7. Code must compile and run immediately without any modifications\nProvide only the complete code, no explanations.",
                "python" => "Generate complete, runnable Python code that:\n1. Includes ALL necessary import statements at the top\n2. Has minimal comments (only for main function and module purpose)\n3. Uses simple data structures (list, dict, set)\n4. Includes proper input handling (input() or sys.stdin)\n5. If PROMPT is provided, implement that specific requirement\n6. If ERROR is present, fix the code to resolve the error\n7. Code must run immediately without any modifications\nProvide only the complete code, no explanations.",
                "cpp" => "Generate complete, runnable C++ code that:\n1. Includes ALL necessary headers (#include <iostream>, <vector>, <map>, <set>, <string>, <algorithm>, etc.)\n2. Has minimal comments (only for main function and purpose)\n3. Uses STL containers (vector, map, set, string)\n4. Includes proper input handling (cin or getline)\n5. Uses 'using namespace std;' for simplicity\n6. If PROMPT is provided, implement that specific requirement\n7. If compiler ERROR is present, fix the code to resolve the error\n8. Code must compile and run immediately without any modifications\nProvide only the complete code, no explanations.",
                "normal" => "You are a helpful assistant. Provide clear, focused answers with main points or summary. If this is a multiple choice question, give the correct option with brief explanation. Keep responses concise and to the point.",
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
        
        // Helper method to check if text is an MCQ
        private bool IsMCQQuestion(string text)
        {
            // Check for common MCQ patterns
            string lowerText = text.ToLower();
            return lowerText.Contains("a)") || lowerText.Contains("b)") || lowerText.Contains("c)") || lowerText.Contains("d)") ||
                   lowerText.Contains("a.") || lowerText.Contains("b.") || lowerText.Contains("c.") || lowerText.Contains("d.") ||
                   lowerText.Contains("option a") || lowerText.Contains("option b") ||
                   (lowerText.Contains("choose") && (lowerText.Contains("correct") || lowerText.Contains("best")));
        }
        
        // Helper method to extract MCQ summary for popup (Option: X - Reason)
        private string ExtractMCQSummary(string response)
        {
            try
            {
                // Look for pattern "Option: X" or just "X -" or "Answer: X"
                var lines = response.Split('\n');
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("Option:", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith("Answer:", StringComparison.OrdinalIgnoreCase) ||
                        System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[A-D]\s*[-:]"))
                    {
                        // Return first 100 chars max for popup
                        return trimmed.Length > 100 ? trimmed.Substring(0, 100) + "..." : trimmed;
                    }
                }
                
                // Fallback: return first non-empty line
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        return trimmed.Length > 100 ? trimmed.Substring(0, 100) + "..." : trimmed;
                    }
                }
                
                return "See clipboard for answer";
            }
            catch
            {
                return "See clipboard for answer";
            }
        }
        
        // Helper method to extract summary (first N lines)
        private string ExtractSummary(string response, int maxLines)
        {
            try
            {
                var lines = response.Split('\n');
                var summaryLines = new List<string>();
                int count = 0;
                
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        summaryLines.Add(trimmed);
                        count++;
                        if (count >= maxLines) break;
                    }
                }
                
                string summary = string.Join(" ", summaryLines);
                // Limit to 150 chars for popup
                return summary.Length > 150 ? summary.Substring(0, 150) + "..." : summary;
            }
            catch
            {
                return "See clipboard for full response";
            }
        }
        
        // Helper method to extract PROMPT content from text
        private string ExtractPromptContent(string text)
        {
            try
            {
                // Look for pattern: prompt{ ... } or Prompt{ ... } or PROMPT{ ... } (case-insensitive)
                var regex = new System.Text.RegularExpressions.Regex(
                    @"prompt\s*\{([^}]+)\}",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                var match = regex.Match(text);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        // Helper method to detect if text contains compiler errors
        private bool ContainsCompilerError(string text)
        {
            try
            {
                string lowerText = text.ToLower();
                
                // Common error indicators
                return lowerText.Contains("error:") ||
                       lowerText.Contains("error ") ||
                       lowerText.Contains("exception") ||
                       lowerText.Contains("traceback") ||
                       lowerText.Contains("syntaxerror") ||
                       lowerText.Contains("nameerror") ||
                       lowerText.Contains("indentationerror") ||
                       lowerText.Contains("typeerror") ||
                       lowerText.Contains("cannot find symbol") ||
                       lowerText.Contains("incompatible types") ||
                       lowerText.Contains("undefined reference") ||
                       lowerText.Contains("no matching function") ||
                       lowerText.Contains("compilation failed") ||
                       lowerText.Contains("build failed");
            }
            catch
            {
                return false;
            }
        }
        
        // Helper method to enhance extracted text with PROMPT and ERROR context
        private string EnhanceExtractedTextForCodeGeneration(string extractedText)
        {
            try
            {
                var enhancedText = new StringBuilder();
                
                // Extract PROMPT if present
                string promptContent = ExtractPromptContent(extractedText);
                bool hasPrompt = !string.IsNullOrEmpty(promptContent);
                
                // Check for compiler errors
                bool hasError = ContainsCompilerError(extractedText);
                
                // Build enhanced text
                if (hasPrompt)
                {
                    enhancedText.AppendLine("REQUIREMENT:");
                    enhancedText.AppendLine(promptContent);
                    enhancedText.AppendLine();
                }
                
                if (hasError)
                {
                    enhancedText.AppendLine("FIX THE FOLLOWING CODE WITH ERRORS:");
                    enhancedText.AppendLine(extractedText);
                }
                else if (!hasPrompt)
                {
                    // No prompt, no error - just the extracted text (problem description or code)
                    enhancedText.AppendLine(extractedText);
                }
                else
                {
                    // Has prompt but no error - include context
                    enhancedText.AppendLine("CONTEXT:");
                    enhancedText.AppendLine(extractedText);
                }
                
                return enhancedText.ToString().Trim();
            }
            catch
            {
                return extractedText; // Fallback to original text
            }
        }
        
        // Helper method to extract code with minimal comments
        private string ExtractCodeWithMinimalComments(string response, string language)
        {
            try
            {
                var lines = response.Split('\n');
                var codeLines = new List<string>();
                bool inCodeBlock = false;
                
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    // Handle code block markers
                    if (trimmedLine.StartsWith("```"))
                    {
                        inCodeBlock = !inCodeBlock;
                        continue;
                    }
                    
                    // Skip explanatory text outside code blocks
                    if (!inCodeBlock && !IsCodeLine(trimmedLine, language))
                    {
                        continue;
                    }
                    
                    // Keep minimal comments (class/method level only)
                    if (trimmedLine.StartsWith("//") || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("/*"))
                    {
                        // Keep only if it's a main/class comment
                        if (trimmedLine.ToLower().Contains("main") || 
                            trimmedLine.ToLower().Contains("class") ||
                            trimmedLine.ToLower().Contains("function"))
                        {
                            codeLines.Add(line);
                        }
                        continue;
                    }
                    
                    codeLines.Add(line);
                }
                
                return string.Join("\n", codeLines).Trim();
            }
            catch
            {
                return response; // Return original if extraction fails
            }
        }
        
        // Helper to detect code lines
        private bool IsCodeLine(string line, string language)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            
            return language switch
            {
                "java" => line.Contains("public") || line.Contains("private") || line.Contains("class") || 
                          line.Contains("void") || line.Contains("int") || line.Contains("String") ||
                          line.Contains("import") || line.Contains("{") || line.Contains("}"),
                "python" => line.StartsWith("def ") || line.StartsWith("class ") || line.StartsWith("import ") ||
                            line.Contains("return") || line.Contains("print(") || line.Contains("if ") ||
                            line.Contains("for ") || line.Contains("while "),
                "cpp" => line.Contains("#include") || line.Contains("int main") || line.Contains("std::") ||
                         line.Contains("class ") || line.Contains("void ") || line.Contains("{") || line.Contains("}"),
                _ => false
            };
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
        
        #endregion
        
        #region Auto-Typing
        
        private void HandleAutoTypePauseResume()
        {
            try
            {
                lock (_autoTypeLock)
                {
                    if (_autoTypeTimer == null || string.IsNullOrEmpty(_autoTypeText))
                    {
                        Debug.WriteLine("Auto-type: Nothing to pause/resume");
                        var popup = new ResponsePopup("Nothing to pause/resume");
                        popup.Show();
                        return; // Nothing to pause/resume
                    }
                    
                    _autoTypePaused = !_autoTypePaused;
                    Debug.WriteLine($"Auto-type: {(_autoTypePaused ? "PAUSED" : "RESUMED")} at position {_autoTypePosition}");
                    var statusPopup = new ResponsePopup(_autoTypePaused ? "PAUSED" : "RESUMED");
                    statusPopup.Show();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto-type pause/resume error: {ex.Message}");
                var errorPopup = new ResponsePopup($"Error: {ex.Message}");
                errorPopup.Show();
            }
        }
        
        private void HandleAutoTypeCompilerMode()
        {
            try
            {
                // Show confirmation popup
                var popup = new ResponsePopup("Compiler mode starting...");
                popup.Show();
                
                // Get clipboard text on STA thread
                string clipboardText = "";
                var thread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        clipboardText = Clipboard.GetText();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Clipboard access error: {ex.Message}");
                    }
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
                
                if (string.IsNullOrEmpty(clipboardText))
                {
                    Debug.WriteLine("Auto-type compiler: Clipboard is empty");
                    var emptyPopup = new ResponsePopup("Clipboard is empty!");
                    emptyPopup.Show();
                    return;
                }
                
                // Process text for compiler mode: strip leading whitespace from each line
                string processedText = PrepareTextForCompilerMode(clipboardText);
                
                lock (_autoTypeLock)
                {
                    // Stop any existing typing
                    _autoTypeTimer?.Dispose();
                    
                    Debug.WriteLine($"Auto-type compiler: Starting with {processedText.Length} characters");
                    var startPopup = new ResponsePopup($"Compiler mode: {processedText.Length} chars");
                    startPopup.Show();
                    
                    // Reset and start from beginning
                    _autoTypeText = processedText;
                    _autoTypePosition = 0;
                    _autoTypePaused = false;
                    
                    // Start typing at same speed as Z+V
                    _autoTypeTimer = new System.Threading.Timer(AutoTypeCallback, null, 0, 1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto-type compiler start error: {ex.Message}");
                var errorPopup = new ResponsePopup($"Error: {ex.Message}");
                errorPopup.Show();
            }
        }
        
        private string PrepareTextForCompilerMode(string text)
        {
            try
            {
                var lines = text.Split('\n');
                var processedLines = new List<string>();
                
                foreach (var line in lines)
                {
                    // Remove leading whitespace (tabs and spaces)
                    // Keep the actual code content
                    string trimmedLine = line.TrimStart(' ', '\t');
                    
                    // Keep empty lines as-is (just newline)
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        processedLines.Add("");
                    }
                    else
                    {
                        processedLines.Add(trimmedLine);
                    }
                }
                
                // Join with \n (our auto-type will convert to Enter key)
                return string.Join("\n", processedLines);
            }
            catch
            {
                // Fallback to original text if processing fails
                return text;
            }
        }
        
        #endregion
        
        #region Auto-Typing
        
        private void HandleAutoTypeStart()
        {
            try
            {
                // Show confirmation popup
                var popup = new ResponsePopup("Auto-type starting...");
                popup.Show();
                
                // Get clipboard text on STA thread
                string clipboardText = "";
                var thread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        clipboardText = Clipboard.GetText();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Clipboard access error: {ex.Message}");
                    }
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
                
                if (string.IsNullOrEmpty(clipboardText))
                {
                    Debug.WriteLine("Auto-type: Clipboard is empty");
                    var emptyPopup = new ResponsePopup("Clipboard is empty!");
                    emptyPopup.Show();
                    return;
                }
                
                lock (_autoTypeLock)
                {
                    // Stop any existing typing
                    _autoTypeTimer?.Dispose();
                    
                    Debug.WriteLine($"Auto-type: Starting with {clipboardText.Length} characters");
                    var startPopup = new ResponsePopup($"Typing {clipboardText.Length} chars...");
                    startPopup.Show();
                    
                    // Reset and start from beginning
                    _autoTypeText = clipboardText;
                    _autoTypePosition = 0;
                    _autoTypePaused = false;
                    
                    // Start typing at 10,000 chars/sec using SendInput (10 chars per 1ms)
                    // SendInput is hardware-level simulation, much harder to detect than SendKeys
                    _autoTypeTimer = new System.Threading.Timer(AutoTypeCallback, null, 0, 1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto-type start error: {ex.Message}");
                var errorPopup = new ResponsePopup($"Error: {ex.Message}");
                errorPopup.Show();
            }
        }
        
        
        private void AutoTypeCallback(object? state)
        {
            try
            {
                lock (_autoTypeLock)
                {
                    // Check if paused
                    if (_autoTypePaused)
                    {
                        return;
                    }
                    
                    // Check if finished
                    if (_autoTypePosition >= _autoTypeText.Length)
                    {
                        _autoTypeTimer?.Dispose();
                        _autoTypeTimer = null;
                        return;
                    }
                    
                    // Type 10 characters per callback using SendInput (10,000 chars/sec)
                    int charsToType = Math.Min(10, _autoTypeText.Length - _autoTypePosition);
                    
                    for (int i = 0; i < charsToType; i++)
                    {
                        char c = _autoTypeText[_autoTypePosition + i];
                        SendCharacterUsingSendInput(c);
                    }
                    
                    // Move to next position
                    _autoTypePosition += charsToType;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto-type callback error: {ex.Message}");
                // Stop on error
                lock (_autoTypeLock)
                {
                    _autoTypeTimer?.Dispose();
                    _autoTypeTimer = null;
                }
            }
        }
        
        private void SendCharacterUsingSendInput(char c)
        {
            // Handle special characters
            if (c == '\r')
            {
                return; // Skip carriage return, we'll handle \n
            }
            else if (c == '\n')
            {
                // Send Enter key
                SendKeyPress(0x0D); // VK_RETURN
                return;
            }
            else if (c == '\t')
            {
                // Send Tab key
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
        
        #endregion
        
        #region Cleanup
        
        public void Dispose()
        {
            _autoTypeTimer?.Dispose();
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
