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
    public class AttendanceForm : Form
    {
        private ComboBox cmbEmployee;
        private Button btnDate;
        private Label lblDate;
        private DateTimePicker dtpCheckIn;
        private DateTimePicker dtpCheckOut;
        private CheckBox chkIsPresent;
        private TextBox txtNotes;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRefresh;
        private DataGridView dgvAttendances;
        private Button btnFromDate;
        private Label lblFromDate;
        private Button btnToDate;
        private Label lblToDate;
        private Button btnFilter;
        private List<Attendance> _attendances;

        // ========== متغیرهای تاریخ شمسی ==========
        private DateTime selectedDate;
        private DateTime fromDateTime;
        private DateTime toDateTime;
        private PersianCalendar pc = new PersianCalendar();
        private bool _loading = false;

        public AttendanceForm()
        {
            this.Text = "📋 ثبت حضور و غیاب پرسنل";
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
            LoadEmployees();
            UpdateDateDisplays();
            LoadAttendances(fromDateTime, toDateTime);
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
            btnFilter.Click += (s, e) => LoadAttendances(fromDateTime, toDateTime);

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
                LoadAttendances(fromDateTime, toDateTime);
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

            // ========== جدول حضور و غیاب ==========
            Label lblTitle = new Label
            {
                Text = "📋 لیست حضور و غیاب:",
                Location = new Point(margin, y),
                Size = new Size(200, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);
            y += 30;

            dgvAttendances = new DataGridView
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

            dgvAttendances.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvAttendances.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvAttendances.Columns.Add("Id", "کد");
            dgvAttendances.Columns.Add("Employee", "پرسنل");
            dgvAttendances.Columns.Add("Date", "تاریخ");
            dgvAttendances.Columns.Add("CheckIn", "ورود");
            dgvAttendances.Columns.Add("CheckOut", "خروج");
            dgvAttendances.Columns.Add("WorkHours", "ساعت کاری");
            dgvAttendances.Columns.Add("Status", "وضعیت");

            dgvAttendances.SelectionChanged += DgvAttendances_SelectionChanged;

            this.Controls.Add(dgvAttendances);
            y += dgvAttendances.Height + 10;

            // ========== پنل ورودی ==========
            GroupBox grpInput = new GroupBox
            {
                Text = "📝 ثبت حضور و غیاب",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            TableLayoutPanel tlpInput = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(10)
            };
            tlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            tlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            tlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // سطر اول
            tlpInput.Controls.Add(new Label { Text = "پرسنل:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            cmbEmployee = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                DisplayMember = "FullName",
                ValueMember = "Id"
            };
            tlpInput.Controls.Add(cmbEmployee, 1, 0);

            tlpInput.Controls.Add(new Label { Text = "تاریخ:", TextAlign = ContentAlignment.MiddleRight }, 2, 0);

            // ========== پنل تاریخ شمسی ==========
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
            tlpInput.Controls.Add(datePanel, 3, 0);

            // سطر دوم
            tlpInput.Controls.Add(new Label { Text = "ورود:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            dtpCheckIn = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                RightToLeft = RightToLeft.No,
                Font = new Font("Tahoma", 10),
                Value = DateTime.Now.Date.AddHours(8)
            };
            tlpInput.Controls.Add(dtpCheckIn, 1, 1);

            tlpInput.Controls.Add(new Label { Text = "خروج:", TextAlign = ContentAlignment.MiddleRight }, 2, 1);
            dtpCheckOut = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                RightToLeft = RightToLeft.No,
                Font = new Font("Tahoma", 10),
                Value = DateTime.Now.Date.AddHours(16)
            };
            tlpInput.Controls.Add(dtpCheckOut, 3, 1);

            grpInput.Controls.Add(tlpInput);
            this.Controls.Add(grpInput);
            y += grpInput.Height + 10;

            // ========== پنل پایین ==========
            Panel pnlBottom = new Panel
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkIsPresent = new CheckBox
            {
                Text = "حاضر",
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
                Location = new Point(200, 10),
                Size = new Size(pnlBottom.Width - 550, 30),
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnAdd = new Button
            {
                Text = "➕ افزودن",
                Location = new Point(pnlBottom.Width - 350, 8),
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
                Location = new Point(pnlBottom.Width - 230, 8),
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
                Location = new Point(pnlBottom.Width - 110, 8),
                Size = new Size(100, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnDelete.Click += BtnDelete_Click;

            pnlBottom.Controls.Add(chkIsPresent);
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
                grpFilter.Width = w;
                dgvAttendances.Width = w;
                grpInput.Width = w;
                pnlBottom.Width = w;

                txtNotes.Location = new Point(200, 10);
                txtNotes.Width = w - 550;

                btnAdd.Location = new Point(pnlBottom.Width - 350, 8);
                btnUpdate.Location = new Point(pnlBottom.Width - 230, 8);
                btnDelete.Location = new Point(pnlBottom.Width - 110, 8);

                // به‌روزرسانی موقعیت دکمه‌های تاریخ در پنل فیلتر
                btnFromDate.Location = new Point(grpFilter.Width - 360, 23);
                lblFromDate.Location = new Point(grpFilter.Width - 300, 25);
                btnToDate.Location = new Point(grpFilter.Width - 730, 23);
                lblToDate.Location = new Point(grpFilter.Width - 670, 25);
                btnFilter.Location = new Point(grpFilter.Width - 810, 23);
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
        private void LoadEmployees()
        {
            cmbEmployee.Items.Clear();
            var employees = CafeManager.GetEmployees(true);
            foreach (var e in employees)
                cmbEmployee.Items.Add(e);

            if (cmbEmployee.Items.Count > 0)
                cmbEmployee.SelectedIndex = 0;
        }

        private void LoadAttendances(DateTime from, DateTime to)
        {
            _loading = true;
            dgvAttendances.Rows.Clear();

            if (cmbEmployee.SelectedItem == null)
            {
                _loading = false;
                return;
            }

            int employeeId = ((Employee)cmbEmployee.SelectedItem).Id;
            var attendances = CafeManager.GetAttendances(employeeId, from, to.AddDays(1).AddSeconds(-1));
            _attendances = attendances;

            foreach (var a in attendances.OrderByDescending(a => a.Date))
            {
                string status = a.IsPresent ? "✅ حاضر" : "❌ غایب";
                dgvAttendances.Rows.Add(
                    a.Id,
                    ((Employee)cmbEmployee.SelectedItem).FullName,
                    ConvertToPersianDate(a.Date), // تاریخ شمسی
                    a.CheckIn.ToString(@"hh\:mm"),
                    a.CheckOut.ToString(@"hh\:mm"),
                    a.WorkHours.ToString(@"hh\:mm"),
                    status
                );

                if (!a.IsPresent)
                    dgvAttendances.Rows[dgvAttendances.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightYellow;
            }

            this.BeginInvoke(new Action(() =>
            {
                dgvAttendances.ClearSelection();
                dgvAttendances.CurrentCell = null;
                _loading = false;
            }));
        }

        private void DgvAttendances_SelectionChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            if (dgvAttendances.CurrentCell == null) return;
            if (dgvAttendances.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvAttendances.SelectedRows[0].Cells["Id"].Value);
            var attendance = CafeManager.GetAttendanceById(id);
            if (attendance == null) return;

            _loading = true;

            // انتخاب پرسنل
            for (int i = 0; i < cmbEmployee.Items.Count; i++)
            {
                var emp = (Employee)cmbEmployee.Items[i];
                if (emp.Id == attendance.EmployeeId)
                {
                    cmbEmployee.SelectedIndex = i;
                    break;
                }
            }

            selectedDate = attendance.Date.Date;
            dtpCheckIn.Value = attendance.Date.Add(attendance.CheckIn);
            dtpCheckOut.Value = attendance.Date.Add(attendance.CheckOut);
            chkIsPresent.Checked = attendance.IsPresent;
            txtNotes.Text = attendance.Notes;
            UpdateDateDisplays();

            _loading = false;
        }

        private void ClearFields()
        {
            selectedDate = DateTime.Now.Date;
            dtpCheckIn.Value = DateTime.Now.Date.AddHours(8);
            dtpCheckOut.Value = DateTime.Now.Date.AddHours(16);
            chkIsPresent.Checked = true;
            txtNotes.Text = "";
            UpdateDateDisplays();

            if (!_loading)
            {
                dgvAttendances.ClearSelection();
            }
        }

        private Attendance GetAttendanceFromFields()
        {
            return new Attendance
            {
                EmployeeId = ((Employee)cmbEmployee.SelectedItem).Id,
                Date = selectedDate, // استفاده از تاریخ شمسی انتخاب شده
                CheckIn = dtpCheckIn.Value.TimeOfDay,
                CheckOut = dtpCheckOut.Value.TimeOfDay,
                IsPresent = chkIsPresent.Checked,
                Notes = txtNotes.Text.Trim()
            };
        }

        private bool ValidateFields()
        {
            if (cmbEmployee.SelectedItem == null)
            {
                MessageBox.Show("لطفاً یک پرسنل انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (dtpCheckIn.Value.TimeOfDay >= dtpCheckOut.Value.TimeOfDay)
            {
                MessageBox.Show("ساعت خروج باید بعد از ساعت ورود باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            try
            {
                var attendance = GetAttendanceFromFields();
                CafeManager.AddAttendance(attendance);
                LoadAttendances(fromDateTime, toDateTime);
                ClearFields();
                MessageBox.Show("✅ حضور و غیاب با موفقیت ثبت شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ثبت حضور و غیاب: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvAttendances.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک رکورد را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateFields()) return;

            try
            {
                int id = Convert.ToInt32(dgvAttendances.SelectedRows[0].Cells["Id"].Value);
                var attendance = GetAttendanceFromFields();
                attendance.Id = id;
                CafeManager.UpdateAttendance(attendance);
                LoadAttendances(fromDateTime, toDateTime);
                ClearFields();
                MessageBox.Show("✅ حضور و غیاب با موفقیت به‌روزرسانی شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ویرایش حضور و غیاب: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAttendances.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک رکورد را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "آیا از حذف این رکورد حضور و غیاب اطمینان دارید؟",
                "تأیید حذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgvAttendances.SelectedRows[0].Cells["Id"].Value);
                    CafeManager.DeleteAttendance(id);
                    LoadAttendances(fromDateTime, toDateTime);
                    ClearFields();
                    MessageBox.Show("✅ رکورد با موفقیت حذف شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطا در حذف رکورد: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}