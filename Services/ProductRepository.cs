using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeManager.Models;

namespace CafeManager.Services
{
    public class ProductRepository : BaseRepository<Product>
    {
        public ProductRepository(string filePath) : base(filePath) { }

        protected override int GetId(Product entity) => entity.Id;
        protected override void SetId(Product entity, int id) => entity.Id = id;

        protected override string Serialize(Product p)
        {
            return $"{p.Id}||{p.Name}||{p.Price}||{p.Stock}||{p.LowStockThreshold}";
        }

        protected override Product Deserialize(string line)
        {
            var parts = line.Split(new[] { "||" }, StringSplitOptions.None);
            return new Product
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
                Price = double.Parse(parts[2]),
                Stock = int.Parse(parts[3]),
                LowStockThreshold = int.Parse(parts[4])
            };
        }

        public override void Save()
        {
            File.WriteAllLines(_filePath, _items.Select(Serialize));
        }

        public override void Load()
        {
            _items.Clear();
            if (!File.Exists(_filePath))
            {
                _items.AddRange(GetDefaultProducts());
                Save();
                return;
            }

            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    _items.Add(Deserialize(line));
            }
        }

        private List<Product> GetDefaultProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Name = "اسپرسو سینگل", Price = 45000, Stock = 50, LowStockThreshold = 5 },
                new Product { Id = 2, Name = "لاته", Price = 65000, Stock = 30, LowStockThreshold = 5 },
                new Product { Id = 3, Name = "بازی پوکر", Price = 50000, Stock = 100, LowStockThreshold = 10 },
                new Product { Id = 4, Name = "بازی شطرنج", Price = 25000, Stock = 100, LowStockThreshold = 10 },
                new Product { Id = 5, Name = "بازی تخته نرد", Price = 20000, Stock = 100, LowStockThreshold = 10 },
                new Product { Id = 6, Name = "بازی بیگ‌بازی", Price = 30000, Stock = 100, LowStockThreshold = 10 }
            };
        }

        public Product? GetByName(string name)
        {
            return _items.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public List<Product> GetLowStock()
        {
            return _items.Where(p => p.Stock <= p.LowStockThreshold).ToList();
        }

        public void UpdateStock(int id, int newStock)
        {
            var product = GetById(id);
            if (product != null)
            {
                product.Stock = newStock;
                Save();
            }
        }
    }
}
