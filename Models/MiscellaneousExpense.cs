using System;

namespace CafeManager.Models
{
    public class MiscellaneousExpense
    {
        public int Id { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.Now;
        public string Description { get; set; } // توضیح هزینه (مثلاً "قبض برق")
        public string Category { get; set; } // دسته‌بندی: آب و برق، اجاره، تعمیرات، تبلیغات، ...
        public double Amount { get; set; }
        public string PaymentMethod { get; set; } // نقدی، کارت، انتقال، ...
        public string ReceiptNumber { get; set; } // شماره فیش یا رسید
        public string Notes { get; set; }
        public bool IsConfirmed { get; set; } = true; // تأیید نهایی
    }
}