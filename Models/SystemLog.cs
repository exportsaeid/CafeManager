using System;

namespace CafeManager.Models
{
    public class SystemLog
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Action { get; set; } = "";
        public string? User { get; set; }
        public LogLevel Level { get; set; } = LogLevel.Info;
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }
}
