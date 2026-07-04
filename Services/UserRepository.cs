using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeManager.Models;

namespace CafeManager.Services
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(string filePath) : base(filePath) { }

        protected override int GetId(User entity) => entity.Id;
        protected override void SetId(User entity, int id) => entity.Id = id;

        protected override string Serialize(User u)
        {
            return $"{u.Id}||{u.Username}||{u.Password}||{u.FullName}||{u.Role}||{u.IsActive}||{u.LastLogin?.ToString("yyyy-MM-dd HH:mm:ss")}";
        }

        protected override User Deserialize(string line)
        {
            var parts = line.Split(new[] { "||" }, StringSplitOptions.None);
            return new User
            {
                Id = int.Parse(parts[0]),
                Username = parts[1],
                Password = parts[2],
                FullName = parts[3],
                Role = parts[4],
                IsActive = bool.Parse(parts[5]),
                LastLogin = !string.IsNullOrEmpty(parts[6]) ? DateTime.Parse(parts[6]) : (DateTime?)null
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

        public User? GetByUsername(string username)
        {
            return _items.FirstOrDefault(u => u.Username == username);
        }

        public List<User> GetActiveUsers()
        {
            return _items.Where(u => u.IsActive).ToList();
        }

        public void HardDelete(int id)
        {
            var user = GetById(id);
            if (user != null)
            {
                _items.Remove(user);
                Save();
            }
        }
    }
}