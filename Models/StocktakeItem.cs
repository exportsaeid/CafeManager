namespace CafeManager.Models
{
    public class StocktakeItem
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int SystemStock { get; set; } // موجودی سیستم قبل از انبارگردانی
        public int PhysicalStock { get; set; } // موجودی شمارش‌شده
        public int Difference => PhysicalStock - SystemStock;
    }
}