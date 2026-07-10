using System;

namespace CafeManager.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public string Method { get; set; } = "نقدی";
        public double Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string ReferenceNumber { get; set; } = "";
        public string Description { get; set; } = "";

        public Invoice Invoice { get; set; }

    }
}
