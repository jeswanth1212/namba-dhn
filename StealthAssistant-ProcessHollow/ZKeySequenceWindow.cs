using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StealthAssistant.ProcessHollow
{
    public class ZKeySequenceWindow : Form
    {
        #region Windows API
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        // Virtual key codes
        private const int VK_Z = 0x5A;
        private const int VK_M = 0x4D;
        private const int VK_W = 0x57;
        private const int VK_J = 0x4A;
        private const int VK_P = 0x50;
        private const int VK_C = 0x43;
        private const int VK_E = 0x45;
        private const int VK_B = 0x42;
        private const int VK_R = 0x52;
        private const int VK_V = 0x56;
        private const int VK_BACKTICK = 0xC0; // ` key
        private const int VK_1 = 0x31;
        private const int VK_T = 0x54;
        private const int VK_L = 0x4C;
        
        #endregion
        
        #region Fields
        
        private IntPtr _hookID = IntPtr.Zero;
        private readonly LowLevelKeyboardProc _proc;
        private bool _zPressed = false;
        private DateTime _lastZPress = DateTime.MinValue;
        private readonly System.Threading.Timer _sequenceTimer;
        private const int SEQUENCE_TIMEOUT_MS = 1000;
        
        #endregion
        
        #region Events
        
        public event EventHandler<KeyPressedEventArgs>? HotkeyPressed;
        
        #endregion

        public ZKeySequenceWindow()
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            
            // Setup sequence timer
            _sequenceTimer = new System.Threading.Timer(OnSequenceTimeout, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            
            // Setup keyboard hook
            _proc = HookCallback;
            SetHook();
        }
        
        private void SetHook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                    GetModuleHandle(curModule.ModuleName!), 0);
            }
        }
        
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    HandleKeyPress(vkCode);
                }
            }
            catch { }
            
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        
        private void HandleKeyPress(int vkCode)
        {
            var now = DateTime.Now;
            
            // Special case: Backtick (`) works standalone without Z
            if (vkCode == VK_BACKTICK)
            {
                OnHotkeyDetected(8); // Pause/Resume auto-type
                return;
            }
            
            if (vkCode == VK_Z)
            {
                _zPressed = true;
                _lastZPress = now;
                _sequenceTimer.Change(SEQUENCE_TIMEOUT_MS, System.Threading.Timeout.Infinite);
            }
            else if (_zPressed && (now - _lastZPress).TotalMilliseconds <= SEQUENCE_TIMEOUT_MS)
            {
                // Two-key sequence: Z+Key
                var hotkeyId = GetHotkeyId(vkCode);
                if (hotkeyId > 0)
                {
                    OnHotkeyDetected(hotkeyId);
                    ResetSequence();
                    return;
                }
                ResetSequence();
            }
            else if (vkCode != VK_Z)
            {
                ResetSequence();
            }
        }
        
        private int GetHotkeyId(int vkCode)
        {
            return vkCode switch
            {
                VK_M => 1,  // Z+M - Status
                VK_W => 2,  // Z+W - AI Query
                VK_J => 3,  // Z+J - Java Code
                VK_P => 4,  // Z+P - Python Code
                VK_C => 5,  // Z+C - C++ Code
                VK_E => 6,  // Z+E - Clipboard Viewer
                VK_V => 7,  // Z+V - Auto-type
                VK_BACKTICK => 8, // ` - Pause/Resume auto-type
                VK_B => 10, // Z+B - Compiler Auto-Type
                VK_1 => 11, // Z+1 - Self Destruct
                VK_R => 12, // Z+R - Reset History
                VK_T => 13, // Z+T - Toggle Theme
                VK_L => 14, // Z+L - Toggle Model
                _ => 0
            };
        }
        
        private void OnSequenceTimeout(object? state)
        {
            ResetSequence();
        }
        
        private void ResetSequence()
        {
            _zPressed = false;
            _sequenceTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }
        
        private void OnHotkeyDetected(int hotkeyId)
        {
            HotkeyPressed?.Invoke(this, new KeyPressedEventArgs(hotkeyId));
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                }
                _sequenceTimer?.Dispose();
            }
            catch { }
            
            base.Dispose(disposing);
        }
    }

    public class KeyPressedEventArgs : EventArgs
    {
        public int HotkeyId { get; set; }
        
        public KeyPressedEventArgs(int hotkeyId)
        {
            HotkeyId = hotkeyId;
        }
    }
}
