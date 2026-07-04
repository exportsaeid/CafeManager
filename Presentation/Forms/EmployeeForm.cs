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
    public class EmployeeForm : Form
    {
        private DataGridView dgvEmployees;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtNationalCode;
        private TextBox txtPhone;
        private TextBox txtPosition;
        private Button btnHireDate;
        private Label lblHireDate;
        private NumericTextBox txtBaseSalary;
        private NumericTextBox txtHourlyRate;
        private CheckBox chkIsActive;
        private TextBox txtNotes;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnSearch;
        private Button btnRefresh;
        private ComboBox cmbSearchType;
        private List<Employee> _employees;

        // ========== متغیرهای تاریخ شمسی ==========
        private DateTime selectedHireDate;
        private PersianCalendar pc = new PersianCalendar();
        private bool _loading = false;

        public EmployeeForm()
        {
            this.Text = "👤 مدیریت پرسنل کافه";
            this.ShowInTaskbar = false; // ← این خط را اضافه کنید
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            selectedHireDate = DateTime.Now.Date;

            InitializeComponents();
            UpdateHireDateDisplay();
            RefreshGrid();
            ClearFields();
        }

        private void InitializeComponents()
        {
            int margin = 20;
            int y = 20;

            // ========== پنل جستجو ==========
            GroupBox grpSearch = new GroupBox
            {
                Text = "🔍 جستجوی پرسنل",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtSearch = new TextBox
            {
                Location = new Point(300, 28),
                Size = new Size(200, 25),
                Font = new Font("Tahoma", 10)
            };

            cmbSearchType = new ComboBox
            {
                Location = new Point(510, 28),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10)
            };
            cmbSearchType.Items.AddRange(new[] { "نام", "کد ملی", "سمت" });
            cmbSearchType.SelectedIndex = 0;

            btnSearch = new Button
            {
                Text = "🔍 جستجو",
                Location = new Point(640, 26),
                Size = new Size(90, 30),
                BackColor = Color.LightBlue,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSearch.Click += BtnSearch_Click;

            btnRefresh = new Button
            {
                Text = "🔄 نمایش همه",
                Location = new Point(740, 26),
                Size = new Size(100, 30),
                BackColor = Color.LightGray,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); RefreshGrid(); };

            grpSearch.Controls.Add(txtSearch);
            grpSearch.Controls.Add(cmbSearchType);
            grpSearch.Controls.Add(btnSearch);
            grpSearch.Controls.Add(btnRefresh);

            this.Controls.Add(grpSearch);
            y += grpSearch.Height + 10;

            // ========== جدول پرسنل ==========
            Label lblTitle = new Label
            {
                Text = "📋 لیست پرسنل:",
                Location = new Point(margin, y),
                Size = new Size(200, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);
            y += 30;

            dgvEmployees = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 250),
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

            dgvEmployees.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvEmployees.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvEmployees.Columns.Add("Id", "کد");
            dgvEmployees.Columns.Add("FullName", "نام و نام‌خانوادگی");
            dgvEmployees.Columns.Add("NationalCode", "کد ملی");
            dgvEmployees.Columns.Add("Phone", "تلفن");
            dgvEmployees.Columns.Add("Position", "سمت");
            dgvEmployees.Columns.Add("HireDate", "تاریخ استخدام");
            dgvEmployees.Columns.Add("BaseSalary", "حقوق پایه");
            dgvEmployees.Columns.Add("Status", "وضعیت");

            dgvEmployees.Columns["BaseSalary"].DefaultCellStyle.Format = "N0";
            dgvEmployees.Columns["BaseSalary"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvEmployees.SelectionChanged += DgvEmployees_SelectionChanged;

            this.Controls.Add(dgvEmployees);
            y += dgvEmployees.Height + 10;

            // ========== پنل اطلاعات ==========
            GroupBox grpInfo = new GroupBox
            {
                Text = "📝 اطلاعات پرسنل",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // ایجاد TableLayoutPanel برای چیدمان منظم
            TableLayoutPanel tlpInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(10)
            };
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            // سطر اول: نام و نام‌خانوادگی
            tlpInfo.Controls.Add(new Label { Text = "نام:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            txtFirstName = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) };
            tlpInfo.Controls.Add(txtFirstName, 1, 0);

            tlpInfo.Controls.Add(new Label { Text = "نام‌خانوادگی:", TextAlign = ContentAlignment.MiddleRight }, 2, 0);
            txtLastName = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) };
            tlpInfo.Controls.Add(txtLastName, 3, 0);

            // سطر دوم: کد ملی و تلفن
            tlpInfo.Controls.Add(new Label { Text = "کد ملی:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            txtNationalCode = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) };
            tlpInfo.Controls.Add(txtNationalCode, 1, 1);

            tlpInfo.Controls.Add(new Label { Text = "تلفن:", TextAlign = ContentAlignment.MiddleRight }, 2, 1);
            txtPhone = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) };
            tlpInfo.Controls.Add(txtPhone, 3, 1);

            // سطر سوم: سمت و تاریخ استخدام (با تاریخ شمسی)
            tlpInfo.Controls.Add(new Label { Text = "سمت:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            txtPosition = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) };
            tlpInfo.Controls.Add(txtPosition, 1, 2);

            tlpInfo.Controls.Add(new Label { Text = "تاریخ استخدام:", TextAlign = ContentAlignment.MiddleRight }, 2, 2);

            // ========== پنل تاریخ شمسی ==========
            Panel hireDatePanel = new Panel { Dock = DockStyle.Fill, Height = 30 };
            lblHireDate = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            btnHireDate = new Button
            {
                Text = "📅",
                Dock = DockStyle.Right,
                Width = 35,
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };
            btnHireDate.Click += (s, e) => ShowPersianDateTimePickerForHireDate();
            hireDatePanel.Controls.Add(lblHireDate);
            hireDatePanel.Controls.Add(btnHireDate);
            lblHireDate.Dock = DockStyle.Fill;
            btnHireDate.BringToFront();
            tlpInfo.Controls.Add(hireDatePanel, 3, 2);

            // سطر چهارم: حقوق پایه و نرخ ساعتی
            tlpInfo.Controls.Add(new Label { Text = "حقوق پایه:", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            txtBaseSalary = new NumericTextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) };
            tlpInfo.Controls.Add(txtBaseSalary, 1, 3);

            tlpInfo.Controls.Add(new Label { Text = "نرخ ساعتی:", TextAlign = ContentAlignment.MiddleRight }, 2, 3);
            txtHourlyRate = new NumericTextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 10) };
            tlpInfo.Controls.Add(txtHourlyRate, 3, 3);

            grpInfo.Controls.Add(tlpInfo);
            this.Controls.Add(grpInfo);
            y += grpInfo.Height + 10;

            // ========== پنل وضعیت و یادداشت ==========
            Panel pnlBottom = new Panel
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkIsActive = new CheckBox
            {
                Text = "فعال",
                Location = new Point(0, 10),
                Size = new Size(80, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Checked = true
            };

            Label lblNotes = new Label
            {
                Text = "یادداشت:",
                Location = new Point(100, 12),
                Size = new Size(70, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            txtNotes = new TextBox
            {
                Location = new Point(180, 8),
                Size = new Size(pnlBottom.Width - 550, 30),
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnAdd = new Button
            {
                Text = "➕ افزودن",
                Location = new Point(pnlBottom.Width - 350, 5),
                Size = new Size(110, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button
            {
                Text = "💾 ویرایش",
                Location = new Point(pnlBottom.Width - 230, 5),
                Size = new Size(110, 35),
                BackColor = Color.LightBlue,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button
            {
                Text = "🗑️ حذف",
                Location = new Point(pnlBottom.Width - 110, 5),
                Size = new Size(100, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnDelete.Click += BtnDelete_Click;

            pnlBottom.Controls.Add(chkIsActive);
            pnlBottom.Controls.Add(lblNotes);
            pnlBottom.Controls.Add(txtNotes);
            pnlBottom.Controls.Add(btnAdd);
            pnlBottom.Controls.Add(btnUpdate);
            pnlBottom.Controls.Add(btnDelete);

            this.Controls.Add(pnlBottom);

            // ========== رویداد Resize ==========
            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width - margin * 2;
                grpSearch.Width = w;
                dgvEmployees.Width = w;
                grpInfo.Width = w;
                pnlBottom.Width = w;

                txtNotes.Width = w - 500;
                btnAdd.Location = new Point(pnlBottom.Width - 350, 5);
                btnUpdate.Location = new Point(pnlBottom.Width - 230, 5);
                btnDelete.Location = new Point(pnlBottom.Width - 110, 5);
            };
        }

        // ==================== متدهای تاریخ شمسی ====================
        private void ShowPersianDateTimePickerForHireDate()
        {
            using (PersianDatePopup popup = new PersianDatePopup(selectedHireDate))
            {
                if (popup.ShowDialog(this) == DialogResult.OK)
                {
                    selectedHireDate = popup.SelectedDateTime.Date;
                    UpdateHireDateDisplay();
                }
            }
        }

        private void UpdateHireDateDisplay()
        {
            lblHireDate.Text = ConvertToPersianDate(selectedHireDate);
        }

        private string ConvertToPersianDate(DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:00}/{day:00}";
        }

        // ==================== متدهای اصلی ====================
        private void RefreshGrid(List<Employee> customList = null)
        {
            _loading = true;

            dgvEmployees.Rows.Clear();
            var list = customList ?? CafeManager.GetEmployees(false);
            _employees = list;

            foreach (var e in list.OrderBy(e => e.FirstName))
            {
                string status = e.IsActive ? "✅ فعال" : "❌ غیرفعال";
                dgvEmployees.Rows.Add(
                    e.Id,
                    e.FullName,
                    e.NationalCode,
                    e.PhoneNumber,
                    e.Position,
                    ConvertToPersianDate(e.HireDate), // تاریخ شمسی
                    e.BaseSalary.ToString("N0"),
                    status
                );

                if (!e.IsActive)
                    dgvEmployees.Rows[dgvEmployees.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGray;
            }

            // ========== پاک‌سازی انتخاب ==========
            this.BeginInvoke(new Action(() =>
            {
                dgvEmployees.ClearSelection();
                dgvEmployees.CurrentCell = null;
                _loading = false;
            }));
        }

        private void DgvEmployees_SelectionChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            if (dgvEmployees.CurrentCell == null) return;
            if (dgvEmployees.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["Id"].Value);
            var employee = CafeManager.GetEmployeeById(id);
            if (employee == null) return;

            _loading = true;

            txtFirstName.Text = employee.FirstName;
            txtLastName.Text = employee.LastName;
            txtNationalCode.Text = employee.NationalCode;
            txtPhone.Text = employee.PhoneNumber;
            txtPosition.Text = employee.Position;

            // ========== تنظیم تاریخ شمسی ==========
            selectedHireDate = employee.HireDate.Date;
            UpdateHireDateDisplay();

            txtBaseSalary.Value = (int)employee.BaseSalary;
            txtHourlyRate.Value = (int)employee.HourlyRate;
            chkIsActive.Checked = employee.IsActive;
            txtNotes.Text = employee.Notes;

            _loading = false;
        }

        private void ClearFields()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtNationalCode.Text = "";
            txtPhone.Text = "";
            txtPosition.Text = "";

            // ========== تنظیم تاریخ پیش‌فرض ==========
            selectedHireDate = DateTime.Now.Date;
            UpdateHireDateDisplay();

            txtBaseSalary.Text = "";
            txtHourlyRate.Text = "";
            chkIsActive.Checked = true;
            txtNotes.Text = "";

            if (!_loading)
            {
                dgvEmployees.ClearSelection();
            }
        }

        private Employee GetEmployeeFromFields()
        {
            return new Employee
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                NationalCode = txtNationalCode.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Position = txtPosition.Text.Trim(),
                HireDate = selectedHireDate, // استفاده از تاریخ شمسی انتخاب شده
                BaseSalary = txtBaseSalary.Value,
                HourlyRate = txtHourlyRate.Value,
                OvertimeRate = 1.4,
                IsActive = chkIsActive.Checked,
                Notes = txtNotes.Text.Trim()
            };
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("لطفاً نام را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("لطفاً نام‌خانوادگی را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return false;
            }
            if (txtBaseSalary.Value <= 0)
            {
                MessageBox.Show("حقوق پایه باید بزرگتر از صفر باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBaseSalary.Focus();
                return false;
            }
            return true;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            try
            {
                var employee = GetEmployeeFromFields();
                CafeManager.AddEmployee(employee);
                RefreshGrid();
                ClearFields();
                MessageBox.Show("✅ پرسنل با موفقیت اضافه شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در افزودن پرسنل: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک پرسنل را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateFields()) return;

            try
            {
                int id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["Id"].Value);
                var employee = GetEmployeeFromFields();
                employee.Id = id;
                CafeManager.UpdateEmployee(employee);
                RefreshGrid();
                ClearFields();
                MessageBox.Show("✅ اطلاعات پرسنل با موفقیت به‌روزرسانی شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ویرایش پرسنل: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک پرسنل را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["Id"].Value);
            var employee = CafeManager.GetEmployeeById(id);

            DialogResult result = MessageBox.Show(
                $"آیا از حذف پرسنل '{employee?.FullName}' اطمینان دارید؟",
                "تأیید حذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    CafeManager.DeleteEmployee(id);
                    RefreshGrid();
                    ClearFields();
                    MessageBox.Show("✅ پرسنل با موفقیت حذف شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطا در حذف پرسنل: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                RefreshGrid();
                return;
            }

            string searchType = cmbSearchType.SelectedItem.ToString();
            var all = CafeManager.GetEmployees(false);
            List<Employee> result = new List<Employee>();

            switch (searchType)
            {
                case "نام":
                    result = all.Where(emp => emp.FullName.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "کد ملی":
                    result = all.Where(emp => emp.NationalCode.Contains(searchText)).ToList();
                    break;
                case "سمت":
                    result = all.Where(emp => emp.Position.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
            }

            RefreshGrid(result);
            if (result.Count == 0)
                MessageBox.Show("نتیجه‌ای یافت نشد.", "جستجو", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}