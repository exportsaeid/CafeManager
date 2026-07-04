using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeManager.Models;

namespace CafeManager.Services
{
    public class AttendanceRepository : BaseRepository<Attendance>
    {
        public AttendanceRepository(string filePath) : base(filePath) { }

        protected override int GetId(Attendance entity) => entity.Id;
        protected override void SetId(Attendance entity, int id) => entity.Id = id;

        protected override string Serialize(Attendance a)
        {
            return $"{a.Id}||{a.EmployeeId}||{a.Date:yyyy-MM-dd}||{a.CheckIn}||{a.CheckOut}||{a.IsPresent}||{a.Notes}";
        }

        protected override Attendance Deserialize(string line)
        {
            var parts = line.Split(new[] { "||" }, StringSplitOptions.None);
            return new Attendance
            {
                Id = int.Parse(parts[0]),
                EmployeeId = int.Parse(parts[1]),
                Date = DateTime.Parse(parts[2]),
                CheckIn = TimeSpan.Parse(parts[3]),
                CheckOut = TimeSpan.Parse(parts[4]),
                IsPresent = bool.Parse(parts[5]),
                Notes = parts[6]
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

        public List<Attendance> GetByEmployeeAndDateRange(int employeeId, DateTime? from, DateTime? to)
        {
            var query = _items.Where(a => a.EmployeeId == employeeId).AsQueryable();
            if (from.HasValue) query = query.Where(a => a.Date >= from.Value);
            if (to.HasValue) query = query.Where(a => a.Date <= to.Value);
            return query.OrderBy(a => a.Date).ToList();
        }

        public List<Attendance> GetByDate(DateTime date)
        {
            return _items.Where(a => a.Date.Date == date.Date).ToList();
        }
    }
}
