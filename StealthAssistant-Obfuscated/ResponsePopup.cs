using System;
using System.Drawing;
using System.Windows.Forms;

namespace StealthAssistant.Obfuscated
{
    public class ResponsePopup : Form
    {
        private Label messageLabel = null!;
        private System.Windows.Forms.Timer autoCloseTimer = null!;

        public ResponsePopup(string message)
        {
            InitializeComponent();
            SetMessage(message);
            SetupAutoClose();
        }

        private void InitializeComponent()
        {
            this.messageLabel = new Label();
            this.SuspendLayout();

            // Form properties
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;

            // Message label
            this.messageLabel.AutoSize = false;
            this.messageLabel.Dock = DockStyle.Fill;
            this.messageLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.messageLabel.ForeColor = Color.White;
            this.messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.messageLabel.Padding = new Padding(10);

            this.Controls.Add(this.messageLabel);
            this.ResumeLayout(false);
        }

        private void SetMessage(string message)
        {
            messageLabel.Text = message;
            
            // Calculate size based on text
            using (Graphics g = this.CreateGraphics())
            {
                SizeF textSize = g.MeasureString(message, messageLabel.Font);
                int width = Math.Max(200, (int)textSize.Width + 40);
                int height = Math.Max(60, (int)textSize.Height + 40);
                
                this.Size = new Size(width, height);
            }

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