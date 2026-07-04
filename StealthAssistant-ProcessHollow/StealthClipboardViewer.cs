using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StealthAssistant.ProcessHollow
{
    public class StealthClipboardViewer : Form
    {
        #region Windows API
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll")]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);
        
        [DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
        
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int SB_VERT = 1;
        private const uint WM_VSCROLL = 0x0115;
        private const int SB_THUMBPOSITION = 4;
        
        private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
        
        #endregion
        
        #region Fields
        
        private RichTextBox _textBox = null!;
        private bool _isVisible = false;
        private int _savedScrollPosition = 0;
        private string _lastContent = "";
        private bool _isDarkTheme = false; // Light theme for clipboard viewer by default
        
        #endregion
        
        #region Constructor
        
        public StealthClipboardViewer()
        {
            InitializeComponents();
            ApplyStealthSettings();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2,
                (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2
            );
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Text = "Clipboard Viewer";
            
            ApplyTheme();
            
            _textBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10F),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                WordWrap = true
            };
            
            ApplyTextBoxTheme();
            
            this.Controls.Add(_textBox);
            this.Visible = false;
        }
        
        private void ApplyTheme()
        {
            // Light theme for clipboard viewer
            this.BackColor = Color.FromArgb(240, 240, 240);
        }
        
        private void ApplyTextBoxTheme()
        {
            // Light theme for clipboard viewer
            _textBox.BackColor = Color.White;
            _textBox.ForeColor = Color.Black;
        }
        
        public void UpdateTheme(bool isDarkTheme)
        {
            _isDarkTheme = isDarkTheme;
            
            if (_isDarkTheme)
            {
                // Dark theme
                this.BackColor = Color.FromArgb(30, 30, 30);
                _textBox.BackColor = Color.FromArgb(45, 45, 48);
                _textBox.ForeColor = Color.White;
            }
            else
            {
                // Light theme
                this.BackColor = Color.FromArgb(240, 240, 240);
                _textBox.BackColor = Color.White;
                _textBox.ForeColor = Color.Black;
            }
            
            this.Refresh();
        }
        
        private void ApplyStealthSettings()
        {
            this.HandleCreated += (s, e) =>
            {
                try
                {
                    int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
                    exStyle |= WS_EX_TOOLWINDOW;
                    exStyle |= WS_EX_NOACTIVATE;
                    exStyle |= WS_EX_TOPMOST;
                    SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);
                    SetWindowDisplayAffinity(this.Handle, WDA_EXCLUDEFROMCAPTURE);
                }
                catch { }
            };
        }
        
        #endregion
        
        #region Public Methods
        
        public void ToggleVisibility(string? clipboardContent = null)
        {
            _isVisible = !_isVisible;
            
            if (_isVisible)
            {
                if (!string.IsNullOrEmpty(clipboardContent))
                {
                    bool contentChanged = clipboardContent != _lastContent;
                    _lastContent = clipboardContent;
                    
                    _textBox.Text = clipboardContent;
                    
                    if (contentChanged)
                    {
                        _textBox.Select(0, 0);
                        _textBox.ScrollToCaret();
                        _savedScrollPosition = 0;
                    }
                    else
                    {
                        try
                        {
                            if (_savedScrollPosition > 0)
                            {
                                SetScrollPos(_textBox.Handle, SB_VERT, _savedScrollPosition, true);
                                PostMessage(_textBox.Handle, WM_VSCROLL, (IntPtr)(SB_THUMBPOSITION | (_savedScrollPosition << 16)), IntPtr.Zero);
                            }
                        }
                        catch
                        {
                            _textBox.Select(0, 0);
                            _textBox.ScrollToCaret();
                        }
                    }
                }
                this.Show();
                this.BringToFront();
            }
            else
            {
                try
                {
                    _savedScrollPosition = GetScrollPos(_textBox.Handle, SB_VERT);
                }
                catch
                {
                    _savedScrollPosition = 0;
                }
                this.Hide();
            }
        }
        
        public void ScrollUp()
        {
            if (_isVisible)
            {
                SendKeys.Send("{PGUP}");
            }
        }
        
        public void ScrollDown()
        {
            if (_isVisible)
            {
                SendKeys.Send("{PGDN}");
            }
        }
        
        public bool IsCurrentlyVisible()
        {
            return _isVisible;
        }
        
        #endregion
        
        #region Form Overrides
        
        protected override bool ShowWithoutActivation => true;
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST;
                return cp;
            }
        }
        
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
        }
        
        #endregion
    }
}
