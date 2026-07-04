using System;

namespace CafeManager.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }

        // فیلدهای لازم برای انبارداری هوشمند
        public int Stock { get; set; } // موجودی فعلی
        public int LowStockThreshold { get; set; } // آستانه آلارم کمبود کالا
    }
}