using System;
using System.Collections.Generic;
using System.Linq;

namespace CafeManager.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public string SupplierName { get; set; }
        public string InvoiceNumber { get; set; }
        public List<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        public bool IsConfirmed { get; set; } = false; // تایید نهایی

        public double TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}