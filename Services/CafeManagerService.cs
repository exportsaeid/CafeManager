using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeManager.Data;
using CafeManager.Models;

namespace CafeManager.Services
{
    public class CafeManagerService
    {
        private readonly DataContext _context;
        private readonly ProductRepository _productRepo;
        private readonly InvoiceRepository _invoiceRepo;
        private readonly EmployeeRepository _employeeRepo;
        private readonly AttendanceRepository _attendanceRepo;
        private readonly PayrollRepository _payrollRepo;
        private readonly UserRepository _userRepo;

        public UserSession CurrentUser { get; set; } = new UserSession();
        public List<GameSession> CompletedGames { get; set; } = new List<GameSession>();
        public Dictionary<string, GameSession> ActiveGames { get; set; } = new Dictionary<string, GameSession>();

        // ==================== Private Lists ====================
        private List<MiscellaneousExpense> _miscExpenses = new List<MiscellaneousExpense>();
        private List<Purchase> _purchases = new List<Purchase>();
        private List<Stocktake> _stocktakes = new List<Stocktake>();

        public CafeManagerService()
        {
            _context = new DataContext();

            _productRepo = new ProductRepository(_context.MenuPath);
            _invoiceRepo = new InvoiceRepository(_context.InvoicePath, _productRepo);
            _employeeRepo = new EmployeeRepository(_context.EmployeesPath);
            _attendanceRepo = new AttendanceRepository(_context.AttendancesPath);
            _payrollRepo = new PayrollRepository(_context.PayrollsPath);
            _userRepo = new UserRepository(_context.UsersPath);

            LoadGamesHistory();
            LoadPurchases();
            LoadMiscExpenses();
            LoadStocktakes();

            EnsureDefaultUsers();
        }

        // ==================== Authentication & User Management ====================
        public bool Login(string username, string password)
        {
            var hashedPassword = SecurityHelper.HashPassword(password);
            var user = _userRepo.GetByUsername(username);

            if (user != null && user.Password == hashedPassword && user.IsActive)
            {
                CurrentUser = new UserSession
                {
                    UserId = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role,
                    LoginTime = DateTime.Now
                };

                user.LastLogin = DateTime.Now;
                _userRepo.Update(user);

                return true;
            }

            CurrentUser = new UserSession();
            return false;
        }

        public void Logout()
        {
            CurrentUser = new UserSession();
        }

        public bool IsAuthenticated() => CurrentUser.IsAuthenticated;
        public bool IsAdmin() => CurrentUser.IsAdmin;
        public bool IsCashier() => CurrentUser.IsCashier;

        public List<User> GetUsers(bool activeOnly = true)
        {
            return activeOnly ? _userRepo.GetAll().Where(u => u.IsActive).ToList()
                              : _userRepo.GetAll();
        }

        public User? GetUserById(int id) => _userRepo.GetById(id);
        public User? GetUserByUsername(string username) => _userRepo.GetByUsername(username);

        public void AddUser(User user)
        {
            user.Password = SecurityHelper.HashPassword(user.Password);
            _userRepo.Add(user);
        }

        public void UpdateUser(User user)
        {
            var existing = _userRepo.GetById(user.Id);
            if (existing != null)
            {
                existing.Username = user.Username;
                existing.FullName = user.FullName;
                existing.Role = user.Role;
                existing.IsActive = user.IsActive;

                if (!string.IsNullOrEmpty(user.Password) && user.Password != existing.Password)
                {
                    existing.Password = SecurityHelper.HashPassword(user.Password);
                }

                _userRepo.Update(existing);
            }
        }

        public void DeleteUser(int id) => _userRepo.Delete(id);
        public void HardDeleteUser(int id) => _userRepo.HardDelete(id);

        public void ChangeUserRole(int userId, string newRole)
        {
            var user = _userRepo.GetById(userId);
            if (user != null)
            {
                user.Role = newRole;
                _userRepo.Update(user);
            }
        }

