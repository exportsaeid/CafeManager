using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using CafeManager;

namespace CafeManager
{
    public class LoginForm : Form
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;
        private PictureBox picLogo;
        private Label lblTitle;
        private Label lblSubTitle;
        private Label lblVersion;   // ← اضافه شد

        public LoginForm()
        {
            // تنظیمات فرم
            this.Text = "ورود به صندوق کافه";
            this.Size = new Size(420, 530);  // ← افزایش ارتفاع برای نمایش نسخه
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.White;
            this.Padding = new Padding(10);
            this.KeyPreview = true;

            // افکت محو شدن هنگام باز شدن
            this.Opacity = 0;
            Timer fadeTimer = new Timer();
            fadeTimer.Interval = 20;
            fadeTimer.Tick += (s, e) =>
            {
                if (this.Opacity < 1)
                    this.Opacity += 0.05;
                else
                    fadeTimer.Stop();
            };
            fadeTimer.Start();

            // ===== لوگو =====
            picLogo = new PictureBox
            {
                Location = new Point(110, 20),
                Size = new Size(180, 160),
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            picLogo.Image = CreateCafeLogo();

            // ===== عنوان =====
            lblTitle = new Label
            {
                Text = "☕ کافه گلستان",
                Location = new Point(20, 190),
                Size = new Size(370, 40),
                Font = new Font("B Mitra", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 40, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            lblSubTitle = new Label
            {
                Text = "نرم‌افزار مدیریت صندوق",
                Location = new Point(20, 225),
                Size = new Size(370, 30),
                Font = new Font("Tahoma", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 80, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // ===== نمایش نسخه و سال =====
            string version = GetVersion();
            lblVersion = new Label
            {
                Text = $"نسخه {version} | © {DateTime.Now.Year} کافه گلستان",
                Location = new Point(20, 258),
                Size = new Size(370, 20),
                Font = new Font("Tahoma", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 120, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // ===== نام کاربری =====
            Label lblUser = new Label
            {
                Text = "👤 نام کاربری:",
                Location = new Point(260, 295),
                Size = new Size(120, 25),
                ForeColor = Color.FromArgb(60, 40, 20),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };

            txtUser = new TextBox
            {
                Location = new Point(40, 292),
                Size = new Size(210, 30),
                Font = new Font("Tahoma", 11),
                BackColor = Color.FromArgb(250, 245, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "admin"
            };
            txtUser.Enter += (s, e) => txtUser.SelectAll();

            // ===== کلمه عبور =====
            Label lblPass = new Label
            {
                Text = "🔒 کلمه عبور:",
                Location = new Point(260, 340),
                Size = new Size(120, 25),
                ForeColor = Color.FromArgb(60, 40, 20),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };

            txtPass = new TextBox
            {
                Location = new Point(40, 337),
                Size = new Size(210, 30),
                Font = new Font("Tahoma", 11),
                BackColor = Color.FromArgb(250, 245, 240),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
                Text = "123"
            };
            txtPass.Enter += (s, e) => txtPass.SelectAll();

            // ===== دکمه ورود =====
            btnLogin = new Button
            {
                Text = "🚀 ورود به برنامه",
                Location = new Point(40, 390),
                Size = new Size(330, 45),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(139, 69, 19),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 90, 30);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 50, 10);
            btnLogin.Click += BtnLogin_Click;

            // ===== رویداد Enter =====
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    BtnLogin_Click(null, null);
            };

            // ===== اضافه کردن کنترل‌ها =====
            this.Controls.AddRange(new Control[]
            {
                picLogo, lblTitle, lblSubTitle, lblVersion,
                lblUser, txtUser, lblPass, txtPass, btnLogin
            });

            this.Paint += LoginForm_Paint;
        }

        private void LoginForm_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = this.ClientRectangle;
            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(255, 248, 240),
                Color.FromArgb(240, 225, 210),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            using (Pen pen = new Pen(Color.FromArgb(180, 120, 70), 2))
            {
                e.Graphics.DrawLine(pen, 50, 280, 350, 280);
            }
        }

        // ================================================================
        // ✅ دریافت شماره نسخه از Assembly
        // ================================================================
        private string GetVersion()
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version != null)
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}";
                }
                return "1.0.0";
            }
            catch
            {
                return "1.0.0";
            }
        }

        private Bitmap CreateCafeLogo()
        {
            Bitmap bmp = new Bitmap(180, 160);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(10, 10, 140, 140),
                    Color.FromArgb(180, 120, 70),
                    Color.FromArgb(100, 60, 30),
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillEllipse(brush, 20, 10, 140, 140);
                }

                using (Pen pen = new Pen(Color.White, 5))
                {
                    g.DrawArc(pen, 50, 50, 70, 80, 0, 180);
                    g.DrawLine(pen, 50, 90, 50, 130);
                    g.DrawLine(pen, 120, 90, 120, 130);
                    g.DrawArc(pen, 50, 120, 70, 15, 0, 180);
                    g.DrawArc(pen, 115, 70, 30, 40, -90, 180);
                }

                using (Pen pen = new Pen(Color.White, 3))
                {
                    pen.DashStyle = DashStyle.Dot;
                    g.DrawCurve(pen, new Point[] {
                        new Point(70, 45), new Point(60, 30), new Point(75, 15)
                    });
                    g.DrawCurve(pen, new Point[] {
                        new Point(90, 40), new Point(85, 25), new Point(95, 10)
                    });
                    g.DrawCurve(pen, new Point[] {
                        new Point(110, 45), new Point(115, 30), new Point(105, 15)
                    });
                }

                using (Font font = new Font("Segoe UI", 28))
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    g.DrawString("☕", font, brush, 65, 55);
                }
            }
            return bmp;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (CafeManager.Login(txtUser.Text, txtPass.Text))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                Timer errorTimer = new Timer();
                int shakeCount = 0;
                int originalLeft = this.Left;
                errorTimer.Interval = 30;
                errorTimer.Tick += (s, ev) =>
                {
                    if (shakeCount < 10)
                    {
                        this.Left = originalLeft + (shakeCount % 2 == 0 ? -12 : 12);
                        shakeCount++;
                    }
                    else
                    {
                        this.Left = originalLeft;
                        errorTimer.Stop();
                        txtUser.Focus();
                        txtUser.SelectAll();
                        MessageBox.Show("❌ نام کاربری یا کلمه عبور اشتباه است!",
                            "خطای امنیتی",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                };
                errorTimer.Start();
            }
        }
    }
}