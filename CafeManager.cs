using System;
using System.Collections.Generic;
using CafeManager.Models;
using CafeManager.Services;

namespace CafeManager
{
    public static class CafeManager
    {
        private static CafeManagerService? _service;
        private static readonly object _lock = new object();

        private static CafeManagerService Service
        {
            get
            {
                if (_service == null)
                {
                    lock (_lock)
                    {
                        if (_service == null)
                            _service = new CafeManagerService();
                    }
                }
                return _service;
            }
        }

        // ==================== Properties ====================
        public static UserSession CurrentUser 
            => Service.CurrentUser;

        public static Dictionary<string, GameSession> ActiveGames
        {
            get => Service.ActiveGames;
            set => Service.ActiveGames = value;
        }

        public static List<GameSession> CompletedGames
        {
            get => Service.CompletedGames;
            set => Service.CompletedGames = value;
        }

        // ==================== Authentication & User Management ====================
        public static bool Login(string username, string password) 
            => Service.Login(username, password);

        public static void Logout() 
            => Service.Logout();

        public static bool IsAuthenticated() 
            => Service.IsAuthenticated();

        public static bool IsAdmin() 
            => Service.IsAdmin();

        public static bool IsCashier() 
            => Service.IsCashier();

        public static bool HasAccess(string requiredRole) 
            => Service.HasAccess(requiredRole);

        public static List<User> GetUsers(bool activeOnly = true) 
            => Service.GetUsers(activeOnly);

        public static User? GetUserById(int id) 
            => Service.GetUserById(id);

        public static User? GetUserByUsername(string username) 
            => Service.GetUserByUsername(username);

        public static void AddUser(User user) 
            => Service.AddUser(user);

        public static void UpdateUser(User user) 
            => Service.UpdateUser(user);

        public static void DeleteUser(int id) 
            => Service.DeleteUser(id);

        public static void ChangeUserRole(int userId, string newRole) 
            => Service.ChangeUserRole(userId, newRole);

        public static void ChangeUserPassword(int userId, string newPassword) 
            => Service.ChangeUserPassword(userId, newPassword);

        // ==================== Products ====================
        public static List<Product> GetMenu() => Service.GetProducts();
        public static List<Product> GetProducts() => Service.GetProducts();
        public static Product? GetProductById(int id) => Service.GetProduct(id);
        
        public static void AddProductToMenu(string name, double price)
        {
            var product = new Product { Name = name, Price = price, Stock = 0, LowStockThreshold = 5 };
            Service.AddProduct(product);
        }
        
        public static void AddProduct(Product product) => Service.AddProduct(product);
        public static void UpdateProduct(Product product) => Service.UpdateProduct(product);
        public static void UpdateProductInMenu(int id, string name, double price)
        {
            var product = Service.GetProduct(id);
            if (product != null)
            {
                product.Name = name;
                product.Price = price;
                Service.UpdateProduct(product);
            }
        }
        public static void DeleteProductFromMenu(int id) => Service.DeleteProduct(id);
        public static void UpdateStock(int id, int newStock, int threshold) 
            => Service.UpdateStock(id, newStock, threshold);
        public static List<Product> GetLowStockAlerts() => Service.GetLowStockAlerts();

        // ==================== Invoices ====================
        public static void SaveInvoice(Invoice invoice) => Service.SaveInvoice(invoice);
        public static List<Invoice> GetSalesHistory() => Service.GetInvoices();
        public static List<Invoice> GetSalesHistoryByDate(DateTime from, DateTime to) 
            => Service.GetInvoicesByDate(from, to);
        public static double GetTotalSales() => Service.GetTotalSales();
        public static Invoice? GetInvoiceById(int id) => Service.GetInvoice(id);
        
        public static void UpdateInvoice(int invoiceId, string customerName, string tableNumber)
        {
            var invoice = Service.GetInvoice(invoiceId);
            if (invoice != null)
            {
                invoice.CustomerName = customerName;
                invoice.TableNumber = tableNumber;
                Service.UpdateInvoice(invoice);
            }
        }
        
        public static void UpdateInvoice(Invoice updatedInvoice) => Service.UpdateInvoice(updatedInvoice);
        public static void DeleteInvoice(int invoiceId) => Service.DeleteInvoice(invoiceId);
        public static void UpdateItemQuantity(int invoiceId, int productId, int newQuantity) 
            => Service.UpdateItemQuantity(invoiceId, productId, newQuantity);
        public static void DeleteItemFromInvoice(int invoiceId, int productId) 
            => Service.DeleteItemFromInvoice(invoiceId, productId);
        public static void UpdateSettlementStatus(int invoiceId, bool isSettled, string payMethod) 
            => Service.UpdateSettlementStatus(invoiceId, isSettled, payMethod);

        // ==================== Employees ====================
        public static List<Employee> GetEmployees(bool activeOnly = true) 
            => Service.GetEmployees(activeOnly);
        public static Employee? GetEmployeeById(int id) => Service.GetEmployee(id);
        public static void AddEmployee(Employee employee) => Service.AddEmployee(employee);
        public static void UpdateEmployee(Employee employee) => Service.UpdateEmployee(employee);
        public static void DeleteEmployee(int id) => Service.DeleteEmployee(id);

