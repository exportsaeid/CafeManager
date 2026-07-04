using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeManager.Models;

namespace CafeManager.Services
{
    public class InvoiceRepository : BaseRepository<Invoice>
    {
        private readonly ProductRepository _productRepository;

        public InvoiceRepository(string filePath, ProductRepository productRepository) : base(filePath)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        protected override int GetId(Invoice entity) => entity.Id;
        protected override void SetId(Invoice entity, int id) => entity.Id = id;

        protected override string Serialize(Invoice inv)
        {
            string paymentsStr = inv.Payments != null && inv.Payments.Any()
                ? string.Join(",", inv.Payments.Select(p => $"{p.Method}:{p.Amount}"))
                : "";

            var lines = new List<string>
            {
                $"H||{inv.Id}||{inv.CustomerName}||{inv.TableNumber}||{inv.OrderDate:yyyy-MM-dd HH:mm:ss}||{inv.IsSettled}||{inv.PayMethod}||{paymentsStr}"
            };

            foreach (var item in inv.Items)
                lines.Add($"D||{item.Product.Id}||{item.Product.Name}||{item.Product.Price}||{item.Quantity}");

            return string.Join(Environment.NewLine, lines);
        }

        protected override Invoice Deserialize(string line)
        {
            throw new NotImplementedException("Use Load method for complex deserialization");
        }

        public override void Save()
        {
            var lines = new List<string>();
            foreach (var inv in _items)
                lines.AddRange(Serialize(inv).Split(Environment.NewLine));

            File.WriteAllLines(_filePath, lines);
        }

        public override void Load()
        {
            try
            {
                _items.Clear();
                if (!File.Exists(_filePath)) return;

                var lines = File.ReadAllLines(_filePath);
                Invoice currentInvoice = null;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { "||" }, StringSplitOptions.None);

                    if (parts[0] == "H")
                    {
                        currentInvoice = new Invoice
                        {
                            Id = int.Parse(parts[1]),
                            CustomerName = parts[2],
                            TableNumber = parts[3],
                            OrderDate = DateTime.Parse(parts[4]),
                            IsSettled = bool.Parse(parts[5]),
                            PayMethod = parts[6],
                            Items = new List<OrderItem>(),
                            Payments = new List<Payment>()
                        };

                        if (parts.Length >= 8 && !string.IsNullOrEmpty(parts[7]))
                        {
                            var paymentPairs = parts[7].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var pair in paymentPairs)
                            {
                                var kv = pair.Split(':');
                                if (kv.Length == 2)
                                {
                                    currentInvoice.Payments.Add(new Payment
                                    {
                                        Method = kv[0],
                                        Amount = double.Parse(kv[1])
                                    });
                                }
                            }
                        }

                        _items.Add(currentInvoice);
                    }
                    else if (parts[0] == "D" && currentInvoice != null && parts.Length >= 5)
                    {
                        int productId = int.Parse(parts[1]);
                        string productName = parts[2];
                        double productPrice = double.Parse(parts[3]);
                        int quantity = int.Parse(parts[4]);

                        Product productItem;

                        if (_productRepository == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Error: _productRepository is null in InvoiceRepository.Load()");
                            productItem = new Product
                            {
                                Id = productId,
                                Name = productName,
                                Price = productPrice
                            };
                        }
                        else
                        {
                            productItem = _productRepository.GetById(productId);
                            if (productItem == null)
                            {
                                productItem = new Product
                                {
                                    Id = productId,
                                    Name = productName,
                                    Price = productPrice
                                };
                            }
                        }

                        currentInvoice.Items.Add(new OrderItem
                        {
                            Product = productItem,
                            Quantity = quantity
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading invoices: {ex.Message}");
                throw;
            }
        }

        public List<Invoice> GetByDateRange(DateTime from, DateTime to)
        {
            return _items.Where(inv => inv.OrderDate >= from && inv.OrderDate <= to)
                         .OrderByDescending(inv => inv.OrderDate)
                         .ToList();
        }

        public double GetTotalSales()
        {
            return _items.Sum(inv => inv.TotalAmount);
        }

        public double GetTotalSales(DateTime from, DateTime to)
        {
            return GetByDateRange(from, to).Sum(inv => inv.TotalAmount);
        }

        public List<Invoice> GetUnsettledInvoices()
        {
            return _items.Where(inv => !inv.IsSettled).ToList();
        }

        public List<Invoice> GetByTable(string tableNumber)
        {
            return _items.Where(inv => inv.TableNumber == tableNumber)
                         .OrderByDescending(inv => inv.OrderDate)
                         .ToList();
        }
    }
}