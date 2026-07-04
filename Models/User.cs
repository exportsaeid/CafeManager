using System; // اضافه شده

namespace CafeManager.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // هش شده
        public string FullName { get; set; }
        public string Role { get; set; } // "Admin" یا "Cashier"
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; } // استفاده از DateTime?
    }
}