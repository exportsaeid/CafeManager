using System; // این خط برای استفاده از DateTime ضروری است

namespace CafeManager.Models
{
    public class UserSession
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTime LoginTime { get; set; }

        public bool IsAdmin => Role == "Admin";
        public bool IsCashier => Role == "Cashier";
        public bool IsAuthenticated => !string.IsNullOrEmpty(Username);

        public string GetRoleDisplay()
        {
            return Role switch
            {
                "Admin" => "مدیر سیستم",
                "Cashier" => "صندوق‌دار",
                _ => "کاربر"
            };
        }

        public string GetRoleEmoji()
        {
            return Role switch
            {
                "Admin" => "👑",
                "Cashier" => "🛒",
                _ => "👤"
            };
        }

        public bool HasAccess(string requiredRole)
        {
            if (!IsAuthenticated) return false;
            if (IsAdmin) return true;
            return Role == requiredRole;
        }
    }
}