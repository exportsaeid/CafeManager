using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;

namespace CafeManager
{
    public class StocktakeForm : Form
    {
        private DataGridView dgvStocktake;
        private Button btnFinalize;
        private Button btnCancel;
        private Label lblStatus;
        private TextBox txtNote;
        private List<Product> _products;
        private Stocktake _currentStocktake = new Stocktake();

        public StocktakeForm()
        {
            this.Text = "📊 انبارگردانی و شمارش موجودی";
            this.Size = new Size(800, 650);
            this.MinimumSize = new Size(750, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            _products = CafeManager.GetProducts();
            InitializeComponents();
            LoadStocktakeItems();
            UpdateDisplay();
        }

        private void InitializeComponents()
        {
            int margin = 20;
            int y = 20;

            // ========== گروه توضیحات ==========
            GroupBox grpInfo = new GroupBox
            {
                Text = "اطلاعات انبارگردانی",
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblNote = new Label { Text = "توضیحات:", Location = new Point(grpInfo.Width - 100, 28), Size = new Size(70, 25) };
            txtNote = new TextBox { Location = new Point(grpInfo.Width - 400, 25), Size = new Size(290, 25), Font = new Font("Tahoma", 10) };

            grpInfo.Controls.Add(lblNote);
            grpInfo.Controls.Add(txtNote);

            y += grpInfo.Height + 10;

            // ========== جدول انبارگردانی ==========
            Label lblTitle = new Label
            {
                Text = "📋 لیست کالاها - موجودی سیستم را با موجودی شمارش‌شده جایگزین کنید:",
                Location = new Point(margin, y),
                Size = new Size(600, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };
            y += 30;

            dgvStocktake = new DataGridView
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 350),
                AllowUserToAddRows = false,
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 35,
                EditMode = DataGridViewEditMode.EditOnEnter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvStocktake.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvStocktake.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // ستون‌ها
            dgvStocktake.Columns.Add("ProductId", "کد کالا");
            dgvStocktake.Columns.Add("ProductName", "نام کالا");
            dgvStocktake.Columns.Add("SystemStock", "موجودی سیستم");
            dgvStocktake.Columns.Add("PhysicalStock", "موجودی شمارش‌شده");
            dgvStocktake.Columns.Add("Difference", "اختلاف");

            dgvStocktake.Columns["SystemStock"].DefaultCellStyle.Format = "N0";
            dgvStocktake.Columns["PhysicalStock"].DefaultCellStyle.Format = "N0";
            dgvStocktake.Columns["Difference"].DefaultCellStyle.Format = "N0";
            dgvStocktake.Columns["SystemStock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvStocktake.Columns["PhysicalStock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvStocktake.Columns["Difference"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // ستون PhysicalStock قابل ویرایش است
            dgvStocktake.Columns["PhysicalStock"].ReadOnly = false;

            dgvStocktake.CellValueChanged += DgvStocktake_CellValueChanged;
            dgvStocktake.CurrentCellDirtyStateChanged += DgvStocktake_CurrentCellDirtyStateChanged;

            y += dgvStocktake.Height + 15;

            // ========== وضعیت و جمع‌بندی ==========
            lblStatus = new Label
            {
                Location = new Point(margin, y),
                Size = new Size(this.ClientSize.Width - (margin * 2), 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.LightGray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            y += 40;

            // ========== دکمه‌ها ==========
            btnFinalize = new Button
            {
                Text = "✅ تایید نهایی انبارگردانی",
                Location = new Point(this.ClientSize.Width - 320, y),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnFinalize.Click += BtnFinalize_Click;

            btnCancel = new Button
            {
                Text = "❌ لغو و بازگشت",
                Location = new Point(this.ClientSize.Width - 160, y),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                grpInfo,
                lblTitle,
                dgvStocktake,
                lblStatus,
                btnFinalize,
                btnCancel
            });

            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width - (margin * 2);
                grpInfo.Width = w;
                dgvStocktake.Width = w;
                lblStatus.Width = w;
                btnFinalize.Location = new Point(this.ClientSize.Width - 320, btnFinalize.Location.Y);
                btnCancel.Location = new Point(this.ClientSize.Width - 160, btnCancel.Location.Y);
            };
        }

        private void LoadStocktakeItems()
        {
            dgvStocktake.Rows.Clear();

            foreach (var product in _products.OrderBy(p => p.Name))
            {
                int rowIndex = dgvStocktake.Rows.Add(
                    product.Id,
                    product.Name,
                    product.Stock,
                    product.Stock, // پیش‌فرض: موجودی سیستم
                    0 // اختلاف بعداً محاسبه می‌شود
                );

                // اگر موجودی سیستم صفر است، با رنگ مشخص شود
                if (product.Stock == 0)
                {
                    dgvStocktake.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }

            CalculateDifferences();
        }

        private void CalculateDifferences()
        {
            foreach (DataGridViewRow row in dgvStocktake.Rows)
            {
                if (row.Cells["SystemStock"].Value != null && row.Cells["PhysicalStock"].Value != null)
                {
                    int systemStock = Convert.ToInt32(row.Cells["SystemStock"].Value);
                    int physicalStock = Convert.ToInt32(row.Cells["PhysicalStock"].Value);
                    int difference = physicalStock - systemStock;
                    row.Cells["Difference"].Value = difference;

                    // رنگ‌بندی اختلاف
                    if (difference != 0)
                    {
                        row.DefaultCellStyle.BackColor = difference > 0 ? Color.LightGreen : Color.LightCoral;
                    }
                    else
                    {
                        // اگر قبلاً رنگی نبود، به حالت عادی برگردان
                        if (row.DefaultCellStyle.BackColor != Color.LightYellow)
                            row.DefaultCellStyle.BackColor = Color.White;
                    }
                }
            }

            // به‌روزرسانی وضعیت
            int diffCount = 0;
            foreach (DataGridViewRow row in dgvStocktake.Rows)
            {
                if (row.Cells["Difference"].Value != null && Convert.ToInt32(row.Cells["Difference"].Value) != 0)
                    diffCount++;
            }

            if (diffCount == 0)
            {
                lblStatus.Text = "✅ تمام موجودی‌ها با سیستم مطابقت دارند. نیازی به تایید نیست.";
                lblStatus.BackColor = Color.LightGreen;
                lblStatus.ForeColor = Color.DarkGreen;
                btnFinalize.Enabled = false;
            }
            else
            {
                lblStatus.Text = $"⚠️ {diffCount} کالا دارای اختلاف هستند. لطفاً بررسی و سپس تایید نهایی را انجام دهید.";
                lblStatus.BackColor = Color.LightCoral;
                lblStatus.ForeColor = Color.White;
                btnFinalize.Enabled = true;
            }
        }

        private void DgvStocktake_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvStocktake.CurrentCell != null &&
                dgvStocktake.CurrentCell.ColumnIndex == dgvStocktake.Columns["PhysicalStock"].Index)
            {
                dgvStocktake.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvStocktake_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvStocktake.Columns["PhysicalStock"].Index)
            {
                CalculateDifferences();
            }
        }

        private void UpdateDisplay()
        {
            CalculateDifferences();
        }

        private void BtnFinalize_Click(object sender, EventArgs e)
        {
            // ساخت آیتم‌های انبارگردانی از جدول
            _currentStocktake.Items.Clear();
            foreach (DataGridViewRow row in dgvStocktake.Rows)
            {
                int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                int systemStock = Convert.ToInt32(row.Cells["SystemStock"].Value);
                int physicalStock = Convert.ToInt32(row.Cells["PhysicalStock"].Value);

                var product = _products.FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    _currentStocktake.Items.Add(new StocktakeItem
                    {
                        ProductId = productId,
                        Product = product,
                        SystemStock = systemStock,
                        PhysicalStock = physicalStock
                    });
                }
            }

            // تنظیم توضیحات
            _currentStocktake.Note = txtNote.Text.Trim();
            _currentStocktake.StocktakeDate = DateTime.Now;

            // نمایش خلاصه برای تایید نهایی
            int diffCount = _currentStocktake.Items.Count(i => i.Difference != 0);
            string summary = $"📊 خلاصه انبارگردانی\n\n" +
                            $"تاریخ: {DateTime.Now:yyyy/MM/dd HH:mm}\n" +
                            $"تعداد کالاهای شمارش‌شده: {_currentStocktake.Items.Count}\n" +
                            $"تعداد کالاهای با اختلاف: {diffCount}\n" +
                            $"توضیحات: {_currentStocktake.Note}\n\n" +
                            $"آیا از تایید نهایی این انبارگردانی اطمینان دارید؟\n" +
                            $"پس از تایید، موجودی انبار با مقادیر شمارش‌شده جایگزین می‌شود.";

            DialogResult confirm = MessageBox.Show(summary, "تایید نهایی انبارگردانی", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                // ذخیره در دیتابیس
                CafeManager.AddStocktake(_currentStocktake);

                // تایید نهایی و اعمال تغییرات
                CafeManager.FinalizeStocktake(_currentStocktake.Id);

                MessageBox.Show("✅ انبارگردانی با موفقیت تایید شد و موجودی انبار به‌روز گردید.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}