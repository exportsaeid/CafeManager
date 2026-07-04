// ============================================================
// فایل: MiscExpenseForm.cs
// فرم مدیریت هزینه‌های متفرقه با تاریخ شمسی
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;

namespace CafeManager
{
    public class MiscExpenseForm : Form
    {
        private DataGridView dgvExpenses;
        private Button btnDate;
        private Label lblDate;
        private TextBox txtDescription;
        private ComboBox cmbCategory;
        private NumericTextBox txtAmount;
        private ComboBox cmbPaymentMethod;
        private TextBox txtReceiptNumber;
        private TextBox txtNotes;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnFromDate;
        private Label lblFromDate;
        private Button btnToDate;
        private Label lblToDate;
        private Button btnFilter;
        private Label lblTotalAmount;
        private List<MiscellaneousExpense> _expenses;

        private DateTime selectedDate;
        private DateTime fromDateTime;
        private DateTime toDateTime;
        private PersianCalendar pc = new PersianCalendar();

        // ========== پرچم بارگذاری ==========
        private bool _loading = false;

        public MiscExpenseForm()
        {
            this.Text = "💰 مدیریت هزینه‌های متفرقه";
            this.ShowInTaskbar = false; // ← این خط را اضافه کنید
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            selectedDate = DateTime.Now.Date;
            fromDateTime = DateTime.Now.Date;
            toDateTime = DateTime.Now.Date;

            InitializeComponents();
            UpdateDateDisplays();
            LoadExpenses(fromDateTime, toDateTime);
            ClearFields();
        }

