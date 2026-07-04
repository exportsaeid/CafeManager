using System;
using System.Collections.Generic;

namespace CafeManager.Models
{
    public class Stocktake
    {
        public int Id { get; set; }
        public DateTime StocktakeDate { get; set; } = DateTime.Now;
        public string Note { get; set; }
        public List<StocktakeItem> Items { get; set; } = new List<StocktakeItem>();
        public bool IsFinalized { get; set; } = false; // تایید نهایی
    }
}