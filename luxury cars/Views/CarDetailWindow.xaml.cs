using System;
using System.Windows;
using luxury_cars.Models;
using luxury_cars.ViewModels;

namespace luxury_cars.Views
{
    public partial class CarDetailWindow : Window
    {
        private readonly CarDetailViewModel _vm;

        // Dashboard subscribes to this to open SaleWindow
        public event Action<Car>? SellRequested;

        public CarDetailWindow(Car car)
        {
            InitializeComponent();
            _vm = new CarDetailViewModel(car);
            DataContext = _vm;
        }

        private void SellBtn_Click(object sender, RoutedEventArgs e)
        {
            SellRequested?.Invoke(_vm.Car);
            Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}