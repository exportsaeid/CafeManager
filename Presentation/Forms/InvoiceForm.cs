using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CafeManager;
using CafeManager.Models;

namespace CafeManager
{
    public class InvoiceForm : Form
    {
        #region فیلدها

        private bool _firstLoad = true;
        private bool _isUpdating = false;
        private bool _isShowingAllInvoices = false;
        private int _currentlyEditingInvoiceId = -1;
        private Invoice _currentSelectedInvoice;
        private List<Invoice> _allInvoices;

        // کنترل‌ها
        private DataGridView _dgvInvoices;
        private DataGridView _dgvInvoiceItems;
        private TextBox _txtEditName;
        private TextBox _txtEditTable;
        private NumericUpDown _numItemQty;
        private ComboBox _cmbAllProducts;
        private NumericUpDown _numNewItemQty;
        private Button _btnAddNewItem;
        private Button _btnUpdateInvoice;
        private Button _btnDeleteInvoice;
        private Button _btnUpdateItem;
        private Button _btnDeleteItem;
        private Button _btnPrintInvoice;
        private Button _btnSearch;
        private Button _btnResetSearch;
        private Button _btnShowAllInvoices;
        private TextBox _txtSearchCustomer;
        private TextBox _txtSearchTable;
        private TextBox _txtSearchInvoiceId;
        private Label _lblTotalInvoiceAmount;
        private Label _lblShiftInfo;
        private Label _lblInvTitle;
        private Label _lblItemsTitle;
        private Label _lblSearchTitle;
        private GroupBox _grpInvoiceEdit;
        private GroupBox _grpItemEdit;
        private GroupBox _grpAddNewItem;

        #endregion

        #region سازنده

        public InvoiceForm()
        {
            InitializeForm();
            LoadInitialData();
        }

        private void InitializeForm()
        {
            this.Text = "مدیریت پیشرفته فاکتورها و رکوردها";
            this.ShowInTaskbar = false;
            this.TopMost = false;
            this.Size = new Size(1000, 800);
            this.MinimumSize = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.WindowState = FormWindowState.Maximized;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.WhiteSmoke;

            InitializeComponents();

            this.Load += (s, e) => {
                AdjustControlsSizeAndPosition();
                RefreshAll();
                _firstLoad = false;
            };

            this.Resize += (s, e) => this.BeginInvoke(new Action(() => {
                AdjustControlsSizeAndPosition();
                this.Refresh();
            }));
        }

        private void LoadInitialData()
        {
            LoadAllInvoices();
            LoadProductsToCombo();
            RefreshInvoiceGrid();
        }

        #endregion

        #region متدهای کمکی تاریخ و شیفت

        private string ConvertToPersianDate(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:00}/{day:00}";
        }

        private string ConvertToPersianDateWithTime(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            return $"{year:0000}/{month:00}/{day:00} - {date.Hour:00}:{date.Minute:00}";
        }

        private string GetTimeAgo(DateTime date)
        {
            var diff = DateTime.Now - date;
            if (diff.TotalMinutes < 1) return "لحظاتی پیش";
            if (diff.TotalMinutes < 60) return $"{diff.Minutes} دقیقه پیش";
            if (diff.TotalHours < 24) return $"{diff.Hours} ساعت پیش";
            if (diff.TotalDays < 7) return $"{diff.Days} روز پیش";
            return ConvertToPersianDate(date);
        }

