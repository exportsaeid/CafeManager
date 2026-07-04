using System;
using System.IO;

namespace CafeManager.Data
{
    public class DataContext
    {
        public string RootPath { get; }

        public string MenuPath { get; }
        public string InvoicePath { get; }
        public string GamesHistoryPath { get; }
        public string PurchasesPath { get; }
        public string StocktakesPath { get; }
        public string EmployeesPath { get; }
        public string AttendancesPath { get; }
        public string PayrollsPath { get; }
        public string MiscExpensesPath { get; }
        public string UsersPath { get; }

        public DataContext()
        {
            RootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CafeManagerData"
            );

            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);

            MenuPath = Path.Combine(RootPath, "menu_db.txt");
            InvoicePath = Path.Combine(RootPath, "invoices_db.txt");
            GamesHistoryPath = Path.Combine(RootPath, "games_history_db.txt");
            PurchasesPath = Path.Combine(RootPath, "purchases_db.txt");
            StocktakesPath = Path.Combine(RootPath, "stocktakes_db.txt");
            EmployeesPath = Path.Combine(RootPath, "employees_db.txt");
            AttendancesPath = Path.Combine(RootPath, "attendances_db.txt");
            PayrollsPath = Path.Combine(RootPath, "payrolls_db.txt");
            MiscExpensesPath = Path.Combine(RootPath, "misc_expenses_db.txt");
            UsersPath = Path.Combine(RootPath, "users_db.txt");
        }
    }
}