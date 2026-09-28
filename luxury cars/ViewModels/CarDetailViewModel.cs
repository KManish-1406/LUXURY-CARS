using luxury_cars.Models;
using System.Windows.Media;

namespace luxury_cars.ViewModels
{
    /// <summary>
    /// ViewModel for CarDetailWindow.
    /// Holds the selected Car and exposes helpers for the UI.
    /// </summary>
    public class CarDetailViewModel : BaseViewModel
    {
        // ── The car being displayed ───────────────────────────────────
        private Car _car = new();
        public Car Car
        {
            get => _car;
            set => SetProperty(ref _car, value);
        }

        // ── Can this car be sold right now? ───────────────────────────
        public bool CanSell => Car.Stock > 0;

        // ── Try to parse car color string to a WPF Color ─────────────
        // Used for the small color dot in the spec sheet
        public Color CarColorBrush
        {
            get
            {
                try
                {
                    var c = (Color)ColorConverter.ConvertFromString(Car.Color);
                    return c;
                }
                catch
                {
                    // Color name not recognized (e.g. "Metallic Silver")
                    // Fall back to a neutral gray dot
                    return Color.FromRgb(148, 163, 184);
                }
            }
        }

        public CarDetailViewModel(Car car)
        {
            Car = car;
        }
    }
}