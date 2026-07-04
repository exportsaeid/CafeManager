using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeManager.Models;

namespace CafeManager.Services
{
    public class PayrollRepository : BaseRepository<Payroll>
    {
        public PayrollRepository(string filePath) : base(filePath) { }

        protected override int GetId(Payroll entity) => entity.Id;
        protected override void SetId(Payroll entity, int id) => entity.Id = id;

        protected override string Serialize(Payroll p)
        {
            string paymentDateStr = p.PaymentDate.HasValue ? p.PaymentDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
            return $"{p.Id}||{p.EmployeeId}||{p.Year}||{p.Month}||{p.BaseSalary}||{p.TotalWorkHours}||{p.OvertimeHours}||{p.OvertimePay}||{p.Bonus}||{p.Deductions}||{p.NetSalary}||{p.IsPaid}||{paymentDateStr}||{p.Notes}";
        }

        protected override Payroll Deserialize(string line)
        {
            var parts = line.Split(new[] { "||" }, StringSplitOptions.None);
            var payroll = new Payroll
            {
                Id = int.Parse(parts[0]),
                EmployeeId = int.Parse(parts[1]),
                Year = int.Parse(parts[2]),
                Month = int.Parse(parts[3]),
                BaseSalary = double.Parse(parts[4]),
                TotalWorkHours = double.Parse(parts[5]),
                OvertimeHours = double.Parse(parts[6]),
                OvertimePay = double.Parse(parts[7]),
                Bonus = double.Parse(parts[8]),
                Deductions = double.Parse(parts[9]),
                NetSalary = double.Parse(parts[10]),
                IsPaid = bool.Parse(parts[11])
            };

            if (!string.IsNullOrEmpty(parts[12]))
                payroll.PaymentDate = DateTime.Parse(parts[12]);
            payroll.Notes = parts[13];

            return payroll;
        }

        public override void Save()
        {
            File.WriteAllLines(_filePath, _items.Select(Serialize));
        }

        public override void Load()
        {
            _items.Clear();
            if (!File.Exists(_filePath)) return;

            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    _items.Add(Deserialize(line));
            }
        }

        public List<Payroll> GetByFilters(int? employeeId = null, int? year = null, int? month = null)
        {
            var query = _items.AsQueryable();
            if (employeeId.HasValue) query = query.Where(p => p.EmployeeId == employeeId.Value);
            if (year.HasValue) query = query.Where(p => p.Year == year.Value);
            if (month.HasValue) query = query.Where(p => p.Month == month.Value);
            return query.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();
        }

        public Payroll? GetByEmployeeAndMonth(int employeeId, int year, int month)
        {
            return _items.FirstOrDefault(p => p.EmployeeId == employeeId && p.Year == year && p.Month == month);
        }
    }
}
