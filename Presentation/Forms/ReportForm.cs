using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;

namespace CafeManager
{
    public class ReportForm : Form
    {
        private DataGridView dgvInvoices;
        private DataGridView dgvInvoiceItems;
        private Button btnFromDate;
        private Button btnToDate;
        private Label lblFromDate;
        private Label lblToDate;
        private Button btnFilterByDate;
        private Button btnResetFilter;
        private Label lblFilteredTotalSummary;
        private Label lblPaymentBreakdown;

        private DateTime fromDateTime;
        private DateTime toDateTime;
        private PersianCalendar pc = new PersianCalendar();

        public ReportForm()
        {
            this.Text = "سیستم گزارش‌گیری پیشرفته کافه (تقویم فارسی)";
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Maximized;
      
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            DateTime now = DateTime.Now;
            fromDateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            toDateTime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 0);

            InitializeComponents();
            UpdateDateDisplays();
            RefreshInvoiceGrid();

            this.Resize += ReportForm_Resize;
        }

        private void ReportForm_Resize(object sender, EventArgs e)
        {
            // در صورت نیاز
        }

        private void InitializeComponents()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.WhiteSmoke
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // ========== ردیف 1: گروه فیلتر ==========
            GroupBox grpDateFilter = new GroupBox
            {
                Text = "🔍 فیلتر بر اساس تاریخ و ساعت شمسی",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                BackColor = Color.White
            };

