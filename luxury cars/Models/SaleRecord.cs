namespace luxury_cars.Models
{
    public class SaleRecord
    {
        // Car details
        public int CarId { get; set; }
        public string ModelNumber { get; set; } = "";
        public string Brand { get; set; } = "";
        public string CarName { get; set; } = "";
        public int Year { get; set; }
        public string Color { get; set; } = "";

        // Customer details
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string CustomerAddress { get; set; } = "";
        public string IdProof { get; set; } = "";

        // Payment details
        public decimal SalePrice { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal DueAmount => SalePrice - AmountPaid;
        public string PaymentType { get; set; } = "Cash";
        public int EmiMonths { get; set; }
        public decimal EmiPerMonth => EmiMonths > 0 ? DueAmount / EmiMonths : 0;

        // Delivery
        public DateTime DeliveryDate { get; set; } = DateTime.Today;
        public string Notes { get; set; } = "";

        // Computed
        public string PriceDisplay => $"₹{SalePrice:N0}";
        public string PaidDisplay => $"₹{AmountPaid:N0}";
        public string DueDisplay => $"₹{DueAmount:N0}";
        public string EmiDisplay => EmiMonths > 0
                                         ? $"₹{EmiPerMonth:N0}/month x {EmiMonths} months"
                                         : "N/A";
    }
}