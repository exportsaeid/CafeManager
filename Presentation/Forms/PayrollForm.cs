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
    public class PayrollForm : Form
    {
        private ComboBox cmbEmployee;
        private NumericUpDown nudYear;
        private ComboBox cmbMonth;
        private DataGridView dgvPayrolls;
        private Button btnCalculate;
        private Button btnSave;
        private Button btnPay;
        private Button btnDelete;
        private Button btnRefresh;
        private Label lblTotalSalary;
        private Label lblStatus;
        private List<Payroll> _payrolls;

        public PayrollForm()
        {
            this.Text = "💰 مدیریت حقوق و دستمزد";
            this.ShowInTaskbar = false; // ← این خط را اضافه کنید
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            InitializeComponents();
            LoadEmployees();
            LoadPayrolls();
        }

        private void InitializeComponents()
        {
            int margin = 20;
            int y = 20;

            // ========== پنل انتخاب ==========
            GroupBox grpSelect = new GroupBox
            {
                Text = "📅 انتخاب ماه و سال",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblEmployee = new Label { Text = "پرسنل:", Location = new Point(grpSelect.Width - 120, 28), Size = new Size(70, 25), TextAlign = ContentAlignment.MiddleRight };
            cmbEmployee = new ComboBox
            {
                Location = new Point(grpSelect.Width - 300, 25),
                Size = new Size(170, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbEmployee.DisplayMember = "FullName";
            cmbEmployee.ValueMember = "Id";
            cmbEmployee.SelectedIndexChanged += (s, e) => LoadPayrolls();

            Label lblYear = new Label { Text = "سال:", Location = new Point(grpSelect.Width - 430, 28), Size = new Size(60, 25), TextAlign = ContentAlignment.MiddleRight };

            // محاسبه سال شمسی فعلی
            PersianCalendar pc = new PersianCalendar();
            int currentPersianYear = pc.GetYear(DateTime.Now);

            nudYear = new NumericUpDown
            {
                Location = new Point(grpSelect.Width - 510, 25),
                Size = new Size(70, 25),
                Minimum = 1390,
                Maximum = 1450,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            nudYear.Value = currentPersianYear; // تنظیم Value بعد از تعیین محدوده
            nudYear.ValueChanged += (s, e) => LoadPayrolls();

            Label lblMonth = new Label { Text = "ماه:", Location = new Point(grpSelect.Width - 640, 28), Size = new Size(60, 25), TextAlign = ContentAlignment.MiddleRight };
            cmbMonth = new ComboBox
            {
                Location = new Point(grpSelect.Width - 760, 25),
                Size = new Size(110, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            string[] months = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
            cmbMonth.Items.AddRange(months);
            cmbMonth.SelectedIndex = DateTime.Now.Month - 1;
            cmbMonth.SelectedIndexChanged += (s, e) => LoadPayrolls();

            btnRefresh = new Button
            {
                Text = "🔄 بروزرسانی",
                Location = new Point(20, 25),
                Size = new Size(100, 30),
                BackColor = Color.LightGray,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadPayrolls();

            grpSelect.Controls.Add(lblEmployee);
            grpSelect.Controls.Add(cmbEmployee);
            grpSelect.Controls.Add(lblYear);
            grpSelect.Controls.Add(nudYear);
            grpSelect.Controls.Add(lblMonth);
            grpSelect.Controls.Add(cmbMonth);
            grpSelect.Controls.Add(btnRefresh);

            this.Controls.Add(grpSelect);
            y += grpSelect.Height + 10;

            // ========== جدول حقوق ==========
            Label lblTitle = new Label
            {
                Text = "📋 لیست حقوق و دستمزد:",
                Location = new Point(margin, y),
                Size = new Size(250, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);
            y += 30;

            dgvPayrolls = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 300),
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

            dgvPayrolls.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPayrolls.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvPayrolls.Columns.Add("Id", "کد");
            dgvPayrolls.Columns.Add("Employee", "پرسنل");
            dgvPayrolls.Columns.Add("Year", "سال");
            dgvPayrolls.Columns.Add("Month", "ماه");
            dgvPayrolls.Columns.Add("BaseSalary", "حقوق پایه");
            dgvPayrolls.Columns.Add("WorkHours", "ساعت کاری");
            dgvPayrolls.Columns.Add("OvertimePay", "اضافه‌کار");
            dgvPayrolls.Columns.Add("Bonus", "پاداش");
            dgvPayrolls.Columns.Add("Deductions", "کسورات");
            dgvPayrolls.Columns.Add("NetSalary", "حقوق خالص");
            dgvPayrolls.Columns.Add("Status", "وضعیت");

            dgvPayrolls.Columns["BaseSalary"].DefaultCellStyle.Format = "N0";
            dgvPayrolls.Columns["OvertimePay"].DefaultCellStyle.Format = "N0";
            dgvPayrolls.Columns["Bonus"].DefaultCellStyle.Format = "N0";
            dgvPayrolls.Columns["Deductions"].DefaultCellStyle.Format = "N0";
            dgvPayrolls.Columns["NetSalary"].DefaultCellStyle.Format = "N0";
            dgvPayrolls.Columns["BaseSalary"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvPayrolls.Columns["OvertimePay"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvPayrolls.Columns["Bonus"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvPayrolls.Columns["Deductions"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvPayrolls.Columns["NetSalary"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvPayrolls.SelectionChanged += DgvPayrolls_SelectionChanged;

            this.Controls.Add(dgvPayrolls);
            y += dgvPayrolls.Height + 10;

            // ========== پنل پایین ==========
            Panel pnlBottom = new Panel
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTotalSalary = new Label
            {
                Text = "💰 جمع کل حقوق: 0 تومان",
                Location = new Point(0, 15),
                Size = new Size(300, 30),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            lblStatus = new Label
            {
                Location = new Point(320, 15),
                Size = new Size(300, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.Orange
            };

            btnCalculate = new Button
            {
                Text = "🧮 محاسبه حقوق",
                Location = new Point(pnlBottom.Width - 470, 8),
                Size = new Size(130, 40),
                BackColor = Color.LightYellow,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCalculate.Click += BtnCalculate_Click;

            btnPay = new Button
            {
                Text = "💰 پرداخت",
                Location = new Point(pnlBottom.Width - 330, 8),
                Size = new Size(100, 40),
                BackColor = Color.LightGreen,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnPay.Click += BtnPay_Click;

            btnDelete = new Button
            {
                Text = "🗑️ حذف",
                Location = new Point(pnlBottom.Width - 220, 8),
                Size = new Size(90, 40),
                BackColor = Color.LightCoral,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnDelete.Click += BtnDelete_Click;

            btnSave = new Button
            {
                Text = "💾 ذخیره",
                Location = new Point(pnlBottom.Width - 120, 8),
                Size = new Size(110, 40),
                BackColor = Color.LightBlue,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSave.Click += BtnSave_Click;

            pnlBottom.Controls.Add(lblTotalSalary);
            pnlBottom.Controls.Add(lblStatus);
            pnlBottom.Controls.Add(btnCalculate);
            pnlBottom.Controls.Add(btnPay);
            pnlBottom.Controls.Add(btnDelete);
            pnlBottom.Controls.Add(btnSave);

            this.Controls.Add(pnlBottom);

            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width - margin * 2;
                grpSelect.Width = w;
                dgvPayrolls.Width = w;
                pnlBottom.Width = w;

                btnCalculate.Location = new Point(pnlBottom.Width - 470, 8);
                btnPay.Location = new Point(pnlBottom.Width - 330, 8);
                btnDelete.Location = new Point(pnlBottom.Width - 220, 8);
                btnSave.Location = new Point(pnlBottom.Width - 120, 8);
            };
        }

        private void LoadEmployees()
        {
            cmbEmployee.Items.Clear();
            var employees = CafeManager.GetEmployees(true);
            foreach (var e in employees)
                cmbEmployee.Items.Add(e);

            // گزینه "همه" برای نمایش کل حقوق
            cmbEmployee.Items.Insert(0, "همه");
            cmbEmployee.SelectedIndex = 0;
        }

        private void LoadPayrolls()
        {
            dgvPayrolls.Rows.Clear();
            int year = (int)nudYear.Value;
            int month = cmbMonth.SelectedIndex + 1;

            List<Payroll> payrolls;

            if (cmbEmployee.SelectedIndex == 0 || cmbEmployee.SelectedItem == null)
            {
                // همه پرسنل
                payrolls = CafeManager.GetPayrolls(null, year, month);
            }
            else
            {
                var employee = (Employee)cmbEmployee.SelectedItem;
                payrolls = CafeManager.GetPayrolls(employee.Id, year, month);
            }

            _payrolls = payrolls;
            double total = 0;

            foreach (var p in payrolls.OrderBy(p => p.EmployeeId))
            {
                var employee = CafeManager.GetEmployeeById(p.EmployeeId);
                string empName = employee != null ? employee.FullName : "نامشخص";
                string monthName = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" }[p.Month - 1];
                string status = p.IsPaid ? "✅ پرداخت شده" : "⏳ پرداخت نشده";

                dgvPayrolls.Rows.Add(
                    p.Id,
                    empName,
                    p.Year,
                    monthName,
                    p.BaseSalary.ToString("N0"),
                    p.TotalWorkHours.ToString("F1"),
                    p.OvertimePay.ToString("N0"),
                    p.Bonus.ToString("N0"),
                    p.Deductions.ToString("N0"),
                    p.NetSalary.ToString("N0"),
                    status
                );

                if (p.IsPaid)
                    dgvPayrolls.Rows[dgvPayrolls.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGreen;
                else
                    dgvPayrolls.Rows[dgvPayrolls.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightYellow;

                total += p.NetSalary;
            }

            lblTotalSalary.Text = $"💰 جمع کل حقوق: {total:N0} تومان";

            if (payrolls.Count == 0)
                lblStatus.Text = "⚠️ هیچ حقوقی برای این ماه محاسبه نشده است.";
            else
                lblStatus.Text = $"✅ {payrolls.Count} حقوق محاسبه شده";
        }

        private void DgvPayrolls_SelectionChanged(object sender, EventArgs e)
        {
            // برای نمایش اطلاعات در صورت نیاز
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            int year = (int)nudYear.Value;
            int month = cmbMonth.SelectedIndex + 1;

            if (cmbEmployee.SelectedIndex == 0 || cmbEmployee.SelectedItem == null)
            {
                // محاسبه برای همه پرسنل
                var employees = CafeManager.GetEmployees(true);
                int count = 0;
                foreach (var emp in employees)
                {
                    var payroll = CafeManager.CalculatePayroll(emp.Id, year, month);
                    if (payroll != null)
                    {
                        CafeManager.SavePayroll(payroll);
                        count++;
                    }
                }
                MessageBox.Show($"✅ حقوق {count} پرسنل با موفقیت محاسبه شد.", "محاسبه حقوق", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var employee = (Employee)cmbEmployee.SelectedItem;
                var payroll = CafeManager.CalculatePayroll(employee.Id, year, month);
                if (payroll != null)
                {
                    CafeManager.SavePayroll(payroll);
                    MessageBox.Show($"✅ حقوق {employee.FullName} با موفقیت محاسبه شد.", "محاسبه حقوق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("خطا در محاسبه حقوق.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            LoadPayrolls();
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (dgvPayrolls.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک رکورد را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvPayrolls.SelectedRows[0].Cells["Id"].Value);
            var payroll = CafeManager.GetPayrollById(id);
            if (payroll == null) return;

            if (payroll.IsPaid)
            {
                MessageBox.Show("این حقوق قبلاً پرداخت شده است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"آیا از پرداخت مبلغ {payroll.NetSalary:N0} تومان به {CafeManager.GetEmployeeById(payroll.EmployeeId)?.FullName} اطمینان دارید؟",
                "تأیید پرداخت",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                CafeManager.MarkPayrollAsPaid(id, DateTime.Now);
                MessageBox.Show("✅ پرداخت با موفقیت ثبت شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPayrolls();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvPayrolls.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک رکورد را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvPayrolls.SelectedRows[0].Cells["Id"].Value);
            var payroll = CafeManager.GetPayrollById(id);
            if (payroll == null) return;

            if (payroll.IsPaid)
            {
                MessageBox.Show("این حقوق پرداخت شده است و قابل حذف نمی‌باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "آیا از حذف این رکورد حقوق اطمینان دارید؟",
                "تأیید حذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                CafeManager.DeletePayroll(id);
                LoadPayrolls();
                MessageBox.Show("✅ رکورد با موفقیت حذف شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            LoadPayrolls();
            MessageBox.Show("✅ اطلاعات با موفقیت ذخیره شد.", "ذخیره", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}