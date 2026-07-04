using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CafeManager.Models;

namespace CafeManager
{
    public class PaymentDialog : Form
    {
        private Invoice _invoice;
        private ComboBox cmbPaymentMethod;
        private NumericUpDown numAmount;
        private Button btnAddPayment;
        private Button btnRemovePayment;
        private Button btnConfirm;
        private Button btnCancel;
        private DataGridView dgvPayments;
        private Label lblTotalAmount;
        private Label lblPaidAmount;
        private Label lblRemainingAmount;
        private Label lblInvoiceInfo;
        private List<Payment> _payments = new List<Payment>();

        // Flag برای جلوگیری از حلقه‌ی بی‌نهایت در رویداد TextChanged
        private bool _isFormatting = false;

        public List<Payment> Payments => _payments;

        public PaymentDialog(Invoice invoice)
        {
            _invoice = invoice;
            this.Text = "💳 پرداخت ترکیبی فاکتور";
            this.Size = new Size(720, 600);
            this.MinimumSize = new Size(650, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.WhiteSmoke;

            InitializeComponents();
            LoadInvoiceInfo();
            UpdateDisplay();
        }

        private void InitializeComponents()
        {
            this.SuspendLayout();

            int margin = 20;
            int y = 20;

            lblInvoiceInfo = new Label
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 35),
                Font = new Font("Tahoma", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(lblInvoiceInfo);
            y += 45;

            GroupBox grpInfo = new GroupBox
            {
                Text = "📊 اطلاعات مالی فاکتور",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 85),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            TableLayoutPanel infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(10)
            };
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            lblTotalAmount = new Label
            {
                Text = "💰 کل فاکتور: 0 تومان",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblPaidAmount = new Label
            {
                Text = "💳 پرداخت شده: 0 تومان",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblRemainingAmount = new Label
            {
                Text = "📊 باقیمانده: 0 تومان",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.MiddleCenter
            };

            infoLayout.Controls.Add(lblTotalAmount, 0, 0);
            infoLayout.Controls.Add(lblPaidAmount, 1, 0);
            infoLayout.Controls.Add(lblRemainingAmount, 2, 0);

            grpInfo.Controls.Add(infoLayout);
            this.Controls.Add(grpInfo);
            y += 95;

            GroupBox grpAdd = new GroupBox
            {
                Text = "➕ افزودن پرداخت جدید",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 95),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblMethod = new Label
            {
                Text = "روش پرداخت:",
                Location = new Point(grpAdd.Width - 150, 35),
                Size = new Size(100, 25),
                Font = new Font("Tahoma", 10),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            cmbPaymentMethod = new ComboBox
            {
                Location = new Point(grpAdd.Width - 310, 32),
                Size = new Size(150, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbPaymentMethod.Items.AddRange(new[] { "نقدی", "کارتخوان", "انتقال", "آنلاین" });
            cmbPaymentMethod.SelectedIndex = 0;

            Label lblAmount = new Label
            {
                Text = "مبلغ:",
                Location = new Point(grpAdd.Width - 430, 35),
                Size = new Size(60, 25),
                Font = new Font("Tahoma", 10),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            numAmount = new NumericUpDown
            {
                Location = new Point(grpAdd.Width - 530, 32),
                Size = new Size(90, 28),
                Minimum = 0,
                Maximum = 100000000,
                Value = 0,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                ThousandsSeparator = true  // این خاصیت برای نمایش جداکننده در حالت عادی مفید است
            };

            // ========== اضافه کردن رویداد TextChanged برای فرمت‌دهی خودکار ==========
            numAmount.TextChanged += NumAmount_TextChanged;

            btnAddPayment = new Button
            {
                Text = "➕ افزودن",
                Location = new Point(20, 30),
                Size = new Size(100, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnAddPayment.Click += BtnAddPayment_Click;

            grpAdd.Controls.AddRange(new Control[]
            {
                lblMethod, cmbPaymentMethod, lblAmount, numAmount, btnAddPayment
            });

            grpAdd.Resize += (s, e) => {
                lblMethod.Location = new Point(grpAdd.Width - 150, 35);
                cmbPaymentMethod.Location = new Point(grpAdd.Width - 310, 32);
                lblAmount.Location = new Point(grpAdd.Width - 430, 35);
                numAmount.Location = new Point(grpAdd.Width - 530, 32);
            };

            this.Controls.Add(grpAdd);
            y += 105;

            Label lblPaymentsTitle = new Label
            {
                Text = "📋 لیست پرداخت‌های ثبت شده:",
                Location = new Point(margin, y),
                Size = new Size(250, 30),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            this.Controls.Add(lblPaymentsTitle);
            y += 35;

            dgvPayments = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 160),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 35,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvPayments.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPayments.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvPayments.Columns.Add("Method", "روش پرداخت");
            dgvPayments.Columns.Add("Amount", "مبلغ (تومان)");
            dgvPayments.Columns["Amount"].DefaultCellStyle.Format = "N0";
            dgvPayments.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            this.Controls.Add(dgvPayments);
            y += 170;

            btnRemovePayment = new Button
            {
                Text = "🗑️ حذف انتخاب شده",
                Location = new Point(margin, y),
                Size = new Size(150, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnRemovePayment.Click += BtnRemovePayment_Click;
            this.Controls.Add(btnRemovePayment);
            y += 50;

            btnConfirm = new Button
            {
                Text = "✅ تایید و تسویه",
                Location = new Point(this.ClientSize.Width - 320, y),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Button
            {
                Text = "❌ انصراف",
                Location = new Point(this.ClientSize.Width - 160, y),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { btnConfirm, btnCancel });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ========== رویداد جدید برای فرمت‌دهی خودکار هنگام تایپ ==========
        private void NumAmount_TextChanged(object sender, EventArgs e)
        {
            // جلوگیری از حلقه‌ی بی‌نهایت
            if (_isFormatting) return;

            // فقط در صورتی که کنترل فوکوس داشته باشد (کاربر در حال تایپ است)
            if (!numAmount.Focused) return;

            // حذف جداکننده‌های فعلی و فاصله‌ها
            string rawText = numAmount.Text.Replace(",", "").Replace(" ", "");

            // اگر رشته خالی است یا فقط شامل '-' است، کاری نکن
            if (string.IsNullOrEmpty(rawText))
                return;

            // تلاش برای تبدیل به عدد (با پشتیبانی از اعشار)
            if (decimal.TryParse(rawText, out decimal value))
            {
                if (value >= 0)
                {
                    // قالب‌بندی با جداکننده هزارگان
                    _isFormatting = true;
                    // برای اعداد صحیح از N0 استفاده می‌کنیم (بدون اعشار)
                    // اگر کاربر اعشار وارد کند، باید آن را حفظ کنیم، اما NumericUpDown اعشار را نمی‌پذیرد
                    // بنابراین به صورت N0 قالب‌بندی می‌کنیم
                    numAmount.Text = value.ToString("N0");
                    // قرار دادن کورسور در انتهای متن
                    numAmount.Select(numAmount.Text.Length, 0);
                    _isFormatting = false;
                }
            }
        }

        private void LoadInvoiceInfo()
        {
            lblInvoiceInfo.Text = $"فاکتور شماره: {_invoice.Id} - {_invoice.CustomerName} - میز {_invoice.TableNumber}";
            lblTotalAmount.Text = $"💰 کل فاکتور: {_invoice.TotalAmount:N0} تومان";
        }

        private void UpdateDisplay()
        {
            double paid = _payments.Sum(p => p.Amount);
            double remaining = _invoice.TotalAmount - paid;

            lblPaidAmount.Text = $"💳 پرداخت شده: {paid:N0} تومان";
            lblRemainingAmount.Text = $"📊 باقیمانده: {remaining:N0} تومان";

            dgvPayments.Rows.Clear();
            foreach (var payment in _payments)
            {
                dgvPayments.Rows.Add(payment.Method, payment.Amount.ToString("N0"));
            }

            numAmount.Maximum = (decimal)Math.Max(remaining, 0);
            btnConfirm.Enabled = _payments.Count > 0 && remaining <= 0;
            lblRemainingAmount.ForeColor = remaining <= 0 ? Color.DarkGreen : Color.DarkRed;
        }

        private void BtnAddPayment_Click(object sender, EventArgs e)
        {
            // مقدار را از طریق Value می‌خوانیم تا عدد صحیح باشد
            double amount = (double)numAmount.Value;
            if (amount <= 0)
            {
                MessageBox.Show("لطفاً مبلغ معتبر وارد کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string method = cmbPaymentMethod.SelectedItem?.ToString() ?? "نقدی";
            double remaining = _invoice.TotalAmount - _payments.Sum(p => p.Amount);

            if (amount > remaining)
            {
                MessageBox.Show($"مبلغ پرداختی ({amount:N0}) بیشتر از باقیمانده ({remaining:N0}) است.",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _payments.Add(new Payment
            {
                Method = method,
                Amount = amount,
                PaymentDate = DateTime.Now
            });

            numAmount.Value = 0;
            UpdateDisplay();

            MessageBox.Show($"✅ پرداخت {method} به مبلغ {amount:N0} تومان ثبت شد.",
                "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRemovePayment_Click(object sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک پرداخت را انتخاب کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int index = dgvPayments.SelectedRows[0].Index;
            if (index < _payments.Count)
            {
                var payment = _payments[index];
                DialogResult result = MessageBox.Show(
                    $"آیا از حذف پرداخت {payment.Method} به مبلغ {payment.Amount:N0} تومان مطمئن هستید؟",
                    "تأیید حذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    _payments.RemoveAt(index);
                    UpdateDisplay();
                    MessageBox.Show("🗑️ پرداخت با موفقیت حذف شد.",
                        "حذف", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            double remaining = _invoice.TotalAmount - _payments.Sum(p => p.Amount);
            if (remaining > 0)
            {
                MessageBox.Show($"فاکتور کامل تسویه نشده است. باقیمانده: {remaining:N0} تومان",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}