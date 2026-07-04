using System;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;
using System.Drawing;

namespace CafeManager
{
    public class UserManagementForm : Form
    {
        private DataGridView dgvUsers;
        private TextBox txtFullName;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private Button btnUpdate;
        private Button btnRefresh;
        private Button btnClose;
        private Label lblInfo;

        private User _selectedUser;

        public UserManagementForm()
        {
            this.Text = "👥 مدیریت کاربران";
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.MinimumSize = new Size(700, 500);
            this.BackColor = Color.WhiteSmoke;

            InitializeComponents();
            LoadUsers();

            this.Resize += (s, e) => AdjustControls();
        }

        private void InitializeComponents()
        {
            int margin = 15;

            // ============================================================
            // ✅ دیتاگرید با هدر زیبا (آبی نفتی)
            // ============================================================
            dgvUsers = new DataGridView
            {
                Location = new Point(margin, 10),
                Size = new Size(this.ClientSize.Width - (margin * 2), this.ClientSize.Height - 220),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = false,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Tahoma", 11),
                RowTemplate = { Height = 35 },
                EditMode = DataGridViewEditMode.EditOnEnter,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(220, 220, 220)
            };

            // ================================================================
            // ✅ تنظیم هدر دیتاگرید با رنگ آبی نفتی
            // ================================================================
            dgvUsers.ColumnHeadersHeight = 45;
            dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // ===== رنگ‌های هدر =====
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80);  // ← آبی نفتی
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(44, 62, 80);
            dgvUsers.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            // ===== تنظیم استایل ردیف‌ها =====
            dgvUsers.DefaultCellStyle.Font = new Font("Tahoma", 11);
            dgvUsers.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUsers.DefaultCellStyle.BackColor = Color.White;
            dgvUsers.DefaultCellStyle.ForeColor = Color.Black;
            dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvUsers.RowTemplate.Height = 35;

            // ===== تنظیم استایل ردیف‌های متناوب =====
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 250);
            dgvUsers.EnableHeadersVisualStyles = false;