        public void ChangeUserPassword(int userId, string newPassword)
        {
            var user = _userRepo.GetById(userId);
            if (user != null)
            {
                user.Password = SecurityHelper.HashPassword(newPassword);
                _userRepo.Update(user);
            }
        }

        public bool HasAccess(string requiredRole)
        {
            if (!IsAuthenticated()) return false;
            if (IsAdmin()) return true;
            return CurrentUser.Role == requiredRole;
        }

        private void EnsureDefaultUsers()
        {
            if (!_userRepo.GetAll().Any())
            {
                _userRepo.Add(new User
                {
                    Username = "admin",
                    Password = SecurityHelper.HashPassword("123"),
                    FullName = "مدیر سیستم",
                    Role = "Admin",
                    IsActive = true
                });

                _userRepo.Add(new User
                {
                    Username = "cashier1",
                    Password = SecurityHelper.HashPassword("123"),
                    FullName = "صندوق‌دار اول",
                    Role = "Cashier",
                    IsActive = true
                });

                _userRepo.Add(new User
                {
                    Username = "cashier2",
                    Password = SecurityHelper.HashPassword("123"),
                    FullName = "صندوق‌دار دوم",
                    Role = "Cashier",
                    IsActive = true
                });
            }
        }

        // ==================== Products ====================
        public List<Product> GetProducts() => _productRepo.GetAll();
        public Product? GetProduct(int id) => _productRepo.GetById(id);

        public void AddProduct(Product product)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند محصول اضافه کند.");
            _productRepo.Add(product);
        }