        private void InitializeComponents()
        {
            int margin = 20;
            int y = 20;

            // ========== پنل فیلتر ==========
            GroupBox grpFilter = new GroupBox
            {
                Text = "🔍 فیلتر تاریخ",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblFromTitle = new Label { Text = "از تاریخ:", Location = new Point(grpFilter.Width - 130, 28), Size = new Size(70, 25), TextAlign = ContentAlignment.MiddleRight };
            lblFromDate = new Label
            {
                Location = new Point(grpFilter.Width - 300, 25),
                Size = new Size(160, 25),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnFromDate = new Button
            {
                Text = "📅 انتخاب",
                Location = new Point(grpFilter.Width - 360, 23),
                Size = new Size(55, 28),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnFromDate.Click += (s, e) => ShowPersianDateTimePicker(true);

            Label lblToTitle = new Label { Text = "تا تاریخ:", Location = new Point(grpFilter.Width - 500, 28), Size = new Size(70, 25), TextAlign = ContentAlignment.MiddleRight };
            lblToDate = new Label
            {
                Location = new Point(grpFilter.Width - 670, 25),
                Size = new Size(160, 25),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnToDate = new Button
            {
                Text = "📅 انتخاب",
                Location = new Point(grpFilter.Width - 730, 23),
                Size = new Size(55, 28),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnToDate.Click += (s, e) => ShowPersianDateTimePicker(false);

            btnFilter = new Button
            {
                Text = "🔍 نمایش",
                Location = new Point(grpFilter.Width - 810, 23),
                Size = new Size(70, 30),
                BackColor = Color.LightBlue,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnFilter.Click += (s, e) => LoadExpenses(fromDateTime, toDateTime);

            btnRefresh = new Button
            {
                Text = "🔄 امروز",
                Location = new Point(20, 23),
                Size = new Size(80, 30),
                BackColor = Color.LightGray,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) =>
            {
                fromDateTime = DateTime.Now.Date;
                toDateTime = DateTime.Now.Date;
                UpdateDateDisplays();
                LoadExpenses(fromDateTime, toDateTime);
            };

            grpFilter.Controls.Add(lblFromTitle);
            grpFilter.Controls.Add(lblFromDate);
            grpFilter.Controls.Add(btnFromDate);
            grpFilter.Controls.Add(lblToTitle);
            grpFilter.Controls.Add(lblToDate);
            grpFilter.Controls.Add(btnToDate);
            grpFilter.Controls.Add(btnFilter);
            grpFilter.Controls.Add(btnRefresh);

            this.Controls.Add(grpFilter);
            y += grpFilter.Height + 10;

            // ========== جدول هزینه‌ها ==========
            Label lblTitle = new Label
            {
                Text = "📋 لیست هزینه‌های متفرقه:",
                Location = new Point(margin, y),
                Size = new Size(250, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);
            y += 30;

            dgvExpenses = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 220),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvExpenses.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvExpenses.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvExpenses.Columns.Add("Id", "کد");
            dgvExpenses.Columns.Add("Date", "تاریخ شمسی");
            dgvExpenses.Columns.Add("Description", "توضیح");
            dgvExpenses.Columns.Add("Category", "دسته‌بندی");
            dgvExpenses.Columns.Add("Amount", "مبلغ");
            dgvExpenses.Columns.Add("PaymentMethod", "روش پرداخت");
            dgvExpenses.Columns.Add("ReceiptNumber", "شماره رسید");
            dgvExpenses.Columns.Add("Status", "وضعیت");

            dgvExpenses.Columns["Amount"].DefaultCellStyle.Format = "N0";
            dgvExpenses.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvExpenses.SelectionChanged += DgvExpenses_SelectionChanged;

            this.Controls.Add(dgvExpenses);
            y += dgvExpenses.Height + 10;

            // ========== پنل ورودی ==========
            GroupBox grpInput = new GroupBox
            {
                Text = "📝 ثبت هزینه جدید",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            TableLayoutPanel tlpInput = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(10)
            };
            tlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            tlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            tlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            tlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            tlpInput.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpInput.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpInput.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // سطر اول: تاریخ و توضیح
            tlpInput.Controls.Add(new Label { Text = "تاریخ:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) }, 0, 0);
            Panel datePanel = new Panel { Dock = DockStyle.Fill, Height = 30 };
            lblDate = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            btnDate = new Button
            {
                Text = "📅",
                Dock = DockStyle.Right,
                Width = 35,
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };
            btnDate.Click += (s, e) => ShowPersianDateTimePickerForDate();
            datePanel.Controls.Add(lblDate);
            datePanel.Controls.Add(btnDate);
            lblDate.Dock = DockStyle.Fill;
            btnDate.BringToFront();
            tlpInput.Controls.Add(datePanel, 1, 0);

            tlpInput.Controls.Add(new Label { Text = "توضیح:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) }, 2, 0);
            txtDescription = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10), Height = 30 };
            tlpInput.Controls.Add(txtDescription, 3, 0);

            // سطر دوم: دسته‌بندی و مبلغ
            tlpInput.Controls.Add(new Label { Text = "دسته‌بندی:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) }, 0, 1);
            cmbCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                Height = 30
            };
            cmbCategory.Items.AddRange(new object[] { "آب و برق", "اجاره", "تعمیرات", "تبلیغات", "لوازم مصرفی", "حقوق", "متفرقه" });
            tlpInput.Controls.Add(cmbCategory, 1, 1);

            tlpInput.Controls.Add(new Label { Text = "مبلغ (تومان):", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) }, 2, 1);
            txtAmount = new NumericTextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10), Height = 30, Text = "" };
            tlpInput.Controls.Add(txtAmount, 3, 1);

            // سطر سوم: روش پرداخت و شماره رسید
            tlpInput.Controls.Add(new Label { Text = "روش پرداخت:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) }, 0, 2);
            cmbPaymentMethod = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                Height = 30
            };
            cmbPaymentMethod.Items.AddRange(new object[] { "نقدی", "کارت", "انتقال", "آنلاین" });
            tlpInput.Controls.Add(cmbPaymentMethod, 1, 2);

            tlpInput.Controls.Add(new Label { Text = "شماره رسید:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) }, 2, 2);
            txtReceiptNumber = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10), Height = 30 };
            tlpInput.Controls.Add(txtReceiptNumber, 3, 2);

            grpInput.Controls.Add(tlpInput);
            this.Controls.Add(grpInput);
            y += grpInput.Height + 10;

            // ========== پنل پایین ==========
            TableLayoutPanel pnlBottom = new TableLayoutPanel
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 55),
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(0, 5, 0, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.WhiteSmoke
            };
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            pnlBottom.Controls.Add(new Label { Text = "یادداشت:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) }, 0, 0);
            txtNotes = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10), Margin = new Padding(5, 3, 5, 3) };
            pnlBottom.Controls.Add(txtNotes, 1, 0);

            lblTotalAmount = new Label
            {
                Text = "💰 جمع کل: 0 تومان",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(5, 0, 5, 0)
            };
            pnlBottom.Controls.Add(lblTotalAmount, 2, 0);

            btnDelete = new Button
            {
                Text = "🗑️ حذف",
                Dock = DockStyle.Fill,
                BackColor = Color.LightCoral,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3, 0, 3, 0)
            };
            btnDelete.Click += BtnDelete_Click;
            pnlBottom.Controls.Add(btnDelete, 3, 0);

            btnUpdate = new Button
            {
                Text = "💾 ویرایش",
                Dock = DockStyle.Fill,
                BackColor = Color.LightBlue,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3, 0, 3, 0)
            };
            btnUpdate.Click += BtnUpdate_Click;
            pnlBottom.Controls.Add(btnUpdate, 4, 0);

            btnAdd = new Button
            {
                Text = "➕ افزودن",
                Dock = DockStyle.Fill,
                BackColor = Color.LightGreen,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3, 0, 0, 0)
            };
            btnAdd.Click += BtnAdd_Click;
            pnlBottom.Controls.Add(btnAdd, 5, 0);

            this.Controls.Add(pnlBottom);

            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width - margin * 2;
                grpFilter.Width = w;
                dgvExpenses.Width = w;
                grpInput.Width = w;
                pnlBottom.Width = w;
            };
        }

        // ==================== متدهای تاریخ شمسی ====================
        private void ShowPersianDateTimePicker(bool isFromDate)
        {
            using (PersianDatePopup popup = new PersianDatePopup(isFromDate ? fromDateTime : toDateTime))
            {
                if (popup.ShowDialog(this) == DialogResult.OK)
                {
                    if (isFromDate)
                        fromDateTime = popup.SelectedDateTime.Date;
                    else
                        toDateTime = popup.SelectedDateTime.Date;
                    UpdateDateDisplays();
                }
            }
        }

        private void ShowPersianDateTimePickerForDate()
        {
            using (PersianDatePopup popup = new PersianDatePopup(selectedDate))
            {
                if (popup.ShowDialog(this) == DialogResult.OK)
                {
                    selectedDate = popup.SelectedDateTime.Date;
                    UpdateDateDisplays();
                }
            }
        }

        private void UpdateDateDisplays()
        {
            lblFromDate.Text = ConvertToPersianDate(fromDateTime);
            lblToDate.Text = ConvertToPersianDate(toDateTime);
            lblDate.Text = ConvertToPersianDate(selectedDate);
        }

        private string ConvertToPersianDate(DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:00}/{day:00}";
        }

        // ==================== متدهای اصلی ====================
        private void LoadExpenses(DateTime from, DateTime to)
        {
            _loading = true;

            dgvExpenses.Rows.Clear();
            _expenses = CafeManager.GetMiscExpenses(from, to.AddDays(1).AddSeconds(-1));

            double total = 0;
            foreach (var e in _expenses.OrderByDescending(e => e.ExpenseDate))
            {
                string status = e.IsConfirmed ? "✅ تایید شده" : "⏳ تایید نشده";
                dgvExpenses.Rows.Add(
                    e.Id,
                    ConvertToPersianDate(e.ExpenseDate),
                    e.Description,
                    e.Category,
                    e.Amount.ToString("N0"),
                    e.PaymentMethod,
                    e.ReceiptNumber,
                    status
                );
                if (!e.IsConfirmed)
                    dgvExpenses.Rows[dgvExpenses.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightYellow;
                total += e.Amount;
            }
            lblTotalAmount.Text = $"💰 جمع کل: {total:N0} تومان";

            // ========== راه‌حل نهایی: پاک‌سازی انتخاب و تنظیم _loading = false داخل BeginInvoke ==========
            this.BeginInvoke(new Action(() =>
            {
                dgvExpenses.ClearSelection();
                dgvExpenses.CurrentCell = null;
                _loading = false; // بعد از پاک‌سازی، پرچم را false کن
            }));
            // _loading را اینجا false نمی‌کنیم، بلکه داخل BeginInvoke false می‌شود
        }

        private void DgvExpenses_SelectionChanged(object sender, EventArgs e)
        {
            // اگر در حال بارگذاری یا سلولی انتخاب نشده است، کاری نکن
            if (_loading) return;
            if (dgvExpenses.CurrentCell == null) return;
            if (dgvExpenses.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvExpenses.SelectedRows[0].Cells["Id"].Value);
            var expense = CafeManager.GetMiscExpenseById(id);
            if (expense == null) return;

            selectedDate = expense.ExpenseDate.Date;
            txtDescription.Text = expense.Description;
            cmbCategory.SelectedItem = expense.Category;
            txtAmount.Value = (int)expense.Amount;
            cmbPaymentMethod.SelectedItem = expense.PaymentMethod;
            txtReceiptNumber.Text = expense.ReceiptNumber;
            txtNotes.Text = expense.Notes;
            UpdateDateDisplays();
        }

        private void ClearFields()
        {
            selectedDate = DateTime.Now.Date;
            txtDescription.Text = "";
            cmbCategory.SelectedIndex = -1;
            txtAmount.Text = "";
            cmbPaymentMethod.SelectedIndex = -1;
            txtReceiptNumber.Text = "";
            txtNotes.Text = "";
            UpdateDateDisplays();
            dgvExpenses.ClearSelection();
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("لطفاً توضیح هزینه را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescription.Focus();
                return false;
            }
            if (txtAmount.Value <= 0)
            {
                MessageBox.Show("مبلغ باید بزرگتر از صفر باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAmount.Focus();
                return false;
            }
            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("لطفاً یک دسته‌بندی انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory.Focus();
                return false;
            }
            if (cmbPaymentMethod.SelectedIndex == -1)
            {
                MessageBox.Show("لطفاً روش پرداخت را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbPaymentMethod.Focus();
                return false;
            }
            return true;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            var expense = new MiscellaneousExpense
            {
                ExpenseDate = selectedDate,
                Description = txtDescription.Text.Trim(),
                Category = cmbCategory.SelectedItem?.ToString() ?? "متفرقه",
                Amount = txtAmount.Value,
                PaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "نقدی",
                ReceiptNumber = txtReceiptNumber.Text.Trim(),
                Notes = txtNotes.Text.Trim(),
                IsConfirmed = true
            };

            CafeManager.AddMiscExpense(expense);
            LoadExpenses(fromDateTime, toDateTime);
            ClearFields();
            MessageBox.Show("✅ هزینه با موفقیت ثبت شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvExpenses.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک رکورد را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateFields()) return;

            int id = Convert.ToInt32(dgvExpenses.SelectedRows[0].Cells["Id"].Value);
            var existing = CafeManager.GetMiscExpenseById(id);
            if (existing == null) return;

            existing.ExpenseDate = selectedDate;
            existing.Description = txtDescription.Text.Trim();
            existing.Category = cmbCategory.SelectedItem?.ToString() ?? "متفرقه";
            existing.Amount = txtAmount.Value;
            existing.PaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "نقدی";
            existing.ReceiptNumber = txtReceiptNumber.Text.Trim();
            existing.Notes = txtNotes.Text.Trim();

            CafeManager.UpdateMiscExpense(existing);
            LoadExpenses(fromDateTime, toDateTime);
            ClearFields();
            MessageBox.Show("✅ هزینه با موفقیت ویرایش شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvExpenses.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک رکورد را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvExpenses.SelectedRows[0].Cells["Id"].Value);
            var expense = CafeManager.GetMiscExpenseById(id);

            DialogResult result = MessageBox.Show(
                $"آیا از حذف هزینه '{expense?.Description}' اطمینان دارید؟",
                "تأیید حذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                CafeManager.DeleteMiscExpense(id);
                LoadExpenses(fromDateTime, toDateTime);
                ClearFields();
                MessageBox.Show("✅ هزینه با موفقیت حذف شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}