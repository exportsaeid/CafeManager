using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;

namespace CafeManager
{
    // ✅ کلاس سفارشی TextBox با قابلیت جداکننده هزارگان
    //public class NumericTextBox : TextBox
    //{
    //    private int _value = 0;

    //    public int Value
    //    {
    //        get
    //        {
    //            if (int.TryParse(Text.Replace(",", ""), out int result))
    //                return result;
    //            return 0;
    //        }
    //        set
    //        {
    //            _value = value;
    //            Text = value.ToString("N0");
    //        }
    //    }

    //    public NumericTextBox()
    //    {
    //        this.TextAlign = HorizontalAlignment.Left;
    //        this.KeyPress += NumericTextBox_KeyPress;
    //        this.Leave += NumericTextBox_Leave;
    //        this.Enter += NumericTextBox_Enter;
    //    }

    //    private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
    //    {
    //        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
    //        {
    //            e.Handled = true;
    //        }
    //    }

    //    private void NumericTextBox_Enter(object sender, EventArgs e)
    //    {
    //        Text = Text.Replace(",", "");
    //        Select(Text.Length, 0);
    //    }

    //    private void NumericTextBox_Leave(object sender, EventArgs e)
    //    {
    //        if (string.IsNullOrEmpty(Text))
    //        {
    //            Text = "0";
    //            return;
    //        }

    //        if (int.TryParse(Text.Replace(",", ""), out int value))
    //        {
    //            Text = value.ToString("N0");
    //        }
    //        else
    //        {
    //            Text = "0";
    //        }
    //    }
    ////}

    public class InventoryForm : Form
    {
        private DataGridView dgvInventory;
        private NumericTextBox txtStock;
        private NumericTextBox txtThreshold;
        private Button btnSaveStock;
        private Label lblAlertStatus;
        private TextBox txtSearch;
        private Button btnSearch;
        private ComboBox cmbSearchType;
        private List<Product> allProducts;

        public InventoryForm()
        {
            this.Text = "سیستم هوشمند مدیریت انبار و موجودی کالا";
            this.ShowInTaskbar = false;
            this.Size = new Size(750, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);

            InitializeComponents();
            LoadAllProducts();
            RefreshInventoryGrid();
            CheckLowStockAlerts();
        }

        private void InitializeComponents()
        {
            // پنل جستجو
            GroupBox grpSearch = new GroupBox
            {
                Text = "جستجوی کالا",
                Location = new Point(20, 20),
                Size = new Size(690, 70)
            };

            txtSearch = new TextBox
            {
                Location = new Point(300, 30),
                Size = new Size(250, 25),
                Font = new Font("Tahoma", 10)
            };

            cmbSearchType = new ComboBox
            {
                Location = new Point(560, 30),
                Size = new Size(110, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 10)
            };
            cmbSearchType.Items.AddRange(new[] { "نام محصول", "کد کالا" }); // ✅ حذف دسته بندی
            cmbSearchType.SelectedIndex = 0;

            btnSearch = new Button
            {
                Text = "🔍 جستجو",
                Location = new Point(200, 28),
                Size = new Size(90, 30),
                BackColor = Color.LightBlue,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };
            btnSearch.Click += BtnSearch_Click;

            grpSearch.Controls.Add(txtSearch);
            grpSearch.Controls.Add(cmbSearchType);
            grpSearch.Controls.Add(btnSearch);

            // ۱. جدول نمایش موجودی انبار
            dgvInventory = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(690, 280),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,

                // تنظیم هدر جدول
                ColumnHeadersHeight = 30,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };

            // وسط‌چین شدن عنوان ستون‌ها
            dgvInventory.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            // وسط‌چین شدن اطلاعات داخل سلول‌ها
            dgvInventory.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            // ✅ حذف ستون دسته بندی
            dgvInventory.Columns.Add("Id", "کد کالا");
            dgvInventory.Columns.Add("Name", "نام محصول");
            dgvInventory.Columns.Add("Stock", "موجودی انبار");
            dgvInventory.Columns.Add("Threshold", "آستانه هشدار (کمبود)");

