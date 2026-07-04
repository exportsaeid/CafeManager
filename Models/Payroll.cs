using System;

namespace CafeManager.Models
{
    public class Payroll
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public double BaseSalary { get; set; }
        public double TotalWorkHours { get; set; }
        public double OvertimeHours { get; set; }
        public double OvertimePay { get; set; }
        public double Bonus { get; set; }
        public double Deductions { get; set; }
        public double NetSalary { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime? PaymentDate { get; set; }
        public string Notes { get; set; }
    }
}