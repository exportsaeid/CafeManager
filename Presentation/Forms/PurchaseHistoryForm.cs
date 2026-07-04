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
    public class PurchaseHistoryForm : Form
    {
        private Button btnFromDate;
        private Button btnToDate;
        private Label lblFromDate;
        private Label lblToDate;
        private Button btnSearch;
        private Button btnRefresh;
        private Button btnConfirmPurchase;
        private Button btnClose;
        private DataGridView dgvPurchases;
        private DataGridView dgvPurchaseItems; // جدول جدید برای نمایش اقلام
        private Label lblTotalAmount;
        private Label lblCount;
        private Label lblItemsTitle;

        private DateTime fromDateTime;
        private DateTime toDateTime;
        private PersianCalendar pc = new PersianCalendar();
        private List<Purchase> _purchases;
        private List<PurchaseItem> _currentItems = new List<PurchaseItem>();

        public PurchaseHistoryForm()
        {
            this.Text = "📊 تاریخچه خریدهای انبار";
            this.ShowInTaskbar = false; // ← این خط را اضافه کنید
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            DateTime now = DateTime.Now;
            fromDateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            toDateTime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            InitializeComponents();
            UpdateDateDisplays();
            LoadPurchases(fromDateTime, toDateTime);
        }

        private void InitializeComponents()
        {
            int margin = 20;
            int y = 20;

            // ========== پنل جستجو ==========
            Panel pnlSearch = new Panel
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 75),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.WhiteSmoke
            };

            // از تاریخ
            Label lblFromTitle = new Label
            {
                Text = "از تاریخ:",
                Location = new Point(pnlSearch.Width - 130, 8),
                Size = new Size(60, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            lblFromDate = new Label
            {
                Location = new Point(pnlSearch.Width - 350, 8),
                Size = new Size(200, 25),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            btnFromDate = new Button
            {
                Text = "📅 انتخاب",
                Location = new Point(pnlSearch.Width - 410, 6),
                Size = new Size(55, 28),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };
            btnFromDate.Click += (s, e) => ShowPersianDateTimePicker(true);

            // تا تاریخ
            Label lblToTitle = new Label
            {
                Text = "تا تاریخ:",
                Location = new Point(pnlSearch.Width - 520, 8),
                Size = new Size(60, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            lblToDate = new Label
            {
                Location = new Point(pnlSearch.Width - 740, 8),
                Size = new Size(200, 25),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            btnToDate = new Button
            {
                Text = "📅 انتخاب",
                Location = new Point(pnlSearch.Width - 800, 6),
                Size = new Size(55, 28),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };
            btnToDate.Click += (s, e) => ShowPersianDateTimePicker(false);

            btnSearch = new Button
            {
                Text = "🔍 جستجو",
                Location = new Point(pnlSearch.Width - 890, 6),
                Size = new Size(80, 30),
                BackColor = Color.LightBlue,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSearch.Click += BtnSearch_Click;

            btnRefresh = new Button
            {
                Text = "🔄 نمایش همه",
                Location = new Point(20, 6),
                Size = new Size(100, 30),
                BackColor = Color.LightGray,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlSearch.Controls.Add(lblFromTitle);
            pnlSearch.Controls.Add(lblFromDate);
            pnlSearch.Controls.Add(btnFromDate);
            pnlSearch.Controls.Add(lblToTitle);
            pnlSearch.Controls.Add(lblToDate);
            pnlSearch.Controls.Add(btnToDate);
            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(btnRefresh);

            this.Controls.Add(pnlSearch);
            y += pnlSearch.Height + 10;

            // ========== عنوان جدول خریدها ==========
            Label lblPurchasesTitle = new Label
            {
                Text = "📋 لیست خریدهای ثبت‌شده:",
                Location = new Point(margin, y),
                Size = new Size(300, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblPurchasesTitle);
            y += 30;

            // ========== جدول خریدها ==========
            dgvPurchases = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 220),
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

            dgvPurchases.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPurchases.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvPurchases.Columns.Add("Id", "شماره خرید");
            dgvPurchases.Columns.Add("Date", "تاریخ خرید (شمسی)");
            dgvPurchases.Columns.Add("Supplier", "تامین‌کننده");
            dgvPurchases.Columns.Add("InvoiceNumber", "شماره فاکتور");
            dgvPurchases.Columns.Add("ItemsCount", "تعداد اقلام");
            dgvPurchases.Columns.Add("TotalAmount", "جمع کل (تومان)");
            dgvPurchases.Columns.Add("Status", "وضعیت");

            dgvPurchases.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
            dgvPurchases.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvPurchases.SelectionChanged += DgvPurchases_SelectionChanged;

            this.Controls.Add(dgvPurchases);
            y += dgvPurchases.Height + 10;

            // ========== عنوان جدول اقلام ==========
            lblItemsTitle = new Label
            {
                Text = "📦 اقلام خرید انتخاب شده:",
                Location = new Point(margin, y),
                Size = new Size(300, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblItemsTitle);
            y += 30;

            // ========== جدول اقلام ==========
            dgvPurchaseItems = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 150),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 30,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvPurchaseItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPurchaseItems.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvPurchaseItems.Columns.Add("ProductId", "کد کالا");
            dgvPurchaseItems.Columns.Add("ProductName", "نام کالا");
            dgvPurchaseItems.Columns.Add("Quantity", "تعداد");
            dgvPurchaseItems.Columns.Add("UnitPrice", "قیمت واحد");
            dgvPurchaseItems.Columns.Add("Total", "جمع");

            dgvPurchaseItems.Columns["Quantity"].DefaultCellStyle.Format = "N0";
            dgvPurchaseItems.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
            dgvPurchaseItems.Columns["Total"].DefaultCellStyle.Format = "N0";
            dgvPurchaseItems.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvPurchaseItems.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            this.Controls.Add(dgvPurchaseItems);
            y += dgvPurchaseItems.Height + 10;

            // ========== پنل پایین ==========
            Panel pnlBottom = new Panel
            {
                Location = new Point(margin, this.ClientSize.Height - 100),
                Size = new Size(this.ClientSize.Width - (margin * 2), 90),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.WhiteSmoke
            };

            lblCount = new Label
            {
                Location = new Point(0, 10),
                Size = new Size(200, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Text = "تعداد خریدها: 0"
            };

            lblTotalAmount = new Label
            {
                Location = new Point(220, 10),
                Size = new Size(350, 30),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Text = "💰 جمع کل: 0 تومان"
            };

            btnConfirmPurchase = new Button
            {
                Text = "✅ تایید خرید انتخاب‌شده",
                Location = new Point(pnlBottom.Width - 270, 8),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnConfirmPurchase.Click += BtnConfirmPurchase_Click;

            btnClose = new Button
            {
                Text = "❌ بستن",
                Location = new Point(pnlBottom.Width - 80, 8),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(lblCount);
            pnlBottom.Controls.Add(lblTotalAmount);
            pnlBottom.Controls.Add(btnConfirmPurchase);
            pnlBottom.Controls.Add(btnClose);

            this.Controls.Add(pnlBottom);

            // ========== رویداد Resize ==========
            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width - (margin * 2);
                int h = this.ClientSize.Height;

                pnlSearch.Width = w;

                // تنظیم ارتفاع جدول‌ها
                int purchasesHeight = 220;
                int itemsHeight = 150;
                int bottomHeight = 100;
                int remainingHeight = h - pnlSearch.Height - 120 - bottomHeight - 30 - 20;

                // تقسیم فضای باقیمانده بین دو جدول (نسبت 60% و 40%)
                purchasesHeight = (int)(remainingHeight * 0.58);
                itemsHeight = remainingHeight - purchasesHeight - 30; // 30 برای عنوان اقلام

                if (purchasesHeight < 100) purchasesHeight = 100;
                if (itemsHeight < 80) itemsHeight = 80;

                dgvPurchases.Height = purchasesHeight;
                dgvPurchases.Width = w;

                // تنظیم موقعیت عنوان و جدول اقلام
                int itemsY = dgvPurchases.Location.Y + dgvPurchases.Height + 10;
                lblItemsTitle.Location = new Point(margin, itemsY);
                dgvPurchaseItems.Location = new Point(margin, itemsY + 30);
                dgvPurchaseItems.Height = itemsHeight;
                dgvPurchaseItems.Width = w;

                pnlBottom.Location = new Point(margin, h - bottomHeight);
                pnlBottom.Width = w;

                btnConfirmPurchase.Location = new Point(pnlBottom.Width - 270, 8);
                btnClose.Location = new Point(pnlBottom.Width - 80, 8);

                // تنظیم موقعیت کنترل‌های پنل جستجو
                btnFromDate.Location = new Point(pnlSearch.Width - 410, 6);
                lblFromDate.Location = new Point(pnlSearch.Width - 350, 8);
                btnToDate.Location = new Point(pnlSearch.Width - 800, 6);
                lblToDate.Location = new Point(pnlSearch.Width - 740, 8);
                btnSearch.Location = new Point(pnlSearch.Width - 890, 6);
            };
        }

        // ========== رویداد انتخاب سطر در جدول خریدها ==========
        private void DgvPurchases_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPurchases.SelectedRows.Count > 0)
            {
                int purchaseId = Convert.ToInt32(dgvPurchases.SelectedRows[0].Cells["Id"].Value);
                var purchase = _purchases.FirstOrDefault(p => p.Id == purchaseId);
                if (purchase != null)
                {
                    DisplayPurchaseItems(purchase);
                    btnConfirmPurchase.Enabled = !purchase.IsConfirmed;
                }
            }
            else
            {
                dgvPurchaseItems.Rows.Clear();
                btnConfirmPurchase.Enabled = false;
            }
        }

        private void DisplayPurchaseItems(Purchase purchase)
        {
            dgvPurchaseItems.Rows.Clear();

            if (purchase.Items.Count == 0)
            {
                dgvPurchaseItems.Rows.Add("", "هیچ اقلامی ثبت نشده", "", "", "");
                return;
            }

            foreach (var item in purchase.Items)
            {
                var product = CafeManager.GetProducts().FirstOrDefault(p => p.Id == item.ProductId);
                string productName = product != null ? product.Name : "نامشخص";
                dgvPurchaseItems.Rows.Add(
                    item.ProductId,
                    productName,
                    item.Quantity.ToString("N0"),
                    item.UnitPurchasePrice.ToString("N0"),
                    item.TotalPrice.ToString("N0")
                );
            }
        }

        // ========== متدهای تاریخ شمسی ==========

        private void ShowPersianDateTimePicker(bool isFromDate)
        {
            using (PersianDatePopup popup = new PersianDatePopup(isFromDate ? fromDateTime : toDateTime))
            {
                if (popup.ShowDialog(this) == DialogResult.OK)
                {
                    if (isFromDate)
                    {
                        fromDateTime = new DateTime(popup.SelectedDateTime.Year, popup.SelectedDateTime.Month,
                            popup.SelectedDateTime.Day, 0, 0, 0);
                    }
                    else
                    {
                        toDateTime = new DateTime(popup.SelectedDateTime.Year, popup.SelectedDateTime.Month,
                            popup.SelectedDateTime.Day, 23, 59, 59);
                    }
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

        private string ConvertToPersianDateTime(DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:00}/{day:00} - {date.Hour:00}:{date.Minute:00}";
        }

        // ========== متدهای اصلی ==========

        private void LoadPurchases(DateTime from, DateTime to)
        {
            _purchases = CafeManager.GetPurchases(from, to);
            DisplayPurchases(_purchases);
            dgvPurchaseItems.Rows.Clear();
            btnConfirmPurchase.Enabled = false;
        }

        private void DisplayPurchases(List<Purchase> purchases)
        {
            dgvPurchases.Rows.Clear();

            double total = 0;
            foreach (var p in purchases.OrderByDescending(p => p.PurchaseDate))
            {
                string status = p.IsConfirmed ? "✅ تایید شده" : "⏳ تایید نشده";
                string persianDateTime = ConvertToPersianDateTime(p.PurchaseDate);

                dgvPurchases.Rows.Add(
                    p.Id,
                    persianDateTime,
                    p.SupplierName,
                    p.InvoiceNumber,
                    p.Items.Count,
                    p.TotalAmount.ToString("N0"),
                    status
                );

                if (p.IsConfirmed)
                {
                    dgvPurchases.Rows[dgvPurchases.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else
                {
                    dgvPurchases.Rows[dgvPurchases.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightYellow;
                }

                total += p.TotalAmount;
            }

            lblCount.Text = $"تعداد خریدها: {purchases.Count}";
            lblTotalAmount.Text = $"💰 جمع کل: {total:N0} تومان";
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadPurchases(fromDateTime, toDateTime);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            fromDateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            toDateTime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            UpdateDateDisplays();
            LoadPurchases(fromDateTime, toDateTime);
        }

        private void BtnConfirmPurchase_Click(object sender, EventArgs e)
        {
            if (dgvPurchases.SelectedRows.Count == 0) return;

            int purchaseId = Convert.ToInt32(dgvPurchases.SelectedRows[0].Cells["Id"].Value);
            var purchase = _purchases.FirstOrDefault(p => p.Id == purchaseId);
            if (purchase == null || purchase.IsConfirmed) return;

            // نمایش خلاصه خرید قبل از تایید
            string summary = $"📊 خلاصه خرید شماره {purchase.Id}\n\n" +
                            $"تامین‌کننده: {purchase.SupplierName}\n" +
                            $"تعداد اقلام: {purchase.Items.Count}\n" +
                            $"جمع کل: {purchase.TotalAmount:N0} تومان\n\n" +
                            $"آیا از تایید این خرید اطمینان دارید؟\n" +
                            $"پس از تایید، موجودی انبار به‌روز می‌شود.";

            DialogResult result = MessageBox.Show(summary, "تایید خرید", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                CafeManager.ConfirmPurchase(purchaseId);
                MessageBox.Show("✅ خرید با موفقیت تایید شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPurchases(fromDateTime, toDateTime);
            }
        }
    }
}