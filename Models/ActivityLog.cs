using System;

namespace CafeManager.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Details { get; set; }
        public string IpAddress { get; set; }
    }
}