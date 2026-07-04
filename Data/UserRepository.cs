// فایل: Data/UserRepository.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeManager.Models;

namespace CafeManager.Data
{
    public class UserRepository
    {
        private readonly string _filePath;
        private List<User> _users;

        public UserRepository(string filePath)
        {
            _filePath = filePath;
            _users = new List<User>();
            Load();
        }

        public List<User> GetAll() => _users;
        public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);
        public User? GetByUsername(string username) => _users.FirstOrDefault(u => u.Username == username);

        public void Add(User user)
        {
            user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);
            Save();
        }

        public void Update(User updatedUser)
        {
            var user = GetById(updatedUser.Id);
            if (user != null)
            {
                user.Username = updatedUser.Username;
                user.Password = updatedUser.Password;
                user.FullName = updatedUser.FullName;
                user.Role = updatedUser.Role;
                user.IsActive = updatedUser.IsActive;
                user.LastLogin = updatedUser.LastLogin;
                Save();
            }
        }

        public void Delete(int id)
        {
            var user = GetById(id);
            if (user != null)
            {
                user.IsActive = false; // Soft delete
                Save();
            }
        }

        public void HardDelete(int id)
        {
            var user = GetById(id);
            if (user != null)
            {
                _users.Remove(user);
                Save();
            }
        }

        private void Load()
        {
            try
            {
                _users.Clear();
                if (!File.Exists(_filePath)) return;

                var lines = File.ReadAllLines(_filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { "||" }, StringSplitOptions.None);
                    if (parts.Length >= 7)
                    {
                        _users.Add(new User
                        {
                            Id = int.Parse(parts[0]),
                            Username = parts[1],
                            Password = parts[2],
                            FullName = parts[3],
                            Role = parts[4],
                            IsActive = bool.Parse(parts[5]),
                            LastLogin = !string.IsNullOrEmpty(parts[6]) ? DateTime.Parse(parts[6]) : (DateTime?)null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        private void Save()
        {
            try
            {
                var lines = new List<string>();
                foreach (var user in _users)
                {
                    lines.Add($"{user.Id}||{user.Username}||{user.Password}||{user.FullName}||{user.Role}||{user.IsActive}||{user.LastLogin?.ToString("yyyy-MM-dd HH:mm:ss")}");
                }
                File.WriteAllLines(_filePath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving users: {ex.Message}");
            }
        }
    }
}