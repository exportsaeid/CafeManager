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
    public class GameManagementForm : Form
    {
        private TextBox txtGMName, txtTableNumber, txtPlayerName;
        private NumericUpDown numPercent;
        private ListBox lstPlayers;
        private Button btnSave, btnAddPlayer, btnEnd;
        private ListBox lstGameMasters;
        private Label lblGameMasterTitle, lblPlayersTitle;
        private GroupBox detailsBox;
        private TableLayoutPanel tblDetails;

        // کنترل‌های جدید برای انتخاب بازی
        private ComboBox cmbSelectGame;
        private Label lblSelectGame;

        // ========== تغییر: استفاده از TextBox + Button به جای DateTimePicker ==========
        private TextBox txtStartTime;
        private Button btnSelectDate;
        private DateTime selectedStartTime = DateTime.Now;

        // ========== کنترل‌های جدید برای نمایش بازی‌های تمام‌شده ==========
        private GroupBox grpCompletedGames;
        private DataGridView dgvCompletedGames;
        private Label lblCompletedTitle;
        private Button btnShowAllGames;
        private Button btnEditGame;
        private Button btnDeleteGame;
        private Label lblShiftInfo;
        private bool isShowingAllGames = false;

        // ========== متغیر سشن جاری ==========
        private GameSession currentSession = null;

        // ========== تقویم فارسی ==========
        private PersianCalendar pc = new PersianCalendar();

        public GameManagementForm()
        {
            // ابتدا تنظیمات اصلی فرم مانند FormMain
            this.Text = "مدیریت بازی‌ها و اعضای تیم";
            this.Size = new Size(950, 560);
            this.StartPosition = FormStartPosition.CenterScreen;

            // تنظیمات RTL مانند FormMain
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);

            // تنظیمات دیگر
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Maximized;

            // بارگذاری فرم
            this.Load += GameManagementForm_Load;
        }

        // ========== متدهای تبدیل تاریخ به شمسی ==========
        private string ConvertToPersianDate(DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:00}/{day:00}";
        }

        private string ConvertToPersianDateWithTime(DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            int hour = date.Hour;
            int minute = date.Minute;
            return $"{year:0000}/{month:00}/{day:00} {hour:00}:{minute:00}";
        }

        private void GameManagementForm_Load(object sender, EventArgs e)
        {
            InitializeControls();
            RefreshGameLists();
            LoadGamesIntoCombo();
            LoadCompletedGames();
            this.Resize += GameManagementForm_Resize;
            AdjustControlsSizeAndPosition();
            UpdateShiftInfoLabel();
        }

        // ========== متدهای مربوط به شیفت کاری ==========

        // متد محاسبه شروع شیفت (ساعت 15)
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

        // متد محاسبه پایان شیفت (ساعت 15 روز بعد)
        private DateTime GetShiftEndTime()
        {
            return GetShiftStartTime().AddHours(24);
        }

        // گرفتن بازی‌های تمام‌شده در شیفت جاری
        private List<GameSession> GetCurrentShiftCompletedGames()
        {
            DateTime shiftStart = GetShiftStartTime();
            DateTime shiftEnd = GetShiftEndTime();

            return CafeManager.CompletedGames
                .Where(g => g.EndTime.HasValue && g.EndTime.Value >= shiftStart && g.EndTime.Value < shiftEnd)
                .ToList();
        }

        // گرفتن همه بازی‌های تمام‌شده
        private List<GameSession> GetAllCompletedGames()
        {
            return CafeManager.CompletedGames.ToList();
        }

        private void UpdateShiftInfoLabel()
        {
            if (!isShowingAllGames)
            {
                DateTime shiftStart = GetShiftStartTime();
                string shiftDate = ConvertToPersianDate(shiftStart);
                int count = GetCurrentShiftCompletedGames().Count;
                lblShiftInfo.Text = $"🕒 شیفت جاری (۱۵ تا ۱۵): {shiftDate} | بازی‌های تمام‌شده: {count}";
                lblShiftInfo.ForeColor = Color.DarkGreen;
            }
            else
            {
                int count = GetAllCompletedGames().Count;
                lblShiftInfo.Text = $"📋 نمایش همه بازی‌ها | مجموع: {count} بازی تمام‌شده";
                lblShiftInfo.ForeColor = Color.DarkBlue;
            }
        }

        private void BtnShowAllGames_Click(object sender, EventArgs e)
        {
            if (!isShowingAllGames)
            {
                isShowingAllGames = true;
                btnShowAllGames.Text = "🔄 بازگشت به شیفت جاری";
                btnShowAllGames.BackColor = Color.FromArgb(46, 204, 113);

                LoadCompletedGames();
                UpdateShiftInfoLabel();

                MessageBox.Show("حالت نمایش به \"همه بازی‌ها\" تغییر کرد.\nبرای بازگشت به شیفت جاری، دکمه مربوطه را بزنید.",
                    "تغییر حالت نمایش", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                isShowingAllGames = false;
                btnShowAllGames.Text = "📋 نمایش همه بازی‌ها";
                btnShowAllGames.BackColor = Color.FromArgb(52, 152, 219);

                LoadCompletedGames();
                UpdateShiftInfoLabel();

                DateTime shiftStart = GetShiftStartTime();
                DateTime shiftEnd = GetShiftEndTime();
                string shiftStartPersian = ConvertToPersianDateWithTime(shiftStart);
                string shiftEndPersian = ConvertToPersianDateWithTime(shiftEnd);

                MessageBox.Show($"بازگشت به شیفت جاری:\nاز {shiftStartPersian}\nتا {shiftEndPersian}",
                    "تغییر حالت نمایش", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ========== متدهای محاسبه فروش بر اساس تعداد بازیکنان ==========
        private double GetGamePrice(string gameName)
        {
            var allProducts = CafeManager.GetMenu();
            var gameProduct = allProducts.FirstOrDefault(p => p.Name == gameName);
            return gameProduct?.Price ?? 50000; // قیمت پیش‌فرض ۵۰,۰۰۰ تومان
        }

        private double CalculateGameRevenue(GameSession session)
        {
            if (session == null || session.Players == null || session.Players.Count == 0)
                return 0;

            double gamePrice = GetGamePrice(session.GameName);
            int playerCount = session.Players.Count;

            // فروش = تعداد بازیکنان × قیمت بازی
            return playerCount * gamePrice;
        }

        private double CalculateGameMasterShare(GameSession session)
        {
            double revenue = CalculateGameRevenue(session);
            return revenue * (session.RevenuePercent / 100);
        }
        // ================================================================

        private void LoadCompletedGames()
        {
            dgvCompletedGames.Rows.Clear();

            List<GameSession> gamesToShow;

            if (!isShowingAllGames)
            {
                gamesToShow = GetCurrentShiftCompletedGames();
            }
            else
            {
                gamesToShow = GetAllCompletedGames();
            }

            // مرتب‌سازی بر اساس زمان پایان (جدیدترین اول)
            var sortedGames = gamesToShow.OrderByDescending(g => g.EndTime).ToList();

            foreach (var game in sortedGames)
            {
                string startDate = ConvertToPersianDateWithTime(game.StartTime);
                string endDate = game.EndTime.HasValue ? ConvertToPersianDateWithTime(game.EndTime.Value) : "-";

                TimeSpan duration = game.EndTime.HasValue ? game.EndTime.Value - game.StartTime : TimeSpan.Zero;
                string durationText = $"{duration.Hours}h {duration.Minutes}m";

                // تعداد بازیکنانی که پرداخت کرده‌اند
                int paidCount = game.PlayerPayments.Count(p => p.Value > 0);

                // ========== محاسبه فروش و سهم گرداننده بر اساس تعداد بازیکنان ==========
                double revenue = CalculateGameRevenue(game);
                double gmShare = CalculateGameMasterShare(game);

                dgvCompletedGames.Rows.Add(
                    game.GameName,
                    game.GameMasterName,
                    game.TableNumber,
                    $"{game.Players.Count} ({paidCount} پرداخت)",
                    startDate,
                    endDate,
                    durationText,
                    revenue.ToString("N0"),
                    gmShare.ToString("N0")
                );
            }

            // به‌روزرسانی عنوان
            int count = sortedGames.Count;
            if (!isShowingAllGames)
            {
                lblCompletedTitle.Text = $"✅ بازی‌های تمام‌شده در شیفت جاری ({count} مورد)";
            }
            else
            {
                lblCompletedTitle.Text = $"📋 همه بازی‌های تمام‌شده ({count} مورد)";
            }
        }

        // ========== متد دریافت بازی انتخاب شده از دیتاگریڈ ==========
        private GameSession GetSelectedGameFromGrid()
        {
            if (dgvCompletedGames.SelectedRows.Count == 0)
                return null;

            DataGridViewRow selectedRow = dgvCompletedGames.SelectedRows[0];
            string gameName = selectedRow.Cells["GameName"].Value?.ToString() ?? "";
            string gameMaster = selectedRow.Cells["GameMaster"].Value?.ToString() ?? "";
            string tableNumber = selectedRow.Cells["Table"].Value?.ToString() ?? "";
            string startTimeStr = selectedRow.Cells["StartTime"].Value?.ToString() ?? "";

            // پیدا کردن سشن مربوطه
            List<GameSession> gamesToSearch = isShowingAllGames ?
                GetAllCompletedGames() : GetCurrentShiftCompletedGames();

            foreach (var game in gamesToSearch)
            {
                string gameStartTime = ConvertToPersianDateWithTime(game.StartTime);
                if (game.GameName == gameName &&
                    game.GameMasterName == gameMaster &&
                    game.TableNumber == tableNumber &&
                    gameStartTime == startTimeStr)
                {
                    return game;
                }
            }

            return null;
        }

        // ========== متد ویرایش بازی ==========
        private void BtnEditGame_Click(object sender, EventArgs e)
        {
            GameSession selectedGame = GetSelectedGameFromGrid();

            if (selectedGame == null)
            {
                MessageBox.Show("لطفاً یک بازی را برای ویرایش انتخاب کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // نمایش اطلاعات در پنل جزئیات برای ویرایش
            currentSession = selectedGame;

            txtTableNumber.Text = selectedGame.TableNumber;
            txtGMName.Text = selectedGame.GameMasterName;
            numPercent.Value = (decimal)selectedGame.RevenuePercent;

            // ========== تغییر: استفاده از selectedStartTime ==========
            selectedStartTime = selectedGame.StartTime;
            txtStartTime.Text = ConvertToPersianDateWithTime(selectedStartTime);

            // انتخاب بازی در کومبوباکس
            for (int i = 0; i < cmbSelectGame.Items.Count; i++)
            {
                if (cmbSelectGame.Items[i] is GameItem item && item.Name == selectedGame.GameName)
                {
                    cmbSelectGame.SelectedIndex = i;
                    break;
                }
            }

            // نمایش بازیکنان
            lstPlayers.Items.Clear();
            foreach (var player in selectedGame.Players)
            {
                lstPlayers.Items.Add(player);
            }

            // تغییر دکمه ذخیره به بروزرسانی
            btnSave.Text = "🔄 بروزرسانی بازی";
            btnSave.BackColor = Color.FromArgb(52, 152, 219);
            btnSave.Click -= BtnSave_Click;
            btnSave.Click += BtnUpdateGame_Click;

            // غیرفعال کردن انتخاب از لیست بازی‌های فعال
            lstGameMasters.Enabled = false;

            MessageBox.Show($"✏️ در حال ویرایش بازی: {selectedGame.GameName}\n" +
                           "پس از اعمال تغییرات، دکمه بروزرسانی را بزنید.",
                           "ویرایش بازی", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdateGame_Click(object sender, EventArgs e)
        {
            if (currentSession == null)
            {
                MessageBox.Show("هیچ بازی برای بروزرسانی انتخاب نشده است.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // اعتبارسنجی
            if (cmbSelectGame.SelectedItem == null || cmbSelectGame.SelectedItem.ToString() == "هیچ بازی‌ای یافت نشد")
            {
                MessageBox.Show("لطفاً یک بازی را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTableNumber.Text))
            {
                MessageBox.Show("لطفاً شماره میز را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtGMName.Text))
            {
                MessageBox.Show("لطفاً نام گرداننده را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // دریافت بازی از لیست کامل
            GameSession originalGame = null;
            foreach (var game in CafeManager.CompletedGames)
            {
                if (game.Id == currentSession.Id)
                {
                    originalGame = game;
                    break;
                }
            }

            if (originalGame == null)
            {
                MessageBox.Show("بازی مورد نظر در لیست تمام‌شده‌ها یافت نشد.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ذخیره اطلاعات قبلی برای بروزرسانی فاکتورها
            string oldTableNumber = originalGame.TableNumber;
            string oldGameName = originalGame.GameName;

            // بروزرسانی اطلاعات
            string selectedGameName = "";
            double newGamePrice = 0;
            if (cmbSelectGame.SelectedItem is GameItem selectedGame)
            {
                selectedGameName = selectedGame.Name;
                newGamePrice = selectedGame.Price;
            }

            originalGame.GameName = selectedGameName;
            originalGame.TableNumber = txtTableNumber.Text;
            originalGame.GameMasterName = txtGMName.Text;
            originalGame.RevenuePercent = (double)numPercent.Value;
            originalGame.Players = lstPlayers.Items.Cast<string>().ToList();

            // ========== تغییر: استفاده از selectedStartTime ==========
            originalGame.StartTime = selectedStartTime;

            // ========== بروزرسانی فاکتورهای مربوط به این بازی ==========
            var allInvoices = CafeManager.GetSalesHistory();

            // 1. اگر شماره میز تغییر کرده، فاکتورهای با میز قدیم را به میز جدید تغییر بده
            if (oldTableNumber != originalGame.TableNumber)
            {
                var relatedInvoices = allInvoices.Where(i => i.TableNumber == oldTableNumber).ToList();
                foreach (var invoice in relatedInvoices)
                {
                    bool isRelated = invoice.Items.Any(item => item.Product.Name == oldGameName);
                    if (isRelated)
                    {
                        invoice.TableNumber = originalGame.TableNumber;
                        CafeManager.UpdateInvoice(invoice.Id, invoice.CustomerName, invoice.TableNumber);
                    }
                }
            }

            // 2. اگر نام بازی تغییر کرده، آیتم‌های فاکتور را بروزرسانی کن
            if (oldGameName != originalGame.GameName)
            {
                var relatedInvoices = allInvoices.Where(i => i.TableNumber == originalGame.TableNumber).ToList();
                foreach (var invoice in relatedInvoices)
                {
                    var gameItem = invoice.Items.FirstOrDefault(item => item.Product.Name == oldGameName);
                    if (gameItem != null)
                    {
                        var allProducts = CafeManager.GetMenu();
                        var newProduct = allProducts.FirstOrDefault(p => p.Name == originalGame.GameName);

                        if (newProduct != null)
                        {
                            gameItem.Product.Name = newProduct.Name;
                            gameItem.Product.Price = newProduct.Price;
                            gameItem.Product.Id = newProduct.Id;
                        }
                        else
                        {
                            gameItem.Product.Name = originalGame.GameName;
                            gameItem.Product.Price = 50000;
                        }

                        CafeManager.UpdateInvoice(invoice.Id, invoice.CustomerName, invoice.TableNumber);
                    }
                }
            }

            // ========== محاسبه مجدد فروش بر اساس تعداد بازیکنان ==========
            originalGame.TotalRevenue = CalculateGameRevenue(originalGame);
            originalGame.GameMasterShare = CalculateGameMasterShare(originalGame);

            // ذخیره تغییرات در تاریخچه بازی‌ها
            CafeManager.AddCompletedGame(originalGame);

            // بازگرداندن دکمه به حالت عادی
            btnSave.Text = "💾 ثبت و شروع بازی";
            btnSave.BackColor = Color.LightGreen;
            btnSave.Click -= BtnUpdateGame_Click;
            btnSave.Click += BtnSave_Click;

            // فعال کردن لیست بازی‌های فعال
            lstGameMasters.Enabled = true;

            // به‌روزرسانی جدول
            LoadCompletedGames();
            UpdateShiftInfoLabel();

            // پاک کردن فرم
            currentSession = null;
            txtTableNumber.Clear();
            txtGMName.Clear();
            txtPlayerName.Clear();
            numPercent.Value = 10;
            lstPlayers.Items.Clear();

            // ========== تغییر: تنظیم مجدد زمان ==========
            selectedStartTime = DateTime.Now;
            txtStartTime.Text = ConvertToPersianDateWithTime(selectedStartTime);

            MessageBox.Show("✅ بازی با موفقیت بروزرسانی شد.\n" +
                           "فاکتورهای مربوطه نیز به‌روزرسانی شدند.",
                           "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ========== متد حذف بازی ==========
        private void BtnDeleteGame_Click(object sender, EventArgs e)
        {
            GameSession selectedGame = GetSelectedGameFromGrid();

            if (selectedGame == null)
            {
                MessageBox.Show("لطفاً یک بازی را برای حذف انتخاب کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // محاسبه فروش جدید بر اساس تعداد بازیکنان
            double revenue = CalculateGameRevenue(selectedGame);

            DialogResult result = MessageBox.Show(
                $"آیا از حذف بازی زیر مطمئن هستید؟\n\n" +
                $"🎮 نام بازی: {selectedGame.GameName}\n" +
                $"🎮 گرداننده: {selectedGame.GameMasterName}\n" +
                $"📅 تاریخ شروع: {ConvertToPersianDateWithTime(selectedGame.StartTime)}\n" +
                $"👥 تعداد بازیکنان: {selectedGame.Players.Count}\n" +
                $"💰 فروش: {revenue:N0} تومان\n\n" +
                $"⚠️ این عمل غیرقابل بازگشت است!\n" +
                $"⚠️ فاکتورهای مربوط به این بازی نیز حذف خواهند شد!",
                "تأیید حذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                // ========== حذف فاکتورهای مربوط به این بازی ==========
                var allInvoices = CafeManager.GetSalesHistory();
                var relatedInvoices = allInvoices.Where(i => i.TableNumber == selectedGame.TableNumber).ToList();

                int deletedInvoices = 0;
                foreach (var invoice in relatedInvoices)
                {
                    bool isRelated = invoice.Items.Any(item => item.Product.Name == selectedGame.GameName);
                    if (isRelated)
                    {
                        CafeManager.DeleteInvoice(invoice.Id);
                        deletedInvoices++;
                    }
                }

                // حذف از لیست بازی‌های تمام شده
                CafeManager.CompletedGames.Remove(selectedGame);

                // به‌روزرسانی جدول
                LoadCompletedGames();
                UpdateShiftInfoLabel();

                MessageBox.Show($"🗑️ بازی با موفقیت حذف شد.\n" +
                               $"📋 تعداد فاکتورهای حذف شده: {deletedInvoices}",
                               "حذف", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ========== متد انتخاب تاریخ شمسی ==========
        private void BtnSelectDate_Click(object sender, EventArgs e)
        {
            using (PersianDatePopup popup = new PersianDatePopup(selectedStartTime))
            {
                if (popup.ShowDialog() == DialogResult.OK)
                {
                    selectedStartTime = popup.SelectedDateTime;
                    txtStartTime.Text = ConvertToPersianDateWithTime(selectedStartTime);

                    // اگر سشن جاری وجود دارد، زمان آن را نیز به‌روزرسانی کنید
                    if (currentSession != null)
                    {
                        currentSession.StartTime = selectedStartTime;
                    }
                }
            }
        }

        private void InitializeControls()
        {
            // ========== سمت چپ: لیست گرداننده‌ها (بازی‌های فعال) ==========
            lblGameMasterTitle = new Label
            {
                Text = "🎮 بازی‌های فعال:",
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(lblGameMasterTitle);

            lstGameMasters = new ListBox
            {
                Font = new Font("Tahoma", 10)
            };
            lstGameMasters.SelectedIndexChanged += LstGameMasters_SelectedIndexChanged;
            this.Controls.Add(lstGameMasters);

            // ========== پنل جزئیات (وسط) ==========
            detailsBox = new GroupBox
            {
                Text = "جزئیات بازی",
                RightToLeft = RightToLeft.Yes,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            // ایجاد TableLayoutPanel برای چیدمان منظم
            tblDetails = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };

            // تنظیم عرض ستون‌ها
            tblDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // برای لیبل‌ها
            tblDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // برای کنترل‌ها

            // تنظیم ارتفاع ردیف‌ها
            for (int i = 0; i < 10; i++)
            {
                tblDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            }

            // انتخاب بازی
            lblSelectGame = new Label
            {
                Text = "انتخاب بازی:",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9)
            };
            cmbSelectGame = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 9),
                Dock = DockStyle.Fill
            };
            tblDetails.Controls.Add(lblSelectGame, 0, 0);
            tblDetails.Controls.Add(cmbSelectGame, 1, 0);

            // شماره میز
            Label lblTable = new Label
            {
                Text = "شماره میز:",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9)
            };
            txtTableNumber = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 9) };
            tblDetails.Controls.Add(lblTable, 0, 1);
            tblDetails.Controls.Add(txtTableNumber, 1, 1);

            // نام گرداننده
            Label lblGM = new Label
            {
                Text = "نام گرداننده:",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9)
            };
            txtGMName = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 9) };
            tblDetails.Controls.Add(lblGM, 0, 2);
            tblDetails.Controls.Add(txtGMName, 1, 2);

            // ========== تاریخ و زمان شروع با تقویم شمسی ==========
            Label lblStartTime = new Label
            {
                Text = "تاریخ و زمان شروع:",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9)
            };

            // ایجاد Panel برای ترکیب TextBox و Button
            TableLayoutPanel timePickerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            timePickerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            timePickerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            txtStartTime = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9),
                ReadOnly = true,
                BackColor = Color.White,
                TextAlign = HorizontalAlignment.Center
            };
            // تنظیم تاریخ فعلی به صورت شمسی
            txtStartTime.Text = ConvertToPersianDateWithTime(DateTime.Now);

            btnSelectDate = new Button
            {
                Text = "📅",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSelectDate.Click += BtnSelectDate_Click;

            timePickerPanel.Controls.Add(txtStartTime, 0, 0);
            timePickerPanel.Controls.Add(btnSelectDate, 1, 0);

            tblDetails.Controls.Add(lblStartTime, 0, 3);
            tblDetails.Controls.Add(timePickerPanel, 1, 3);

            // درصد سهم
            Label lblPercent = new Label
            {
                Text = "درصد سهم:",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9)
            };
            numPercent = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                Value = 10,
                Font = new Font("Tahoma", 9),
                Dock = DockStyle.Fill
            };
            tblDetails.Controls.Add(lblPercent, 0, 4);
            tblDetails.Controls.Add(numPercent, 1, 4);

            // بازیکن جدید
            Label lblPlayer = new Label
            {
                Text = "بازیکن جدید:",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9)
            };
            txtPlayerName = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 9) };
            btnAddPlayer = new Button
            {
                Text = "➕ افزودن",
                BackColor = Color.LightBlue,
                Font = new Font("Tahoma", 9),
                Dock = DockStyle.Fill
            };
            btnAddPlayer.Click += BtnAddPlayer_Click;
            tblDetails.Controls.Add(lblPlayer, 0, 5);
            tblDetails.Controls.Add(txtPlayerName, 1, 5);
            tblDetails.Controls.Add(btnAddPlayer, 1, 6);

            // دکمه ثبت
            btnSave = new Button
            {
                Text = "💾 ثبت و شروع بازی",
                BackColor = Color.LightGreen,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                Dock = DockStyle.Fill
            };
            btnSave.Click += BtnSave_Click;
            tblDetails.Controls.Add(btnSave, 1, 7);

            // دکمه پایان
            btnEnd = new Button
            {
                Text = "🏁 پایان و تسویه",
                BackColor = Color.LightCoral,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                Dock = DockStyle.Fill
            };
            btnEnd.Click += BtnEnd_Click;
            tblDetails.Controls.Add(btnEnd, 1, 8);

            // اضافه کردن TableLayoutPanel به GroupBox
            detailsBox.Controls.Add(tblDetails);
            this.Controls.Add(detailsBox);

            // ========== سمت راست: لیست بازیکنان ==========
            lblPlayersTitle = new Label
            {
                Text = "👥 بازیکنان:",
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(lblPlayersTitle);

            lstPlayers = new ListBox
            {
                Font = new Font("Tahoma", 10)
            };
            this.Controls.Add(lstPlayers);

            // ========== بخش پایین: بازی‌های تمام‌شده ==========
            grpCompletedGames = new GroupBox
            {
                Text = "📊 بازی‌های تمام‌شده",
                RightToLeft = RightToLeft.Yes,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                Dock = DockStyle.Bottom,
                Height = 220
            };

            // دکمه نمایش همه
            btnShowAllGames = new Button
            {
                Text = "📋 نمایش همه بازی‌ها",
                Location = new Point(15, 15),
                Size = new Size(160, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnShowAllGames.Click += BtnShowAllGames_Click;

            // دکمه ویرایش
            btnEditGame = new Button
            {
                Text = "✏️ ویرایش",
                Location = new Point(185, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnEditGame.Click += BtnEditGame_Click;

            // دکمه حذف
            btnDeleteGame = new Button
            {
                Text = "🗑️ حذف",
                Location = new Point(295, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnDeleteGame.Click += BtnDeleteGame_Click;

            // لیبل اطلاعات شیفت
            lblShiftInfo = new Label
            {
                Location = new Point(grpCompletedGames.Width - 350, 18),
                Size = new Size(330, 25),
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // عنوان لیست
            lblCompletedTitle = new Label
            {
                Text = "✅ بازی‌های تمام‌شده در شیفت جاری (۰ مورد)",
                Location = new Point(15, 50),
                Size = new Size(400, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            // جدول بازی‌های تمام‌شده
            dgvCompletedGames = new DataGridView
            {
                Location = new Point(10, 80),
                Size = new Size(grpCompletedGames.Width - 25, grpCompletedGames.Height - 95),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Tahoma", 9)
            };

            // اضافه کردن ستون‌ها به جدول
            dgvCompletedGames.Columns.Add("GameName", "نام بازی");
            dgvCompletedGames.Columns.Add("GameMaster", "گرداننده");
            dgvCompletedGames.Columns.Add("Table", "میز");
            dgvCompletedGames.Columns.Add("Players", "تعداد بازیکنان");
            dgvCompletedGames.Columns.Add("StartTime", "زمان شروع");
            dgvCompletedGames.Columns.Add("EndTime", "زمان پایان");
            dgvCompletedGames.Columns.Add("Duration", "مدت زمان");
            dgvCompletedGames.Columns.Add("Revenue", "فروش (تومان)");
            dgvCompletedGames.Columns.Add("GMShare", "سهم گرداننده");

            // تنظیم عرض ستون‌ها
            dgvCompletedGames.Columns["GameName"].FillWeight = 15;
            dgvCompletedGames.Columns["GameMaster"].FillWeight = 12;
            dgvCompletedGames.Columns["Table"].FillWeight = 8;
            dgvCompletedGames.Columns["Players"].FillWeight = 10;
            dgvCompletedGames.Columns["StartTime"].FillWeight = 15;
            dgvCompletedGames.Columns["EndTime"].FillWeight = 15;
            dgvCompletedGames.Columns["Duration"].FillWeight = 8;
            dgvCompletedGames.Columns["Revenue"].FillWeight = 10;
            dgvCompletedGames.Columns["GMShare"].FillWeight = 10;

            // اضافه کردن کنترل‌ها به GroupBox
            grpCompletedGames.Controls.Add(btnShowAllGames);
            grpCompletedGames.Controls.Add(btnEditGame);
            grpCompletedGames.Controls.Add(btnDeleteGame);
            grpCompletedGames.Controls.Add(lblShiftInfo);
            grpCompletedGames.Controls.Add(lblCompletedTitle);
            grpCompletedGames.Controls.Add(dgvCompletedGames);

            this.Controls.Add(grpCompletedGames);
        }

        private void LoadGamesIntoCombo()
        {
            cmbSelectGame.Items.Clear();
            var allProducts = CafeManager.GetMenu();
            var gamesList = allProducts.Where(p => p.Name.Contains("بازی")).ToList();

            if (gamesList.Count > 0)
            {
                foreach (var game in gamesList)
                {
                    cmbSelectGame.Items.Add(new GameItem { Id = game.Id, Name = game.Name, Price = game.Price });
                }

                if (cmbSelectGame.Items.Count > 0)
                    cmbSelectGame.SelectedIndex = 0;
            }
            else
            {
                cmbSelectGame.Items.Add("هیچ بازی‌ای یافت نشد");
                cmbSelectGame.Enabled = false;
            }
        }

        private void GameManagementForm_Resize(object sender, EventArgs e)
        {
            AdjustControlsSizeAndPosition();
        }

        private void AdjustControlsSizeAndPosition()
        {
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // تنظیم ارتفاع بخش پایین (بازی‌های تمام‌شده)
            int bottomHeight = 220;
            grpCompletedGames.Height = bottomHeight;
            grpCompletedGames.Width = formWidth - 30;
            grpCompletedGames.Location = new Point(15, formHeight - bottomHeight - 10);

            // تنظیم موقعیت لیبل اطلاعات شیفت
            lblShiftInfo.Location = new Point(grpCompletedGames.Width - 350, 18);

            // تنظیم اندازه جدول
            dgvCompletedGames.Width = grpCompletedGames.Width - 25;
            dgvCompletedGames.Height = grpCompletedGames.Height - 95;

            // تنظیم عرض ستون‌ها
            int topHeight = formHeight - bottomHeight - 30;

            // تنظیم عرض ستون‌ها
            int leftColumnWidth = (int)(formWidth * 0.22);   // لیست بازی‌های فعال
            int midColumnWidth = (int)(formWidth * 0.38);    // جزئیات
            int rightColumnWidth = (int)(formWidth * 0.28);  // لیست بازیکنان

            int margin = 15;
            int topMargin = 50;

            // ========== لیست بازی‌های فعال (سمت چپ) ==========
            lblGameMasterTitle.Location = new Point(margin, 20);
            lstGameMasters.Location = new Point(margin, topMargin);
            lstGameMasters.Size = new Size(leftColumnWidth, topHeight - topMargin - 10);

            // ========== جزئیات (وسط) ==========
            int secondX = margin + leftColumnWidth + margin;
            detailsBox.Location = new Point(secondX, 20);
            detailsBox.Size = new Size(midColumnWidth, topHeight - 20);

            // ========== بازیکنان (سمت راست) ==========
            int thirdX = secondX + midColumnWidth + margin;
            lblPlayersTitle.Location = new Point(thirdX, 20);
            lstPlayers.Location = new Point(thirdX, topMargin);
            lstPlayers.Size = new Size(rightColumnWidth, topHeight - topMargin - 10);
        }

        private void CreateInvoiceForPlayer(string playerName, GameSession session)
        {
            try
            {
                // ========== پیدا کردن محصول بازی از منو ==========
                var allProducts = CafeManager.GetMenu();
                var gameProduct = allProducts.FirstOrDefault(p => p.Name == session.GameName);

                // ========== ایجاد فاکتور ==========
                var invoice = new Invoice
                {
                    CustomerName = playerName,
                    TableNumber = session.TableNumber,
                    OrderDate = DateTime.Now,
                    IsSettled = false,
                    PayMethod = "نقدی",
                    Items = new List<OrderItem>()
                };

                // ========== اضافه کردن آیتم بازی ==========
                if (gameProduct != null)
                {
                    var gameItem = new OrderItem
                    {
                        Product = new Product
                        {
                            Id = gameProduct.Id,
                            Name = gameProduct.Name,
                            Price = gameProduct.Price
                        },
                        Quantity = 1
                    };
                    invoice.Items.Add(gameItem);

                    MessageBox.Show($"✅ محصول '{gameProduct.Name}' با قیمت {gameProduct.Price:N0} تومان به فاکتور اضافه شد.",
                        "اطلاعات", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // اگر محصول پیدا نشد، یک آیتم با قیمت پیش‌فرض ایجاد می‌کنیم
                    var gameItem = new OrderItem
                    {
                        Product = new Product
                        {
                            Id = 0,
                            Name = session.GameName,
                            Price = 50000
                        },
                        Quantity = 1
                    };
                    invoice.Items.Add(gameItem);

                    MessageBox.Show($"⚠️ محصول '{session.GameName}' در منو پیدا نشد. قیمت پیش‌فرض: ۵۰,۰۰۰ تومان",
                        "هشدار", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // ========== ذخیره فاکتور ==========
                CafeManager.SaveInvoice(invoice);

                // ========== ثبت اطلاعات در سشن ==========
                session.RegisterPlayerInvoice(playerName, invoice.Id);

                double gamePrice = invoice.Items.Sum(i => i.TotalPrice);
                session.RecordPlayerPayment(playerName, gamePrice);

                // ========== نمایش پیام موفقیت ==========
                MessageBox.Show($"✅ فاکتور برای بازیکن {playerName} با شماره {invoice.Id} ثبت شد.\n" +
                               $"📋 نام مشتری: {invoice.CustomerName}\n" +
                               $"📋 شماره میز: {invoice.TableNumber}\n" +
                               $"💰 مبلغ: {gamePrice:N0} تومان",
                               "ثبت فاکتور بازیکن",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطا در ثبت فاکتور برای بازیکن {playerName}:\n{ex.Message}\n\n{ex.StackTrace}",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddPlayer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlayerName.Text))
            {
                MessageBox.Show("لطفاً نام بازیکن را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string playerName = txtPlayerName.Text.Trim();

            // بررسی تکراری نبودن
            if (lstPlayers.Items.Cast<string>().Any(p => p.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"بازیکن '{playerName}' قبلاً اضافه شده است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPlayerName.Clear();
                return;
            }

            lstPlayers.Items.Add(playerName);
            txtPlayerName.Clear();

            // اگر سشن فعالی وجود داره، بازیکن رو به سشن اضافه کن
            if (currentSession != null && currentSession.IsActive)
            {
                if (!currentSession.Players.Contains(playerName))
                {
                    currentSession.Players.Add(playerName);

                    // ایجاد فاکتور برای بازیکن
                    CreateInvoiceForPlayer(playerName, currentSession);

                    RefreshGameLists();
                }
            }
        }

        private void RefreshGameLists()
        {
            lstGameMasters.Items.Clear();
            foreach (var game in CafeManager.ActiveGames)
            {
                int paidCount = game.Value.PlayerPayments.Count(p => p.Value > 0);
                int totalPlayers = game.Value.Players.Count;
                string status = totalPlayers > 0 ? $" | پرداخت: {paidCount}/{totalPlayers}" : "";
                // استفاده از تاریخ شمسی
                lstGameMasters.Items.Add($"{game.Value.GameName} | {game.Value.GameMasterName} | میز {game.Key} | {ConvertToPersianDateWithTime(game.Value.StartTime)}{status}");
            }
        }

        private void LstGameMasters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGameMasters.SelectedItem == null) return;

            string selectedText = lstGameMasters.SelectedItem.ToString();

            // پیدا کردن سشن
            currentSession = null;
            foreach (var game in CafeManager.ActiveGames.Values)
            {
                // استفاده از تاریخ شمسی برای مقایسه
                string compareText = $"{game.GameName} | {game.GameMasterName} | میز {game.TableNumber} | {ConvertToPersianDateWithTime(game.StartTime)}";
                if (selectedText.StartsWith(compareText))
                {
                    currentSession = game;
                    break;
                }
            }

            if (currentSession == null) return;

            txtTableNumber.Text = currentSession.TableNumber;
            txtGMName.Text = currentSession.GameMasterName;
            numPercent.Value = (decimal)currentSession.RevenuePercent;

            // ========== تغییر: استفاده از selectedStartTime ==========
            selectedStartTime = currentSession.StartTime;
            txtStartTime.Text = ConvertToPersianDateWithTime(selectedStartTime);

            // انتخاب بازی در کومبوباکس
            for (int i = 0; i < cmbSelectGame.Items.Count; i++)
            {
                if (cmbSelectGame.Items[i] is GameItem item && item.Name == currentSession.GameName)
                {
                    cmbSelectGame.SelectedIndex = i;
                    break;
                }
            }

            lstPlayers.Items.Clear();
            foreach (var player in currentSession.Players)
            {
                lstPlayers.Items.Add(player);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // اعتبارسنجی
            if (cmbSelectGame.SelectedItem == null || cmbSelectGame.SelectedItem.ToString() == "هیچ بازی‌ای یافت نشد")
            {
                MessageBox.Show("لطفاً یک بازی را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTableNumber.Text))
            {
                MessageBox.Show("لطفاً شماره میز را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtGMName.Text))
            {
                MessageBox.Show("لطفاً نام گرداننده را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // بررسی وجود بازیکن
            if (lstPlayers.Items.Count == 0)
            {
                MessageBox.Show("لطفاً حداقل یک بازیکن به بازی اضافه کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedGameName = "";
            double gamePrice = 0;
            if (cmbSelectGame.SelectedItem is GameItem selectedGame)
            {
                selectedGameName = selectedGame.Name;
                gamePrice = selectedGame.Price;
            }

            // ایجاد سشن جدید
            var newSession = new GameSession
            {
                Id = Guid.NewGuid().ToString(),
                GameName = selectedGameName,
                TableNumber = txtTableNumber.Text,
                GameMasterName = txtGMName.Text,
                RevenuePercent = (double)numPercent.Value,
                Players = lstPlayers.Items.Cast<string>().ToList(),

                // ========== تغییر: استفاده از selectedStartTime ==========
                StartTime = selectedStartTime,
                IsActive = true
            };

            // ثبت بازی در لیست بازی‌های فعال
            CafeManager.ActiveGames[txtTableNumber.Text] = newSession;
            currentSession = newSession;

            // ========== ایجاد فاکتور برای هر بازیکن ==========
            int invoiceCount = 0;
            foreach (string playerName in lstPlayers.Items)
            {
                try
                {
                    // پیدا کردن محصول بازی از منو
                    var allProducts = CafeManager.GetMenu();
                    var gameProduct = allProducts.FirstOrDefault(p => p.Name == selectedGameName);

                    // ایجاد فاکتور برای بازیکن
                    var invoice = new Invoice
                    {
                        CustomerName = playerName,
                        TableNumber = txtTableNumber.Text,
                        OrderDate = DateTime.Now,
                        IsSettled = false,
                        PayMethod = "نقدی",
                        Items = new List<OrderItem>()
                    };

                    // اضافه کردن آیتم بازی به فاکتور
                    if (gameProduct != null)
                    {
                        var gameItem = new OrderItem
                        {
                            Product = new Product
                            {
                                Id = gameProduct.Id,
                                Name = gameProduct.Name,
                                Price = gameProduct.Price
                            },
                            Quantity = 1
                        };
                        invoice.Items.Add(gameItem);
                    }
                    else
                    {
                        // اگر محصول پیدا نشد، با قیمت پیش‌فرض
                        var gameItem = new OrderItem
                        {
                            Product = new Product
                            {
                                Id = 0,
                                Name = selectedGameName,
                                Price = 50000
                            },
                            Quantity = 1
                        };
                        invoice.Items.Add(gameItem);
                    }

                    // ذخیره فاکتور
                    CafeManager.SaveInvoice(invoice);

                    // ثبت اطلاعات در سشن
                    newSession.RegisterPlayerInvoice(playerName, invoice.Id);
                    double gamePriceForPlayer = invoice.Items.Sum(i => i.TotalPrice);
                    newSession.RecordPlayerPayment(playerName, gamePriceForPlayer);

                    invoiceCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ خطا در ثبت فاکتور برای بازیکن {playerName}:\n{ex.Message}",
                        "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // به‌روزرسانی لیست
            RefreshGameLists();

            // استفاده از تاریخ شمسی در پیام موفقیت
            MessageBox.Show($"✅ بازی {selectedGameName} با موفقیت ثبت شد.\n" +
                           $"تعداد بازیکنان: {lstPlayers.Items.Count}\n" +
                           $"تعداد فاکتورهای ایجاد شده: {invoiceCount}\n" +
                           $"زمان شروع: {ConvertToPersianDateWithTime(selectedStartTime)}\n" +
                           $"💰 برای هر بازیکن یک فاکتور به مبلغ {gamePrice:N0} تومان ثبت شد.\n\n" +
                           $"📋 فاکتورها در بخش \"مدیریت فاکتورها\" قابل مشاهده هستند.",
                           "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // پاک کردن فرم برای ثبت بازی جدید
            txtTableNumber.Clear();
            txtGMName.Clear();
            txtPlayerName.Clear();
            numPercent.Value = 10;
            lstPlayers.Items.Clear();

            // ========== تغییر: تنظیم مجدد زمان ==========
            selectedStartTime = DateTime.Now;
            txtStartTime.Text = ConvertToPersianDateWithTime(selectedStartTime);
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstGameMasters.SelectedItem == null)
                {
                    MessageBox.Show("لطفاً ابتدا یک بازی را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string selectedText = lstGameMasters.SelectedItem.ToString();

                // پیدا کردن سشن انتخاب شده
                GameSession session = null;
                foreach (var game in CafeManager.ActiveGames.Values)
                {
                    // استفاده از تاریخ شمسی برای مقایسه
                    string compareText = $"{game.GameName} | {game.GameMasterName} | میز {game.TableNumber} | {ConvertToPersianDateWithTime(game.StartTime)}";
                    if (selectedText.StartsWith(compareText))
                    {
                        session = game;
                        break;
                    }
                }

                if (session == null)
                {
                    MessageBox.Show("بازی انتخاب شده یافت نشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // بررسی اینکه آیا بازیکنی وجود دارد
                if (session.Players.Count == 0)
                {
                    MessageBox.Show("این بازی هیچ بازیکنی ندارد و نمی‌توان آن را تسویه کرد.",
                        "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ========== محاسبه فروش بر اساس تعداد بازیکنان ==========
                double totalRevenue = CalculateGameRevenue(session);
                double gmShare = CalculateGameMasterShare(session);

                // ========== به‌روزرسانی سشن ==========
                session.TotalRevenue = totalRevenue;
                session.GameMasterShare = gmShare;

                // ========== بررسی پرداخت بازیکنان ==========
                bool allPaid = true;
                string unpaidPlayers = "";
                foreach (var player in session.Players)
                {
                    double payment = session.GetPlayerTotalPayment(player);
                    if (payment == 0)
                    {
                        allPaid = false;
                        unpaidPlayers += $"   👤 {player}\n";
                    }
                }

                if (!allPaid)
                {
                    DialogResult result = MessageBox.Show(
                        $"⚠️ همه بازیکنان پرداخت نکرده‌اند!\n\n" +
                        $"بازیکنانی که پرداخت نکرده‌اند:\n{unpaidPlayers}\n" +
                        $"آیا از پایان بازی با وجود پرداخت‌نشده‌ها مطمئن هستید؟",
                        "تأیید پایان بازی",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.No)
                        return;
                }

                // ========== محاسبه مدت زمان بازی ==========
                TimeSpan duration = DateTime.Now - session.StartTime;
                string durationText = $"{duration.Hours} ساعت و {duration.Minutes} دقیقه";

                // ========== ساخت گزارش با تاریخ شمسی ==========
                string report = $"📊 گزارش نهایی بازی\n" +
                               $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                               $"🎮 نام بازی: {session.GameName}\n" +
                               $"🎮 گرداننده: {session.GameMasterName}\n" +
                               $"📅 تاریخ شروع: {ConvertToPersianDateWithTime(session.StartTime)}\n" +
                               $"⏱️ مدت زمان: {durationText}\n" +
                               $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                               $"💰 جزئیات پرداخت بازیکنان:\n";

                foreach (var player in session.Players)
                {
                    double payment = session.GetPlayerTotalPayment(player);
                    int invoiceId = session.GetPlayerInvoiceId(player);
                    string status = payment > 0 ? "✅" : "❌";
                    report += $"   {status} {player}: {payment:N0} تومان (فاکتور {invoiceId})\n";
                }

                report += $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                          $"💰 مجموع فروش: {totalRevenue:N0} تومان\n" +
                          $"👥 تعداد بازیکنان: {session.Players.Count}\n" +
                          $"💰 قیمت هر بازی: {GetGamePrice(session.GameName):N0} تومان\n" +
                          $"🎯 درصد سهم گرداننده: {session.RevenuePercent}%\n" +
                          $"🎯 سهم گرداننده: {gmShare:N0} تومان\n" +
                          $"━━━━━━━━━━━━━━━━━━━━━━";

                MessageBox.Show(report, "تسویه حساب", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ========== ذخیره در تاریخچه ==========
                session.EndTime = DateTime.Now;
                session.IsActive = false;

                // ✅ استفاده از متد AddCompletedGame
                CafeManager.AddCompletedGame(session);

                // حذف از بازی‌های فعال
                CafeManager.ActiveGames.Remove(session.TableNumber);
                currentSession = null;
                RefreshGameLists();

                // بارگذاری مجدد بازی‌های تمام‌شده
                LoadCompletedGames();
                UpdateShiftInfoLabel();

                // پاک کردن فرم
                txtTableNumber.Clear();
                txtGMName.Clear();
                txtPlayerName.Clear();
                numPercent.Value = 10;
                lstPlayers.Items.Clear();

                // ========== تغییر: تنظیم مجدد زمان ==========
                selectedStartTime = DateTime.Now;
                txtStartTime.Text = ConvertToPersianDateWithTime(selectedStartTime);

                MessageBox.Show("✅ بازی با موفقیت پایان یافت و در تاریخچه ذخیره شد.",
                    "پایان بازی", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در پایان بازی:\n{ex.Message}\n\n{ex.StackTrace}",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class GameItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }

            public override string ToString()
            {
                return $"{Name} : {Price:N0} تومان";
            }
        }
    }
}