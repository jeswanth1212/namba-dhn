using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StealthAssistant.Obfuscated
{
    public class ZKeySequenceWindow : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_NONE = 0x0000;
        private const uint VK_Z = 0x5A;
        private const uint VK_M = 0x4D;
        private const uint VK_W = 0x57;
        private const uint VK_1 = 0x31;

        public event EventHandler<KeyPressedEventArgs>? HotkeyPressed;

        public ZKeySequenceWindow()
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            
            // Register hotkeys
            RegisterHotKey(this.Handle, 1, MOD_NONE, VK_M); // Z+M
            RegisterHotKey(this.Handle, 2, MOD_NONE, VK_W); // Z+W  
            RegisterHotKey(this.Handle, 11, MOD_NONE, VK_1); // Z+1
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                HotkeyPressed?.Invoke(this, new KeyPressedEventArgs(hotkeyId));
            }
            
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            UnregisterHotKey(this.Handle, 1);
            UnregisterHotKey(this.Handle, 2);
            UnregisterHotKey(this.Handle, 11);
            base.Dispose(disposing);
        }
    }
}