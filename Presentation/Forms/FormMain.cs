using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;

namespace CafeManager
{
    public class FormMain : Form
    {
        private List<OrderItem> _currentOrder = new List<OrderItem>();
        private List<Product> _menuProducts = new List<Product>();
        private ListBox lstMenu;
        private DataGridView dgvOrder;
        private NumericUpDown numQuantity;
        private Button btnAdd, btnCheckout, btnInvoiceManager;
        private Label lblTotal, lblCustomer, lblTable, lblMenuTitle, lblOrderTitle, lblQty;
        private TextBox txtCustomerName, txtTableNumber, txtSearch;
        private Label lblSearch;

        private ToolStripMenuItem menuOperations;
        private ToolStripMenuItem menuPersonnel;
        private ToolStripMenuItem menuReports;
        private ToolStripMenuItem menuMisc;
        private ToolStripMenuItem menuSettlement;
        private MenuStrip mainMainMenu;

        private Panel headerPanel;
        private Label lblAppTitle;
        private PictureBox picAvatar;
        private Label lblUserName;
        private Label lblRoleIcon;
        private Label lblLoginTime;
        private Label lblCurrentTime;
        private Button btnLogoutHeader;

        public FormMain()
        {
            this.Size = new Size(950, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;
            this.MinimumSize = new Size(800, 500);

            UpdateFormTitle();

            this.Load += (s, e) =>
            {
                this.WindowState = FormWindowState.Maximized;
                ConfigureAccessByRole();
                UpdateHeaderPanel();
                this.BeginInvoke(new Action(() => FormMain_SizeChanged(this, EventArgs.Empty)));
            };
            this.SizeChanged += FormMain_SizeChanged;
            _menuProducts = CafeManager.GetMenu();
            InitializeCustomComponents();
            LoadMenuToUI();
        }

        private void UpdateFormTitle()
        {
            if (CafeManager.IsAuthenticated())
            {
                var user = CafeManager.CurrentUser;
                string title = $"☕ کافه گلستان";

                if (user.IsAdmin)
                    title += $" 👑 {user.FullName} ";
                else if (user.IsCashier)
                    title += $" 🛒 {user.FullName} ";
                else
                    title += $" 👤 {user.FullName} ({user.GetRoleDisplay()})";

                title += $" | ورود: {user.LoginTime:HH:mm}";
                this.Text = title;
            }
            else
            {
                this.Text = "☕ کافه گلستان - [ورود نشده]";
            }
        }

        private void InitializeHeaderPanel()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(60, 40, 20),
                Padding = new Padding(10, 5, 10, 5)
            };

