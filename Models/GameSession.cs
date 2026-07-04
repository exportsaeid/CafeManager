using System;
using System.Collections.Generic;
using System.Linq;

namespace CafeManager.Models
{
    public class GameSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GameName { get; set; }
        public string TableNumber { get; set; }
        public string GameMasterName { get; set; }
        public double RevenuePercent { get; set; }
        public List<string> Players { get; set; } = new List<string>();
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double TotalRevenue { get; set; }
        public double GameMasterShare { get; set; }
        public bool IsActive { get; set; } = true;

        // ========== پراپرتی‌های جدید برای تسویه ==========
        public bool IsSettled { get; set; } = false;
        public DateTime? SettlementDate { get; set; }
        // ================================================

        // ========== دیکشنری برای ثبت پرداخت هر بازیکن ==========
        public Dictionary<string, double> PlayerPayments { get; set; } = new Dictionary<string, double>();

        // ========== جدید: لیست فاکتورهای هر بازیکن ==========
        public Dictionary<string, int> PlayerInvoiceIds { get; set; } = new Dictionary<string, int>();

        // متد ثبت پرداخت بازیکن
        public void RecordPlayerPayment(string playerName, double amount)
        {
            if (!Players.Contains(playerName))
                throw new InvalidOperationException($"بازیکن {playerName} در این بازی وجود ندارد");

            if (amount <= 0)
                throw new ArgumentException("مبلغ پرداختی باید مثبت باشد");

            if (PlayerPayments.ContainsKey(playerName))
                PlayerPayments[playerName] += amount;
            else
                PlayerPayments[playerName] = amount;

            // محاسبه مجدد مجموع فروش و سهم گرداننده
            TotalRevenue = PlayerPayments.Values.Sum();
            GameMasterShare = TotalRevenue * (RevenuePercent / 100);
        }

        // متد دریافت مجموع پرداخت یک بازیکن
        public double GetPlayerTotalPayment(string playerName)
        {
            return PlayerPayments.ContainsKey(playerName) ? PlayerPayments[playerName] : 0;
        }

        // متد بررسی اینکه همه بازیکنان پرداخت کرده‌اند؟
        public bool AllPlayersPaid()
        {
            if (Players.Count == 0) return false;
            return Players.All(p => PlayerPayments.ContainsKey(p) && PlayerPayments[p] > 0);
        }

        // متد ثبت فاکتور برای بازیکن
        public void RegisterPlayerInvoice(string playerName, int invoiceId)
        {
            if (!Players.Contains(playerName))
                throw new InvalidOperationException($"بازیکن {playerName} در این بازی وجود ندارد");

            PlayerInvoiceIds[playerName] = invoiceId;
        }

        // متد دریافت فاکتور بازیکن
        public int GetPlayerInvoiceId(string playerName)
        {
            return PlayerInvoiceIds.ContainsKey(playerName) ? PlayerInvoiceIds[playerName] : 0;
        }

        public string DisplayText => $"{GameName} | {GameMasterName} | میز {TableNumber} | {StartTime:yyyy/MM/dd HH:mm}";
    }
}