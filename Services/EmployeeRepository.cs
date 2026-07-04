using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeManager.Models;

namespace CafeManager.Services
{
    public class EmployeeRepository : BaseRepository<Employee>
    {
        public EmployeeRepository(string filePath) : base(filePath) { }

        protected override int GetId(Employee entity) => entity.Id;
        protected override void SetId(Employee entity, int id) => entity.Id = id;

        protected override string Serialize(Employee e)
        {
            return $"{e.Id}||{e.FirstName}||{e.LastName}||{e.NationalCode}||{e.PhoneNumber}||{e.Position}||{e.HireDate:yyyy-MM-dd HH:mm:ss}||{e.IsActive}||{e.BaseSalary}||{e.HourlyRate}||{e.OvertimeRate}||{e.Notes}";
        }

        protected override Employee Deserialize(string line)
        {
            var parts = line.Split(new[] { "||" }, StringSplitOptions.None);
            return new Employee
            {
                Id = int.Parse(parts[0]),
                FirstName = parts[1],
                LastName = parts[2],
                NationalCode = parts[3],
                PhoneNumber = parts[4],
                Position = parts[5],
                HireDate = DateTime.Parse(parts[6]),
                IsActive = bool.Parse(parts[7]),
                BaseSalary = double.Parse(parts[8]),
                HourlyRate = double.Parse(parts[9]),
                OvertimeRate = double.Parse(parts[10]),
                Notes = parts[11]
            };
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

        public List<Employee> GetActive()
        {
            return _items.Where(e => e.IsActive).OrderBy(e => e.FirstName).ToList();
        }

        public Employee? GetByNationalCode(string nationalCode)
        {
            return _items.FirstOrDefault(e => e.NationalCode == nationalCode);
        }
    }
}