        public void UpdateProduct(Product product)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند محصول را ویرایش کند.");
            _productRepo.Update(product);
        }

        public void DeleteProduct(int id)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند محصول را حذف کند.");
            _productRepo.Delete(id);
        }

        public List<Product> GetLowStockAlerts() => _productRepo.GetLowStock();

        public void UpdateStock(int id, int newStock, int threshold)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند موجودی را ویرایش کند.");

            var product = _productRepo.GetById(id);
            if (product != null)
            {
                product.Stock = newStock;
                product.LowStockThreshold = threshold;
                _productRepo.Update(product);
            }
        }

        // ==================== Invoices ====================
        public List<Invoice> GetInvoices() => _invoiceRepo.GetAll();
        public Invoice? GetInvoice(int id) => _invoiceRepo.GetById(id);

        public void SaveInvoice(Invoice invoice)
        {
            if (!HasAccess("Cashier") && !HasAccess("Admin"))
                throw new UnauthorizedAccessException("شما مجوز ثبت فاکتور را ندارید.");

            invoice.OrderDate = DateTime.Now;

            foreach (var item in invoice.Items)
            {
                var product = _productRepo.GetById(item.Product.Id);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    _productRepo.Update(product);
                }
            }

            _invoiceRepo.Add(invoice);
        }

        public void UpdateInvoice(Invoice invoice)
        {
            if (!HasAccess("Cashier") && !HasAccess("Admin"))
                throw new UnauthorizedAccessException("شما مجوز ویرایش فاکتور را ندارید.");
            _invoiceRepo.Update(invoice);
        }

        public void DeleteInvoice(int id)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند فاکتور را حذف کند.");

            var invoice = _invoiceRepo.GetById(id);
            if (invoice != null)
            {
                foreach (var item in invoice.Items)
                {
                    var product = _productRepo.GetById(item.Product.Id);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        _productRepo.Update(product);
                    }
                }
                _invoiceRepo.Delete(id);
            }
        }

        public List<Invoice> GetInvoicesByDate(DateTime from, DateTime to)
            => _invoiceRepo.GetByDateRange(from, to);

        public double GetTotalSales() => _invoiceRepo.GetTotalSales();

        public void UpdateItemQuantity(int invoiceId, int productId, int newQuantity)
        {
            if (!HasAccess("Cashier") && !HasAccess("Admin"))
                throw new UnauthorizedAccessException("شما مجوز ویرایش آیتم فاکتور را ندارید.");

            var invoice = _invoiceRepo.GetById(invoiceId);
            if (invoice != null)
            {
                var item = invoice.Items.FirstOrDefault(i => i.Product.Id == productId);
                if (item != null)
                {
                    var product = _productRepo.GetById(productId);
                    if (product != null)
                    {
                        int diff = newQuantity - item.Quantity;
                        product.Stock -= diff;
                        _productRepo.Update(product);
                    }
                    item.Quantity = newQuantity;
                    _invoiceRepo.Update(invoice);
                }
            }
        }

        public void DeleteItemFromInvoice(int invoiceId, int productId)
        {
            if (!HasAccess("Cashier") && !HasAccess("Admin"))
                throw new UnauthorizedAccessException("شما مجوز حذف آیتم از فاکتور را ندارید.");

            var invoice = _invoiceRepo.GetById(invoiceId);
            if (invoice != null)
            {
                var item = invoice.Items.FirstOrDefault(i => i.Product.Id == productId);
                if (item != null)
                {
                    var product = _productRepo.GetById(productId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        _productRepo.Update(product);
                    }
                    invoice.Items.Remove(item);
                    _invoiceRepo.Update(invoice);
                }
            }
        }

        public void UpdateSettlementStatus(int invoiceId, bool isSettled, string payMethod)
        {
            if (!HasAccess("Cashier") && !HasAccess("Admin"))
                throw new UnauthorizedAccessException("شما مجوز تغییر وضعیت تسویه را ندارید.");

            var invoice = _invoiceRepo.GetById(invoiceId);
            if (invoice != null)
            {
                invoice.IsSettled = isSettled;
                invoice.PayMethod = payMethod;
                _invoiceRepo.Update(invoice);
            }
        }

        // ==================== Employees ====================
        public List<Employee> GetEmployees(bool activeOnly = true)
        {
            return activeOnly ? _employeeRepo.GetActive() : _employeeRepo.GetAll();
        }
        public Employee? GetEmployee(int id) => _employeeRepo.GetById(id);

        public void AddEmployee(Employee employee)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند کارمند اضافه کند.");
            _employeeRepo.Add(employee);
        }

        public void UpdateEmployee(Employee employee)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند کارمند را ویرایش کند.");
            _employeeRepo.Update(employee);
        }

        public void DeleteEmployee(int id)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند کارمند را حذف کند.");
            _employeeRepo.Delete(id);
        }

        // ==================== Attendance ====================
        public List<Attendance> GetAttendances(int employeeId, DateTime? from = null, DateTime? to = null)
            => _attendanceRepo.GetByEmployeeAndDateRange(employeeId, from, to);
        public Attendance? GetAttendance(int id) => _attendanceRepo.GetById(id);

        public void AddAttendance(Attendance attendance)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند حضور و غیاب ثبت کند.");
            _attendanceRepo.Add(attendance);
        }

        public void UpdateAttendance(Attendance attendance)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند حضور و غیاب را ویرایش کند.");
            _attendanceRepo.Update(attendance);
        }

        public void DeleteAttendance(int id)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند حضور و غیاب را حذف کند.");
            _attendanceRepo.Delete(id);
        }

        // ==================== Payroll ====================
        public List<Payroll> GetPayrolls(int? employeeId = null, int? year = null, int? month = null)
            => _payrollRepo.GetByFilters(employeeId, year, month);
        public Payroll? GetPayroll(int id) => _payrollRepo.GetById(id);

        public Payroll? CalculatePayroll(int employeeId, int year, int month)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند حقوق محاسبه کند.");

            var employee = GetEmployee(employeeId);
            if (employee == null) return null;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var attendances = GetAttendances(employeeId, startDate, endDate);

            double totalWorkHours = 0;
            double overtimeHours = 0;

            foreach (var a in attendances.Where(a => a.IsPresent))
            {
                double hours = a.WorkHours.TotalHours;
                totalWorkHours += hours;
                if (hours > 8)
                    overtimeHours += (hours - 8);
            }

            double baseSalary = employee.BaseSalary;
            double hourlyRate = employee.HourlyRate > 0 ? employee.HourlyRate : (baseSalary / (22 * 8));
            double overtimePay = overtimeHours * hourlyRate * employee.OvertimeRate;
            double netSalary = baseSalary + overtimePay;

            return new Payroll
            {
                EmployeeId = employeeId,
                Year = year,
                Month = month,
                BaseSalary = baseSalary,
                TotalWorkHours = totalWorkHours,
                OvertimeHours = overtimeHours,
                OvertimePay = overtimePay,
                Bonus = 0,
                Deductions = 0,
                NetSalary = netSalary,
                IsPaid = false,
                Notes = "محاسبه خودکار"
            };
        }

        public void SavePayroll(Payroll payroll)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند حقوق ثبت کند.");

            var existing = _payrollRepo.GetById(payroll.Id);
            if (existing != null)
                _payrollRepo.Update(payroll);
            else
                _payrollRepo.Add(payroll);
        }

        public void MarkPayrollAsPaid(int payrollId, DateTime paymentDate)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند وضعیت پرداخت حقوق را تغییر دهد.");

            var payroll = _payrollRepo.GetById(payrollId);
            if (payroll != null)
            {
                payroll.IsPaid = true;
                payroll.PaymentDate = paymentDate;
                _payrollRepo.Update(payroll);
            }
        }

        public void DeletePayroll(int id)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند حقوق را حذف کند.");
            _payrollRepo.Delete(id);
        }

        // ==================== Games ====================
        public void AddCompletedGame(GameSession game)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند بازی ثبت کند.");

            var existing = CompletedGames.FirstOrDefault(g => g.Id == game.Id);
            if (existing != null)
            {
                var index = CompletedGames.IndexOf(existing);
                CompletedGames[index] = game;
            }
            else
                CompletedGames.Add(game);

            SaveGamesHistory();
        }

        public List<GameSession> GetGamesHistory()
            => CompletedGames.OrderByDescending(g => g.StartTime).ToList();

        public List<GameSession> GetGamesByDate(DateTime from, DateTime to)
            => CompletedGames.Where(g => g.StartTime.Date >= from.Date && g.StartTime.Date <= to.Date)
                             .OrderByDescending(g => g.StartTime).ToList();

        public List<GameSession> GetGamesByMaster(string gameMasterName)
            => CompletedGames.Where(g => g.GameMasterName.Contains(gameMasterName, StringComparison.OrdinalIgnoreCase))
                             .OrderByDescending(g => g.StartTime).ToList();

        // ==================== متدهای جدید برای بازی‌ها ====================
        public List<GameSession> GetGamesByGameName(string gameName)
        {
            return CompletedGames.Where(g => g.GameName.Contains(gameName, StringComparison.OrdinalIgnoreCase))
                                 .OrderByDescending(g => g.StartTime).ToList();
        }

        public List<GameSession> GetGamesByGameMasterAndDateRange(string gameMasterName, DateTime startDate, DateTime endDate)
        {
            return CompletedGames.Where(g => g.GameMasterName == gameMasterName
                                             && g.EndTime.HasValue
                                             && g.EndTime.Value >= startDate
                                             && g.EndTime.Value <= endDate)
                                 .OrderBy(g => g.EndTime).ToList();
        }

        public List<string> GetActiveGameMasters(DateTime startDate, DateTime endDate)
        {
            return CompletedGames.Where(g => g.EndTime.HasValue
                                             && g.EndTime.Value >= startDate
                                             && g.EndTime.Value <= endDate)
                                 .Select(g => g.GameMasterName).Distinct().ToList();
        }

        public void SaveGamesHistoryPublic()
        {
            SaveGamesHistory();
        }

        public void UpdateGamesSettlement(List<GameSession> games)
        {
            foreach (var game in games)
            {
                var existing = CompletedGames.FirstOrDefault(g => g.Id == game.Id);
                if (existing != null)
                {
                    existing.IsSettled = game.IsSettled;
                    existing.SettlementDate = game.SettlementDate;
                }
            }
            SaveGamesHistory();
        }
        // ==================== پایان متدهای جدید ====================

        public double GetTotalRevenueFromGames()
            => CompletedGames.Sum(g => g.TotalRevenue);

        public double GetTotalGameMasterShare()
            => CompletedGames.Sum(g => g.GameMasterShare);

        private void LoadGamesHistory()
        {
            try
            {
                CompletedGames.Clear();
                if (!File.Exists(_context.GamesHistoryPath)) return;

                var lines = File.ReadAllLines(_context.GamesHistoryPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { "||" }, StringSplitOptions.None);
                    if (parts.Length >= 11)
                    {
                        var game = new GameSession
                        {
                            Id = parts[0],
                            GameName = parts[1],
                            TableNumber = parts[2],
                            GameMasterName = parts[3],
                            RevenuePercent = double.Parse(parts[4]),
                            Players = parts[5].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            StartTime = DateTime.Parse(parts[6]),
                            EndTime = !string.IsNullOrEmpty(parts[7]) ? DateTime.Parse(parts[7]) : (DateTime?)null,
                            TotalRevenue = double.Parse(parts[8]),
                            GameMasterShare = double.Parse(parts[9]),
                            IsActive = bool.Parse(parts[10])
                        };

                        if (parts.Length >= 12 && !string.IsNullOrEmpty(parts[11]))
                        {
                            var paymentParts = parts[11].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var payment in paymentParts)
                            {
                                var kv = payment.Split(':');
                                if (kv.Length == 2)
                                    game.PlayerPayments[kv[0]] = double.Parse(kv[1]);
                            }
                        }

                        if (parts.Length >= 13 && !string.IsNullOrEmpty(parts[12]))
                        {
                            var invoiceParts = parts[12].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var invoice in invoiceParts)
                            {
                                var kv = invoice.Split(':');
                                if (kv.Length == 2)
                                    game.PlayerInvoiceIds[kv[0]] = int.Parse(kv[1]);
                            }
                        }

                        if (parts.Length >= 14 && !string.IsNullOrEmpty(parts[13]))
                            game.IsSettled = bool.Parse(parts[13]);

                        if (parts.Length >= 15 && !string.IsNullOrEmpty(parts[14]))
                            game.SettlementDate = DateTime.Parse(parts[14]);

                        CompletedGames.Add(game);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading games: {ex.Message}");
            }
        }

        private void SaveGamesHistory()
        {
            try
            {
                var lines = new List<string>();
                foreach (var game in CompletedGames)
                {
                    string playersStr = string.Join(",", game.Players);
                    string playerPaymentsStr = string.Join(",", game.PlayerPayments.Select(p => $"{p.Key}:{p.Value}"));
                    string playerInvoicesStr = string.Join(",", game.PlayerInvoiceIds.Select(p => $"{p.Key}:{p.Value}"));
                    string endTimeStr = game.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

                    lines.Add($"{game.Id}||{game.GameName}||{game.TableNumber}||{game.GameMasterName}||{game.RevenuePercent}||{playersStr}||{game.StartTime:yyyy-MM-dd HH:mm:ss}||{endTimeStr}||{game.TotalRevenue}||{game.GameMasterShare}||{game.IsActive}||{playerPaymentsStr}||{playerInvoicesStr}||{game.IsSettled}||{game.SettlementDate?.ToString("yyyy-MM-dd HH:mm:ss")}");
                }
                File.WriteAllLines(_context.GamesHistoryPath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving games: {ex.Message}");
            }
        }

        // ==================== Purchases ====================
        public List<Purchase> GetPurchases(DateTime? from = null, DateTime? to = null)
        {
            var query = _purchases.AsQueryable();
            if (from.HasValue) query = query.Where(p => p.PurchaseDate >= from.Value);
            if (to.HasValue) query = query.Where(p => p.PurchaseDate <= to.Value);
            return query.OrderByDescending(p => p.PurchaseDate).ToList();
        }

        public Purchase? GetPurchaseById(int id)
            => _purchases.FirstOrDefault(p => p.Id == id);

        public void AddPurchase(Purchase purchase)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند خرید ثبت کند.");

            purchase.Id = _purchases.Count > 0 ? _purchases.Max(p => p.Id) + 1 : 1;
            _purchases.Add(purchase);
            SavePurchases();
        }

        public void UpdatePurchase(Purchase purchase)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند خرید را ویرایش کند.");

            var existing = GetPurchaseById(purchase.Id);
            if (existing != null)
            {
                existing.PurchaseDate = purchase.PurchaseDate;
                existing.SupplierName = purchase.SupplierName;
                existing.InvoiceNumber = purchase.InvoiceNumber;
                existing.Items = purchase.Items;
                SavePurchases();
            }
        }

        public void DeletePurchase(int id)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند خرید را حذف کند.");

            var purchase = GetPurchaseById(id);
            if (purchase != null && !purchase.IsConfirmed)
            {
                _purchases.Remove(purchase);
                SavePurchases();
            }
        }

        public void ConfirmPurchase(int purchaseId)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند خرید را تایید کند.");

            var purchase = GetPurchaseById(purchaseId);
            if (purchase == null || purchase.IsConfirmed) return;

            foreach (var item in purchase.Items)
            {
                var product = _productRepo.GetById(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    _productRepo.Update(product);
                }
            }

            purchase.IsConfirmed = true;
            SavePurchases();
        }

        private void SavePurchases()
        {
            try
            {
                var lines = new List<string>();
                foreach (var p in _purchases)
                {
                    lines.Add($"H||{p.Id}||{p.PurchaseDate:yyyy-MM-dd HH:mm:ss}||{p.SupplierName}||{p.InvoiceNumber}||{p.IsConfirmed}");
                    foreach (var item in p.Items)
                        lines.Add($"D||{item.ProductId}||{item.Quantity}||{item.UnitPurchasePrice}");
                }
                File.WriteAllLines(_context.PurchasesPath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving purchases: {ex.Message}");
            }
        }

        private void LoadPurchases()
        {
            try
            {
                _purchases.Clear();
                if (!File.Exists(_context.PurchasesPath)) return;

                var lines = File.ReadAllLines(_context.PurchasesPath);
                Purchase current = null;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { "||" }, StringSplitOptions.None);

                    if (parts[0] == "H")
                    {
                        current = new Purchase
                        {
                            Id = int.Parse(parts[1]),
                            PurchaseDate = DateTime.Parse(parts[2]),
                            SupplierName = parts[3],
                            InvoiceNumber = parts[4],
                            IsConfirmed = bool.Parse(parts[5]),
                            Items = new List<PurchaseItem>()
                        };
                        _purchases.Add(current);
                    }
                    else if (parts[0] == "D" && current != null && parts.Length >= 4)
                    {
                        var item = new PurchaseItem
                        {
                            ProductId = int.Parse(parts[1]),
                            Quantity = int.Parse(parts[2]),
                            UnitPurchasePrice = double.Parse(parts[3])
                        };
                        item.Product = _productRepo.GetById(item.ProductId);
                        current.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading purchases: {ex.Message}");
            }
        }

        // ==================== Stocktakes ====================
        public List<Stocktake> GetStocktakes(DateTime? from = null, DateTime? to = null)
        {
            var query = _stocktakes.AsQueryable();
            if (from.HasValue) query = query.Where(s => s.StocktakeDate >= from.Value);
            if (to.HasValue) query = query.Where(s => s.StocktakeDate <= to.Value);
            return query.OrderByDescending(s => s.StocktakeDate).ToList();
        }

        public Stocktake? GetStocktakeById(int id)
            => _stocktakes.FirstOrDefault(s => s.Id == id);

        public void AddStocktake(Stocktake stocktake)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند انبارگردانی ثبت کند.");

            stocktake.Id = _stocktakes.Count > 0 ? _stocktakes.Max(s => s.Id) + 1 : 1;
            _stocktakes.Add(stocktake);
            SaveStocktakes();
        }

        public void FinalizeStocktake(int stocktakeId)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند انبارگردانی را نهایی کند.");

            var stocktake = GetStocktakeById(stocktakeId);
            if (stocktake == null || stocktake.IsFinalized) return;

            foreach (var item in stocktake.Items)
            {
                var product = _productRepo.GetById(item.ProductId);
                if (product != null)
                {
                    product.Stock = item.PhysicalStock;
                    _productRepo.Update(product);
                }
            }

            stocktake.IsFinalized = true;
            SaveStocktakes();
        }

        public void DeleteStocktake(int id)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند انبارگردانی را حذف کند.");

            var stocktake = GetStocktakeById(id);
            if (stocktake != null && !stocktake.IsFinalized)
            {
                _stocktakes.Remove(stocktake);
                SaveStocktakes();
            }
        }

        private void SaveStocktakes()
        {
            try
            {
                var lines = new List<string>();
                foreach (var st in _stocktakes)
                {
                    lines.Add($"H||{st.Id}||{st.StocktakeDate:yyyy-MM-dd HH:mm:ss}||{st.Note}||{st.IsFinalized}");
                    foreach (var item in st.Items)
                        lines.Add($"D||{item.ProductId}||{item.SystemStock}||{item.PhysicalStock}");
                }
                File.WriteAllLines(_context.StocktakesPath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving stocktakes: {ex.Message}");
            }
        }

        private void LoadStocktakes()
        {
            try
            {
                _stocktakes.Clear();
                if (!File.Exists(_context.StocktakesPath)) return;

                var lines = File.ReadAllLines(_context.StocktakesPath);
                Stocktake current = null;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { "||" }, StringSplitOptions.None);

                    if (parts[0] == "H")
                    {
                        current = new Stocktake
                        {
                            Id = int.Parse(parts[1]),
                            StocktakeDate = DateTime.Parse(parts[2]),
                            Note = parts[3],
                            IsFinalized = bool.Parse(parts[4]),
                            Items = new List<StocktakeItem>()
                        };
                        _stocktakes.Add(current);
                    }
                    else if (parts[0] == "D" && current != null && parts.Length >= 4)
                    {
                        var item = new StocktakeItem
                        {
                            ProductId = int.Parse(parts[1]),
                            SystemStock = int.Parse(parts[2]),
                            PhysicalStock = int.Parse(parts[3])
                        };
                        item.Product = _productRepo.GetById(item.ProductId);
                        current.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading stocktakes: {ex.Message}");
            }
        }

        // ==================== Miscellaneous Expenses ====================
        public List<MiscellaneousExpense> GetMiscExpenses(DateTime? from = null, DateTime? to = null)
        {
            var query = _miscExpenses.AsQueryable();
            if (from.HasValue) query = query.Where(e => e.ExpenseDate >= from.Value);
            if (to.HasValue) query = query.Where(e => e.ExpenseDate <= to.Value);
            return query.OrderByDescending(e => e.ExpenseDate).ToList();
        }

        public MiscellaneousExpense? GetMiscExpenseById(int id)
            => _miscExpenses.FirstOrDefault(e => e.Id == id);

        public void AddMiscExpense(MiscellaneousExpense expense)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند هزینه متفرقه ثبت کند.");

            expense.Id = _miscExpenses.Count > 0 ? _miscExpenses.Max(e => e.Id) + 1 : 1;
            _miscExpenses.Add(expense);
            SaveMiscExpenses();
        }

        public void UpdateMiscExpense(MiscellaneousExpense updated)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند هزینه متفرقه را ویرایش کند.");

            var existing = GetMiscExpenseById(updated.Id);
            if (existing != null)
            {
                existing.ExpenseDate = updated.ExpenseDate;
                existing.Description = updated.Description;
                existing.Category = updated.Category;
                existing.Amount = updated.Amount;
                existing.PaymentMethod = updated.PaymentMethod;
                existing.ReceiptNumber = updated.ReceiptNumber;
                existing.Notes = updated.Notes;
                existing.IsConfirmed = updated.IsConfirmed;
                SaveMiscExpenses();
            }
        }

        public void DeleteMiscExpense(int id)
        {
            if (!HasAccess("Admin"))
                throw new UnauthorizedAccessException("فقط مدیر سیستم می‌تواند هزینه متفرقه را حذف کند.");

            var expense = GetMiscExpenseById(id);
            if (expense != null)
            {
                _miscExpenses.Remove(expense);
                SaveMiscExpenses();
            }
        }

        public double GetTotalMiscExpenses(DateTime from, DateTime to)
        {
            return _miscExpenses.Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to && e.IsConfirmed)
                                .Sum(e => e.Amount);
        }

        private void SaveMiscExpenses()
        {
            try
            {
                var lines = new List<string>();
                foreach (var e in _miscExpenses)
                {
                    lines.Add($"{e.Id}||{e.ExpenseDate:yyyy-MM-dd HH:mm:ss}||{e.Description}||{e.Category}||{e.Amount}||{e.PaymentMethod}||{e.ReceiptNumber}||{e.Notes}||{e.IsConfirmed}");
                }
                File.WriteAllLines(_context.MiscExpensesPath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving misc expenses: {ex.Message}");
            }
        }

        private void LoadMiscExpenses()
        {
            try
            {
                _miscExpenses.Clear();
                if (!File.Exists(_context.MiscExpensesPath)) return;

                var lines = File.ReadAllLines(_context.MiscExpensesPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { "||" }, StringSplitOptions.None);
                    if (parts.Length >= 9)
                    {
                        _miscExpenses.Add(new MiscellaneousExpense
                        {
                            Id = int.Parse(parts[0]),
                            ExpenseDate = DateTime.Parse(parts[1]),
                            Description = parts[2],
                            Category = parts[3],
                            Amount = double.Parse(parts[4]),
                            PaymentMethod = parts[5],
                            ReceiptNumber = parts[6],
                            Notes = parts[7],
                            IsConfirmed = bool.Parse(parts[8])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading misc expenses: {ex.Message}");
            }
        }

        // ==================== Logs ====================
        public List<SystemLog> GetLogs()
        {
            var logs = new List<SystemLog>();

            foreach (var inv in _invoiceRepo.GetAll())
                logs.Add(new SystemLog
                {
                    Timestamp = inv.OrderDate,
                    Action = $"فاکتور شماره {inv.Id} برای میز {inv.TableNumber} به مبلغ {inv.TotalAmount:N0} تومان ثبت شد."
                });

            foreach (var game in CompletedGames)
                logs.Add(new SystemLog
                {
                    Timestamp = game.EndTime ?? game.StartTime,
                    Action = $"بازی {game.GameName} - میز {game.TableNumber} - گرداننده: {game.GameMasterName} - فروش: {game.TotalRevenue:N0} تومان"
                });

            if (!logs.Any())
                logs.Add(new SystemLog { Timestamp = DateTime.Now, Action = "هیچ گزارشی در سیستم ثبت نشده است." });

            return logs.OrderByDescending(l => l.Timestamp).ToList();
        }
    }
}