            lblAppTitle = new Label
            {
                Text = "☕ کافه گلستان",
                Font = new Font("B Mitra", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(200, 35),
                Location = new Point(10, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(lblAppTitle);

            btnLogoutHeader = new Button
            {
                Text = "🚪 خروج",
                Location = new Point(215, 10),
                Size = new Size(55, 32),
                BackColor = Color.FromArgb(220, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogoutHeader.FlatAppearance.BorderSize = 0;
            btnLogoutHeader.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 70, 70);
            btnLogoutHeader.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 30, 30);

            ToolTip tip = new ToolTip();
            tip.SetToolTip(btnLogoutHeader, "خروج از حساب کاربری");

            btnLogoutHeader.Click += (s, e) =>
            {
                var result = MessageBox.Show("آیا از خروج از حساب کاربری مطمئن هستید؟",
                    "خروج از حساب",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    CafeManager.Logout();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };
            headerPanel.Controls.Add(btnLogoutHeader);

            var panelUser = new Panel
            {
                Dock = DockStyle.Right,
                Width = 400,
                Height = 45,
                BackColor = Color.Transparent
            };

            lblCurrentTime = new Label
            {
                Location = new Point(5, 12),
                Size = new Size(80, 25),
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 200, 220),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Timer clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) =>
            {
                lblCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            clockTimer.Start();
            lblCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
            panelUser.Controls.Add(lblCurrentTime);

            Label lblSeparator = new Label
            {
                Location = new Point(88, 10),
                Size = new Size(2, 30),
                BackColor = Color.FromArgb(100, 80, 60),
                Text = ""
            };
            panelUser.Controls.Add(lblSeparator);

            picAvatar = new PictureBox
            {
                Size = new Size(36, 36),
                Location = new Point(95, 4),
                BackColor = Color.FromArgb(100, 60, 30),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            panelUser.Controls.Add(picAvatar);

            lblUserName = new Label
            {
                Location = new Point(140, 8),
                Size = new Size(160, 30),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelUser.Controls.Add(lblUserName);

            lblRoleIcon = new Label
            {
                Location = new Point(300, 8),
                Size = new Size(110, 30),
                Font = new Font("Tahoma", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 180, 150),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelUser.Controls.Add(lblRoleIcon);

            lblLoginTime = new Label
            {
                Location = new Point(350, 12),
                Size = new Size(60, 20),
                Font = new Font("Tahoma", 8),
                ForeColor = Color.FromArgb(180, 160, 130),
                TextAlign = ContentAlignment.MiddleRight
            };
            panelUser.Controls.Add(lblLoginTime);

            headerPanel.Controls.Add(panelUser);
            this.Controls.Add(headerPanel);
        }

        private void UpdateHeaderPanel()
        {
            if (CafeManager.IsAuthenticated())
            {
                var user = CafeManager.CurrentUser;

                lblUserName.Text = user.FullName;

                if (user.IsAdmin)
                {
                    lblRoleIcon.Text = "👑 مدیر سیستم";
                    lblRoleIcon.ForeColor = Color.Gold;
                }
                else if (user.IsCashier)
                {
                    lblRoleIcon.Text = "🛒 صندوق‌دار";
                    lblRoleIcon.ForeColor = Color.LightBlue;
                }
                else
                {
                    lblRoleIcon.Text = "👤 کاربر";
                    lblRoleIcon.ForeColor = Color.FromArgb(200, 180, 150);
                }

                lblLoginTime.Text = $"⏰ {user.LoginTime:HH:mm}";
                CreateAvatar(user.FullName);
            }
            else
            {
                lblUserName.Text = "مهمان";
                lblRoleIcon.Text = "👤 مهمان";
                lblLoginTime.Text = "";
                picAvatar.Image = null;
            }
        }

        private void CreateAvatar(string fullName)
        {
            var bmp = new Bitmap(36, 36);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(139, 69, 19)))
                    g.FillEllipse(brush, 0, 0, 36, 36);

                var letter = fullName.Length > 0 ? fullName[0].ToString() : "?";
                using (var font = new Font("Tahoma", 16, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(letter, font, brush, new Rectangle(0, 0, 36, 36), sf);
                }
            }
            picAvatar.Image = bmp;
        }

        private void ConfigureAccessByRole()
        {
            if (!CafeManager.IsAuthenticated())
            {
                MessageBox.Show("لطفاً ابتدا وارد سیستم شوید!", "نیاز به احراز هویت",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            var user = CafeManager.CurrentUser;
            UpdateFormTitle();

            if (user.IsAdmin)
                EnableAllFeatures();
            else if (user.IsCashier)
                EnableCashierFeatures();
            else
            {
                MessageBox.Show("نقش کاربر نامعتبر است!", "خطای امنیتی",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void EnableAllFeatures()
        {
            menuOperations.Enabled = true;
            menuPersonnel.Enabled = true;
            menuReports.Enabled = true;
            menuMisc.Enabled = true;
            btnAdd.Enabled = true;
            btnCheckout.Enabled = true;
            btnInvoiceManager.Enabled = true;
            numQuantity.Enabled = true;
            txtSearch.Enabled = true;
            lstMenu.Enabled = true;
        }

        private void EnableCashierFeatures()
        {
            menuOperations.Enabled = false;
            menuPersonnel.Enabled = false;
            menuMisc.Enabled = false;
            menuReports.Enabled = true;
            btnAdd.Enabled = true;
            btnCheckout.Enabled = true;
            btnInvoiceManager.Enabled = true;
            numQuantity.Enabled = true;
            txtSearch.Enabled = true;
            lstMenu.Enabled = true;
        }

        private void InitializeCustomComponents()
        {
            this.SuspendLayout();
            this.Controls.Clear();

            InitializeHeaderPanel();
            InitializeMenu();
            InitializeMainControls();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeMenu()
        {
            mainMainMenu = new MenuStrip { BackColor = Color.WhiteSmoke };

            menuOperations = new ToolStripMenuItem("⚙️ عملیات سیستم");

            var menuInventory = new ToolStripMenuItem("📦 مدیریت انبارداری", null, (s, e) => { new InventoryForm().ShowDialog(); _menuProducts = CafeManager.GetMenu(); LoadMenuToUI(); });
            menuOperations.DropDownItems.Add(menuInventory);

            var menuMenuManager = new ToolStripMenuItem("☕ مدیریت منو کافه", null, (s, e) => { new MenuManagerForm().ShowDialog(); _menuProducts = CafeManager.GetMenu(); LoadMenuToUI(); });
            menuOperations.DropDownItems.Add(menuMenuManager);

            var menuGameManagement = new ToolStripMenuItem("🎲 مدیریت بازی و تیم‌ها", null, (s, e) => {
                using (var frm = new GameManagementForm()) { frm.ShowDialog(this); }
            });
            menuOperations.DropDownItems.Add(menuGameManagement);

            var menuPurchaseHistory = new ToolStripMenuItem("📊 تاریخچه خرید", null, (s, e) => {
                using (var frm = new PurchaseHistoryForm()) { frm.ShowDialog(this); }
            });
            menuOperations.DropDownItems.Add(menuPurchaseHistory);

            var menuPurchase = new ToolStripMenuItem("🛒 خرید روزانه", null, (s, e) => {
                using (var frm = new PurchaseForm())
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        _menuProducts = CafeManager.GetMenu();
                        LoadMenuToUI();
                    }
                }
            });
            menuOperations.DropDownItems.Add(menuPurchase);

            var menuStocktake = new ToolStripMenuItem("📊 انبارگردانی", null, (s, e) => {
                using (var frm = new StocktakeForm())
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        _menuProducts = CafeManager.GetMenu();
                        LoadMenuToUI();
                    }
                }
            });
            menuOperations.DropDownItems.Add(menuStocktake);

            menuSettlement = new ToolStripMenuItem("🧾 تسویه گرداننده");
            menuSettlement.Click += (s, e) => {
                using (var frm = new SettlementForm()) { frm.ShowDialog(this); }
            };
            menuOperations.DropDownItems.Add(menuSettlement);

            mainMainMenu.Items.Add(menuOperations);

            menuPersonnel = new ToolStripMenuItem("👤 مدیریت پرسنل");

            var menuEmployee = new ToolStripMenuItem("👤 مدیریت پرسنل", null, (s, e) => {
                using (var frm = new EmployeeForm()) { frm.ShowDialog(this); }
            });
            menuPersonnel.DropDownItems.Add(menuEmployee);

            var menuAttendance = new ToolStripMenuItem("📋 حضور و غیاب", null, (s, e) => {
                using (var frm = new AttendanceForm()) { frm.ShowDialog(this); }
            });
            menuPersonnel.DropDownItems.Add(menuAttendance);

            var menuPayroll = new ToolStripMenuItem("💰 حقوق و دستمزد", null, (s, e) => {
                using (var frm = new PayrollForm()) { frm.ShowDialog(this); }
            });
            menuPersonnel.DropDownItems.Add(menuPayroll);

            menuPersonnel.DropDownItems.Add(new ToolStripSeparator());

            var menuUserManagement = new ToolStripMenuItem("👥 مدیریت کاربران", null, (s, e) => {
                if (!CafeManager.IsAdmin())
                {
                    MessageBox.Show("⚠️ فقط مدیر سیستم به این بخش دسترسی دارد!",
                        "دسترسی غیرمجاز",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                using (var frm = new UserManagementForm())
                {
                    frm.ShowDialog(this);
                }
            });
            menuPersonnel.DropDownItems.Add(menuUserManagement);

            mainMainMenu.Items.Add(menuPersonnel);

            menuReports = new ToolStripMenuItem("📊 گزارشات");

            var menuSalesReport = new ToolStripMenuItem("📈 گزارش فروش (تاریخ شمسی)", null, (s, e) => {
                using (var frm = new ReportForm()) { frm.ShowDialog(this); }
            });
            menuReports.DropDownItems.Add(menuSalesReport);

            mainMainMenu.Items.Add(menuReports);

            menuMisc = new ToolStripMenuItem("💼 متفرقه");

            var menuMiscExpense = new ToolStripMenuItem("💰 هزینه‌های متفرقه", null, (s, e) => {
                using (var frm = new MiscExpenseForm()) { frm.ShowDialog(this); }
            });
            menuMisc.DropDownItems.Add(menuMiscExpense);

            var menuMiscReport = new ToolStripMenuItem("📊 گزارش هزینه‌ها", null, (s, e) => {
                using (var frm = new MiscExpenseReportForm()) { frm.ShowDialog(this); }
            });
            menuMisc.DropDownItems.Add(menuMiscReport);

            var menuProfitReport = new ToolStripMenuItem("📊 گزارش سود خالص", null, (s, e) => {
                using (var frm = new ProfitReportForm()) { frm.ShowDialog(this); }
            });
            menuMisc.DropDownItems.Add(menuProfitReport);

            mainMainMenu.Items.Add(menuMisc);

            this.MainMenuStrip = mainMainMenu;
            this.Controls.Add(mainMainMenu);
        }

        private void InitializeMainControls()
        {
            lblSearch = new Label
            {
                Text = "جستجوی محصول:",
                Size = new Size(100, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };

            txtSearch = new TextBox
            {
                Size = new Size(220, 25),
                Font = new Font("Tahoma", 10)
            };
            txtSearch.TextChanged += (s, e) => {
                var filtered = _menuProducts.Where(p => p.Name.ToLower().Contains(txtSearch.Text.ToLower())).ToList();
                lstMenu.Items.Clear();
                foreach (var p in filtered) lstMenu.Items.Add($"{p.Name} - {p.Price:N0} تومان");
            };

            lblCustomer = new Label
            {
                Text = "نام مشتری:",
                Size = new Size(80, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            txtCustomerName = new TextBox
            {
                Size = new Size(180, 25),
                Font = new Font("Tahoma", 10)
            };

            lblTable = new Label
            {
                Text = "شماره میز:",
                Size = new Size(80, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            txtTableNumber = new TextBox
            {
                Size = new Size(100, 25),
                Font = new Font("Tahoma", 10)
            };

            int searchWidth = 220;
            int labelWidth = 100;
            int totalWidth = labelWidth + searchWidth + 20;

            lblMenuTitle = new Label
            {
                Text = "منوی کافه:",
                Size = new Size(totalWidth, 25),
                Font = new Font("Tahoma", 12, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            lblOrderTitle = new Label
            {
                Text = "فاکتور جاری مشتری:",
                Size = new Size(200, 25),
                Font = new Font("Tahoma", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            lstMenu = new ListBox();
            lstMenu.Font = new Font("Tahoma", 11, FontStyle.Bold);
            lstMenu.Size = new Size(totalWidth, 200);
            lstMenu.MouseDoubleClick += (s, e) => BtnAdd_Click(btnAdd, EventArgs.Empty);

            dgvOrder = new DataGridView
            {
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray,
                RowHeadersVisible = false,
                Font = new Font("Tahoma", 10),
                ColumnHeadersHeight = 30,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            };

            dgvOrder.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrder.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrder.CellContentClick += dgvOrder_CellContentClick;

            dgvOrder.Columns.Add("Name", "نام محصول");
            dgvOrder.Columns.Add("Price", "قیمت واحد");
            dgvOrder.Columns.Add("Qty", "تعداد");
            dgvOrder.Columns.Add("Total", "قیمت کل");

            var editColumn = new DataGridViewButtonColumn
            {
                Name = "EditAction",
                HeaderText = "ویرایش",
                Text = "✏️",
                UseColumnTextForButtonValue = true,
                Width = 70,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.LightBlue,
                    ForeColor = Color.DarkBlue,
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Tahoma", 9, FontStyle.Bold)
                }
            };

            var deleteColumn = new DataGridViewButtonColumn
            {
                Name = "DeleteAction",
                HeaderText = "حذف",
                Text = "🗑️",
                UseColumnTextForButtonValue = true,
                Width = 70,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 80, 80),
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Tahoma", 9, FontStyle.Bold)
                }
            };

            dgvOrder.Columns.Add(editColumn);
            dgvOrder.Columns.Add(deleteColumn);

            lblQty = new Label
            {
                Text = "تعداد:",
                Size = new Size(50, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };

            numQuantity = new NumericUpDown
            {
                Minimum = 1,
                Value = 1,
                Size = new Size(60, 25),
                Font = new Font("Tahoma", 10)
            };

            btnAdd = new Button
            {
                Text = "➕ افزودن به فاکتور",
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            btnAdd.Click += BtnAdd_Click;

            lblTotal = new Label
            {
                Text = "جمع کل فاکتور: 0 تومان",
                Size = new Size(400, 30),
                Font = new Font("Tahoma", 12, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            btnCheckout = new Button
            {
                Text = "ثبت فاکتور",
                Size = new Size(160, 40),
                BackColor = Color.LightCoral,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            btnCheckout.Click += BtnCheckout_Click;

            btnInvoiceManager = new Button
            {
                Text = "مدیریت فاکتورها",
                Size = new Size(160, 40),
                BackColor = Color.SkyBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            btnInvoiceManager.Click += (s, e) => {
                using (var frm = new InvoiceForm()) { frm.ShowDialog(this); }
            };

            this.Controls.AddRange(new Control[]
            {
                lblSearch, txtSearch,
                lblCustomer, txtCustomerName, lblTable, txtTableNumber,
                lblMenuTitle, lblOrderTitle,
                lstMenu, dgvOrder,
                lblQty, numQuantity, btnAdd,
                lblTotal, btnCheckout, btnInvoiceManager
            });
        }

        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            if (ClientSize.Width < 900 || ClientSize.Height < 600)
                return;

            int margin = 20;
            int gap = 20;

            int top = headerPanel.Height + mainMainMenu.Height + 10;

            //----------------------------------------
            // ردیف اول
            //----------------------------------------

            lblSearch.Location = new Point(margin, top);
            txtSearch.Location = new Point(120, top);
            txtSearch.Size = new Size(220, 28);

            lblCustomer.Location = new Point(380, top);
            txtCustomerName.Location = new Point(465, top);
            txtCustomerName.Size = new Size(170, 28);

            lblTable.Location = new Point(655, top);
            txtTableNumber.Location = new Point(735, top);
            txtTableNumber.Size = new Size(90, 28);

            //----------------------------------------
            // عنوان ها
            //----------------------------------------

            int titleY = top + 40;

            lblMenuTitle.Location = new Point(margin, titleY);

            int menuWidth = 340;

            int gridX = margin + menuWidth + gap;

            lblOrderTitle.Location = new Point(gridX, titleY);

            //----------------------------------------
            // محاسبه ارتفاع مشترک
            //----------------------------------------
            //----------------------------------------
            // محاسبه ارتفاع مشترک ListBox و DataGridView
            //----------------------------------------

            int bodyTop = titleY + 30;

            // اگر ارتفاع دکمه هنوز مقدار نگرفته بود
            int buttonHeight = btnAdd.Height > 0 ? btnAdd.Height : 40;

            // فاصله پایین فرم
            int bottomMargin = 15;

            // ارتفاع مورد نیاز پایین فرم (دکمه + فاصله)
            int buttonArea = buttonHeight + bottomMargin;

            // ارتفاع مشترک لیست و گرید
            int bodyHeight = ClientSize.Height - bodyTop - buttonArea;

            if (bodyHeight < 250)
                bodyHeight = 250;

            //----------------------------------------
            // ListBox
            //----------------------------------------

            lstMenu.Location = new Point(margin, bodyTop);
            lstMenu.Size = new Size(menuWidth, bodyHeight);

            //----------------------------------------
            // DataGridView
            //----------------------------------------

            int gridWidth = ClientSize.Width - gridX - margin;

            dgvOrder.Location = new Point(gridX, bodyTop);
            dgvOrder.Size = new Size(gridWidth, bodyHeight);

            //----------------------------------------
            // تعداد
            //----------------------------------------

            int bottomY = bodyTop + bodyHeight + 10;

            lblQty.Location = new Point(margin, bottomY + 6);

            numQuantity.Location = new Point(75, bottomY + 2);
            numQuantity.Size = new Size(70, 28);

            //----------------------------------------
            // سه دکمه + جمع کل در یک ردیف
            //----------------------------------------

            btnAdd.Size = new Size(180, 40);
            btnCheckout.Size = new Size(180, 40);
            btnInvoiceManager.Size = new Size(180, 40);

            btnAdd.Location = new Point(170, bottomY);

            btnCheckout.Location = new Point(370, bottomY);

            btnInvoiceManager.Location = new Point(570, bottomY);

            // جمع کل کنار دکمه مدیریت
            lblTotal.Location = new Point(770, bottomY + 8);
            lblTotal.Size = new Size(ClientSize.Width - 780, 25);
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
        }

        private void LoadMenuToUI()
        {
            lstMenu.Items.Clear();
            lstMenu.Font = new Font("Tahoma", 11, FontStyle.Bold);
            foreach (var p in _menuProducts)
                lstMenu.Items.Add($"{p.Name} - {p.Price:N0} تومان");
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (lstMenu.SelectedItem == null) return;
            string name = lstMenu.SelectedItem.ToString().Split('-')[0].Trim();
            var product = _menuProducts.FirstOrDefault(p => p.Name == name);
            if (product == null) return;
            int qtyToAdd = (int)numQuantity.Value;
            var existingItem = _currentOrder.FirstOrDefault(item => item.Product.Id == product.Id);
            if (existingItem != null)
                existingItem.Quantity += qtyToAdd;
            else
                _currentOrder.Add(new OrderItem { Product = product, Quantity = qtyToAdd });
            UpdateOrderGrid();
        }

        private void UpdateOrderGrid()
        {
            dgvOrder.Rows.Clear();
            double total = 0;
            foreach (var i in _currentOrder)
            {
                dgvOrder.Rows.Add(i.Product.Name, i.Product.Price.ToString("N0"), i.Quantity, i.TotalPrice.ToString("N0"));
                total += i.TotalPrice;
            }
            lblTotal.Text = $"جمع کل فاکتور: {total:N0} تومان";
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (_currentOrder.Count == 0)
            {
                MessageBox.Show("هیچ آیتمی در فاکتور وجود ندارد!", "هشدار", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newInvoice = new Invoice
            {
                CustomerName = txtCustomerName.Text.Trim(),
                TableNumber = txtTableNumber.Text.Trim(),
                OrderDate = DateTime.Now,
                Items = new List<OrderItem>(_currentOrder)
            };

            CafeManager.SaveInvoice(newInvoice);
            _currentOrder.Clear();
            UpdateOrderGrid();
            txtCustomerName.Clear();
            txtTableNumber.Clear();
            numQuantity.Value = 1;

            MessageBox.Show($"✅ فاکتور با موفقیت ثبت شد.\nشماره فاکتور: {newInvoice.Id}\nتاریخ: {newInvoice.OrderDate:yyyy/MM/dd HH:mm}",
                            "ثبت فاکتور", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtCustomerName.Focus();
        }

        private void dgvOrder_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvOrder.Columns[e.ColumnIndex].Name == "DeleteAction")
            {
                _currentOrder.RemoveAt(e.RowIndex);
                UpdateOrderGrid();
            }
            else if (dgvOrder.Columns[e.ColumnIndex].Name == "EditAction")
            {
                Form f = new Form
                {
                    Text = "ویرایش تعداد",
                    Size = new Size(250, 180),
                    RightToLeft = RightToLeft.Yes,
                    StartPosition = FormStartPosition.CenterParent
                };

                Label lbl = new Label { Text = "تعداد جدید:", Location = new Point(30, 30), Size = new Size(100, 25), Font = new Font("Tahoma", 10, FontStyle.Bold) };
                NumericUpDown n = new NumericUpDown
                {
                    Value = _currentOrder[e.RowIndex].Quantity,
                    Location = new Point(140, 28),
                    Size = new Size(70, 25),
                    Minimum = 1,
                    Font = new Font("Tahoma", 10)
                };

                Button b = new Button
                {
                    Text = "تایید تغییرات",
                    Location = new Point(80, 80),
                    Size = new Size(100, 35),
                    BackColor = Color.LightGreen,
                    Font = new Font("Tahoma", 10, FontStyle.Bold)
                };

                b.Click += (s, ev) =>
                {
                    _currentOrder[e.RowIndex].Quantity = (int)n.Value;
                    UpdateOrderGrid();
                    f.Close();
                };

                f.Controls.Add(lbl);
                f.Controls.Add(n);
                f.Controls.Add(b);
                f.ShowDialog();
            }
        }
    }
}