            TableLayoutPanel filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 2,
                Padding = new Padding(10)
            };
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));

            Label lblFromTitle = new Label { Text = "از تاریخ و ساعت:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            lblFromDate = new Label
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Dock = DockStyle.Fill
            };
            btnFromDate = new Button
            {
                Text = "📅 انتخاب",
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill
            };
            btnFromDate.Click += (s, e) => ShowPersianDateTimePicker(true);

            Label lblToTitle = new Label { Text = "تا تاریخ و ساعت:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            lblToDate = new Label
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Dock = DockStyle.Fill
            };
            btnToDate = new Button
            {
                Text = "📅 انتخاب",
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill
            };
            btnToDate.Click += (s, e) => ShowPersianDateTimePicker(false);

            btnFilterByDate = new Button
            {
                Text = "اعمال فیلتر 🔎",
                BackColor = Color.LightGoldenrodYellow,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };
            btnFilterByDate.Click += BtnFilterByDate_Click;

            filterLayout.Controls.Add(lblFromTitle, 0, 0);
            filterLayout.Controls.Add(lblFromDate, 1, 0);
            filterLayout.Controls.Add(btnFromDate, 2, 0);
            filterLayout.Controls.Add(lblToTitle, 3, 0);
            filterLayout.Controls.Add(lblToDate, 4, 0);
            filterLayout.Controls.Add(btnToDate, 5, 0);
            filterLayout.Controls.Add(btnFilterByDate, 6, 0);

            btnResetFilter = new Button
            {
                Text = "❌ نمایش همه فاکتورها",
                BackColor = Color.WhiteSmoke,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnResetFilter.Click += BtnResetFilter_Click;

            Panel resetPanel = new Panel { Dock = DockStyle.Fill };
            resetPanel.Controls.Add(btnResetFilter);
            btnResetFilter.Location = new Point(resetPanel.Width - 160, 5);
            resetPanel.Resize += (s, e) => btnResetFilter.Location = new Point(resetPanel.Width - 160, 5);

            filterLayout.SetColumnSpan(resetPanel, 7);
            filterLayout.Controls.Add(resetPanel, 0, 1);

            grpDateFilter.Controls.Add(filterLayout);
            mainLayout.Controls.Add(grpDateFilter, 0, 0);

            // ========== ردیف 2: عنوان فاکتورها ==========
            Label lblInvTitle = new Label
            {
                Text = "📋 لیست فاکتورهای صادر شده:",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleRight
            };
            mainLayout.Controls.Add(lblInvTitle, 0, 1);

            // ========== ردیف 3: جدول فاکتورها ==========
            dgvInvoices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro
            };
            dgvInvoices.Columns.Add("Id", "شماره فاکتور");
            dgvInvoices.Columns.Add("Name", "نام مشتری");
            dgvInvoices.Columns.Add("Table", "شماره میز");
            dgvInvoices.Columns.Add("Total", "مبلغ کل (تومان)");
            dgvInvoices.SelectionChanged += dgvInvoices_SelectionChanged;
            mainLayout.Controls.Add(dgvInvoices, 0, 2);

            // ========== ردیف 4: عنوان اقلام ==========
            Label lblItemsTitle = new Label
            {
                Text = "📦 جزئیات اقلام فاکتور انتخاب شده:",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleRight
            };
            mainLayout.Controls.Add(lblItemsTitle, 0, 3);

            // ========== ردیف 5: جدول اقلام ==========
            dgvInvoiceItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro
            };
            dgvInvoiceItems.Columns.Add("ProdId", "کد کالا");
            dgvInvoiceItems.Columns.Add("ProdName", "نام محصول");
            dgvInvoiceItems.Columns.Add("Price", "قیمت واحد");
            dgvInvoiceItems.Columns.Add("Qty", "تعداد");
            dgvInvoiceItems.Columns.Add("Total", "قیمت کل");
            mainLayout.Controls.Add(dgvInvoiceItems, 0, 4);

            // ========== ردیف 6: جمع‌ها ==========
            TableLayoutPanel summaryLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(5),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // ستون اول: مجموع فروش کل
            lblFilteredTotalSummary = new Label
            {
                Text = "💰 مجموع فروش در بازه زمانی انتخاب شده: 0 تومان",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.LightYellow,
                Padding = new Padding(10, 0, 10, 0),
                AutoSize = false,
                MinimumSize = new Size(200, 35)
            };

            // ستون دوم: تفکیک روش‌های پرداخت
            lblPaymentBreakdown = new Label
            {
                Text = "💳 تفکیک روش‌های پرداخت:\nنقدی: 0\nکارت: 0\nانتقال: 0\nآنلاین: 0",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.LightCyan,
                Padding = new Padding(10, 5, 10, 5),
                AutoSize = true
            };

            summaryLayout.Controls.Add(lblFilteredTotalSummary, 0, 0);
            summaryLayout.Controls.Add(lblPaymentBreakdown, 1, 0);

            mainLayout.Controls.Add(summaryLayout, 0, 5);

            this.Controls.Add(mainLayout);
        }

        // ==================== متدهای تاریخ شمسی ====================

        private void ShowPersianDateTimePicker(bool isFromDate)
        {
            using (PersianDatePopup popup = new PersianDatePopup(isFromDate ? fromDateTime : toDateTime))
            {
                if (popup.ShowDialog(this) == DialogResult.OK)
                {
                    if (isFromDate)
                        fromDateTime = popup.SelectedDateTime;
                    else
                        toDateTime = popup.SelectedDateTime;

                    UpdateDateDisplays();
                }
            }
        }

        private void UpdateDateDisplays()
        {
            lblFromDate.Text = ConvertToPersianDateTime(fromDateTime);
            lblToDate.Text = ConvertToPersianDateTime(toDateTime);
        }

        private string ConvertToPersianDateTime(DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:00}/{day:00} {date.Hour:00}:{date.Minute:00}";
        }

        // ==================== متدهای گزارش ====================

        private void RefreshInvoiceGrid(List<Invoice> customList = null)
        {
            dgvInvoices.Rows.Clear();
            dgvInvoiceItems.Rows.Clear();

            var history = customList ?? CafeManager.GetSalesHistory();
            double totalSalesInPeriod = 0;

            var paymentBreakdown = new Dictionary<string, double>
            {
                { "نقدی", 0 },
                { "کارت", 0 },
                { "انتقال", 0 },
                { "آنلاین", 0 }
            };

            foreach (var inv in history)
            {
                dgvInvoices.Rows.Add(inv.Id, inv.CustomerName, inv.TableNumber, inv.TotalAmount.ToString("N0"));
                totalSalesInPeriod += inv.TotalAmount;

                if (inv.IsSettled)
                {
                    if (inv.Payments != null && inv.Payments.Count > 0)
                    {
                        foreach (var payment in inv.Payments)
                        {
                            string method = payment.Method;
                            if (paymentBreakdown.ContainsKey(method))
                                paymentBreakdown[method] += payment.Amount;
                            else
                                paymentBreakdown["نقدی"] += payment.Amount;
                        }
                    }
                    else if (!string.IsNullOrEmpty(inv.PayMethod))
                    {
                        string method = inv.PayMethod;
                        if (paymentBreakdown.ContainsKey(method))
                            paymentBreakdown[method] += inv.TotalAmount;
                        else
                            paymentBreakdown["نقدی"] += inv.TotalAmount;
                    }
                }
            }

            lblFilteredTotalSummary.Text = $"💰 مجموع فروش در بازه زمانی انتخاب شده: {totalSalesInPeriod:N0} تومان";

            StringBuilder breakdownText = new StringBuilder();
            breakdownText.AppendLine("💳 تفکیک روش‌های پرداخت:");
            foreach (var item in paymentBreakdown)
            {
                if (item.Value > 0)
                    breakdownText.AppendLine($"   {item.Key}: {item.Value:N0} تومان");
            }
            if (breakdownText.ToString().Trim() == "💳 تفکیک روش‌های پرداخت:")
                breakdownText.AppendLine("   هیچ پرداختی ثبت نشده است.");

            lblPaymentBreakdown.Text = breakdownText.ToString();
        }

        // ==================== رویدادها ====================

        private void BtnFilterByDate_Click(object sender, EventArgs e)
        {
            if (fromDateTime > toDateTime)
            {
                MessageBox.Show("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد!", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var filtered = CafeManager.GetSalesHistoryByDate(fromDateTime, toDateTime);
            RefreshInvoiceGrid(filtered);

            if (filtered.Count == 0)
            {
                MessageBox.Show("هیچ فاکتوری در این بازه زمانی یافت نشد.", "گزارش سیستم", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnResetFilter_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            fromDateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            toDateTime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 0);
            UpdateDateDisplays();
            RefreshInvoiceGrid();
        }

        private void dgvInvoices_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count > 0)
            {
                int invoiceId = Convert.ToInt32(dgvInvoices.SelectedRows[0].Cells["Id"].Value);
                var invoice = CafeManager.GetSalesHistory().Find(i => i.Id == invoiceId);

                dgvInvoiceItems.Rows.Clear();
                if (invoice != null)
                {
                    foreach (var item in invoice.Items)
                    {
                        dgvInvoiceItems.Rows.Add(item.Product.Id, item.Product.Name, item.Product.Price.ToString("N0"), item.Quantity, item.TotalPrice.ToString("N0"));
                    }
                }
            }
        }
    }

    
}