            dgvInventory.Columns["Stock"].DefaultCellStyle.Format = "N0";
            dgvInventory.Columns["Threshold"].DefaultCellStyle.Format = "N0";
            dgvInventory.Columns["Stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInventory.Columns["Threshold"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvInventory.SelectionChanged += DgvInventory_SelectionChanged;

            lblAlertStatus = new Label
            {
                Location = new Point(20, 390),
                Size = new Size(690, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.LightGray
            };

            GroupBox grpEdit = new GroupBox
            {
                Text = "به‌روزرسانی موجودی انبار",
                Location = new Point(20, 430),
                Size = new Size(690, 120)
            };

            txtStock = new NumericTextBox
            {
                Location = new Point(430, 45),
                Size = new Size(120, 25),
                Font = new Font("Tahoma", 10)
            };

            txtThreshold = new NumericTextBox
            {
                Location = new Point(140, 45),
                Size = new Size(120, 25),
                Font = new Font("Tahoma", 10)
            };

            btnSaveStock = new Button
            {
                Text = "ثبت و اصلاح انبار 💾",
                Location = new Point(20, 40),
                Size = new Size(110, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };
            btnSaveStock.Click += BtnSaveStock_Click;

            Label lblStockTitle = new Label
            {
                Text = "موجودی انبار:",
                Location = new Point(555, 48),
                Size = new Size(90, 20),
                Font = new Font("Tahoma", 10)
            };

            Label lblThresholdTitle = new Label
            {
                Text = "حداقل برای هشدار:",
                Location = new Point(265, 48),
                Size = new Size(120, 20),
                Font = new Font("Tahoma", 10)
            };

            grpEdit.Controls.Add(lblStockTitle);
            grpEdit.Controls.Add(txtStock);
            grpEdit.Controls.Add(lblThresholdTitle);
            grpEdit.Controls.Add(txtThreshold);
            grpEdit.Controls.Add(btnSaveStock);

            this.Controls.AddRange(new Control[] { grpSearch, dgvInventory, lblAlertStatus, grpEdit });
        }

        private void LoadAllProducts()
        {
            allProducts = CafeManager.GetProducts();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                RefreshInventoryGridWithFilter(allProducts);
                return;
            }

            string searchType = cmbSearchType.SelectedItem.ToString();
            List<Product> filteredProducts = new List<Product>();

            switch (searchType)
            {
                case "نام محصول":
                    filteredProducts = allProducts.Where(p =>
                        p.Name != null && p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case "کد کالا":
                    if (int.TryParse(searchText, out int id))
                    {
                        filteredProducts = allProducts.Where(p => p.Id == id).ToList();
                    }
                    else
                    {
                        filteredProducts = new List<Product>();
                    }
                    break;
            }

            RefreshInventoryGridWithFilter(filteredProducts);

            if (filteredProducts.Count == 0)
            {
                lblAlertStatus.Text = $"⚠️ هیچ نتیجه‌ای برای جستجوی '{searchText}' در {searchType} یافت نشد.";
                lblAlertStatus.BackColor = Color.LightYellow;
                lblAlertStatus.ForeColor = Color.DarkOrange;
            }
            else
            {
                CheckLowStockAlerts();
            }
        }

        private void RefreshInventoryGridWithFilter(List<Product> productsToShow)
        {
            dgvInventory.Rows.Clear();

            foreach (var p in productsToShow)
            {
                // ✅ حذف دسته بندی از نمایش
                int rowIndex = dgvInventory.Rows.Add(p.Id, p.Name, p.Stock, p.LowStockThreshold);

                if (p.Stock <= p.LowStockThreshold)
                {
                    dgvInventory.Rows[rowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
                    dgvInventory.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }

        private void RefreshInventoryGrid()
        {
            RefreshInventoryGridWithFilter(allProducts);
        }

        private void CheckLowStockAlerts()
        {
            var lowStockItems = CafeManager.GetLowStockAlerts();
            if (lowStockItems.Count > 0)
            {
                lblAlertStatus.Text = $"⚠️ آلارم انبار: تعداد {lowStockItems.Count:N0} کالا رو به اتمام است! لطفا انبار را شارژ کنید.";
                lblAlertStatus.BackColor = Color.LightCoral;
                lblAlertStatus.ForeColor = Color.White;
            }
            else
            {
                lblAlertStatus.Text = "✅ وضعیت انبار مطلوب است؛ هیچ کالایی در وضعیت بحرانی نیست.";
                lblAlertStatus.BackColor = Color.LightGreen;
                lblAlertStatus.ForeColor = Color.DarkGreen;
            }
        }

        private void DgvInventory_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count > 0)
            {
                // ✅ ایندکس‌ها به دلیل حذف ستون دسته بندی تغییر کرده است
                int stock = Convert.ToInt32(dgvInventory.SelectedRows[0].Cells["Stock"].Value);
                int threshold = Convert.ToInt32(dgvInventory.SelectedRows[0].Cells["Threshold"].Value);

                txtStock.Value = stock;
                txtThreshold.Value = threshold;
            }
        }

        private void BtnSaveStock_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفا یک محصول را انتخاب کنید.", "اخطار", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(dgvInventory.SelectedRows[0].Cells["Id"].Value);
            int newStock = txtStock.Value;
            int threshold = txtThreshold.Value;

            CafeManager.UpdateStock(productId, newStock, threshold);

            LoadAllProducts();
            RefreshInventoryGrid();
            CheckLowStockAlerts();

            MessageBox.Show("موجودی انبار با موفقیت به‌روزرسانی شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}