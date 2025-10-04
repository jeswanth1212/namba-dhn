using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StealthAssistant
{
    public class StealthClipboardViewer : Form
    {
        #region Windows API
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOPMOST = 0x00000008;
        
        #endregion
        
        #region Fields
        
        private RichTextBox _textBox = null!;
        private bool _isVisible = false;
        
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
            // Form settings
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
            this.BackColor = Color.FromArgb(240, 240, 240);
            
            // Text box for content
            _textBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10F),
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                WordWrap = true
            };
            
            this.Controls.Add(_textBox);
            
            // Initially hidden
            this.Visible = false;
        }
        
        private void ApplyStealthSettings()
        {
            // Apply stealth window styles after handle is created
            this.HandleCreated += (s, e) =>
            {
                try
                {
                    int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
                    exStyle |= WS_EX_TOOLWINDOW; // Hide from taskbar
                    exStyle |= WS_EX_NOACTIVATE; // Don't steal focus
                    exStyle |= WS_EX_TOPMOST; // Always on top
                    SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);
                }
                catch
                {
                    // Silently handle any errors
                }
            };
        }
        
        #endregion
        
        #region Public Methods
        
        public void ToggleVisibility(string? clipboardContent = null)
        {
            _isVisible = !_isVisible;
            
            if (_isVisible)
            {
                // Show the viewer
                if (!string.IsNullOrEmpty(clipboardContent))
                {
                    _textBox.Text = clipboardContent;
                    _textBox.Select(0, 0); // Reset scroll to top
                }
                this.Show();
                this.BringToFront();
            }
            else
            {
                // Hide the viewer
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
            // Don't auto-hide when losing focus
            base.OnDeactivate(e);
        }
        
        #endregion
    }
}

