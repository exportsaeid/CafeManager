// ============================================================
// فایل: ProfitReportForm.cs
// فرم گزارش سود خالص با تنظیم خودکار ارتفاع جدول خلاصه
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
    public class ProfitReportForm : Form
    {
        private Button btnFromDate;
        private Label lblFromDate;
        private Button btnToDate;
        private Label lblToDate;
        private Button btnFilter;
        private Button btnRefresh;
        private Label lblTotalSales;
        private Label lblTotalExpenses;
        private Label lblTotalPurchases;
        private Label lblTotalPayroll;
        private Label lblNetProfitValue;
        private DataGridView dgvSummary;
        private DataGridView dgvDetails;

        private DateTime fromDateTime;
        private DateTime toDateTime;
        private PersianCalendar pc = new PersianCalendar();

        public ProfitReportForm()
        {
            this.Text = "📊 گزارش سود خالص";
            this.ShowInTaskbar = false; // ← این خط را اضافه کنید
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            fromDateTime = DateTime.Now.Date;
            toDateTime = DateTime.Now.Date;

            InitializeComponents();
            UpdateDateDisplays();
            LoadReport(fromDateTime, toDateTime);
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
                Text = "🔍 محاسبه سود",
                Location = new Point(grpFilter.Width - 830, 23),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnFilter.Click += (s, e) => LoadReport(fromDateTime, toDateTime);

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
                LoadReport(fromDateTime, toDateTime);
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

            // ========== پنل خلاصه سود ==========
            Panel pnlSummary = new Panel
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - margin * 2, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.WhiteSmoke
            };

            TableLayoutPanel tlpSummary = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Padding = new Padding(5)
            };
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            Panel pnlSales = CreateSummaryCard("💰 کل فروش", "0 تومان", Color.FromArgb(52, 152, 219), Color.FromArgb(235, 245, 251));
            tlpSummary.Controls.Add(pnlSales, 0, 0);

            Panel pnlMisc = CreateSummaryCard("💸 هزینه متفرقه", "0 تومان", Color.FromArgb(231, 76, 60), Color.FromArgb(251, 235, 235));
            tlpSummary.Controls.Add(pnlMisc, 1, 0);

            Panel pnlPurchase = CreateSummaryCard("📦 خرید کالا", "0 تومان", Color.FromArgb(243, 156, 18), Color.FromArgb(252, 245, 230));
            tlpSummary.Controls.Add(pnlPurchase, 2, 0);

            Panel pnlPayroll = CreateSummaryCard("👤 حقوق و دستمزد", "0 تومان", Color.FromArgb(155, 89, 182), Color.FromArgb(245, 235, 250));
            tlpSummary.Controls.Add(pnlPayroll, 3, 0);

            Panel pnlProfit = CreateSummaryCard("💰 سود خالص", "0 تومان", Color.FromArgb(241, 196, 15), Color.FromArgb(255, 250, 230));
            tlpSummary.Controls.Add(pnlProfit, 4, 0);

            pnlSummary.Controls.Add(tlpSummary);

            lblTotalSales = (Label)pnlSales.Tag;
            lblTotalExpenses = (Label)pnlMisc.Tag;
            lblTotalPurchases = (Label)pnlPurchase.Tag;
            lblTotalPayroll = (Label)pnlPayroll.Tag;
            lblNetProfitValue = (Label)pnlProfit.Tag;

            this.Controls.Add(pnlSummary);
            y += pnlSummary.Height + 10;

            // ========== جدول خلاصه (با AutoSize) ==========
            Label lblSummaryTitle = new Label
            {
                Text = "📋 خلاصه سود و هزینه",
                Location = new Point(margin, y),
                Size = new Size(250, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblSummaryTitle);
            y += 30;

            dgvSummary = new DataGridView
            {
                Location = new Point(margin, y),
                Width = this.ClientSize.Width - margin * 2,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells, // ارتفاع ردیف‌ها متناسب با محتوا
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvSummary.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSummary.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvSummary.Columns.Add("Title", "عنوان");
            dgvSummary.Columns.Add("Amount", "مبلغ (تومان)");
            dgvSummary.Columns["Amount"].DefaultCellStyle.Format = "N0";
            dgvSummary.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            this.Controls.Add(dgvSummary);

            // ========== جدول جزئیات ==========
            Label lblDetailsTitle = new Label
            {
                Text = "📊 جزئیات بیشتر",
                Location = new Point(margin, y + 30), // موقتاً تنظیم می‌شود بعداً
                Size = new Size(250, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblDetailsTitle);

            dgvDetails = new DataGridView
            {
                Location = new Point(margin, y + 60),
                Size = new Size(this.ClientSize.Width - margin * 2, 180),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvDetails.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDetails.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvDetails.Columns.Add("Date", "تاریخ");
            dgvDetails.Columns.Add("Description", "شرح");
            dgvDetails.Columns.Add("Type", "نوع");
            dgvDetails.Columns.Add("Amount", "مبلغ (تومان)");
            dgvDetails.Columns["Amount"].DefaultCellStyle.Format = "N0";
            dgvDetails.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            this.Controls.Add(dgvDetails);

            // ذخیره مرجع برای به‌روزرسانی موقعیت بعد از تنظیم ارتفاع
            this.Controls.SetChildIndex(dgvDetails, 0);
            this.Controls.SetChildIndex(lblDetailsTitle, 0);
            this.Controls.SetChildIndex(dgvSummary, 0);
        }

        // ========== متد ساخت کارت ==========
        private Panel CreateSummaryCard(string title, string amountText, Color accentColor, Color backColor)
        {
            Panel pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backColor,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(5),
                Margin = new Padding(2)
            };

            Panel innerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            Panel colorBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 6,
                BackColor = accentColor
            };
            innerPanel.Controls.Add(colorBar);

            Label lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 30,
                ForeColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(5, 8, 5, 0)
            };
            innerPanel.Controls.Add(lblTitle);

            Label lblAmount = new Label
            {
                Text = amountText,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(5)
            };
            innerPanel.Controls.Add(lblAmount);

            pnl.Controls.Add(innerPanel);
            pnl.Tag = lblAmount;

            return pnl;
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

        // ==================== گزارش ====================
        private void LoadReport(DateTime from, DateTime to)
        {
            DateTime endDate = to.AddDays(1).AddSeconds(-1);

            // دریافت داده‌ها
            var invoices = CafeManager.GetSalesHistoryByDate(from, endDate);
            double totalSales = invoices.Sum(i => i.TotalAmount);

            var miscExpenses = CafeManager.GetMiscExpenses(from, endDate);
            double totalMisc = miscExpenses.Where(e => e.IsConfirmed).Sum(e => e.Amount);

            var purchases = CafeManager.GetPurchases(from, endDate);
            double totalPurchases = purchases.Where(p => p.IsConfirmed).Sum(p => p.TotalAmount);

            var payrolls = CafeManager.GetPayrolls(null, from.Year, from.Month);
            double totalPayroll = 0;
            if (from.Month == to.Month && from.Year == to.Year)
            {
                totalPayroll = payrolls.Where(p => p.IsPaid).Sum(p => p.NetSalary);
            }
            else
            {
                for (int year = from.Year; year <= to.Year; year++)
                {
                    int startMonth = (year == from.Year) ? from.Month : 1;
                    int endMonth = (year == to.Year) ? to.Month : 12;
                    for (int month = startMonth; month <= endMonth; month++)
                    {
                        var monthPayrolls = CafeManager.GetPayrolls(null, year, month);
                        totalPayroll += monthPayrolls.Where(p => p.IsPaid).Sum(p => p.NetSalary);
                    }
                }
            }

            double totalExpenses = totalMisc + totalPurchases + totalPayroll;
            double netProfit = totalSales - totalExpenses;

            // نمایش در کارت‌ها
            lblTotalSales.Text = totalSales.ToString("N0") + " تومان";
            lblTotalExpenses.Text = totalMisc.ToString("N0") + " تومان";
            lblTotalPurchases.Text = totalPurchases.ToString("N0") + " تومان";
            lblTotalPayroll.Text = totalPayroll.ToString("N0") + " تومان";
            lblNetProfitValue.Text = netProfit.ToString("N0") + " تومان";
            lblNetProfitValue.ForeColor = netProfit >= 0 ? Color.FromArgb(39, 174, 96) : Color.FromArgb(192, 57, 43);

            // پر کردن جدول خلاصه
            dgvSummary.Rows.Clear();
            dgvSummary.Rows.Add("💰 کل فروش", totalSales);
            dgvSummary.Rows.Add("💸 هزینه متفرقه", totalMisc);
            dgvSummary.Rows.Add("📦 خرید کالا", totalPurchases);
            dgvSummary.Rows.Add("👤 حقوق و دستمزد", totalPayroll);
            dgvSummary.Rows.Add("📊 سود خالص", netProfit);

            if (netProfit < 0)
                dgvSummary.Rows[dgvSummary.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
            else
                dgvSummary.Rows[dgvSummary.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Green;

            // ========== تنظیم ارتفاع جدول خلاصه بر اساس تعداد رکوردها ==========
            dgvSummary.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
            int totalHeight = dgvSummary.ColumnHeadersHeight;
            foreach (DataGridViewRow row in dgvSummary.Rows)
            {
                totalHeight += row.Height;
            }
            totalHeight += 2; // حاشیه
            dgvSummary.Height = totalHeight;

            // ========== تنظیم موقعیت جدول جزئیات و عنوان آن ==========
            int newY = dgvSummary.Location.Y + dgvSummary.Height + 15; // فاصله ۱۵ پیکسل
            // پیدا کردن lblDetailsTitle و dgvDetails در Controls و تغییر موقعیت آنها
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Label lbl && lbl.Text == "📊 جزئیات بیشتر")
                {
                    lbl.Location = new Point(lbl.Location.X, newY);
                }
                if (ctrl == dgvDetails)
                {
                    dgvDetails.Location = new Point(dgvDetails.Location.X, newY + 30);
                    // تنظیم ارتفاع dgvDetails بر اساس فضای باقیمانده
                    int remainingHeight = this.ClientSize.Height - dgvDetails.Location.Y - 30;
                    if (remainingHeight > 100)
                        dgvDetails.Height = remainingHeight;
                }
            }

            // پر کردن جزئیات
            dgvDetails.Rows.Clear();
            foreach (var inv in invoices.OrderByDescending(i => i.OrderDate))
                dgvDetails.Rows.Add(ConvertToPersianDate(inv.OrderDate), $"فاکتور {inv.Id} - {inv.CustomerName}", "فروش", inv.TotalAmount);

            foreach (var e in miscExpenses.Where(e => e.IsConfirmed).OrderByDescending(e => e.ExpenseDate))
                dgvDetails.Rows.Add(ConvertToPersianDate(e.ExpenseDate), e.Description, "هزینه متفرقه", e.Amount);

            foreach (var p in purchases.Where(p => p.IsConfirmed).OrderByDescending(p => p.PurchaseDate))
                dgvDetails.Rows.Add(ConvertToPersianDate(p.PurchaseDate), $"خرید از {p.SupplierName}", "خرید کالا", p.TotalAmount);

            foreach (var p in payrolls.Where(p => p.IsPaid))
            {
                var emp = CafeManager.GetEmployeeById(p.EmployeeId);
                dgvDetails.Rows.Add($"{p.Year}/{p.Month:00}", $"حقوق {emp?.FullName ?? "پرسنل"}", "حقوق", p.NetSalary);
            }
        }
    }
}