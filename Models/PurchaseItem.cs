namespace CafeManager.Models
{
    public class PurchaseItem
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public double UnitPurchasePrice { get; set; } // قیمت خرید هر واحد

        public double TotalPrice => Quantity * UnitPurchasePrice;
    }
}