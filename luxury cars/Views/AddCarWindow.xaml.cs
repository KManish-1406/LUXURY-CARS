using System;
using System.Windows;
using luxury_cars.ViewModels;

namespace luxury_cars.Views
{
    public partial class AddCarWindow : Window
    {
        public event Action? CarAdded;

        private readonly AddCarViewModel _vm;

        public AddCarWindow()
        {
            InitializeComponent();

            _vm = new AddCarViewModel();
            DataContext = _vm;

            _vm.CarAdded += () =>
            {
                CarAdded?.Invoke();
                Close();
            };

            _vm.CancelRequested += Close;
        }
    }
}