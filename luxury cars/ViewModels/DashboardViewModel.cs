using System;
using System.Collections.ObjectModel;
using System.Windows;
using luxury_cars.Models;

namespace luxury_cars.ViewModels
{
    /// <summary>
    /// ViewModel for DashboardWindow.
    /// Every property uses SetProperty() — WPF binding refreshes the UI automatically
    /// whenever TotalCount, AvailableCount, SearchText, etc. change.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        // ── Car list (ObservableCollection notifies grid on Add/Remove) ──
        private ObservableCollection<Car> _cars = new();
        public ObservableCollection<Car> Cars
        {
            get => _cars;
            private set => SetProperty(ref _cars, value);
        }

        // ── Stat cards ───────────────────────────────────────────────────
        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            private set => SetProperty(ref _totalCount, value);
        }

        private int _availableCount;
        public int AvailableCount
        {
            get => _availableCount;
            private set => SetProperty(ref _availableCount, value);
        }

        private int _outOfStockCount;
        public int OutOfStockCount
        {
            get => _outOfStockCount;
            private set => SetProperty(ref _outOfStockCount, value);
        }

        // ── Search ───────────────────────────────────────────────────────
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                // SetProperty fires PropertyChanged("SearchText")
                // → TextBox binding updates, then we trigger a live search
                if (SetProperty(ref _searchText, value))
                    ExecuteSearch();
            }
        }

        // ── Status bar ───────────────────────────────────────────────────
        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // ── Welcome label ────────────────────────────────────────────────
        private string _welcomeText = "";
        public string WelcomeText
        {
            get => _welcomeText;
            set => SetProperty(ref _welcomeText, value);
        }

        // ── Selected car (from DataGrid) ─────────────────────────────────
        private Car? _selectedCar;
        public Car? SelectedCar
        {
            get => _selectedCar;
            set => SetProperty(ref _selectedCar, value);
        }

        // ── Commands ─────────────────────────────────────────────────────
        public RelayCommand RefreshCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SellCommand { get; }

        // ── Constructor ──────────────────────────────────────────────────
        public DashboardViewModel(string username)
        {
            WelcomeText = $"👋 {username}";
            RefreshCommand = new RelayCommand(_ => Refresh());
            DeleteCommand = new RelayCommand(param => DeleteCar(param),
                                              param => param is Car c && c.Id > 0);
            SellCommand = new RelayCommand(param => InitiateSell(param),
                                              param => param is Car c && c.Stock > 0);

            Refresh();
        }

        // ── Public API called by View ─────────────────────────────────────

        /// <summary>Reload grid + stats after AddCar or SaleCompleted.</summary>
        public void Refresh()
        {
            LoadCars();
            LoadStats();
        }

        // ── Private operations ────────────────────────────────────────────

        private void LoadCars()
        {
            try
            {
                Cars.Clear();
                foreach (var car in DatabaseHelper.GetAllCars())
                    Cars.Add(car);

                StatusMessage = $"{Cars.Count} car(s) loaded.";
            }
            catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
        }

        private void LoadStats()
        {
            try
            {
                (int total, int available, int oos) = DatabaseHelper.GetStats();
                // Each assignment fires OnPropertyChanged → UI cards update instantly
                TotalCount = total;
                AvailableCount = available;
                OutOfStockCount = oos;
            }
            catch (Exception ex) { StatusMessage = $"Stats error: {ex.Message}"; }
        }

        private void ExecuteSearch()
        {
            try
            {
                Cars.Clear();
                var results = string.IsNullOrWhiteSpace(SearchText)
                    ? DatabaseHelper.GetAllCars()
                    : DatabaseHelper.SearchCars(SearchText);

                foreach (var car in results)
                    Cars.Add(car);

                StatusMessage = string.IsNullOrWhiteSpace(SearchText)
                    ? $"{Cars.Count} car(s) loaded."
                    : $"{Cars.Count} result(s) for '{SearchText}'.";
            }
            catch (Exception ex) { StatusMessage = $"Search error: {ex.Message}"; }
        }

        private void DeleteCar(object? param)
        {
            if (param is not Car car) return;

            var result = MessageBox.Show(
                $"Delete {car.Brand} {car.Name}?\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool success = DatabaseHelper.DeleteCar(car.Id);
                if (success)
                {
                    Cars.Remove(car);   // ObservableCollection removes row live
                    LoadStats();        // Stat cards update via INotifyPropertyChanged
                    StatusMessage = $"✔ {car.Brand} {car.Name} deleted.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Delete Failed",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // SellCommand.Execute — View opens SaleWindow with this car
        // Actual sale logic stays in SaleViewModel; this just validates
        private void InitiateSell(object? param)
        {
            if (param is not Car car) return;
            if (car.Stock <= 0)
            {
                MessageBox.Show($"{car.Brand} {car.Name} is already out of stock!",
                                "Cannot Sell", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
            // DashboardWindow opens SaleWindow using this car
        }
    }
}