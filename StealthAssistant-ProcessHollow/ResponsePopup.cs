using System;
using System.Drawing;
using System.Windows.Forms;

namespace StealthAssistant.ProcessHollow
{
    public class ResponsePopup : Form
    {
        private Label messageLabel = null!;
        private System.Windows.Forms.Timer autoCloseTimer = null!;
        private bool _isDarkTheme;

        public ResponsePopup(string message, bool isDarkTheme = true)
        {
            _isDarkTheme = isDarkTheme;
            InitializeComponent();
            SetMessage(message);
            SetupAutoClose();
        }

        private void InitializeComponent()
        {
            this.messageLabel = new Label();
            this.SuspendLayout();

            // Theme colors
            Color bgColor = _isDarkTheme ? Color.FromArgb(45, 45, 48) : Color.FromArgb(240, 240, 240);
            Color fgColor = _isDarkTheme ? Color.White : Color.Black;

            // Form properties
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = bgColor;
            this.ForeColor = fgColor;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;

            // Message label
            this.messageLabel.AutoSize = false;
            this.messageLabel.Dock = DockStyle.Fill;
            this.messageLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.messageLabel.ForeColor = fgColor;
            this.messageLabel.BackColor = bgColor;
            this.messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.messageLabel.Padding = new Padding(10);

            this.Controls.Add(this.messageLabel);
            this.ResumeLayout(false);
        }

        private void SetMessage(string message)
        {
            // Truncate message to first 100 characters for fixed-size popup
            string displayMessage = message.Length > 100 ? message.Substring(0, 100) + "..." : message;
            messageLabel.Text = displayMessage;
            
            // Fixed size for popup
            int width = 300;
            int height = 80;
            
            this.Size = new Size(width, height);

            // Position in bottom-right corner
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                workingArea.Right - this.Width - 20,
                workingArea.Bottom - this.Height - 20
            );
        }

        private void SetupAutoClose()
        {
            autoCloseTimer = new System.Windows.Forms.Timer();
            autoCloseTimer.Interval = 3000; // 3 seconds
            autoCloseTimer.Tick += (s, e) =>
            {
                autoCloseTimer.Stop();
                this.Close();
            };
            autoCloseTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                autoCloseTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}