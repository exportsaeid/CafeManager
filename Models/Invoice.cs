using System;
using System.Collections.Generic;
using System.Linq;

namespace CafeManager.Models
{
    // ========== کلاس Payment برای پرداخت ترکیبی ==========
    public class Payment
    {
        public string Method { get; set; } // نقدی, کارتخوان, انتقال, آنلاین
        public double Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
    // =====================================================

    public class Invoice
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string TableNumber { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // --- ویژگی‌های جدید ---
        public bool IsSettled { get; set; } = false;
        public string PayMethod { get; set; } = "نقدی";

        // --- ویژگی‌های مدیریت بازی ---
        public bool IsGameSession { get; set; } = false;
        public string GameMasterName { get; set; } = "";
        public double GameMasterSharePercent { get; set; } = 0;

        // ========== لیست پرداخت‌ها (پرداخت ترکیبی) ==========
        public List<Payment> Payments { get; set; } = new List<Payment>();

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public double TotalAmount
        {
            get
            {
                double total = 0;
                foreach (var item in Items)
                {
                    total += item.TotalPrice;
                }
                return total;
            }
        }

        // ========== مبلغ پرداخت شده ==========
        public double PaidAmount => Payments?.Sum(p => p.Amount) ?? 0;

        // ========== مبلغ باقیمانده ==========
        public double RemainingAmount => TotalAmount - PaidAmount;

        // ========== آیا فاکتور به طور کامل پرداخت شده است؟ ==========
        public bool IsFullyPaid => RemainingAmount <= 0;

        public double CalculateGameMasterShare()
        {
            if (!IsGameSession) return 0;
            return TotalAmount * (GameMasterSharePercent / 100);
        }
    }
}