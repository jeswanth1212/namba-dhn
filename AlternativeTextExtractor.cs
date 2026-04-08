using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StealthAssistant
{
    public static class AlternativeTextExtractor
    {
        #region Windows API Imports
        
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        
        [DllImport("oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint id, ref Guid iid, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);
        
        private const uint PW_RENDERFULLCONTENT = 0x00000002;
        private const uint OBJID_CLIENT = 0xFFFFFFFC;
        
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        
        #endregion
        
        #region PrintWindow Capture
        
        public static string CaptureWindowUsingPrintWindow(IntPtr hWnd)
        {
            try
            {
                if (hWnd == IntPtr.Zero)
                    return string.Empty;
                
                // Get window dimensions
                if (!GetWindowRect(hWnd, out RECT rect))
                    return string.Empty;
                
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                
                if (width <= 0 || height <= 0)
                    return string.Empty;
                
                // Create bitmap
                using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        IntPtr hdc = graphics.GetHdc();
                        try
                        {
                            // Use PrintWindow to capture
                            PrintWindow(hWnd, hdc, PW_RENDERFULLCONTENT);
                        }
                        finally
                        {
                            graphics.ReleaseHdc(hdc);
                        }
                    }
                    
                    // Convert to base64
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        
        #endregion
        
        #region IAccessible Extraction
        
        public static string ExtractTextUsingIAccessible(IntPtr hWnd)
        {
            try
            {
                if (hWnd == IntPtr.Zero)
                    return string.Empty;
                
                StringBuilder result = new StringBuilder();
                
                // Try to get IAccessible interface
                Guid IID_IAccessible = new Guid("{618736e0-3c3d-11cf-810c-00aa00389b71}");
                object accessible = null;
                
                int hr = AccessibleObjectFromWindow(hWnd, OBJID_CLIENT, ref IID_IAccessible, ref accessible);
                
                if (hr == 0 && accessible != null)
                {
                    // Try to get text from accessible object
                    // Note: This is a simplified implementation
                    // Full implementation would require proper IAccessible interface handling
                    result.AppendLine(accessible.ToString());
                }
                
                return result.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
        
        #endregion
        
        #region Window Enumeration
        
        public static string ExtractTextFromWindowTree(IntPtr hWnd)
        {
            try
            {
                if (hWnd == IntPtr.Zero)
                    return string.Empty;
                
                StringBuilder result = new StringBuilder();
                
                // Get text from main window
                StringBuilder windowText = new StringBuilder(256);
                GetWindowText(hWnd, windowText, 256);
                if (windowText.Length > 0)
                {
                    result.AppendLine(windowText.ToString());
                }
                
                // Enumerate child windows
                EnumChildWindows(hWnd, (childHWnd, lParam) =>
                {
                    if (IsWindowVisible(childHWnd))
                    {
                        StringBuilder childText = new StringBuilder(256);
                        int length = GetWindowText(childHWnd, childText, 256);
                        
                        if (length > 0)
                        {
                            result.AppendLine(childText.ToString());
                        }
                    }
                    return true; // Continue enumeration
                }, IntPtr.Zero);
                
                return result.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
        
        #endregion
        
        #region Browser Extraction
        
        public static async Task<string> ExtractTextFromBrowser()
        {
            try
            {
                // Get foreground window
                IntPtr hWnd = GetForegroundWindow();
                if (hWnd == IntPtr.Zero)
                    return string.Empty;
                
                // Check if it's a browser window
                StringBuilder className = new StringBuilder(256);
                GetClassName(hWnd, className, 256);
                string classNameStr = className.ToString();
                
                bool isBrowser = classNameStr.Contains("Chrome") ||
                                classNameStr.Contains("Mozilla") ||
                                classNameStr.Contains("ApplicationFrame") || // Edge
                                classNameStr.Contains("IEFrame");
                
                if (!isBrowser)
                    return string.Empty;
                
                StringBuilder result = new StringBuilder();
                
                // Try PrintWindow first
                string printWindowBase64 = CaptureWindowUsingPrintWindow(hWnd);
                if (!string.IsNullOrEmpty(printWindowBase64))
                {
                    result.AppendLine("[PrintWindow capture successful]");
                }
                
                // Try IAccessible
                string accessibleText = ExtractTextUsingIAccessible(hWnd);
                if (!string.IsNullOrWhiteSpace(accessibleText))
                {
                    result.AppendLine(accessibleText);
                }
                
                // Try Window Enumeration
                string windowText = ExtractTextFromWindowTree(hWnd);
                if (!string.IsNullOrWhiteSpace(windowText))
                {
                    result.AppendLine(windowText);
                }
                
                return result.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        
        #endregion
    }
}
