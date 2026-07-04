using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;

namespace CafeManager
{
    public class MenuManagerForm : Form
    {
        private DataGridView dgvMenu;
        private TextBox txtName;
        private TextBox txtPrice;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnSearch;
        private Button btnRefresh;
        private List<Product> allProducts; // برای نگهداری لیست کامل محصولات

        public MenuManagerForm()
        {
            this.Text = "مدیریت و تعریف کالاها (منوی کافه)";
            this.Size = new Size(650, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            InitializeComponents();
            RefreshMenuGrid();
        }

        private void InitializeComponents()
        {
            // ۱. بخش جستجو
            GroupBox grpSearch = new GroupBox
            {
                Text = "جستجوی کالا",
                Location = new Point(20, 20),
                Size = new Size(590, 65)
            };

            Label lblSearch = new Label
            {
                Text = "نام کالا:",
                Location = new Point(500, 28),
                Size = new Size(70, 25)
            };

            txtSearch = new TextBox
            {
                Location = new Point(330, 25),
                Size = new Size(160, 25)
            };

            btnSearch = new Button
            {
                Text = "جستجو 🔍",
                Location = new Point(200, 23),
                Size = new Size(100, 30),
                BackColor = Color.LightYellow,
                FlatStyle = FlatStyle.Flat
            };
            btnSearch.Click += BtnSearch_Click;

            btnRefresh = new Button
            {
                Text = "نمایش همه 🔄",
                Location = new Point(90, 23),
                Size = new Size(100, 30),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += BtnRefresh_Click;

            grpSearch.Controls.Add(lblSearch);
            grpSearch.Controls.Add(txtSearch);
            grpSearch.Controls.Add(btnSearch);
            grpSearch.Controls.Add(btnRefresh);

            // ۲. جدول نمایش لیست منو
            dgvMenu = new DataGridView
            {
                Location = new Point(20, 95),
                Size = new Size(590, 200),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvMenu.Columns.Add("Id", "کد کالا");
            dgvMenu.Columns.Add("Name", "نام محصول");
            dgvMenu.Columns.Add("Price", "قیمت (تومان)");
            dgvMenu.SelectionChanged += DgvMenu_SelectionChanged;

            // ۳. باکس ورودی‌ها
            GroupBox grpInputs = new GroupBox
            {
                Text = "مشخصات کالا",
                Location = new Point(20, 310),
                Size = new Size(590, 85)
            };

            Label lblName = new Label
            {
                Text = "نام محصول:",
                Location = new Point(500, 38),
                Size = new Size(80, 25)
            };

            Label lblPrice = new Label
            {
                Text = "قیمت واحد:",
                Location = new Point(200, 38),
                Size = new Size(80, 25)
            };

            txtName = new TextBox
            {
                Location = new Point(330, 35),
                Size = new Size(160, 25)
            };

            txtPrice = new TextBox
            {
                Location = new Point(60, 35),
                Size = new Size(130, 25),
                TextAlign = HorizontalAlignment.Right
            };

            // رویدادهای فرمتینگ قیمت
            txtPrice.KeyPress += TxtPrice_KeyPress;
            txtPrice.TextChanged += TxtPrice_TextChanged;

            grpInputs.Controls.Add(lblName);
            grpInputs.Controls.Add(lblPrice);
            grpInputs.Controls.Add(txtName);
            grpInputs.Controls.Add(txtPrice);

            // ۴. دکمه‌های عملیات
            btnAdd = new Button
            {
                Text = "افزودن به منو ➕",
                Location = new Point(440, 420),
                Size = new Size(170, 45),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button
            {
                Text = "ویرایش کالا 💾",
                Location = new Point(250, 420),
                Size = new Size(170, 45),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button
            {
                Text = "حذف از منو ❌",
                Location = new Point(20, 420),
                Size = new Size(200, 45),
                BackColor = Color.MistyRose,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.Click += BtnDelete_Click;

            // اضافه کردن کنترل‌ها به فرم
            this.Controls.Add(grpSearch);
            this.Controls.Add(dgvMenu);
            this.Controls.Add(grpInputs);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnUpdate);
            this.Controls.Add(btnDelete);
        }

        // ====================== فرمتینگ قیمت ======================
        private void TxtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // فقط عدد و Backspace مجاز است
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void TxtPrice_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPrice.Text))
                return;

            // حذف کاماهای قبلی برای پردازش
            string cleanText = txtPrice.Text.Replace(",", "");

            if (long.TryParse(cleanText, out long number))
            {
                // اعمال فرمت با جداکننده هزارگان
                txtPrice.Text = number.ToString("N0");
                txtPrice.SelectionStart = txtPrice.Text.Length; // کرسر در آخر
                txtPrice.SelectionLength = 0;
            }
        }

        // ====================== متدهای جستجو ======================
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                MessageBox.Show("لطفاً نام کالای مورد نظر را وارد کنید.", "راهنما", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // جستجو در لیست کامل محصولات
            var results = allProducts.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            if (results.Count == 0)
            {
                MessageBox.Show("کالایی با این نام یافت نشد.", "نتیجه جستجو", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgvMenu.Rows.Clear();
            }
            else
            {
                DisplayProducts(results);
                MessageBox.Show($"{results.Count} کالا یافت شد.", "نتیجه جستجو", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            RefreshMenuGrid();
        }

        private void DisplayProducts(List<Product> products)
        {
            dgvMenu.Rows.Clear();
            foreach (var p in products)
            {
                dgvMenu.Rows.Add(p.Id, p.Name, p.Price.ToString("N0"));
            }
        }

        // ====================== متدهای اصلی ======================
        private void RefreshMenuGrid()
        {
            allProducts = CafeManager.GetMenu();
            DisplayProducts(allProducts);
        }

        private void DgvMenu_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMenu.SelectedRows.Count > 0)
            {
                txtName.Text = dgvMenu.SelectedRows[0].Cells["Name"].Value.ToString();

                // قیمت بدون کاما برای ویرایش
                string priceStr = dgvMenu.SelectedRows[0].Cells["Price"].Value.ToString().Replace(",", "");
                txtPrice.Text = priceStr;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || !double.TryParse(txtPrice.Text.Replace(",", ""), out double price))
            {
                MessageBox.Show("لطفاً نام محصول و قیمت معتبر وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string newProductName = txtName.Text.Trim();

            // بررسی تکراری نبودن نام محصول
            var existingProducts = CafeManager.GetMenu();
            bool isDuplicate = existingProducts.Any(p => p.Name.Equals(newProductName, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                MessageBox.Show("این نام محصول قبلاً در منو وجود دارد. لطفاً نام دیگری انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CafeManager.AddProductToMenu(newProductName, price);
            RefreshMenuGrid();
            txtName.Clear();
            txtPrice.Clear();
            txtSearch.Clear();

            MessageBox.Show("محصول جدید با موفقیت به منو اضافه شد.\nنکته: اکنون موجودی انبار آن صفر است؛ لطفاً از بخش مدیریت انبار شارژ کنید.",
                "ثبت موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvMenu.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک کالا را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text) || !double.TryParse(txtPrice.Text.Replace(",", ""), out double price))
            {
                MessageBox.Show("اطلاعات جهت ویرایش معتبر نیست.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int productId = Convert.ToInt32(dgvMenu.SelectedRows[0].Cells["Id"].Value);
            string newProductName = txtName.Text.Trim();

            // بررسی تکراری نبودن نام محصول در هنگام ویرایش (به جز خود محصول جاری)
            var existingProducts = CafeManager.GetMenu();
            bool isDuplicate = existingProducts.Any(p => p.Id != productId && p.Name.Equals(newProductName, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                MessageBox.Show("این نام محصول قبلاً در منو وجود دارد. لطفاً نام دیگری انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CafeManager.UpdateProductInMenu(productId, newProductName, price);
            RefreshMenuGrid();
            txtSearch.Clear();
            MessageBox.Show("تغییرات کالا با موفقیت ذخیره شد.", "ویرایش", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvMenu.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً یک کالا را برای حذف انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("آیا از حذف کامل این کالا از منو مطمئن هستید؟", "تایید حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int productId = Convert.ToInt32(dgvMenu.SelectedRows[0].Cells["Id"].Value);
                CafeManager.DeleteProductFromMenu(productId);
                RefreshMenuGrid();
                txtName.Clear();
                txtPrice.Clear();
                txtSearch.Clear();
                MessageBox.Show("کالا از منو حذف شد.", "حذف کالا", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}