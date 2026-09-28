namespace luxury_cars.Models
{
    public class SaleReportRow
    {
        public int SaleId { get; set; }
        public string SaleDate { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string ModelNumber { get; set; } = "";
        public string Brand { get; set; } = "";
        public string CarName { get; set; } = "";
        public int Year { get; set; }
        public string Color { get; set; } = "";
        public string PaymentType { get; set; } = "";
        public int EmiMonths { get; set; }
        public decimal SalePrice { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal DueAmount { get; set; }
        public string DeliveryDate { get; set; } = "";
        public string Notes { get; set; } = "";

        public string SalePriceDisplay => $"₹{SalePrice:N0}";
        public string AmountPaidDisplay => $"₹{AmountPaid:N0}";
        public string DueAmountDisplay => DueAmount > 0 ? $"₹{DueAmount:N0}" : "Paid";
        public string EmiDisplay => EmiMonths > 0 ? $"{EmiMonths} months" : "-";
        public string SaleIdDisplay => $"#SR-{SaleId:D4}";
    }
}