// ============================================================
// فایل: MiscExpenseReportForm.cs
// فرم گزارش هزینه‌های متفرقه با تاریخ شمسی
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
    public class MiscExpenseReportForm : Form
    {
        private DataGridView dgvExpenses;
        private DataGridView dgvCategorySummary;
        private Button btnFromDate;
        private Label lblFromDate;
        private Button btnToDate;
        private Label lblToDate;
        private Button btnFilter;
        private Button btnRefresh;
        private Label lblTotalExpenses;
        private Label lblTotalCategories;
        private List<MiscellaneousExpense> _expenses;

        private DateTime fromDateTime;
        private DateTime toDateTime;
        private PersianCalendar pc = new PersianCalendar();

        public MiscExpenseReportForm()
        {
            this.Text = "📊 گزارش هزینه‌های متفرقه";
            this.ShowInTaskbar = false; // ← این خط را اضافه کنید
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            fromDateTime = DateTime.Now.Date;
            toDateTime = DateTime.Now.Date;

            InitializeComponents();
            UpdateDateDisplays();
            LoadExpenses(fromDateTime, toDateTime);
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

            // ========== عنوان جدول هزینه‌ها ==========
            Label lblExpensesTitle = new Label
            {
                Text = "📋 لیست هزینه‌ها:",
                Location = new Point(margin, y),
                Size = new Size(200, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblExpensesTitle);
            y += 30;

            // ========== جدول هزینه‌ها ==========
            dgvExpenses = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 200),
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

            dgvExpenses.Columns.Add("Date", "تاریخ شمسی");
            dgvExpenses.Columns.Add("Description", "توضیح");
            dgvExpenses.Columns.Add("Category", "دسته‌بندی");
            dgvExpenses.Columns.Add("Amount", "مبلغ");
            dgvExpenses.Columns.Add("PaymentMethod", "روش پرداخت");

            dgvExpenses.Columns["Amount"].DefaultCellStyle.Format = "N0";
            dgvExpenses.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            this.Controls.Add(dgvExpenses);
            y += dgvExpenses.Height + 10;

            // ========== عنوان جدول خلاصه دسته‌بندی ==========
            Label lblSummaryTitle = new Label
            {
                Text = "📊 خلاصه هزینه‌ها بر اساس دسته‌بندی:",
                Location = new Point(margin, y),
                Size = new Size(300, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblSummaryTitle);
            y += 30;

            // ========== جدول خلاصه دسته‌بندی ==========
            dgvCategorySummary = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 150),
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

            dgvCategorySummary.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCategorySummary.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvCategorySummary.Columns.Add("Category", "دسته‌بندی");
            dgvCategorySummary.Columns.Add("Count", "تعداد");
            dgvCategorySummary.Columns.Add("TotalAmount", "جمع مبلغ");

            dgvCategorySummary.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
            dgvCategorySummary.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            this.Controls.Add(dgvCategorySummary);
            y += dgvCategorySummary.Height + 10;

            // ========== پنل پایین ==========
            Panel pnlBottom = new Panel
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTotalExpenses = new Label
            {
                Text = "💰 جمع کل هزینه‌ها: 0 تومان",
                Location = new Point(0, 10),
                Size = new Size(300, 30),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            lblTotalCategories = new Label
            {
                Text = "📊 تعداد دسته‌بندی‌ها: 0",
                Location = new Point(320, 10),
                Size = new Size(250, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            Button btnClose = new Button
            {
                Text = "❌ بستن",
                Location = new Point(pnlBottom.Width - 100, 8),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(lblTotalExpenses);
            pnlBottom.Controls.Add(lblTotalCategories);
            pnlBottom.Controls.Add(btnClose);

            this.Controls.Add(pnlBottom);

            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width - margin * 2;
                grpFilter.Width = w;
                dgvExpenses.Width = w;
                dgvCategorySummary.Width = w;
                pnlBottom.Width = w;
                btnClose.Location = new Point(pnlBottom.Width - 100, 8);
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

        private void UpdateDateDisplays()
        {
            lblFromDate.Text = ConvertToPersianDate(fromDateTime);
            lblToDate.Text = ConvertToPersianDate(toDateTime);
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
            _expenses = CafeManager.GetMiscExpenses(from, to.AddDays(1).AddSeconds(-1));
            DisplayExpenses(_expenses);
        }

        private void DisplayExpenses(List<MiscellaneousExpense> expenses)
        {
            // ========== نمایش لیست هزینه‌ها ==========
            dgvExpenses.Rows.Clear();
            double total = 0;

            foreach (var e in expenses.OrderByDescending(e => e.ExpenseDate))
            {
                dgvExpenses.Rows.Add(
                    ConvertToPersianDate(e.ExpenseDate),
                    e.Description,
                    e.Category,
                    e.Amount.ToString("N0"),
                    e.PaymentMethod
                );
                total += e.Amount;
            }

            lblTotalExpenses.Text = $"💰 جمع کل هزینه‌ها: {total:N0} تومان";

            // ========== نمایش خلاصه دسته‌بندی ==========
            var categorySummary = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .OrderBy(g => g.Category)
                .ToList();

            dgvCategorySummary.Rows.Clear();
            foreach (var item in categorySummary)
            {
                dgvCategorySummary.Rows.Add(
                    item.Category,
                    item.Count,
                    item.TotalAmount.ToString("N0")
                );
            }

            lblTotalCategories.Text = $"📊 تعداد دسته‌بندی‌ها: {categorySummary.Count}";

            // ========== هشدار در صورت عدم وجود داده ==========
            if (expenses.Count == 0)
            {
                dgvCategorySummary.Rows.Add("هیچ هزینه‌ای یافت نشد", 0, "0");
            }
        }
    }
}