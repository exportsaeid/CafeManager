// ============================================================
// فایل: PersianDatePopup.cs
// ============================================================
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CafeManager
{
    public class PersianDatePopup : Form
    {
        private ComboBox cmbYear;
        private ComboBox cmbMonth;
        private ComboBox cmbDay;
        private NumericUpDown nudHour;
        private NumericUpDown nudMinute;
        private Button btnOK;
        private Button btnToday;
        private Button btnCancel;

        private PersianCalendar pc = new PersianCalendar();
        private DateTime currentDateTime;

        public DateTime SelectedDateTime { get; private set; }

        public PersianDatePopup(DateTime defaultDateTime)
        {
            currentDateTime = defaultDateTime;
            SelectedDateTime = defaultDateTime;

            InitializeComponents();
            LoadYearMonthDay();
            SetCurrentDate();

            this.Text = "انتخاب تاریخ و ساعت شمسی";

            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
          
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.Size = new Size(510, 390);
            this.MinimumSize = new Size(470, 340);
            this.BackColor = Color.White;
            this.Font = new Font("Tahoma", 10);
        }

        private void InitializeComponents()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.White
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            mainLayout.RightToLeft = RightToLeft.No;
            // عنوان
            Label lblTitle = new Label
            {
                Text = "📅 انتخاب تاریخ و ساعت",
                Font = new Font("Tahoma", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue,
                Dock = DockStyle.Fill
            };
            mainLayout.Controls.Add(lblTitle, 0, 0);

            // پنل سال، ماه، روز
            TableLayoutPanel dateLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(0)
            };
            dateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            dateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
            dateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            dateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
            dateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            dateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));

            dateLayout.Controls.Add(new Label { Text = "سال:", TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 9) }, 0, 0);
            cmbYear = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                IntegralHeight = false
            };
            dateLayout.Controls.Add(cmbYear, 1, 0);

            dateLayout.Controls.Add(new Label { Text = "ماه:", TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 9) }, 2, 0);
            cmbMonth = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                IntegralHeight = false
            };
            cmbMonth.SelectedIndexChanged += CmbMonth_SelectedIndexChanged;
            dateLayout.Controls.Add(cmbMonth, 3, 0);

            dateLayout.Controls.Add(new Label { Text = "روز:", TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 9) }, 4, 0);
            cmbDay = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10),
                IntegralHeight = false
            };
            dateLayout.Controls.Add(cmbDay, 5, 0);

            mainLayout.Controls.Add(dateLayout, 0, 1);

            // پنل ساعت و دقیقه
            TableLayoutPanel timeLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0)
            };
            timeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            timeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            timeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            timeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            timeLayout.Controls.Add(new Label { Text = "ساعت:", TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 9) }, 0, 0);
            nudHour = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 23,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Tahoma", 10)
            };
            timeLayout.Controls.Add(nudHour, 1, 0);

            timeLayout.Controls.Add(new Label { Text = "دقیقه:", TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 9) }, 2, 0);
            nudMinute = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 59,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Tahoma", 10)
            };
            timeLayout.Controls.Add(nudMinute, 3, 0);

            mainLayout.Controls.Add(timeLayout, 0, 2);

            // فاصله
            Label lblSpacer = new Label
            {
                Dock = DockStyle.Fill,
                Text = "",
                Height = 20
            };
            mainLayout.Controls.Add(lblSpacer, 0, 3);

            // دکمه‌ها
            TableLayoutPanel buttonLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 5, 0, 5)
            };
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            buttonLayout.RightToLeft = RightToLeft.Yes;
            btnToday = new Button
            {
                Text = "📅 امروز",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Size = new Size(110, 42),
                Anchor = AnchorStyles.None,
                FlatAppearance = { BorderSize = 0 }
            };
            btnToday.Click += BtnToday_Click;
            buttonLayout.Controls.Add(btnToday, 0, 0);

            btnOK = new Button
            {
                Text = "✅ تأیید",
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Size = new Size(110, 42),
                Anchor = AnchorStyles.None,
                FlatAppearance = { BorderSize = 0 }
            };
            btnOK.Click += BtnOK_Click;
            buttonLayout.Controls.Add(btnOK, 1, 0);

            btnCancel = new Button
            {
                Text = "❌ انصراف",
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Size = new Size(110, 42),
                Anchor = AnchorStyles.None,
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            buttonLayout.Controls.Add(btnCancel, 2, 0);

            mainLayout.Controls.Add(buttonLayout, 0, 4);

            this.Controls.Add(mainLayout);
        }

        private void LoadYearMonthDay()
        {
            for (int year = 1300; year <= 1450; year++)
                cmbYear.Items.Add(year);

            string[] persianMonths = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
            foreach (string month in persianMonths)
                cmbMonth.Items.Add(month);
        }

        private void SetCurrentDate()
        {
            int year = pc.GetYear(currentDateTime);
            int month = pc.GetMonth(currentDateTime);
            int day = pc.GetDayOfMonth(currentDateTime);

            cmbYear.SelectedItem = year;
            cmbMonth.SelectedIndex = month - 1;
            UpdateDays();
            cmbDay.SelectedItem = day;

            nudHour.Value = currentDateTime.Hour;
            nudMinute.Value = currentDateTime.Minute;
        }

        private void UpdateDays()
        {
            if (cmbYear.SelectedItem == null || cmbMonth.SelectedIndex == -1) return;

            int year = (int)cmbYear.SelectedItem;
            int month = cmbMonth.SelectedIndex + 1;
            int daysInMonth = pc.GetDaysInMonth(year, month);

            cmbDay.Items.Clear();
            for (int day = 1; day <= daysInMonth; day++)
                cmbDay.Items.Add(day);

            int currentDay = pc.GetDayOfMonth(currentDateTime);
            if (currentDay <= daysInMonth && cmbDay.Items.Count > 0)
                cmbDay.SelectedItem = currentDay;
            else if (cmbDay.Items.Count > 0)
                cmbDay.SelectedIndex = 0;
        }

        private void CmbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDays();
        }

        private void BtnToday_Click(object sender, EventArgs e)
        {
            currentDateTime = DateTime.Now;
            SetCurrentDate();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbYear.SelectedItem == null || cmbMonth.SelectedIndex == -1 || cmbDay.SelectedItem == null)
                {
                    MessageBox.Show("لطفاً تاریخ کامل را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int year = (int)cmbYear.SelectedItem;
                int month = cmbMonth.SelectedIndex + 1;
                int day = (int)cmbDay.SelectedItem;
                int hour = (int)nudHour.Value;
                int minute = (int)nudMinute.Value;

                SelectedDateTime = pc.ToDateTime(year, month, day, hour, minute, 0, 0);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"تاریخ وارد شده معتبر نیست: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}