            // ================================================================
            // ✅ طراحی زیبای هدر با خط جداکننده
            // ================================================================
            dgvUsers.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80);
            dgvUsers.RowHeadersDefaultCellStyle.ForeColor = Color.White;

            // ===== ستون‌ها =====
            dgvUsers.Columns.Add("Id", "کد");
            dgvUsers.Columns.Add("Username", "نام کاربری");
            dgvUsers.Columns.Add("Password", "رمز عبور");
            dgvUsers.Columns.Add("FullName", "نام کامل");
            dgvUsers.Columns.Add("Role", "نقش");
            dgvUsers.Columns.Add("IsActive", "فعال");

            // ================================================================
            // ✅ تنظیم ReadOnly برای ستون‌های غیرقابل ویرایش
            // ================================================================
            dgvUsers.Columns["Id"].ReadOnly = true;

            // ===== تنظیم رمز عبور با کاراکترهای مخفی =====
            dgvUsers.Columns["Password"].DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvUsers.Columns["Password"].DefaultCellStyle.ForeColor = Color.Gray;

            // ===== تنظیم عرض ستون‌ها =====
            dgvUsers.Columns["Id"].Width = 70;
            dgvUsers.Columns["Username"].Width = 140;
            dgvUsers.Columns["Password"].Width = 120;
            dgvUsers.Columns["FullName"].Width = 180;
            dgvUsers.Columns["Role"].Width = 120;
            dgvUsers.Columns["IsActive"].Width = 80;

            // ===== ساخت کامبوباکس برای ستون نقش =====
            DataGridViewComboBoxColumn roleColumn = new DataGridViewComboBoxColumn
            {
                Name = "Role",
                HeaderText = "نقش",
                DataPropertyName = "Role",
                Items = { "Admin", "Cashier" },
                FlatStyle = FlatStyle.Flat
            };
            dgvUsers.Columns.RemoveAt(4);
            dgvUsers.Columns.Insert(4, roleColumn);

            // ===== ساخت چک‌باکس برای ستون فعال =====
            DataGridViewCheckBoxColumn activeColumn = new DataGridViewCheckBoxColumn
            {
                Name = "IsActive",
                HeaderText = "فعال",
                DataPropertyName = "IsActive",
                TrueValue = true,
                FalseValue = false,
                FlatStyle = FlatStyle.Standard
            };
            dgvUsers.Columns.RemoveAt(5);
            dgvUsers.Columns.Insert(5, activeColumn);

            // ================================================================
            // ✅ رویداد ویرایش سلول - ذخیره خودکار
            // ================================================================
            dgvUsers.CellValueChanged += DgvUsers_CellValueChanged;
            dgvUsers.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvUsers.IsCurrentCellDirty)
                {
                    dgvUsers.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            // ================================================================
            // ✅ رویداد انتخاب ردیف
            // ================================================================
            dgvUsers.SelectionChanged += (s, e) =>
            {
                if (dgvUsers.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["Id"].Value);
                    _selectedUser = CafeManager.GetUserById(id);
                    if (_selectedUser != null)
                    {
                        txtFullName.Text = _selectedUser.FullName;
                        txtPassword.Text = "";
                        txtPassword.PlaceholderText = "رمز جدید وارد کنید...";
                        cmbRole.SelectedItem = _selectedUser.Role;
                        lblInfo.Text = $"👤 کاربر انتخاب شده: {_selectedUser.Username}";
                    }
                }
            };

            // ============================================================
            // ✅ پنل ویرایش (پایین صفحه)
            // ============================================================
            GroupBox grpEdit = new GroupBox
            {
                Text = "✏️ ویرایش کاربر انتخاب شده",
                Location = new Point(margin, this.ClientSize.Height - 120),
                Size = new Size(this.ClientSize.Width - (margin * 2), 120),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Tahoma", 10, FontStyle.Bold)
            };

            // ===== اطلاعات =====
            lblInfo = new Label
            {
                Text = "👤 کاربری را انتخاب کنید",
                Location = new Point(grpEdit.Width - 350, 20),
                Size = new Size(320, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleLeft
            };
            grpEdit.Controls.Add(lblInfo);

            // ===== نام کامل =====
            Label lblFullName = new Label
            {
                Text = "نام کامل:",
                Location = new Point(grpEdit.Width - 620, 25),
                Size = new Size(90, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };
            grpEdit.Controls.Add(lblFullName);

            txtFullName = new TextBox
            {
                Location = new Point(grpEdit.Width - 520, 22),
                Size = new Size(160, 30),
                Font = new Font("Tahoma", 11)
            };
            grpEdit.Controls.Add(txtFullName);

            // ===== رمز عبور =====
            Label lblPassword = new Label
            {
                Text = "رمز عبور:",
                Location = new Point(grpEdit.Width - 620, 60),
                Size = new Size(90, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };
            grpEdit.Controls.Add(lblPassword);

            txtPassword = new TextBox
            {
                Location = new Point(grpEdit.Width - 520, 57),
                Size = new Size(160, 30),
                Font = new Font("Tahoma", 11),
                PasswordChar = '●',
                PlaceholderText = "برای تغییر وارد کنید..."
            };
            grpEdit.Controls.Add(txtPassword);

            // ===== نقش =====
            Label lblRole = new Label
            {
                Text = "نقش:",
                Location = new Point(grpEdit.Width - 350, 25),
                Size = new Size(60, 30),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };
            grpEdit.Controls.Add(lblRole);

            cmbRole = new ComboBox
            {
                Location = new Point(grpEdit.Width - 280, 22),
                Size = new Size(130, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 11)
            };
            cmbRole.Items.AddRange(new object[] { "Admin", "Cashier" });
            grpEdit.Controls.Add(cmbRole);

            // ===== دکمه ذخیره =====
            btnUpdate = new Button
            {
                Text = "💾 ذخیره تغییرات",
                Location = new Point(200, 22),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
            btnUpdate.Click += BtnUpdate_Click;
            grpEdit.Controls.Add(btnUpdate);

            // ===== دکمه بروزرسانی =====
            btnRefresh = new Button
            {
                Text = "🔄 بروزرسانی",
                Location = new Point(50, 22),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            btnRefresh.Click += (s, e) => LoadUsers();
            grpEdit.Controls.Add(btnRefresh);

            // ===== دکمه بستن =====
            btnClose = new Button
            {
                Text = "❌ بستن",
                Location = new Point(this.ClientSize.Width - 130, this.ClientSize.Height - 60),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 57, 43);
            btnClose.Click += (s, e) => this.Close();

            // ============================================================
            // ✅ اضافه کردن کنترل‌ها
            // ============================================================
            this.Controls.AddRange(new Control[]
            {
                dgvUsers,
                grpEdit,
                btnClose
            });
        }

        // ================================================================
        // ✅ رویداد ویرایش سلول - ذخیره خودکار
        // ================================================================
        private void DgvUsers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var row = dgvUsers.Rows[e.RowIndex];
            int userId = Convert.ToInt32(row.Cells["Id"].Value);
            var user = CafeManager.GetUserById(userId);

            if (user == null) return;

            string columnName = dgvUsers.Columns[e.ColumnIndex].Name;

            try
            {
                switch (columnName)
                {
                    case "Username":
                        string newUsername = row.Cells["Username"].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(newUsername) && newUsername != user.Username)
                        {
                            var existingUser = CafeManager.GetUserByUsername(newUsername);
                            if (existingUser != null && existingUser.Id != userId)
                            {
                                MessageBox.Show($"⚠️ نام کاربری '{newUsername}' قبلاً ثبت شده است!",
                                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                LoadUsers();
                                return;
                            }

                            user.Username = newUsername;
                            CafeManager.UpdateUser(user);
                            ShowStatusMessage($"✅ نام کاربری به {newUsername} تغییر یافت");

                            if (_selectedUser != null && _selectedUser.Id == userId)
                            {
                                lblInfo.Text = $"👤 کاربر انتخاب شده: {newUsername}";
                            }
                        }
                        break;

                    case "Password":
                        string newPassword = row.Cells["Password"].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(newPassword) && newPassword.Length >= 3)
                        {
                            CafeManager.ChangeUserPassword(userId, newPassword);
                            ShowStatusMessage($"✅ رمز عبور کاربر {user.Username} تغییر یافت");
                            row.Cells["Password"].Value = "●●●●●●●●";
                        }
                        else if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 3)
                        {
                            MessageBox.Show("⚠️ رمز عبور باید حداقل 3 کاراکتر باشد!",
                                "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            LoadUsers();
                            return;
                        }
                        break;

                    case "FullName":
                        string newName = row.Cells["FullName"].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(newName) && newName != user.FullName)
                        {
                            user.FullName = newName;
                            CafeManager.UpdateUser(user);
                            ShowStatusMessage($"✅ نام کاربر {user.Username} به {newName} تغییر یافت");
                        }
                        break;

                    case "Role":
                        string newRole = row.Cells["Role"].Value?.ToString();
                        if (!string.IsNullOrEmpty(newRole) && newRole != user.Role)
                        {
                            if (user.Role == "Admin" && newRole == "Cashier")
                            {
                                var adminCount = CafeManager.GetUsers().Count(u => u.Role == "Admin");
                                if (adminCount <= 1)
                                {
                                    MessageBox.Show("⚠️ حداقل یک مدیر باید در سیستم وجود داشته باشد!",
                                        "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    LoadUsers();
                                    return;
                                }
                            }

                            user.Role = newRole;
                            CafeManager.UpdateUser(user);
                            ShowStatusMessage($"✅ نقش کاربر {user.Username} به {newRole} تغییر یافت");
                        }
                        break;

                    case "IsActive":
                        bool isActive = Convert.ToBoolean(row.Cells["IsActive"].Value);
                        if (isActive != user.IsActive)
                        {
                            if (user.Role == "Admin" && !isActive)
                            {
                                var adminCount = CafeManager.GetUsers().Count(u => u.Role == "Admin" && u.IsActive);
                                if (adminCount <= 1)
                                {
                                    MessageBox.Show("⚠️ حداقل یک مدیر فعال باید در سیستم وجود داشته باشد!",
                                        "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    LoadUsers();
                                    return;
                                }
                            }

                            user.IsActive = isActive;
                            CafeManager.UpdateUser(user);
                            ShowStatusMessage($"✅ وضعیت کاربر {user.Username} به {(isActive ? "فعال" : "غیرفعال")} تغییر یافت");
                        }
                        break;
                }

                if (_selectedUser != null && _selectedUser.Id == userId)
                {
                    txtFullName.Text = user.FullName;
                    txtPassword.Text = "";
                    txtPassword.PlaceholderText = "رمز جدید وارد کنید...";
                    cmbRole.SelectedItem = user.Role;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره تغییرات: {ex.Message}", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadUsers();
            }
        }

        private void ShowStatusMessage(string message)
        {
            this.Text = $"👥 مدیریت کاربران - {message}";

            var timer = new Timer { Interval = 3000 };
            timer.Tick += (s, e) =>
            {
                this.Text = "👥 مدیریت کاربران";
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void AdjustControls()
        {
            int margin = 15;

            dgvUsers.Size = new Size(
                this.ClientSize.Width - (margin * 2),
                this.ClientSize.Height - 160
            );

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is GroupBox grp && grp.Text == "✏️ ویرایش کاربر انتخاب شده")
                {
                    grp.Location = new Point(margin, this.ClientSize.Height - 160);
                    grp.Size = new Size(this.ClientSize.Width - (margin * 2), 140);

                    int grpW = grp.Width;

                    foreach (Control innerCtrl in grp.Controls)
                    {
                        if (innerCtrl is Label lbl)
                        {
                            if (lbl.Text == "نام کامل:")
                                lbl.Location = new Point(grpW - 620, 25);
                            else if (lbl.Text == "رمز عبور:")
                                lbl.Location = new Point(grpW - 620, 60);
                            else if (lbl.Text == "نقش:")
                                lbl.Location = new Point(grpW - 350, 25);
                            else if (lbl.Text.StartsWith("👤"))
                                lbl.Location = new Point(grpW - 350, 20);
                        }
                        else if (innerCtrl is TextBox txt)
                        {
                            if (txt.Name == "txtFullName")
                                txt.Location = new Point(grpW - 520, 22);
                            else if (txt.Name == "txtPassword")
                                txt.Location = new Point(grpW - 520, 57);
                        }
                        else if (innerCtrl is ComboBox cmb && cmb.Name == "cmbRole")
                            cmb.Location = new Point(grpW - 280, 22);
                        else if (innerCtrl is Button btn)
                        {
                            if (btn.Text.Contains("ذخیره"))
                                btn.Location = new Point(200, 22);
                            else if (btn.Text.Contains("بروزرسانی"))
                                btn.Location = new Point(50, 22);
                        }
                    }
                    break;
                }
            }

            btnClose.Location = new Point(
                this.ClientSize.Width - 130,
                this.ClientSize.Height - 60
            );
        }

        private void LoadUsers()
        {
            dgvUsers.Rows.Clear();
            var users = CafeManager.GetUsers(false);

            foreach (var user in users)
            {
                dgvUsers.Rows.Add(
                    user.Id,
                    user.Username,
                    "●●●●●●●●",
                    user.FullName,
                    user.Role,
                    user.IsActive
                );
            }

            if (dgvUsers.Rows.Count > 0)
            {
                dgvUsers.Rows[0].Selected = true;
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("لطفاً یک کاربر را انتخاب کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newFullName = txtFullName.Text.Trim();
            string newPassword = txtPassword.Text.Trim();
            string newRole = cmbRole.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(newFullName))
            {
                MessageBox.Show("لطفاً نام کامل را وارد کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(newRole))
            {
                MessageBox.Show("لطفاً نقش را انتخاب کنید.", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedUser.Role == "Admin" && newRole == "Cashier")
            {
                var adminCount = CafeManager.GetUsers().Count(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    MessageBox.Show("⚠️ حداقل یک مدیر باید در سیستم وجود داشته باشد!",
                        "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                _selectedUser.FullName = newFullName;
                _selectedUser.Role = newRole;
                CafeManager.UpdateUser(_selectedUser);

                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length >= 3)
                {
                    CafeManager.ChangeUserPassword(_selectedUser.Id, newPassword);
                    MessageBox.Show($"✅ رمز عبور کاربر {_selectedUser.Username} نیز تغییر یافت.",
                        "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 3)
                {
                    MessageBox.Show("⚠️ رمز عبور باید حداقل 3 کاراکتر باشد!",
                        "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show($"✅ کاربر {_selectedUser.Username} با موفقیت بروزرسانی شد.",
                    "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}