        private DateTime GetShiftStartTime()
        {
            DateTime now = DateTime.Now;
            DateTime shiftStart = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0);
            if (now.Hour < 15)
                shiftStart = shiftStart.AddDays(-1);
            return shiftStart;
        }

        private DateTime GetShiftEndTime() => GetShiftStartTime().AddHours(24);

        private List<Invoice> GetCurrentShiftInvoices() =>
            _allInvoices.Where(inv => inv.OrderDate >= GetShiftStartTime() && inv.OrderDate < GetShiftEndTime()).ToList();

        #endregion

        #region متدهای مدیریت داده

        private void LoadAllInvoices()
        {
            var allInvoicesTemp = CafeManager.GetSalesHistory();

            if (!_isShowingAllInvoices)
            {
                DateTime shiftStart = GetShiftStartTime();
                DateTime shiftEnd = GetShiftEndTime();
                _allInvoices = allInvoicesTemp.Where(inv => inv.OrderDate >= shiftStart && inv.OrderDate < shiftEnd).ToList();
            }
            else
            {
                _allInvoices = allInvoicesTemp;
            }
        }

        private Invoice GetInvoiceById(int id) => _allInvoices.FirstOrDefault(i => i.Id == id);

        private List<Invoice> GetFilteredInvoices()
        {
            var query = _allInvoices.AsEnumerable();

            if (!string.IsNullOrEmpty(_txtSearchInvoiceId.Text.Trim()))
            {
                if (int.TryParse(_txtSearchInvoiceId.Text.Trim(), out int invoiceId))
                    query = query.Where(i => i.Id == invoiceId);
                else
                    query = query.Where(i => i.Id.ToString().Contains(_txtSearchInvoiceId.Text));
            }

            if (!string.IsNullOrEmpty(_txtSearchCustomer.Text.Trim()))
                query = query.Where(i => i.CustomerName != null &&
                    i.CustomerName.IndexOf(_txtSearchCustomer.Text.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(_txtSearchTable.Text.Trim()))
                query = query.Where(i => i.TableNumber != null &&
                    i.TableNumber.IndexOf(_txtSearchTable.Text.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);

            return query.ToList();
        }

        #endregion

        #region متدهای بروزرسانی UI

        private void RefreshAll()
        {
            LoadAllInvoices();
            RefreshInvoiceGrid();
            UpdateShiftInfoLabel();
        }

        private void UpdateShiftInfoLabel()
        {
            if (!_isShowingAllInvoices)
            {
                DateTime shiftStart = GetShiftStartTime();
                string shiftDate = ConvertToPersianDate(shiftStart);
                int count = GetCurrentShiftInvoices().Count;
                _lblShiftInfo.Text = $"🕒 شیفت جاری (۱۵ تا ۱۵): {shiftDate} | تعداد: {count} فاکتور";
                _lblShiftInfo.ForeColor = Color.DarkGreen;
            }
            else
            {
                _lblShiftInfo.Text = $"📋 نمایش همه فاکتورها | مجموع: {_allInvoices.Count} فاکتور";
                _lblShiftInfo.ForeColor = Color.DarkBlue;
            }
        }

        private void UpdateTotalAmountLabel(Invoice invoice)
        {
            _lblTotalInvoiceAmount.Text = invoice != null
                ? $"جمع کل فاکتور شماره {invoice.Id}: {invoice.TotalAmount:N0} تومان"
                : "جمع کل فاکتور انتخاب شده: 0 تومان";
        }

        private void SetEditMode(bool enabled)
        {
            _txtEditName.Enabled = enabled;
            _txtEditTable.Enabled = enabled;
            _btnUpdateInvoice.Enabled = enabled;
            _btnDeleteInvoice.Enabled = enabled;
            _btnAddNewItem.Enabled = enabled;
            _btnUpdateItem.Enabled = enabled;
            _btnDeleteItem.Enabled = enabled;
        }

        private void LoadProductsToCombo()
        {
            _cmbAllProducts.Items.Clear();
            var products = CafeManager.GetMenu();
            var sortedProducts = products.OrderBy(p => p.Name).ToList();

            foreach (var p in sortedProducts)
            {
                _cmbAllProducts.Items.Add(p);
            }
            _cmbAllProducts.DisplayMember = "Name";
            _cmbAllProducts.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _cmbAllProducts.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        #endregion

        #region متدهای بروزرسانی Gridها

        private void RefreshInvoiceGrid(int selectInvoiceId = -1)
        {
            _dgvInvoices.Rows.Clear();
            _dgvInvoiceItems.Rows.Clear();

            var sortedHistory = _allInvoices.OrderByDescending(inv => inv.OrderDate).ToList();

            var comboColumn = (DataGridViewComboBoxColumn)_dgvInvoices.Columns["PayMethod"];
            if (!comboColumn.Items.Contains("نقدی")) comboColumn.Items.Add("نقدی");
            if (!comboColumn.Items.Contains("کارت")) comboColumn.Items.Add("کارت");
            if (!comboColumn.Items.Contains("انتقال")) comboColumn.Items.Add("انتقال");
            if (!comboColumn.Items.Contains("آنلاین")) comboColumn.Items.Add("آنلاین");
            if (!comboColumn.Items.Contains("ترکیبی")) comboColumn.Items.Add("ترکیبی");

            foreach (var inv in sortedHistory)
            {
                string persianDate = ConvertToPersianDate(inv.OrderDate);
                string payMethod = string.IsNullOrEmpty(inv.PayMethod) ? "نقدی" : inv.PayMethod;

                if (!comboColumn.Items.Contains(payMethod))
                    comboColumn.Items.Add(payMethod);

                int rowIndex = _dgvInvoices.Rows.Add(
                    inv.Id,
                    inv.CustomerName,
                    inv.TableNumber,
                    persianDate,
                    inv.TotalAmount.ToString("N0"),
                    inv.IsSettled,
                    payMethod);

                if (inv.IsSettled)
                {
                    _dgvInvoices.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                    _dgvInvoices.Rows[rowIndex].Cells["IsSettled"].ReadOnly = true;
                    _dgvInvoices.Rows[rowIndex].Cells["PayMethod"].ReadOnly = true;
                }
                else
                {
                    _dgvInvoices.Rows[rowIndex].DefaultCellStyle.BackColor = Color.White;
                    _dgvInvoices.Rows[rowIndex].Cells["IsSettled"].ReadOnly = false;
                    _dgvInvoices.Rows[rowIndex].Cells["PayMethod"].ReadOnly = false;
                }

                if (inv.OrderDate.Date == DateTime.Now.Date)
                {
                    _dgvInvoices.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkBlue;
                    _dgvInvoices.Rows[rowIndex].DefaultCellStyle.Font = new Font("Tahoma", 10, FontStyle.Bold);
                }

                string timeAgo = GetTimeAgo(inv.OrderDate);
                _dgvInvoices.Rows[rowIndex].Cells["Table"].ToolTipText = $"ثبت: {ConvertToPersianDate(inv.OrderDate)} ({timeAgo})";
            }

            if (!_isShowingAllInvoices)
            {
                DateTime shiftStart = GetShiftStartTime();
                string shiftDate = ConvertToPersianDate(shiftStart);
                this.Text = $"مدیریت فاکتورها - شیفت جاری (۱۵ تا ۱۵) - شروع: {shiftDate}";
            }
            else
            {
                this.Text = "مدیریت فاکتورها - نمایش همه فاکتورها";
            }

            if (_dgvInvoices.Rows.Count > 0)
            {
                int indexToSelect = 0;

                if (!_firstLoad)
                    indexToSelect = _dgvInvoices.CurrentRow?.Index ?? 0;
                else
                    indexToSelect = 0;

                if (selectInvoiceId != -1)
                {
                    for (int i = 0; i < _dgvInvoices.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(_dgvInvoices.Rows[i].Cells["Id"].Value) == selectInvoiceId)
                        {
                            indexToSelect = i;
                            break;
                        }
                    }
                }

                _dgvInvoices.ClearSelection();
                _dgvInvoices.Rows[indexToSelect].Selected = true;
                _dgvInvoices.CurrentCell = _dgvInvoices.Rows[indexToSelect].Cells[0];
                _dgvInvoices.FirstDisplayedScrollingRowIndex = indexToSelect;

                int invoiceId = Convert.ToInt32(_dgvInvoices.Rows[indexToSelect].Cells["Id"].Value);
                _currentlyEditingInvoiceId = invoiceId;

                UpdateEditBoxes();
            }
        }

        private void UpdateEditBoxes()
        {
            if (_dgvInvoices.SelectedRows.Count == 0) return;

            var row = _dgvInvoices.SelectedRows[0];
            _txtEditName.Text = row.Cells["Name"].Value?.ToString() ?? "";
            _txtEditTable.Text = row.Cells["Table"].Value?.ToString() ?? "";

            bool isSettled = Convert.ToBoolean(row.Cells["IsSettled"].Value);
            SetEditMode(!isSettled);

            var invoice = GetInvoiceById(_currentlyEditingInvoiceId);
            _dgvInvoiceItems.Rows.Clear();

            if (invoice != null)
            {
                _currentSelectedInvoice = invoice;
                foreach (var item in invoice.Items)
                {
                    _dgvInvoiceItems.Rows.Add(
                        item.Product.Id,
                        item.Product.Name,
                        item.Product.Price.ToString("N0"),
                        item.Quantity,
                        item.TotalPrice.ToString("N0")
                    );
                }
                UpdateTotalAmountLabel(invoice);
            }
        }

        #endregion

        #region متدهای مدیریت عملیات

        private void PerformSearch()
        {
            string searchInvoiceId = _txtSearchInvoiceId.Text.Trim();
            string searchCustomer = _txtSearchCustomer.Text.Trim();
            string searchTable = _txtSearchTable.Text.Trim();

            if (string.IsNullOrEmpty(searchInvoiceId) && string.IsNullOrEmpty(searchCustomer) && string.IsNullOrEmpty(searchTable))
            {
                RefreshAll();
                this.Text = _isShowingAllInvoices ? "مدیریت فاکتورها - همه فاکتورها" : "مدیریت پیشرفته فاکتورها و رکوردها";
                return;
            }

            var filteredInvoices = GetFilteredInvoices();

            _dgvInvoices.Rows.Clear();

            foreach (var inv in filteredInvoices)
            {
                string persianDate = ConvertToPersianDate(inv.OrderDate);
                int rowIndex = _dgvInvoices.Rows.Add(
                    inv.Id, inv.CustomerName, inv.TableNumber,
                    persianDate,
                    inv.TotalAmount.ToString("N0"),
                    inv.IsSettled, inv.PayMethod);

                if (inv.IsSettled)
                    _dgvInvoices.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGray;
            }

            int resultCount = filteredInvoices.Count();
            if (resultCount == 0)
            {
                _dgvInvoiceItems.Rows.Clear();
                _lblTotalInvoiceAmount.Text = "جمع کل فاکتور انتخاب شده: 0 تومان";
                _txtEditName.Text = "";
                _txtEditTable.Text = "";
                this.Text = "مدیریت پیشرفته فاکتورها - هیچ نتیجه‌ای یافت نشد";
            }
            else
            {
                this.Text = $"مدیریت پیشرفته فاکتورها - {resultCount} نتیجه یافت شد";
            }
        }

        // ============================================================
        // ✅ متد HandleSettlement
        // ============================================================
        private void HandleSettlement(int id, bool settled, string method, DataGridViewRow row)
        {
            if (method == "ترکیبی" && !settled)
            {
                var invoice = GetInvoiceById(id);
                if (invoice == null) return;

                var originalPayments = invoice.Payments?.Select(p => new Payment
                {
                    Method = p.Method,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate
                }).ToList() ?? new List<Payment>();

                using (var dialog = new PaymentDialog(invoice))
                {
                    try
                    {
                        var result = dialog.ShowDialog(this);

                        if (result == DialogResult.OK)
                        {
                            var payments = dialog.Payments;
                            if (payments != null && payments.Count > 0)
                            {
                                invoice.Payments = payments.ToList();
                                CafeManager.UpdateInvoice(invoice);

                                double totalPaid = payments.Sum(p => p.Amount);
                                if (totalPaid >= invoice.TotalAmount)
                                {
                                    CafeManager.UpdateSettlementStatus(id, true, "ترکیبی");
                                    row.Cells["IsSettled"].Value = true;
                                    row.DefaultCellStyle.BackColor = Color.LightGray;

                                    MessageBox.Show($"✅ فاکتور با پرداخت ترکیبی تسویه شد.\nتعداد پرداخت‌ها: {payments.Count}",
                                        "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show($"⚠️ پرداخت‌ها ثبت شدند اما فاکتور کامل تسویه نشد.\nمبلغ باقیمانده: {invoice.TotalAmount - totalPaid:N0} تومان",
                                        "توجه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            else
                            {
                                MessageBox.Show("هیچ پرداختی ثبت نشد.", "توجه",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            invoice.Payments = originalPayments;
                            CafeManager.UpdateInvoice(invoice);

                            if (originalPayments.Count == 0)
                                row.Cells["PayMethod"].Value = "نقدی";
                            else if (originalPayments.Count > 1)
                                row.Cells["PayMethod"].Value = "ترکیبی";
                            else
                                row.Cells["PayMethod"].Value = originalPayments.FirstOrDefault()?.Method ?? "نقدی";

                            RefreshInvoiceGrid(id);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطا در پرداخت: {ex.Message}", "خطا",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                        invoice.Payments = originalPayments;
                        CafeManager.UpdateInvoice(invoice);
                        RefreshInvoiceGrid(id);
                    }
                }
                return;
            }

            try
            {
                CafeManager.UpdateSettlementStatus(id, settled, method);
                row.DefaultCellStyle.BackColor = settled ? Color.LightGray : Color.White;

                if (settled)
                {
                    if (_dgvInvoices.SelectedRows.Count > 0)
                    {
                        _dgvInvoices.Rows[_dgvInvoices.SelectedRows[0].Index].Cells["IsSettled"].ReadOnly = true;
                        _dgvInvoices.Rows[_dgvInvoices.SelectedRows[0].Index].Cells["PayMethod"].ReadOnly = true;
                    }
                    SetEditMode(false);
                    MessageBox.Show("فاکتور با موفقیت تسویه شد و دیگر قابل ویرایش نمی‌باشد.",
                        "تسویه فاکتور", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در تسویه فاکتور: {ex.Message}", "خطا",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region رویدادهای Gridها

        private void DgvInvoices_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgvInvoices.SelectedRows.Count == 0) return;

            int invoiceId = Convert.ToInt32(_dgvInvoices.SelectedRows[0].Cells["Id"].Value);
            _currentlyEditingInvoiceId = invoiceId;
            UpdateEditBoxes();
        }

        private void DgvInvoiceItems_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgvInvoiceItems.SelectedRows.Count == 0)
                return;

            if (!int.TryParse(
                _dgvInvoiceItems.SelectedRows[0].Cells["Qty"].Value?.ToString(),
                out int qty))
                return;

            if (qty < _numItemQty.Minimum)
                qty = (int)_numItemQty.Minimum;

            if (qty > _numItemQty.Maximum)
                qty = (int)_numItemQty.Maximum;

            _numItemQty.Value = qty;
        }

        private void DgvInvoices_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdating) return;
            if (e.RowIndex < 0) return;

            var row = _dgvInvoices.Rows[e.RowIndex];
            int id = Convert.ToInt32(row.Cells["Id"].Value);

            if (_currentlyEditingInvoiceId != id)
            {
                RefreshInvoiceGrid(_currentlyEditingInvoiceId);
                return;
            }

            if (_dgvInvoices.Columns[e.ColumnIndex].Name == "IsSettled" ||
                _dgvInvoices.Columns[e.ColumnIndex].Name == "PayMethod")
            {
                bool settled = Convert.ToBoolean(row.Cells["IsSettled"].Value);
                string method = row.Cells["PayMethod"].Value?.ToString() ?? "نقدی";

                try
                {
                    _isUpdating = true;
                    HandleSettlement(id, settled, method, row);
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        private void DgvInvoices_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_dgvInvoices.CurrentCell != null)
            {
                int colIndex = _dgvInvoices.CurrentCell.ColumnIndex;
                if (colIndex == _dgvInvoices.Columns["IsSettled"].Index ||
                    colIndex == _dgvInvoices.Columns["PayMethod"].Index)
                {
                    _dgvInvoices.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private void DgvInvoices_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int invoiceId = Convert.ToInt32(_dgvInvoices.Rows[e.RowIndex].Cells["Id"].Value);

            if (_currentlyEditingInvoiceId != invoiceId)
            {
                e.Cancel = true;
                MessageBox.Show("برای ویرایش این فاکتور، ابتدا آن را انتخاب کنید.", "تذکر",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DgvInvoices_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int invoiceId = Convert.ToInt32(_dgvInvoices.Rows[e.RowIndex].Cells["Id"].Value);
            _currentlyEditingInvoiceId = invoiceId;

            bool isSettled = Convert.ToBoolean(_dgvInvoices.Rows[e.RowIndex].Cells["IsSettled"].Value);

            _dgvInvoices.Columns["IsSettled"].ReadOnly = isSettled;
            _dgvInvoices.Columns["PayMethod"].ReadOnly = isSettled;

            SetEditMode(!isSettled);

            if (isSettled)
                _dgvInvoices.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
        }

        private void DgvInvoices_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;

            if (e.ColumnIndex == _dgvInvoices.Columns["PayMethod"].Index)
            {
                try
                {
                    var cell = _dgvInvoices.Rows[e.RowIndex].Cells["PayMethod"];
                    string currentValue = cell.Value?.ToString();

                    if (string.IsNullOrEmpty(currentValue))
                        cell.Value = "نقدی";
                    else
                    {
                        var comboColumn = (DataGridViewComboBoxColumn)_dgvInvoices.Columns["PayMethod"];
                        if (!comboColumn.Items.Contains(currentValue))
                        {
                            comboColumn.Items.Add(currentValue);
                            cell.Value = currentValue;
                        }
                    }
                }
                catch
                {
                    _dgvInvoices.Rows[e.RowIndex].Cells["PayMethod"].Value = "نقدی";
                }
            }
        }

        #endregion

        #region رویدادهای جستجو و فیلتر

        private void TxtSearch_TextChanged(object sender, EventArgs e) => PerformSearch();

        private void BtnSearch_Click(object sender, EventArgs e) => PerformSearch();

        private void BtnResetSearch_Click(object sender, EventArgs e)
        {
            _txtSearchInvoiceId.Text = "";
            _txtSearchCustomer.Text = "";
            _txtSearchTable.Text = "";
            RefreshAll();
        }

        private void BtnShowAllInvoices_Click(object sender, EventArgs e)
        {
            _isShowingAllInvoices = !_isShowingAllInvoices;

            _btnShowAllInvoices.Text = _isShowingAllInvoices
                ? "🔄 بازگشت به شیفت جاری"
                : "📋 نمایش همه فاکتورها";

            _btnShowAllInvoices.BackColor = _isShowingAllInvoices
                ? Color.FromArgb(46, 204, 113)
                : Color.FromArgb(52, 152, 219);

            RefreshAll();

            MessageBox.Show(_isShowingAllInvoices
                ? "حالت نمایش به \"همه فاکتورها\" تغییر کرد.\nبرای بازگشت به شیفت جاری، دکمه مربوطه را بزنید."
                : $"بازگشت به شیفت جاری:\nاز {ConvertToPersianDateWithTime(GetShiftStartTime())}\nتا {ConvertToPersianDateWithTime(GetShiftEndTime())}",
                "تغییر حالت نمایش", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region رویدادهای عملیات روی فاکتور

        private void BtnUpdateInvoice_Click(object sender, EventArgs e)
        {
            if (_dgvInvoices.SelectedRows.Count == 0) return;

            bool isSettled = Convert.ToBoolean(_dgvInvoices.SelectedRows[0].Cells["IsSettled"].Value);
            if (isSettled)
            {
                MessageBox.Show("این فاکتور تسویه شده است و قابل ویرایش نمی‌باشد.",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(_dgvInvoices.SelectedRows[0].Cells["Id"].Value);
            CafeManager.UpdateInvoice(id, _txtEditName.Text, _txtEditTable.Text);
            RefreshAll();
            MessageBox.Show("مشخصات فاکتور بروزرسانی شد.", "صندوق کافه", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDeleteInvoice_Click(object sender, EventArgs e)
        {
            if (_dgvInvoices.SelectedRows.Count == 0) return;

            bool isSettled = Convert.ToBoolean(_dgvInvoices.SelectedRows[0].Cells["IsSettled"].Value);
            if (isSettled)
            {
                MessageBox.Show("این فاکتور تسویه شده است و قابل حذف نمی‌باشد.",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("کل فاکتور حذف شود؟ (موجودی اقلام به انبار بازمی‌گردد)", "توجه", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(_dgvInvoices.SelectedRows[0].Cells["Id"].Value);
                CafeManager.DeleteInvoice(id);
                RefreshAll();
            }
        }

        private void BtnPrintInvoice_Click(object sender, EventArgs e)
        {
            if (_dgvInvoices.SelectedRows.Count == 0)
            {
                MessageBox.Show("لطفاً ابتدا یک فاکتور را انتخاب کنید.",
                    "خطا",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            int invoiceId = Convert.ToInt32(_dgvInvoices.SelectedRows[0].Cells["Id"].Value);
            _currentSelectedInvoice = GetInvoiceById(invoiceId);

            if (_currentSelectedInvoice == null)
                return;

            PrintDocument printDoc = new PrintDocument();
            PaperSize paperSize = new PaperSize("Receipt80mm", 315, 1200);
            printDoc.DefaultPageSettings.PaperSize = paperSize;
            printDoc.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);
            printDoc.PrintPage += PrintDoc_PrintPage;

            PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = printDoc,
                WindowState = FormWindowState.Maximized
            };

            preview.ShowDialog();
        }

        #endregion

        #region رویدادهای عملیات روی آیتم‌ها

        private void BtnAddNewItem_Click(object sender, EventArgs e)
        {
            if (_dgvInvoices.SelectedRows.Count == 0 || _cmbAllProducts.SelectedItem == null)
            {
                MessageBox.Show("لطفاً فاکتور و محصول را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int invoiceId = Convert.ToInt32(_dgvInvoices.SelectedRows[0].Cells["Id"].Value);

            bool isSettled = Convert.ToBoolean(_dgvInvoices.SelectedRows[0].Cells["IsSettled"].Value);
            if (isSettled)
            {
                MessageBox.Show("این فاکتور تسویه شده است و نمی‌توان به آن آیتم اضافه کرد.",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Product selectedProduct = (Product)_cmbAllProducts.SelectedItem;
            int countToAdd = (int)_numNewItemQty.Value;

            var invoice = GetInvoiceById(invoiceId);
            if (invoice != null)
            {
                var existingItem = invoice.Items.Find(item => item.Product.Id == selectedProduct.Id);

                if (existingItem != null)
                {
                    CafeManager.UpdateItemQuantity(invoiceId, selectedProduct.Id, existingItem.Quantity + countToAdd);
                }
                else
                {
                    if (selectedProduct.Stock < countToAdd)
                    {
                        MessageBox.Show($"❌ کسری انبار! موجودی کالا فقط {selectedProduct.Stock} عدد است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    selectedProduct.Stock -= countToAdd;
                    invoice.Items.Add(new OrderItem { Product = selectedProduct, Quantity = countToAdd });
                    CafeManager.UpdateInvoice(invoice.Id, invoice.CustomerName, invoice.TableNumber);
                }

                RefreshAll();
                _numNewItemQty.Value = 1;
                MessageBox.Show($"محصول '{selectedProduct.Name}' به فاکتور اضافه شد.", "عملیات موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnUpdateItem_Click(object sender, EventArgs e)
        {
            if (_dgvInvoices.SelectedRows.Count == 0 || _dgvInvoiceItems.SelectedRows.Count == 0) return;

            bool isSettled = Convert.ToBoolean(_dgvInvoices.SelectedRows[0].Cells["IsSettled"].Value);
            if (isSettled)
            {
                MessageBox.Show("این فاکتور تسویه شده است و نمی‌توان آیتم آن را ویرایش کرد.",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int invoiceId = Convert.ToInt32(_dgvInvoices.SelectedRows[0].Cells["Id"].Value);
            int productId = Convert.ToInt32(_dgvInvoiceItems.SelectedRows[0].Cells["ProdId"].Value);
            int newQty = (int)_numItemQty.Value;
            CafeManager.UpdateItemQuantity(invoiceId, productId, newQty);
            RefreshAll();
            MessageBox.Show("تعداد رکورد تغییر کرد، موجودی انبار همگام شد و جمع کل فاکتور اصلاح گردید.", "ویرایش آیتم", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDeleteItem_Click(object sender, EventArgs e)
        {
            if (_dgvInvoices.SelectedRows.Count == 0 || _dgvInvoiceItems.SelectedRows.Count == 0) return;

            bool isSettled = Convert.ToBoolean(_dgvInvoices.SelectedRows[0].Cells["IsSettled"].Value);
            if (isSettled)
            {
                MessageBox.Show("این فاکتور تسویه شده است و نمی‌توان آیتم آن را حذف کرد.",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("این آیتم از فاکتور حذف شود؟ (موجودی آن به انبار برگشت داده می‌شود)", "حذف رکورد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int invoiceId = Convert.ToInt32(_dgvInvoices.SelectedRows[0].Cells["Id"].Value);
                int productId = Convert.ToInt32(_dgvInvoiceItems.SelectedRows[0].Cells["ProdId"].Value);
                CafeManager.DeleteItemFromInvoice(invoiceId, productId);
                RefreshAll();
            }
        }

        #endregion

        #region رویدادهای چاپ


        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_currentSelectedInvoice == null) return;

            Graphics g = e.Graphics;

            // تنظیمات کیفیت بالا
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            // فونت‌ها
            Font titleFont = new Font("Tahoma", 13, FontStyle.Bold);
            Font tableFont = new Font("Tahoma", 7, FontStyle.Bold);
            Font totalFont = new Font("Tahoma", 8, FontStyle.Bold);
            Font footerFont = new Font("Tahoma", 8);

            float pageWidth = 320f;
            float y = 15f;
            float padding = 6f;

            // فرمت‌های RTL
            StringFormat centerRTL = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            StringFormat rightRTL = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            // ============================================================
            // هدر
            // ============================================================
            g.DrawString("☕ کافه گلستان", titleFont, Brushes.Black, pageWidth / 2, y, centerRTL);
            y += 28;

            string headerLine1 = $"فاکتور شماره: {_currentSelectedInvoice.Id}     تاریخ: {ConvertToPersianDate(_currentSelectedInvoice.OrderDate)}";
            g.DrawString(headerLine1, tableFont, Brushes.Black, pageWidth / 2, y, centerRTL);
            y += 18;

            string headerLine2 = $"مشتری: {_currentSelectedInvoice.CustomerName}     میز: {_currentSelectedInvoice.TableNumber}";
            g.DrawString(headerLine2, tableFont, Brushes.Black, pageWidth / 2, y, centerRTL);
            y += 24;

            // ============================================================
            // اندازه‌گیری طول متن برای هر ستون
            // ============================================================
            float maxMablagh = g.MeasureString("مبلغ", tableFont).Width;
            float maxTedad = g.MeasureString("تعداد", tableFont).Width;
            float maxGheymat = g.MeasureString("قیمت", tableFont).Width;
            float maxName = g.MeasureString("نام کالا", tableFont).Width;
            float maxRadif = g.MeasureString("ردیف", tableFont).Width;

            int rowIndex = 1;
            foreach (var item in _currentSelectedInvoice.Items)
            {
                maxMablagh = Math.Max(maxMablagh, g.MeasureString(item.TotalPrice.ToString("N0"), tableFont).Width);
                maxTedad = Math.Max(maxTedad, g.MeasureString(item.Quantity.ToString(), tableFont).Width);
                maxGheymat = Math.Max(maxGheymat, g.MeasureString(item.Product.Price.ToString("N0"), tableFont).Width);
                maxName = Math.Max(maxName, g.MeasureString(item.Product.Name, tableFont).Width);
                maxRadif = Math.Max(maxRadif, g.MeasureString(rowIndex.ToString(), tableFont).Width);
                rowIndex++;
            }

            // ============================================================
            // تنظیم عرض ستون‌ها
            // ============================================================
            float colMablagh = maxMablagh + padding;
            float colTedad = maxTedad + padding;
            float colGheymat = maxGheymat + padding;
            float colName = maxName + padding;
            float colRadif = maxRadif + padding;

            float tableWidth = colMablagh + colTedad + colGheymat + colName + colRadif;

            if (tableWidth > pageWidth - 20)
            {
                float scale = (pageWidth - 20) / tableWidth;
                colMablagh *= scale;
                colTedad *= scale;
                colGheymat *= scale;
                colName *= scale;
                colRadif *= scale;
                tableWidth = colMablagh + colTedad + colGheymat + colName + colRadif;
            }

            float tableLeft = (pageWidth - tableWidth) / 2;

            float xMablagh = tableLeft;
            float xTedad = xMablagh + colMablagh;
            float xGheymat = xTedad + colTedad;
            float xName = xGheymat + colGheymat;
            float xRadif = xName + colName;

            float rowHeight = 20f;

            // ============================================================
            // هدر جدول
            // ============================================================
            g.FillRectangle(new SolidBrush(Color.LightGray), tableLeft, y, tableWidth, rowHeight);
            g.DrawRectangle(new Pen(Color.Black, 1.5f), tableLeft, y, tableWidth, rowHeight);

            g.DrawLine(new Pen(Color.Black, 1.5f), xMablagh, y, xMablagh, y + rowHeight);
            g.DrawLine(new Pen(Color.Black, 1.5f), xTedad, y, xTedad, y + rowHeight);
            g.DrawLine(new Pen(Color.Black, 1.5f), xGheymat, y, xGheymat, y + rowHeight);
            g.DrawLine(new Pen(Color.Black, 1.5f), xName, y, xName, y + rowHeight);
            g.DrawLine(new Pen(Color.Black, 1.5f), xRadif, y, xRadif, y + rowHeight);

            g.DrawString("مبلغ", tableFont, Brushes.Black, new RectangleF(xMablagh + 2, y, colMablagh - 4, rowHeight), centerRTL);
            g.DrawString("تعداد", tableFont, Brushes.Black, new RectangleF(xTedad + 2, y, colTedad - 4, rowHeight), centerRTL);
            g.DrawString("قیمت", tableFont, Brushes.Black, new RectangleF(xGheymat + 2, y, colGheymat - 4, rowHeight), centerRTL);
            g.DrawString("نام کالا", tableFont, Brushes.Black, new RectangleF(xName + 2, y, colName - 4, rowHeight), centerRTL);
            g.DrawString("ردیف", tableFont, Brushes.Black, new RectangleF(xRadif + 2, y, colRadif - 4, rowHeight), centerRTL);

            y += rowHeight;

            // ============================================================
            // آیتم‌های جدول
            // ============================================================
            int i = 1;
            bool alt = false;
            foreach (var item in _currentSelectedInvoice.Items)
            {
                if (alt) g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), tableLeft, y, tableWidth, rowHeight);

                g.DrawRectangle(new Pen(Color.Black, 0.8f), tableLeft, y, tableWidth, rowHeight);

                g.DrawLine(new Pen(Color.Black, 0.8f), xMablagh, y, xMablagh, y + rowHeight);
                g.DrawLine(new Pen(Color.Black, 0.8f), xTedad, y, xTedad, y + rowHeight);
                g.DrawLine(new Pen(Color.Black, 0.8f), xGheymat, y, xGheymat, y + rowHeight);
                g.DrawLine(new Pen(Color.Black, 0.8f), xName, y, xName, y + rowHeight);
                g.DrawLine(new Pen(Color.Black, 0.8f), xRadif, y, xRadif, y + rowHeight);

                // مبلغ
                string totalText = item.TotalPrice.ToString("N0");
                Font priceFont = tableFont;
                SizeF sz = g.MeasureString(totalText, tableFont);
                if (sz.Width > colMablagh - 8)
                {
                    float ns = tableFont.Size * ((colMablagh - 8) / sz.Width);
                    if (ns < 5) ns = 5;
                    priceFont = new Font(tableFont.FontFamily, ns, tableFont.Style);
                }
                g.DrawString(totalText, priceFont, Brushes.Black, new RectangleF(xMablagh + 2, y, colMablagh - 4, rowHeight), rightRTL);

                g.DrawString(item.Quantity.ToString(), tableFont, Brushes.Black, new RectangleF(xTedad + 2, y, colTedad - 4, rowHeight), rightRTL);
                g.DrawString(item.Product.Price.ToString("N0"), tableFont, Brushes.Black, new RectangleF(xGheymat + 2, y, colGheymat - 4, rowHeight), rightRTL);
                g.DrawString(item.Product.Name, tableFont, Brushes.Black, new RectangleF(xName + 2, y, colName - 4, rowHeight), centerRTL);
                g.DrawString(i.ToString(), tableFont, Brushes.Black, new RectangleF(xRadif + 2, y, colRadif - 4, rowHeight), centerRTL);

                y += rowHeight;
                i++;
                alt = !alt;
            }

            // ============================================================
            // سطر آخر - یک سلول کامل (جمع کل)
            // ============================================================
            string totalAmount = _currentSelectedInvoice.TotalAmount.ToString("N0");
            string totalDisplay = $"جمع کل: {totalAmount} تومان";

            g.FillRectangle(new SolidBrush(Color.FromArgb(230, 240, 255)), tableLeft, y, tableWidth, rowHeight);
            g.DrawRectangle(new Pen(Color.Black, 1.5f), tableLeft, y, tableWidth, rowHeight);

            g.DrawString(totalDisplay, totalFont, Brushes.DarkBlue, new RectangleF(tableLeft + 2, y, tableWidth - 4, rowHeight), centerRTL);

            y += rowHeight;

            // ============================================================
            // ✅ افزایش فاصله بین جدول و فوتر
            // ============================================================
            y += 20;  // قبلاً 8 بود، الان 20

            g.DrawString("با تشکر از خرید شما", new Font("Tahoma", 10, FontStyle.Bold), Brushes.Black, pageWidth / 2, y, centerRTL);
            y += 18;

            g.DrawString("☕ کافه گلستان - بهترین انتخاب شما", footerFont, Brushes.Black, pageWidth / 2, y, centerRTL);

            e.HasMorePages = false;
        }

        #endregion

        #region متدهای تنظیم UI (کنترل‌ها)

        private void InitializeComponents()
        {
            this.SuspendLayout();

            int margin = 20;

            // ۱. بخش جستجو
            _lblSearchTitle = new Label
            {
                Text = "جستجوی فاکتورها:",
                Location = new Point(margin, 15),
                Size = new Size(110, 25),
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            Label lblSearchInvoiceId = new Label
            {
                Text = "شماره فاکتور:",
                Location = new Point(margin + 120, 18),
                Size = new Size(100, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _txtSearchInvoiceId = new TextBox
            {
                Location = new Point(margin + 225, 15),
                Size = new Size(100, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Tahoma", 10)
            };
            _txtSearchInvoiceId.TextChanged += TxtSearch_TextChanged;

            Label lblSearchCustomer = new Label
            {
                Text = "نام مشتری:",
                Location = new Point(margin + 340, 18),
                Size = new Size(80, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _txtSearchCustomer = new TextBox
            {
                Location = new Point(margin + 425, 15),
                Size = new Size(130, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Tahoma", 10)
            };
            _txtSearchCustomer.TextChanged += TxtSearch_TextChanged;

            Label lblSearchTable = new Label
            {
                Text = "شماره میز:",
                Location = new Point(margin + 570, 18),
                Size = new Size(80, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _txtSearchTable = new TextBox
            {
                Location = new Point(margin + 655, 15),
                Size = new Size(100, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Tahoma", 10)
            };
            _txtSearchTable.TextChanged += TxtSearch_TextChanged;

            _btnSearch = new Button
            {
                Text = "🔍 جستجو",
                Location = new Point(margin + 770, 13),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnSearch.Click += BtnSearch_Click;

            _btnResetSearch = new Button
            {
                Text = "🔄 نمایش همه",
                Location = new Point(margin + 870, 13),
                Size = new Size(100, 30),
                BackColor = Color.LightGray,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnResetSearch.Click += BtnResetSearch_Click;

            _btnShowAllInvoices = new Button
            {
                Text = "📋 نمایش همه فاکتورها",
                Location = new Point(margin, 55),
                Size = new Size(160, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnShowAllInvoices.Click += BtnShowAllInvoices_Click;

            _lblShiftInfo = new Label
            {
                Location = new Point(this.ClientSize.Width - 350, 58),
                Size = new Size(330, 25),
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // ۲. عنوان و جدول بالایی: لیست فاکتورها
            _lblInvTitle = new Label
            {
                Text = "۱. لیست فاکتورهای صادر شده:",
                Location = new Point(margin, 95),
                Size = new Size(300, 25),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _dgvInvoices = new DataGridView
            {
                Location = new Point(margin, 125),
                Size = new Size(this.ClientSize.Width - (margin * 2), 170),
                AllowUserToAddRows = false,
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EditMode = DataGridViewEditMode.EditOnKeystroke,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                GridColor = Color.Gainsboro,
                ColumnHeadersHeight = 30,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _dgvInvoices.Columns.Add("Id", "شماره فاکتور");
            _dgvInvoices.Columns.Add("Name", "نام مشتری");
            _dgvInvoices.Columns.Add("Table", "شماره میز");
            _dgvInvoices.Columns.Add("Date", "تاریخ فاکتور");
            _dgvInvoices.Columns.Add("Total", "مبلغ کل فاکتور (تومان)");

            DataGridViewCheckBoxColumn colSettled = new DataGridViewCheckBoxColumn
            {
                Name = "IsSettled",
                HeaderText = "تسویه شده",
                Width = 90,
                TrueValue = true,
                FalseValue = false
            };
            _dgvInvoices.Columns.Add(colSettled);

            DataGridViewComboBoxColumn colPayMethod = new DataGridViewComboBoxColumn
            {
                Name = "PayMethod",
                HeaderText = "روش پرداخت",
                Width = 130
            };
            colPayMethod.Items.AddRange("نقدی", "کارت", "انتقال", "آنلاین", "ترکیبی");
            _dgvInvoices.Columns.Add(colPayMethod);

            _dgvInvoices.Columns["Id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            _dgvInvoices.SelectionChanged += DgvInvoices_SelectionChanged;
            _dgvInvoices.CellValueChanged += DgvInvoices_CellValueChanged;
            _dgvInvoices.CurrentCellDirtyStateChanged += DgvInvoices_CurrentCellDirtyStateChanged;
            _dgvInvoices.CellBeginEdit += DgvInvoices_CellBeginEdit;
            _dgvInvoices.RowEnter += DgvInvoices_RowEnter;
            _dgvInvoices.DataError += DgvInvoices_DataError;

            // ۳. باکس ویرایش اطلاعات کلی فاکتور
            _grpInvoiceEdit = new GroupBox
            {
                Text = "عملیات روی کل فاکتور",
                Location = new Point(margin, 125 + 170 + 10),
                Size = new Size(this.ClientSize.Width - (margin * 2), 85),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _grpInvoiceEdit.RightToLeft = RightToLeft.Yes;

            _btnPrintInvoice = new Button
            {
                Text = "🖨️ چاپ فاکتور",
                Location = new Point(_grpInvoiceEdit.Width - 150, 23),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Tahoma", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnPrintInvoice.Click += BtnPrintInvoice_Click;

            _grpInvoiceEdit.Controls.Add(new Label { Text = "شماره میز:", Location = new Point(_grpInvoiceEdit.Width - 290, 33), Size = new Size(80, 20) });
            _txtEditTable = new TextBox { Location = new Point(_grpInvoiceEdit.Width - 350, 30), Size = new Size(50, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            _grpInvoiceEdit.Controls.Add(new Label { Text = "نام مشتری:", Location = new Point(_grpInvoiceEdit.Width - 480, 33), Size = new Size(80, 20) });
            _txtEditName = new TextBox { Location = new Point(_grpInvoiceEdit.Width - 620, 30), Size = new Size(130, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            _btnDeleteInvoice = new Button
            {
                Text = "حذف کل فاکتور ❌",
                Location = new Point(160, 25),
                Size = new Size(130, 38),
                BackColor = Color.MistyRose,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnDeleteInvoice.Click += BtnDeleteInvoice_Click;

            _btnUpdateInvoice = new Button
            {
                Text = "ویرایش نام/میز 💾",
                Location = new Point(20, 25),
                Size = new Size(130, 38),
                BackColor = Color.LightBlue,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnUpdateInvoice.Click += BtnUpdateInvoice_Click;

            _grpInvoiceEdit.Controls.AddRange(new Control[]
            {
                _txtEditName, _txtEditTable, _btnUpdateInvoice,
                _btnDeleteInvoice, _btnPrintInvoice
            });

            // ۴. عنوان و جدول پایینی: اقلام فاکتور
            _lblItemsTitle = new Label
            {
                Text = "۲. اقلام و رکوردهای فاکتور انتخاب شده:",
                Location = new Point(margin, 125 + 170 + 85 + 20),
                Size = new Size(350, 25),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _dgvInvoiceItems = new DataGridView
            {
                Location = new Point(margin, 125 + 170 + 85 + 25 + 10),
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
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _dgvInvoiceItems.Columns.Add("ProdId", "کد کالا");
            _dgvInvoiceItems.Columns.Add("ProdName", "نام محصول");
            _dgvInvoiceItems.Columns.Add("Price", "قیمت واحد");
            _dgvInvoiceItems.Columns.Add("Qty", "تعداد سفارش");
            _dgvInvoiceItems.Columns.Add("Total", "قیمت کل رکورد");
            _dgvInvoiceItems.SelectionChanged += DgvInvoiceItems_SelectionChanged;

            // ۵. جمع کل
            _lblTotalInvoiceAmount = new Label
            {
                Text = "جمع کل فاکتور انتخاب شده: 0 تومان",
                Location = new Point(margin, 0),
                Size = new Size(500, 35),
                Font = new Font("Tahoma", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            // ۶. عملیات روی اقلام
            _grpItemEdit = new GroupBox
            {
                Text = "عملیات روی رکوردهای (اقلام) موجود",
                Location = new Point(margin, 0),
                Size = new Size(this.ClientSize.Width - (margin * 2), 80),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _numItemQty = new NumericUpDown { Location = new Point(520, 28), Size = new Size(100, 25), Minimum = 1 };

            _grpItemEdit.Controls.Add(new Label { Text = "تعداد جدید رکورد:", Location = new Point(630, 31), Size = new Size(110, 20) });
            _grpItemEdit.Controls.Add(_numItemQty);

            _btnUpdateItem = new Button { Text = "تغییر تعداد رکورد 🔄", Location = new Point(160, 22), Size = new Size(140, 38), BackColor = Color.LightGreen };
            _btnUpdateItem.Click += BtnUpdateItem_Click;

            _btnDeleteItem = new Button { Text = "حذف این رکورد 🗑️", Location = new Point(20, 22), Size = new Size(130, 38), BackColor = Color.LightCoral };
            _btnDeleteItem.Click += BtnDeleteItem_Click;

            _grpItemEdit.Controls.AddRange(new Control[] { _btnUpdateItem, _btnDeleteItem });

            // ۷. افزودن آیتم جدید
            _grpAddNewItem = new GroupBox
            {
                Text = "➕ افزودن کالا (آیتم جدید) به این فاکتور",
                Location = new Point(margin, 0),
                Size = new Size(this.ClientSize.Width - (margin * 2), 80),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _cmbAllProducts = new ComboBox
            {
                Location = new Point(470, 28),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            _numNewItemQty = new NumericUpDown { Location = new Point(340, 28), Size = new Size(70, 25), Minimum = 1, Value = 1 };
            _btnAddNewItem = new Button { Text = "اضافه کردن به فاکتور ➕", Location = new Point(20, 22), Size = new Size(180, 38), BackColor = Color.Khaki };
            _btnAddNewItem.Click += BtnAddNewItem_Click;

            _grpAddNewItem.Controls.Add(new Label { Text = "انتخاب محصول:", Location = new Point(680, 31), Size = new Size(95, 20) });
            _grpAddNewItem.Controls.Add(_cmbAllProducts);
            _grpAddNewItem.Controls.Add(new Label { Text = "تعداد:", Location = new Point(420, 31), Size = new Size(50, 20) });
            _grpAddNewItem.Controls.Add(_numNewItemQty);
            _grpAddNewItem.Controls.Add(_btnAddNewItem);

            this.Controls.AddRange(new Control[]
            {
                _lblSearchTitle, lblSearchInvoiceId, _txtSearchInvoiceId,
                lblSearchCustomer, _txtSearchCustomer,
                lblSearchTable, _txtSearchTable,
                _btnSearch, _btnResetSearch,
                _btnShowAllInvoices, _lblShiftInfo,
                _lblInvTitle, _dgvInvoices, _grpInvoiceEdit, _lblItemsTitle,
                _dgvInvoiceItems, _lblTotalInvoiceAmount, _grpItemEdit,
                _grpAddNewItem
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void AdjustControlsSizeAndPosition()
        {
            if (this.ClientSize.Width < 800 || this.ClientSize.Height < 600) return;

            int margin = 20;
            int usableWidth = this.ClientSize.Width - (margin * 2);

            _dgvInvoices.Width = usableWidth;
            _grpInvoiceEdit.Width = usableWidth;
            _dgvInvoiceItems.Width = usableWidth;
            _grpItemEdit.Width = usableWidth;
            _grpAddNewItem.Width = usableWidth;

            _lblShiftInfo.Location = new Point(this.ClientSize.Width - 350, 58);

            if (_grpInvoiceEdit != null)
            {
                _btnPrintInvoice.Location = new Point(_grpInvoiceEdit.Width - 150, 23);

                foreach (Control ctrl in _grpInvoiceEdit.Controls)
                {
                    if (ctrl is Label lbl && lbl.Text == "شماره میز:")
                    {
                        lbl.Location = new Point(_grpInvoiceEdit.Width - 290, 33);
                    }
                    if (ctrl == _txtEditTable)
                    {
                        _txtEditTable.Location = new Point(_grpInvoiceEdit.Width - 350, 30);
                    }
                    if (ctrl is Label lbl2 && lbl2.Text == "نام مشتری:")
                    {
                        lbl2.Location = new Point(_grpInvoiceEdit.Width - 480, 33);
                    }
                    if (ctrl == _txtEditName)
                    {
                        _txtEditName.Location = new Point(_grpInvoiceEdit.Width - 620, 30);
                    }
                }
            }

            int bottomY = this.ClientSize.Height - margin;
            _grpAddNewItem.Location = new Point(margin, bottomY - 80);
            _grpItemEdit.Location = new Point(margin, _grpAddNewItem.Top - 10 - 80);
            _lblTotalInvoiceAmount.Location = new Point(margin, _grpItemEdit.Top - 10 - 35);

            int itemsGridTop = _lblItemsTitle.Bottom + 10;
            int itemsGridHeight = _lblTotalInvoiceAmount.Top - itemsGridTop - 10;

            const int reductionPixels = 75;
            itemsGridHeight = itemsGridHeight - reductionPixels;

            if (itemsGridHeight < 80) itemsGridHeight = 80;
            if (itemsGridHeight > 180) itemsGridHeight = 180;

            _dgvInvoiceItems.Location = new Point(margin, itemsGridTop);
            _dgvInvoiceItems.Height = itemsGridHeight;
        }

        #endregion
    }
}