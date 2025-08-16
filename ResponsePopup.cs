using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StealthAssistant
{
    public class ResponsePopup : Form
    {
        #region Windows API
        
        [DllImport("user32.dll")]
        private static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);
        
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WS_EX_TOPMOST = 0x8;
        
        private const int AW_SLIDE = 0x40000;
        private const int AW_HOR_POSITIVE = 0x1;
        private const int AW_HOR_NEGATIVE = 0x2;
        private const int AW_VER_POSITIVE = 0x4;
        private const int AW_VER_NEGATIVE = 0x8;
        private const int AW_HIDE = 0x10000;
        
        #endregion
        
        #region Fields
        
        private readonly System.Windows.Forms.Timer _autoCloseTimer;
        private Label _messageLabel = null!;
        private readonly string _message;
        private const int POPUP_DURATION = 2000; // 2 seconds
        
        #endregion
        
        #region Constructor
        
        public ResponsePopup(string message)
        {
            _message = message;
            
            InitializeComponents();
            SetupWindowProperties();
            PositionWindow();
            
            _autoCloseTimer = new System.Windows.Forms.Timer();
            _autoCloseTimer.Interval = POPUP_DURATION;
            _autoCloseTimer.Tick += OnAutoClose;
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            _messageLabel = new Label
            {
                Text = _message,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Dock = DockStyle.Fill
            };
            
            this.Controls.Add(_messageLabel);
        }
        
        private void SetupWindowProperties()
        {
            // Basic form properties
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(45, 45, 48); // Dark background
            this.Size = CalculateOptimalSize();
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Normal;
            
            // Rounded corners effect
            this.Region = new Region(GetRoundedRectangle(this.ClientRectangle, 8));
        }
        
        private Size CalculateOptimalSize()
        {
            using (var graphics = this.CreateGraphics())
            {
                var font = new Font("Segoe UI", 9F, FontStyle.Regular);
                var textSize = graphics.MeasureString(_message, font);
                
                var width = Math.Max(200, (int)textSize.Width + 40);
                var height = Math.Max(50, (int)textSize.Height + 20);
                
                // Limit maximum size
                width = Math.Min(width, 400);
                height = Math.Min(height, 100);
                
                return new Size(width, height);
            }
        }
        
        private void PositionWindow()
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            var x = screen.Right - this.Width - 20; // 20px from right edge
            var y = screen.Bottom - this.Height - 20; // 20px from bottom edge
            
            this.Location = new Point(x, y);
        }
        
        #endregion
        
        #region Visual Effects
        
        private GraphicsPath GetRoundedRectangle(Rectangle rectangle, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            
            // Top-left corner
            path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
            
            // Top-right corner
            path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
            
            // Bottom-right corner
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
            
            // Bottom-left corner
            path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
            
            path.CloseFigure();
            return path;
        }
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                
                // Make it a tool window (doesn't appear in Alt+Tab)
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                
                // Make it topmost
                cp.ExStyle |= WS_EX_TOPMOST;
                
                return cp;
            }
        }
        
        #endregion
        
        #region Show/Hide Animation
        
        public new void Show()
        {
            base.Show();
            
            // Slide in animation from bottom-right
            AnimateWindow(this.Handle, 300, AW_SLIDE | AW_VER_NEGATIVE);
            
            // Start auto-close timer
            _autoCloseTimer.Start();
        }
        
        private void OnAutoClose(object? sender, EventArgs e)
        {
            _autoCloseTimer.Stop();
            CloseWithAnimation();
        }
        
        private void CloseWithAnimation()
        {
            try
            {
                // Slide out animation
                AnimateWindow(this.Handle, 300, AW_SLIDE | AW_VER_POSITIVE | AW_HIDE);
                this.Close();
            }
            catch
            {
                // Fallback to immediate close
                this.Close();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            
            // Close on click
            _autoCloseTimer.Stop();
            CloseWithAnimation();
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw subtle border
            using (var pen = new Pen(Color.FromArgb(100, Color.Gray), 1))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, GetRoundedRectangle(this.ClientRectangle, 8));
            }
        }
        
        #endregion
        
        #region Cleanup
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoCloseTimer?.Dispose();
                _messageLabel?.Dispose();
            }
            base.Dispose(disposing);
        }
        
        #endregion
    }
}
