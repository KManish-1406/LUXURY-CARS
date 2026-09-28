using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using luxury_cars.Models;

namespace luxury_cars.Views
{
    public partial class DashboardWindow : Window
    {
        private string _loggedInUser;

        public DashboardWindow(string username)
        {
            InitializeComponent();
            _loggedInUser = username;

            // Show username + role in navbar
            WelcomeLabel.Text = $"👋 {username} ({SessionManager.RoleName})";

            // Populate theme dropdown from enum
            ThemeComboBox.SelectionChanged -= ThemeComboBox_SelectionChanged;
            foreach (var theme in Enum.GetNames(typeof(ThemeManager.AppTheme)))
                ThemeComboBox.Items.Add(theme);

            // Set current theme selection
            ThemeComboBox.SelectedItem =
                ThemeManager.Instance.CurrentTheme.ToString();
            ThemeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;

            // Apply RBAC permissions to UI
            ApplyPermissions();

            LoadCars();
            LoadStats();
        }

        // ── Apply permissions to UI ───────────────────────
        private void ApplyPermissions()
        {
            AddCarBtn.Visibility = SessionManager.HasPermission("can_add_car")
                ? Visibility.Visible : Visibility.Collapsed;

            ReportsBtn.Visibility = SessionManager.HasPermission("can_view_reports")
                ? Visibility.Visible : Visibility.Collapsed;

            ManageUsersBtn.Visibility = SessionManager.HasPermission("can_manage_users")
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Theme switcher ────────────────────────────────
        private void ThemeComboBox_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is string themeName &&
                Enum.TryParse<ThemeManager.AppTheme>(themeName, out var theme))
            {
                ThemeManager.Instance.CurrentTheme = theme;
            }
        }

        // ── Load all cars ─────────────────────────────────
        private void LoadCars()
        {
            CarsGrid.ItemsSource = DatabaseHelper.GetAllCars();
        }

        // ── Load stats ────────────────────────────────────
        private void LoadStats()
        {
            (int total, int available, int outOfStock) = DatabaseHelper.GetStats();
            TotalCount.Text = total.ToString();
            AvailableCount.Text = available.ToString();
            OutOfStockCount.Text = outOfStock.ToString();
        }

        // ── Search ────────────────────────────────────────
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchBox.Text.Trim();
            CarsGrid.ItemsSource = string.IsNullOrWhiteSpace(keyword)
                ? DatabaseHelper.GetAllCars()
                : DatabaseHelper.SearchCars(keyword);
        }

        // ── Add Car ───────────────────────────────────────
        private void AddCarBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission("can_add_car"))
            {
                MessageBox.Show("You don't have permission to add cars.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addWin = new AddCarWindow();
            addWin.Owner = this;
            addWin.CarAdded += () => { LoadCars(); LoadStats(); };
            addWin.ShowDialog();
        }

        // ── Reports ───────────────────────────────────────
        private void ReportsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission("can_view_reports"))
            {
                MessageBox.Show("You don't have permission to view reports.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reportWin = new ReportWindow();
            reportWin.Owner = this;
            reportWin.ShowDialog();
        }

        // ── Manage Users ──────────────────────────────────
        private void ManageUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission("can_manage_users"))
            {
                MessageBox.Show("You don't have permission to manage users.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var userWin = new ManageUsersWindow();
            userWin.Owner = this;
            userWin.ShowDialog();
        }

        // ── Sell Car ──────────────────────────────────────
        private void SellCar_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission("can_sell_car"))
            {
                MessageBox.Show("You don't have permission to sell cars.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var btn = sender as Button;
            if (btn == null) return;

            var row = FindParent<DataGridRow>(btn);
            if (row == null) return;

            var car = row.Item as Car;
            if (car == null) return;

            if (car.Stock <= 0)
            {
                MessageBox.Show(
                    $"{car.Brand} {car.Name} is already out of stock!",
                    "Cannot Sell", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saleWin = new SaleWindow(car);
            saleWin.Owner = this;
            saleWin.SaleCompleted += () => { LoadCars(); LoadStats(); };
            saleWin.ShowDialog();
        }

        // ── Delete Car ────────────────────────────────────
        private void DeleteCar_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission("can_delete_car"))
            {
                MessageBox.Show("You don't have permission to delete cars.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var btn = sender as Button;
            if (btn == null) return;

            int carId = 0;
            string carName = "";

            if (btn.Tag is int tagId)
            {
                carId = tagId;
                var allCars = CarsGrid.ItemsSource as List<Car>;
                var tagCar = allCars?.FirstOrDefault(c => c.Id == carId);
                carName = tagCar != null
                    ? $"{tagCar.Brand} {tagCar.Name}"
                    : $"Car #{carId}";
            }
            else
            {
                var row = FindParent<DataGridRow>(btn);
                if (row?.Item is Car rowCar)
                {
                    carId = rowCar.Id;
                    carName = $"{rowCar.Brand} {rowCar.Name}";
                }
            }

            if (carId == 0)
            {
                MessageBox.Show("Could not identify car.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Delete {carName}?\nThis cannot be undone.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool success = DatabaseHelper.DeleteCar(carId);
                if (success) { LoadCars(); LoadStats(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Delete Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Logout ────────────────────────────────────────
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.Clear();
            new LoginWindow().Show();
            Close();
        }

        // ── Helper ────────────────────────────────────────
        private static T? FindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T found) return found;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}