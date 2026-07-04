using System;

namespace CafeManager.Models
{
    // این کلاس مشخص می‌کند هر آیتم در فاکتور مشتری شامل چه چیزهایی است
    public class OrderItem
    {
        // محصولی که مشتری انتخاب کرده (از کلاس قبلی استفاده میکنیم)
        public Product Product { get; set; }

        // تعدادی که مشتری سفارش داده است
        public int Quantity { get; set; }

        // قیمت کل این سطر که به صورت خودکار از ضرب قیمت واحد در تعداد به دست می‌آید
        public double TotalPrice => Product.Price * Quantity;
    }
}