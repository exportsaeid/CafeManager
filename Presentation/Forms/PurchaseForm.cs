using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;

namespace CafeManager
{
    public class PurchaseForm : Form
    {
        private ComboBox cmbProducts;
        private NumericTextBox txtQuantity;
        private NumericTextBox txtUnitPrice;
        private Button btnAddItem;
        private Button btnRemoveItem;
        private Button btnConfirm;
        private Button btnCancel;
        private DataGridView dgvItems;
        private TextBox txtSupplier;
        private TextBox txtInvoiceNumber;
        private Label lblTotalAmount;
        private Label lblStatus;

        private Purchase _currentPurchase = new Purchase();
        private List<Product> _products;

        public PurchaseForm()
        {
            this.Text = "🛒 خرید روزانه کالا";
            this.ShowInTaskbar = false; // ← این خط را اضافه کنید
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            _products = CafeManager.GetProducts();
            InitializeComponents();
            LoadProducts();
            UpdateDisplay();
        }

        private void InitializeComponents()
        {
            int margin = 20;
            int y = 20;

            // ========== گروه اطلاعات خرید ==========
            GroupBox grpInfo = new GroupBox
            {
                Text = "اطلاعات خرید",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSupplier = new Label { Text = "تامین‌کننده:", Location = new Point(grpInfo.Width - 130, 30), Size = new Size(90, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            txtSupplier = new TextBox { Location = new Point(grpInfo.Width - 300, 27), Size = new Size(160, 25), Font = new Font("Tahoma", 10), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            Label lblInvoice = new Label { Text = "شماره فاکتور:", Location = new Point(grpInfo.Width - 450, 30), Size = new Size(100, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            txtInvoiceNumber = new TextBox { Location = new Point(grpInfo.Width - 560, 27), Size = new Size(100, 25), Font = new Font("Tahoma", 10), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            grpInfo.Controls.Add(lblSupplier);
            grpInfo.Controls.Add(txtSupplier);
            grpInfo.Controls.Add(lblInvoice);
            grpInfo.Controls.Add(txtInvoiceNumber);

            this.Controls.Add(grpInfo);
            y += grpInfo.Height + 10;

            // ========== گروه افزودن آیتم ==========
            GroupBox grpAdd = new GroupBox
            {
                Text = "➕ افزودن کالا به خرید",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 95),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblProduct = new Label { Text = "کالا:", Location = new Point(grpAdd.Width - 80, 30), Size = new Size(60, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            cmbProducts = new ComboBox
            {
                Location = new Point(grpAdd.Width - 300, 27),
                Size = new Size(210, 25),
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbProducts.DisplayMember = "Name";
            cmbProducts.ValueMember = "Id";

            Label lblQty = new Label { Text = "تعداد:", Location = new Point(grpAdd.Width - 480, 30), Size = new Size(50, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            txtQuantity = new NumericTextBox
            {
                Location = new Point(grpAdd.Width - 550, 27),
                Size = new Size(60, 25),
                Font = new Font("Tahoma", 10),
                Text = "",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            Label lblPrice = new Label { Text = "قیمت واحد:", Location = new Point(grpAdd.Width - 670, 30), Size = new Size(80, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            txtUnitPrice = new NumericTextBox
            {
                Location = new Point(grpAdd.Width - 760, 27),
                Size = new Size(80, 25),
                Font = new Font("Tahoma", 10),
                Text = "",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnAddItem = new Button
            {
                Text = "➕ افزودن به لیست",
                Location = new Point(20, 25),
                Size = new Size(120, 35),
                BackColor = Color.Khaki,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnAddItem.Click += BtnAddItem_Click;

            grpAdd.Controls.Add(lblProduct);
            grpAdd.Controls.Add(cmbProducts);
            grpAdd.Controls.Add(lblQty);
            grpAdd.Controls.Add(txtQuantity);
            grpAdd.Controls.Add(lblPrice);
            grpAdd.Controls.Add(txtUnitPrice);
            grpAdd.Controls.Add(btnAddItem);

            this.Controls.Add(grpAdd);
            y += grpAdd.Height + 10;

            // ========== جدول آیتم‌ها ==========
            Label lblItemsTitle = new Label
            {
                Text = "📋 لیست کالاهای خریداری‌شده:",
                Location = new Point(margin, y),
                Size = new Size(300, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            this.Controls.Add(lblItemsTitle);
            y += 30;

            dgvItems = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 200),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvItems.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvItems.Columns.Add("ProductId", "کد کالا");
            dgvItems.Columns.Add("ProductName", "نام کالا");
            dgvItems.Columns.Add("Quantity", "تعداد");
            dgvItems.Columns.Add("UnitPrice", "قیمت واحد");
            dgvItems.Columns.Add("Total", "جمع");

            dgvItems.Columns["Quantity"].DefaultCellStyle.Format = "N0";
            dgvItems.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
            dgvItems.Columns["Total"].DefaultCellStyle.Format = "N0";
            dgvItems.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            this.Controls.Add(dgvItems);

            // ========== پنل پایین ==========
            Panel pnlBottom = new Panel
            {
                Location = new Point(margin, this.ClientSize.Height - 100),
                Size = new Size(this.ClientSize.Width - (margin * 2), 90),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTotalAmount = new Label
            {
                Location = new Point(0, 5),
                Size = new Size(350, 30),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Text = "💰 جمع کل خرید: 0 تومان",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            lblStatus = new Label
            {
                Location = new Point(360, 5),
                Size = new Size(300, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.Orange,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            btnRemoveItem = new Button
            {
                Text = "🗑️ حذف انتخاب‌شده",
                Location = new Point(pnlBottom.Width - 430, 5),
                Size = new Size(150, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnRemoveItem.Click += BtnRemoveItem_Click;

            btnConfirm = new Button
            {
                Text = "✅ تایید نهایی خرید",
                Location = new Point(pnlBottom.Width - 270, 5),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Button
            {
                Text = "❌ لغو",
                Location = new Point(pnlBottom.Width - 110, 5),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnCancel.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(lblTotalAmount);
            pnlBottom.Controls.Add(lblStatus);
            pnlBottom.Controls.Add(btnRemoveItem);
            pnlBottom.Controls.Add(btnConfirm);
            pnlBottom.Controls.Add(btnCancel);

            this.Controls.Add(pnlBottom);

            // ========== رویداد Resize ==========
            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width - (margin * 2);
                int h = this.ClientSize.Height;

                grpInfo.Width = w;
                grpAdd.Width = w;

                int dgvHeight = h - grpInfo.Height - grpAdd.Height - 120 - 100 - 30;
                if (dgvHeight < 150) dgvHeight = 150;
                dgvItems.Height = dgvHeight;
                dgvItems.Width = w;

                pnlBottom.Location = new Point(margin, h - 100);
                pnlBottom.Width = w;

                // تنظیم موقعیت دکمه‌ها بر اساس عرض پنل
                btnRemoveItem.Location = new Point(pnlBottom.Width - 430, 5);
                btnConfirm.Location = new Point(pnlBottom.Width - 270, 5);
                btnCancel.Location = new Point(pnlBottom.Width - 110, 5);
            };
        }

        private void LoadProducts()
        {
            cmbProducts.Items.Clear();
            foreach (var p in _products.OrderBy(p => p.Name))
                cmbProducts.Items.Add(p);
            if (cmbProducts.Items.Count > 0) cmbProducts.SelectedIndex = 0;
        }

        private void UpdateDisplay()
        {
            dgvItems.Rows.Clear();
            foreach (var item in _currentPurchase.Items)
            {
                var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                string productName = product != null ? product.Name : "نامشخص";
                dgvItems.Rows.Add(item.ProductId, productName, item.Quantity, item.UnitPurchasePrice.ToString("N0"), item.TotalPrice.ToString("N0"));
            }

            lblTotalAmount.Text = $"💰 جمع کل خرید: {_currentPurchase.TotalAmount:N0} تومان";

            if (_currentPurchase.Items.Count == 0)
            {
                lblStatus.Text = "⚠️ هیچ کالایی به لیست اضافه نشده است.";
                lblStatus.ForeColor = Color.Orange;
                btnConfirm.Enabled = false;
            }
            else
            {
                lblStatus.Text = $"✅ {_currentPurchase.Items.Count} کالا در لیست خرید موجود است.";
                lblStatus.ForeColor = Color.DarkGreen;
                btnConfirm.Enabled = true;
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (cmbProducts.SelectedItem == null)
            {
                MessageBox.Show("لطفاً یک کالا انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Product selectedProduct = (Product)cmbProducts.SelectedItem;
            int quantity = txtQuantity.Value;
            double unitPrice = txtUnitPrice.Value;

            if (quantity <= 0)
            {
                MessageBox.Show("تعداد باید بزرگتر از صفر باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantity.Focus();
                return;
            }

            if (unitPrice <= 0)
            {
                MessageBox.Show("قیمت واحد باید بزرگتر از صفر باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnitPrice.Focus();
                return;
            }

            var existingItem = _currentPurchase.Items.FirstOrDefault(i => i.ProductId == selectedProduct.Id);
            if (existingItem != null)
            {
                DialogResult result = MessageBox.Show(
                    $"کالا '{selectedProduct.Name}' قبلاً در لیست خرید با تعداد {existingItem.Quantity} وجود دارد.\nآیا می‌خواهید تعداد را افزایش دهید؟",
                    "تکرار کالا",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (result == DialogResult.Yes)
                {
                    existingItem.Quantity += quantity;
                    existingItem.UnitPurchasePrice = unitPrice;
                }
            }
            else
            {
                _currentPurchase.Items.Add(new PurchaseItem
                {
                    ProductId = selectedProduct.Id,
                    Product = selectedProduct,
                    Quantity = quantity,
                    UnitPurchasePrice = unitPrice
                });
            }

            txtQuantity.Text = "";
            txtUnitPrice.Text = "";
            txtQuantity.Focus();

            UpdateDisplay();
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک آیتم را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(dgvItems.SelectedRows[0].Cells["ProductId"].Value);
            var item = _currentPurchase.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                DialogResult result = MessageBox.Show(
                    $"آیا از حذف کالا '{dgvItems.SelectedRows[0].Cells["ProductName"].Value}' از لیست خرید مطمئن هستید؟",
                    "تأیید حذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (result == DialogResult.Yes)
                {
                    _currentPurchase.Items.Remove(item);
                    UpdateDisplay();
                }
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (_currentPurchase.Items.Count == 0)
            {
                MessageBox.Show("هیچ کالایی برای خرید انتخاب نشده است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentPurchase.SupplierName = txtSupplier.Text.Trim();
            _currentPurchase.InvoiceNumber = txtInvoiceNumber.Text.Trim();
            _currentPurchase.PurchaseDate = DateTime.Now;

            CafeManager.AddPurchase(_currentPurchase);

            string summary = $"🛒 خلاصه خرید\n\n" +
                            $"تامین‌کننده: {_currentPurchase.SupplierName}\n" +
                            $"شماره فاکتور: {_currentPurchase.InvoiceNumber}\n" +
                            $"تاریخ: {DateTime.Now:yyyy/MM/dd HH:mm}\n" +
                            $"تعداد اقلام: {_currentPurchase.Items.Count}\n" +
                            $"جمع کل: {_currentPurchase.TotalAmount:N0} تومان\n\n" +
                            $"آیا از تایید نهایی این خرید اطمینان دارید؟\n" +
                            $"پس از تایید، موجودی انبار به‌روز می‌شود.";

            DialogResult confirm = MessageBox.Show(summary, "تایید نهایی خرید", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                CafeManager.ConfirmPurchase(_currentPurchase.Id);
                MessageBox.Show("✅ خرید با موفقیت تایید شد و موجودی انبار به‌روز گردید.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("خرید ذخیره شد اما تایید نهایی نشد.\nبرای تایید بعداً به بخش تاریخچه خرید مراجعه کنید.", "ذخیره موقت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ========== ریست کردن فرم برای خرید بعدی ==========
            _currentPurchase = new Purchase();
            txtSupplier.Text = "";
            txtInvoiceNumber.Text = "";
            txtQuantity.Text = "";
            txtUnitPrice.Text = "";
            if (cmbProducts.Items.Count > 0) cmbProducts.SelectedIndex = 0;
            UpdateDisplay();
            txtSupplier.Focus();
        }
    }
}