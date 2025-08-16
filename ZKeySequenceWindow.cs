using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StealthAssistant
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
        
        #endregion
        
        #region Fields
        
        private IntPtr _hookID = IntPtr.Zero;
        private readonly LowLevelKeyboardProc _proc;
        private bool _zPressed = false;
        private DateTime _lastZPress = DateTime.MinValue;
        private readonly System.Windows.Forms.Timer _sequenceTimer;
        private const int SEQUENCE_TIMEOUT_MS = 1000;
        
        #endregion
        
        #region Events
        
        public event EventHandler<KeyPressedEventArgs>? HotkeyPressed;
        
        #endregion
        
        #region Constructor
        
        public ZKeySequenceWindow()
        {
            // Make window invisible
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            this.Size = new System.Drawing.Size(0, 0);
            this.Location = new System.Drawing.Point(-5000, -5000);
            
            // Setup sequence timer
            _sequenceTimer = new System.Windows.Forms.Timer();
            _sequenceTimer.Interval = SEQUENCE_TIMEOUT_MS;
            _sequenceTimer.Tick += OnSequenceTimeout;
            
            // Setup keyboard hook
            _proc = HookCallback;
            SetHook();
            
            this.CreateHandle();
        }
        
        #endregion
        
        #region Keyboard Hook
        
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
                    HandleKeyPress((Keys)vkCode);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Hook callback error: {ex.Message}");
            }
            
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        
        private void HandleKeyPress(Keys key)
        {
            var now = DateTime.Now;
            
            if (key == Keys.Z)
            {
                _zPressed = true;
                _lastZPress = now;
                _sequenceTimer.Stop();
                _sequenceTimer.Start();
                Debug.WriteLine("Z key pressed - waiting for sequence key");
            }
            else if (_zPressed && (now - _lastZPress).TotalMilliseconds <= SEQUENCE_TIMEOUT_MS)
            {
                var hotkeyId = GetHotkeyId(key);
                if (hotkeyId > 0)
                {
                    Debug.WriteLine($"Z+{key} sequence detected");
                    OnHotkeyDetected(hotkeyId);
                    ResetSequence();
                }
            }
            else if (key != Keys.Z)
            {
                ResetSequence();
            }
        }
        
        private int GetHotkeyId(Keys key)
        {
            return key switch
            {
                Keys.S => 1, // Z+S - Normal query
                Keys.Q => 2, // Z+Q - MCQ query  
                Keys.J => 3, // Z+J - Java code
                Keys.A => 4, // Z+A - Append
                Keys.M => 5, // Z+M - Status
                _ => 0
            };
        }
        
        private void OnSequenceTimeout(object? sender, EventArgs e)
        {
            ResetSequence();
        }
        
        private void ResetSequence()
        {
            _zPressed = false;
            _sequenceTimer.Stop();
        }
        
        private void OnHotkeyDetected(int hotkeyId)
        {
            HotkeyPressed?.Invoke(this, new KeyPressedEventArgs(hotkeyId));
        }
        
        #endregion
        
        #region Form Overrides
        
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false); // Always stay hidden
        }
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW - hide from Alt+Tab
                return cp;
            }
        }
        
        #endregion
        
        #region Cleanup
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Dispose error: {ex.Message}");
                }
            }
            base.Dispose(disposing);
        }
        
        #endregion
    }
}



