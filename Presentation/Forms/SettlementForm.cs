using System;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace CafeManager
{
    public class SettlementForm : Form
    {
        private ComboBox cmbGameMaster;
        private Button btnCalculate;
        private Button btnSettle;
        private DataGridView dgvGames;
        private Label lblTotalGames;
        private Label lblTotalRevenue;
        private Label lblTotalShare;
        private Label lblGameMasterName;
        private Label lblPeriod;
        private Label lblDebitCredit;
        private GroupBox grpSummary;
        private GroupBox grpGamesList;
        private Panel panelButtons;

        private Label lblFromDate;
        private Label lblToDate;
        private Button btnFromDate;
        private Button btnToDate;
        private DateTime fromDateTime;
        private DateTime toDateTime;
        private PersianCalendar pc = new PersianCalendar();

        private DateTime startDate;
        private DateTime endDate;
        private List<GameSession> currentGames;

        private Dictionary<string, double> gameMasterDebts = new Dictionary<string, double>();
        private Dictionary<string, double> gameMasterCredits = new Dictionary<string, double>();
        private Dictionary<string, int> gameMasterGameCount = new Dictionary<string, int>();

        private List<string> allGameMasters = new List<string>();

        public SettlementForm()
        {
            this.ShowInTaskbar = false;
            this.Text = "🧾 تسویه حساب گرداننده";
            this.Size = new Size(950, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.WindowState = FormWindowState.Maximized;

            DateTime now = DateTime.Now;
            fromDateTime = GetShiftStartTime();
            toDateTime = now;

            InitializeComponents();
            UpdateDateDisplays();
            LoadGameMasters();
        }

        private void InitializeComponents()
        {
            this.ShowInTaskbar = false;
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };

            Label lblMaster = new Label
            {
                Text = "نام گرداننده:",
                Location = new Point(topPanel.Width - 120, 15),
                Size = new Size(100, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            cmbGameMaster = new ComboBox
            {
                Location = new Point(topPanel.Width - 310, 12),
                Size = new Size(180, 25),
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbGameMaster.SelectedIndexChanged += CmbGameMaster_SelectedIndexChanged;
            cmbGameMaster.TextChanged += CmbGameMaster_TextChanged;
            cmbGameMaster.KeyDown += CmbGameMaster_KeyDown;

            Label lblFromTitle = new Label
            {
                Text = "از تاریخ :",
                Location = new Point(topPanel.Width - 470, 15),
                Size = new Size(100, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            lblFromDate = new Label
            {
                Location = new Point(topPanel.Width - 630, 12),
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
                Location = new Point(topPanel.Width - 800, 12),
                Size = new Size(80, 25),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnFromDate.Click += (s, e) => ShowPersianDateTimePicker(true);

            Label lblToTitle = new Label
            {
                Text = "تا تاریخ:",
                Location = new Point(topPanel.Width - 920, 15),
                Size = new Size(100, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            lblToDate = new Label
            {
                Location = new Point(topPanel.Width - 1080, 12),
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
                Location = new Point(topPanel.Width - 1160, 12),
                Size = new Size(80, 25),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnToDate.Click += (s, e) => ShowPersianDateTimePicker(false);

            btnCalculate = new Button
            {
                Text = "🧮 محاسبه",
                Location = new Point(topPanel.Width - 140, 50),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCalculate.Click += BtnCalculate_Click;

            topPanel.Controls.AddRange(new Control[]
            {
                lblMaster, cmbGameMaster,
                lblFromTitle, lblFromDate, btnFromDate,
                lblToTitle, lblToDate, btnToDate,
                btnCalculate
            });

            grpSummary = new GroupBox
            {
                Text = "📊 خلاصه تسویه",
                Dock = DockStyle.Top,
                Height = 100,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Padding = new Padding(10),
                RightToLeft = RightToLeft.Yes
            };

            lblGameMasterName = new Label
            {
                Location = new Point(20, 25),
                Size = new Size(350, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblPeriod = new Label
            {
                Location = new Point(20, 50),
                Size = new Size(450, 25),
                Font = new Font("Tahoma", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblTotalGames = new Label
            {
                Location = new Point(grpSummary.Width - 250, 25),
                Size = new Size(200, 25),
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            lblTotalRevenue = new Label
            {
                Location = new Point(grpSummary.Width - 300, 50),
                Size = new Size(250, 25),
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                ForeColor = Color.DarkOrange,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            lblTotalShare = new Label
            {
                Location = new Point(grpSummary.Width - 300, 75),
                Size = new Size(250, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            grpSummary.Controls.AddRange(new Control[]
            {
                lblGameMasterName, lblPeriod,
                lblTotalGames, lblTotalRevenue, lblTotalShare
            });

            lblDebitCredit = new Label
            {
                Dock = DockStyle.Top,
                Height = 80,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10, 5, 10, 5),
                AutoSize = false
            };

            grpGamesList = new GroupBox
            {
                Text = "📋 لیست بازی‌های این گرداننده",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Padding = new Padding(10),
                RightToLeft = RightToLeft.Yes
            };

            dgvGames = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 30,
                Font = new Font("Tahoma", 9),
                RightToLeft = RightToLeft.Yes
            };

            dgvGames.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvGames.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvGames.Columns.Add("GameName", "نام بازی");
            dgvGames.Columns.Add("GameMaster", "نام گرداننده");
            dgvGames.Columns.Add("TableNumber", "شماره میز");
            dgvGames.Columns.Add("PlayersCount", "تعداد بازیکنان");
            dgvGames.Columns.Add("StartTime", "زمان شروع");
            dgvGames.Columns.Add("EndTime", "زمان پایان");
            dgvGames.Columns.Add("Revenue", "فروش (تومان)");
            dgvGames.Columns.Add("Share", "سهم گرداننده (تومان)");
            dgvGames.Columns.Add("Status", "وضعیت");

            grpGamesList.Controls.Add(dgvGames);

            panelButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };

            btnSettle = new Button
            {
                Text = "✅ تسویه نهایی",
                Location = new Point(panelButtons.Width - 170, 15),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnSettle.Click += BtnSettle_Click;

            Button btnExport = new Button
            {
                Text = "📤 خروجی Excel",
                Location = new Point(panelButtons.Width - 330, 15),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnExport.Click += BtnExport_Click;

            Button btnClose = new Button
            {
                Text = "❌ بستن",
                Location = new Point(20, 15),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnClose.Click += (s, e) => this.Close();

            panelButtons.Controls.AddRange(new Control[]
            {
                btnSettle, btnExport, btnClose
            });

            this.Controls.Add(panelButtons);
            this.Controls.Add(grpGamesList);
            this.Controls.Add(lblDebitCredit);
            this.Controls.Add(grpSummary);
            this.Controls.Add(topPanel);
        }

        private void CmbGameMaster_TextChanged(object sender, EventArgs e)
        {
            string searchText = cmbGameMaster.Text.Trim();

            if (string.IsNullOrEmpty(searchText) || searchText == "همه گرداننده‌ها")
            {
                cmbGameMaster.Items.Clear();
                cmbGameMaster.Items.Add("همه گرداننده‌ها");
                foreach (var master in allGameMasters)
                {
                    cmbGameMaster.Items.Add(master);
                }
                cmbGameMaster.DroppedDown = false;
                return;
            }

            cmbGameMaster.Items.Clear();
            cmbGameMaster.Items.Add("همه گرداننده‌ها");

            var filtered = allGameMasters
                .Where(m => m.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var master in filtered)
            {
                cmbGameMaster.Items.Add(master);
            }

            if (filtered.Count > 0 && !cmbGameMaster.DroppedDown)
            {
                cmbGameMaster.DroppedDown = true;
            }
        }

        private void CmbGameMaster_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string newName = cmbGameMaster.Text.Trim();
                if (!string.IsNullOrEmpty(newName) && newName != "همه گرداننده‌ها")
                {
                    if (!allGameMasters.Contains(newName))
                    {
                        allGameMasters.Add(newName);
                        cmbGameMaster.Items.Add(newName);
                        cmbGameMaster.SelectedItem = newName;
                    }
                    else
                    {
                        cmbGameMaster.SelectedItem = newName;
                    }
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                cmbGameMaster.DroppedDown = false;
                e.Handled = true;
            }
        }

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

        private string ConvertToPersianDateWithTime(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:00}/{day:00} - {date.Hour:00}:{date.Minute:00}";
        }

        private void LoadGameMasters()
        {
            cmbGameMaster.Items.Clear();
            allGameMasters.Clear();

            cmbGameMaster.Items.Add("همه گرداننده‌ها");

            var masters = CafeManager.GetActiveGameMasters(
                fromDateTime,
                toDateTime
            );

            foreach (var master in masters)
            {
                if (!allGameMasters.Contains(master))
                {
                    allGameMasters.Add(master);
                    cmbGameMaster.Items.Add(master);
                }
            }

            if (cmbGameMaster.Items.Count > 0)
                cmbGameMaster.SelectedIndex = 0;
        }

        private void CmbGameMaster_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearSummary();
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            CalculateSettlement();
        }

        private void CalculateSettlement()
        {
            if (cmbGameMaster.SelectedItem == null)
            {
                MessageBox.Show("لطفاً یک گرداننده انتخاب کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedMaster = cmbGameMaster.SelectedItem.ToString();
            startDate = fromDateTime;
            endDate = toDateTime;

            if (startDate > endDate)
            {
                MessageBox.Show("تاریخ شروع نباید از تاریخ پایان بزرگتر باشد.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ========== دریافت بازی‌ها ==========
            List<GameSession> allGamesInRange;
            if (selectedMaster == "همه گرداننده‌ها")
            {
                allGamesInRange = CafeManager.CompletedGames
                    .Where(g => g.EndTime.HasValue &&
                               g.EndTime.Value >= startDate &&
                               g.EndTime.Value <= endDate)
                    .OrderBy(g => g.EndTime)
                    .ToList();
            }
            else
            {
                allGamesInRange = CafeManager.GetGamesByGameMasterAndDateRange(
                    selectedMaster, startDate, endDate);
            }

            // ========== فقط بازی‌های تسویه نشده ==========
            currentGames = allGamesInRange.Where(g => !g.IsSettled).ToList();

            // ========== دریافت فاکتورهای تسویه نشده گرداننده (اقلام مصرفی) ==========
            var allInvoices = CafeManager.GetSalesHistory();
            List<Invoice> gameMasterInvoices = new List<Invoice>();

            if (selectedMaster == "همه گرداننده‌ها")
            {
                foreach (var master in allGameMasters)
                {
                    var invoices = allInvoices
                        .Where(i => i.CustomerName == master &&
                                   !i.IsSettled &&
                                   i.OrderDate >= startDate &&
                                   i.OrderDate <= endDate)
                        .ToList();
                    gameMasterInvoices.AddRange(invoices);
                }
            }
            else
            {
                gameMasterInvoices = allInvoices
                    .Where(i => i.CustomerName == selectedMaster &&
                               !i.IsSettled &&
                               i.OrderDate >= startDate &&
                               i.OrderDate <= endDate)
                    .ToList();
            }

            double gameMasterItemsTotal = gameMasterInvoices.Sum(i => i.TotalAmount);
            int gameMasterItemsCount = gameMasterInvoices.Count;

            // ========== محاسبه بدهکار و بستانکار فقط از تسویه نشده‌ها ==========
            gameMasterDebts.Clear();
            gameMasterCredits.Clear();
            gameMasterGameCount.Clear();

            foreach (var game in currentGames)
            {
                string master = game.GameMasterName;

                if (!gameMasterDebts.ContainsKey(master))
                {
                    gameMasterDebts[master] = 0;
                    gameMasterCredits[master] = 0;
                    gameMasterGameCount[master] = 0;
                }

                // سهم گرداننده = بستانکاری (از دیتابیس)
                gameMasterCredits[master] += game.GameMasterShare;
                // فروش بازی = بدهی (از دیتابیس)
                gameMasterDebts[master] += game.TotalRevenue;
                gameMasterGameCount[master]++;
            }

            // اضافه کردن اقلام مصرفی تسویه نشده به بدهی
            foreach (var invoice in gameMasterInvoices)
            {
                string master = invoice.CustomerName;
                if (!gameMasterDebts.ContainsKey(master))
                {
                    gameMasterDebts[master] = 0;
                    gameMasterCredits[master] = 0;
                    gameMasterGameCount[master] = 0;
                }
                gameMasterDebts[master] += invoice.TotalAmount;
            }

            // ========== نمایش در جدول ==========
            dgvGames.Rows.Clear();
            foreach (var game in allGamesInRange)
            {
                string startTimePersian = ConvertToPersianDateWithTime(game.StartTime);
                string endTimePersian = game.EndTime.HasValue ? ConvertToPersianDateWithTime(game.EndTime.Value) : "-";
                string status = game.IsSettled ? "✅ تسویه شده" : "⏳ در انتظار تسویه";

                dgvGames.Rows.Add(
                    game.GameName,
                    game.GameMasterName,
                    game.TableNumber,
                    game.Players.Count,
                    startTimePersian,
                    endTimePersian,
                    game.TotalRevenue.ToString("N0"),
                    game.GameMasterShare.ToString("N0"),
                    status
                );
            }

            // ========== نمایش خلاصه ==========
            int totalGames = allGamesInRange.Count;
            int unsettledGamesCount = currentGames.Count;

            double totalRevenueUnsettled = currentGames.Sum(g => g.TotalRevenue);
            double totalShare = currentGames.Sum(g => g.GameMasterShare);
            double totalItemsCost = gameMasterInvoices.Sum(i => i.TotalAmount);

            lblGameMasterName.Text = $"👤 گرداننده: {selectedMaster}";
            lblPeriod.Text = $"📅 بازه زمانی: {ConvertToPersianDateWithTime(startDate)} تا {ConvertToPersianDateWithTime(endDate)}";
            lblTotalGames.Text = $"🎮 تعداد بازی‌ها: {totalGames} (تسویه نشده: {unsettledGamesCount})";
            lblTotalRevenue.Text = $"💰 فروش تسویه نشده: {totalRevenueUnsettled:N0} تومان";
            lblTotalShare.Text = $"💵 سهم گرداننده (تسویه نشده): {totalShare:N0} تومان";

            // ========== نمایش مانده نهایی ==========
            if (selectedMaster == "همه گرداننده‌ها")
            {
                string debitCreditText = "📊 خلاصه مانده نهایی گرداننده‌ها (فقط تسویه نشده):\n";
                debitCreditText += "═══════════════════════════════════════════════════════════════════════════════════\n";
                foreach (var master in gameMasterDebts.Keys)
                {
                    double credit = gameMasterCredits.ContainsKey(master) ? gameMasterCredits[master] : 0;
                    double itemsCost = gameMasterInvoices.Where(i => i.CustomerName == master).Sum(i => i.TotalAmount);
                    int itemsCount = gameMasterInvoices.Where(i => i.CustomerName == master).Count();

                    // ========== مانده نهایی = سهم گرداننده - هزینه اقلام مصرفی ==========
                    double balance = credit - itemsCost;
                    int gameCount = gameMasterGameCount.ContainsKey(master) ? gameMasterGameCount[master] : 0;

                    string balanceStatus = balance >= 0 ? "🟢 بستانکار" : "🔴 بدهکار";
                    double absBalance = Math.Abs(balance);

                    debitCreditText += $"👤 {master}:\n";
                    debitCreditText += $"   💵 سهم گرداننده: +{credit:N0} تومان\n";
                    if (itemsCost > 0)
                        debitCreditText += $"   🧾 هزینه اقلام مصرفی (تسویه نشده): -{itemsCost:N0} تومان ({itemsCount} مورد)\n";
                    debitCreditText += $"   📊 مانده نهایی: {balanceStatus} {absBalance:N0} تومان\n";
                    debitCreditText += "───────────────────────────────────────────────────────────────────────────────\n";
                }
                lblDebitCredit.Text = debitCreditText;
                lblDebitCredit.ForeColor = Color.DarkBlue;
            }
            else
            {
                double credit = gameMasterCredits.ContainsKey(selectedMaster) ? gameMasterCredits[selectedMaster] : 0;
                double itemsCost = gameMasterInvoices.Where(i => i.CustomerName == selectedMaster).Sum(i => i.TotalAmount);
                int itemsCount = gameMasterInvoices.Where(i => i.CustomerName == selectedMaster).Count();

                // ========== مانده نهایی = سهم گرداننده - هزینه اقلام مصرفی ==========
                double balance = credit - itemsCost;
                int gameCount = gameMasterGameCount.ContainsKey(selectedMaster) ? gameMasterGameCount[selectedMaster] : 0;

                string balanceStatus = balance >= 0 ? "🟢 بستانکار" : "🔴 بدهکار";
                Color balanceColor = balance >= 0 ? Color.Green : Color.Red;
                double absBalance = Math.Abs(balance);

                string debitCreditText = $"📊 مانده نهایی {selectedMaster} (فقط تسویه نشده):\n";
                debitCreditText += $"   💵 سهم گرداننده: +{credit:N0} تومان\n";
                if (itemsCost > 0)
                    debitCreditText += $"   🧾 هزینه اقلام مصرفی (تسویه نشده): -{itemsCost:N0} تومان ({itemsCount} مورد)\n";
                debitCreditText += $"   📊 مانده نهایی: {balanceStatus} {absBalance:N0} تومان";

                lblDebitCredit.Text = debitCreditText;
                lblDebitCredit.ForeColor = balanceColor;
            }

            btnSettle.Enabled = currentGames.Count > 0 && selectedMaster != "همه گرداننده‌ها";
        }

        private void BtnSettle_Click(object sender, EventArgs e)
        {
            if (currentGames == null || currentGames.Count == 0)
            {
                MessageBox.Show("هیچ بازی برای تسویه وجود ندارد.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedMaster = cmbGameMaster.SelectedItem.ToString();
            if (selectedMaster == "همه گرداننده‌ها")
            {
                MessageBox.Show("لطفاً یک گرداننده خاص را برای تسویه انتخاب کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // فقط بازی‌های تسویه نشده
            var unsettledGames = currentGames.Where(g => !g.IsSettled).ToList();

            // فاکتورهای تسویه نشده گرداننده
            var allInvoices = CafeManager.GetSalesHistory();
            var unsettledInvoices = allInvoices
                .Where(i => i.CustomerName == selectedMaster &&
                           !i.IsSettled &&
                           i.OrderDate >= startDate &&
                           i.OrderDate <= endDate)
                .ToList();

            if (unsettledGames.Count == 0 && unsettledInvoices.Count == 0)
            {
                MessageBox.Show($"همه موارد {selectedMaster} قبلاً تسویه شده‌اند.", "اطلاعات",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ========== محاسبه مانده نهایی ==========
            double totalCredit = unsettledGames.Sum(g => g.GameMasterShare);
            double totalItemsCost = unsettledInvoices.Sum(i => i.TotalAmount);
            double balance = totalCredit - totalItemsCost; // سهم گرداننده - هزینه اقلام مصرفی
            double absBalance = Math.Abs(balance);
            string balanceStatus = balance >= 0 ? "بستانکار" : "بدهکار";

            string message = $"آیا از تسویه نهایی {selectedMaster} مطمئن هستید؟\n\n";
            if (unsettledGames.Count > 0)
                message += $"تعداد بازی‌های تسویه نشده: {unsettledGames.Count}\n";
            if (unsettledInvoices.Count > 0)
                message += $"تعداد فاکتورهای تسویه نشده: {unsettledInvoices.Count}\n";
            message += $"💰 سهم گرداننده: +{totalCredit:N0} تومان\n";
            if (unsettledInvoices.Count > 0)
                message += $"🧾 هزینه اقلام مصرفی: -{totalItemsCost:N0} تومان\n";
            message += $"📊 مانده نهایی: {balanceStatus} {absBalance:N0} تومان\n\n" +
                      $"⚠️ پس از تسویه، این موارد قابل ویرایش نخواهند بود.";

            DialogResult result = MessageBox.Show(
                message,
                "تأیید تسویه",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // تسویه بازی‌ها
                foreach (var game in unsettledGames)
                {
                    game.IsSettled = true;
                    game.SettlementDate = DateTime.Now;
                }

                // تسویه فاکتورهای گرداننده
                foreach (var invoice in unsettledInvoices)
                {
                    invoice.IsSettled = true;
                    CafeManager.UpdateSettlementStatus(invoice.Id, true, "تسویه با گرداننده");
                }

                CafeManager.UpdateGamesSettlement(unsettledGames);

                string finalStatus = balance >= 0 ? "بستانکار" : "بدهکار";

                MessageBox.Show($"✅ تسویه {selectedMaster} با موفقیت انجام شد.\n" +
                               $"تعداد بازی‌های تسویه شده: {unsettledGames.Count}\n" +
                               $"تعداد فاکتورهای تسویه شده: {unsettledInvoices.Count}\n" +
                               $"💰 مانده نهایی: {finalStatus} {absBalance:N0} تومان",
                               "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);

                btnSettle.Enabled = false;
                CalculateSettlement();
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (currentGames == null || currentGames.Count == 0)
            {
                MessageBox.Show("هیچ داده‌ای برای خروجی وجود ندارد.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("خروجی Excel در حال پیاده‌سازی است.", "اطلاعات",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearSummary()
        {
            lblGameMasterName.Text = "";
            lblPeriod.Text = "";
            lblTotalGames.Text = "";
            lblTotalRevenue.Text = "";
            lblTotalShare.Text = "";
            lblDebitCredit.Text = "";
            dgvGames.Rows.Clear();
            btnSettle.Enabled = false;
        }

        private DateTime GetShiftStartTime()
        {
            DateTime now = DateTime.Now;
            DateTime shiftStart = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0);
            if (now.Hour < 15)
            {
                shiftStart = shiftStart.AddDays(-1);
            }
            return shiftStart;
        }
    }
}