namespace luxury_cars.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string ModelNumber { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Name { get; set; } = "";
        public int Year { get; set; }
        public string Color { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; } = "Available";
        public string CreatedAt { get; set; } = "";

        // ── NEW: path to image file on disk ───────────────────────────
        // Stores relative path like "CarImages\ferrari_roma.jpg"
        // Null/empty means no image uploaded yet
        public string? ImagePath { get; set; }

        // Computed — shown in UI
        public string DisplayName => $"{Brand} {Name} ({Year})";
        public string PriceDisplay => $"₹{Price:N0}";
        public string StockDisplay => Stock == 0 ? "Out of Stock" : $"{Stock} units";

        // Full absolute path for WPF Image control binding
        // Returns a default placeholder path if no image set
        public string FullImagePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagePath))
                    return System.IO.Path.Combine(
                        System.AppDomain.CurrentDomain.BaseDirectory,
                        "CarImages", "placeholder.png");

                return System.IO.Path.Combine(
                    System.AppDomain.CurrentDomain.BaseDirectory,
                    ImagePath);
            }
        }

        // True if a real image has been uploaded
        public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);
    }
}