        // ==================== Attendance ====================
        public static List<Attendance> GetAttendances(int employeeId, DateTime? from = null, DateTime? to = null) 
            => Service.GetAttendances(employeeId, from, to);
        public static Attendance? GetAttendanceById(int id) => Service.GetAttendance(id);
        public static void AddAttendance(Attendance attendance) => Service.AddAttendance(attendance);
        public static void UpdateAttendance(Attendance attendance) => Service.UpdateAttendance(attendance);
        public static void DeleteAttendance(int id) => Service.DeleteAttendance(id);

        // ==================== Payroll ====================
        public static List<Payroll> GetPayrolls(int? employeeId = null, int? year = null, int? month = null) 
            => Service.GetPayrolls(employeeId, year, month);
        public static Payroll? GetPayrollById(int id) => Service.GetPayroll(id);
        public static Payroll? CalculatePayroll(int employeeId, int year, int month) 
            => Service.CalculatePayroll(employeeId, year, month);
        public static void SavePayroll(Payroll payroll) => Service.SavePayroll(payroll);
        public static void MarkPayrollAsPaid(int payrollId, DateTime paymentDate) 
            => Service.MarkPayrollAsPaid(payrollId, paymentDate);
        public static void DeletePayroll(int id) => Service.DeletePayroll(id);

        // ==================== Games ====================
        public static void AddCompletedGame(GameSession game) => Service.AddCompletedGame(game);
        public static List<GameSession> GetGamesHistory() => Service.GetGamesHistory();
        public static List<GameSession> GetGamesByDate(DateTime from, DateTime to) 
            => Service.GetGamesByDate(from, to);
        public static List<GameSession> GetGamesByGameMaster(string gameMasterName) 
            => Service.GetGamesByMaster(gameMasterName);
        public static List<GameSession> GetGamesByGameName(string gameName)
            => Service.GetGamesByGameName(gameName);
        public static List<GameSession> GetGamesByGameMasterAndDateRange(string gameMasterName, DateTime startDate, DateTime endDate)
            => Service.GetGamesByGameMasterAndDateRange(gameMasterName, startDate, endDate);
        public static List<string> GetActiveGameMasters(DateTime startDate, DateTime endDate)
            => Service.GetActiveGameMasters(startDate, endDate);
        public static double GetTotalRevenueFromGames() => Service.GetTotalRevenueFromGames();
        public static double GetTotalGameMasterShare() => Service.GetTotalGameMasterShare();
        public static void SaveGamesHistoryPublic() => Service.SaveGamesHistoryPublic();
        public static void UpdateGamesSettlement(List<GameSession> games)
            => Service.UpdateGamesSettlement(games);

        // ==================== Purchases ====================
        public static List<Purchase> GetPurchases(DateTime? from = null, DateTime? to = null)
            => Service.GetPurchases(from, to);
        public static Purchase? GetPurchaseById(int id) 
            => Service.GetPurchaseById(id);
        public static void AddPurchase(Purchase purchase) 
            => Service.AddPurchase(purchase);
        public static void UpdatePurchase(Purchase purchase) 
            => Service.UpdatePurchase(purchase);
        public static void DeletePurchase(int id) 
            => Service.DeletePurchase(id);
        public static void ConfirmPurchase(int purchaseId) 
            => Service.ConfirmPurchase(purchaseId);

        // ==================== Stocktakes ====================
        public static List<Stocktake> GetStocktakes(DateTime? from = null, DateTime? to = null)
            => Service.GetStocktakes(from, to);
        public static Stocktake? GetStocktakeById(int id) 
            => Service.GetStocktakeById(id);
        public static void AddStocktake(Stocktake stocktake) 
            => Service.AddStocktake(stocktake);
        public static void FinalizeStocktake(int stocktakeId) 
            => Service.FinalizeStocktake(stocktakeId);
        public static void DeleteStocktake(int id) 
            => Service.DeleteStocktake(id);

        // ==================== Miscellaneous Expenses ====================
        public static List<MiscellaneousExpense> GetMiscExpenses(DateTime? from = null, DateTime? to = null)
            => Service.GetMiscExpenses(from, to);
        public static MiscellaneousExpense? GetMiscExpenseById(int id) 
            => Service.GetMiscExpenseById(id);
        public static void AddMiscExpense(MiscellaneousExpense expense) 
            => Service.AddMiscExpense(expense);
        public static void UpdateMiscExpense(MiscellaneousExpense expense) 
            => Service.UpdateMiscExpense(expense);
        public static void DeleteMiscExpense(int id) 
            => Service.DeleteMiscExpense(id);
        public static double GetTotalMiscExpenses(DateTime from, DateTime to) 
            => Service.GetTotalMiscExpenses(from, to);

        // ==================== Logs ====================
        public static List<SystemLog> GetLogs() => Service.GetLogs();
    }
}