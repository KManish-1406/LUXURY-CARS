using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using luxury_cars.Models;

namespace luxury_cars.ViewModels
{
    public class AddCarViewModel : BaseViewModel
    {
        // ── Form fields ───────────────────────────────────────────────
        private string _modelNumber = "";
        public string ModelNumber
        {
            get => _modelNumber;
            set => SetProperty(ref _modelNumber, value);
        }

        private string _brand = "";
        public string Brand
        {
            get => _brand;
            set => SetProperty(ref _brand, value);
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _yearText = "";
        public string YearText
        {
            get => _yearText;
            set => SetProperty(ref _yearText, value);
        }

        private string _color = "";
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        private string _priceText = "";
        public string PriceText
        {
            get => _priceText;
            set => SetProperty(ref _priceText, value);
        }

        private string _stockText = "";
        public string StockText
        {
            get => _stockText;
            set => SetProperty(ref _stockText, value);
        }

        // ── Image properties ──────────────────────────────────────────

        // Full absolute path shown in the preview Image control
        private string _selectedImageFullPath = "";
        public string SelectedImageFullPath
        {
            get => _selectedImageFullPath;
            set
            {
                SetProperty(ref _selectedImageFullPath, value);
                OnPropertyChanged(nameof(HasSelectedImage));
            }
        }

        // True when user has picked an image
        public bool HasSelectedImage => !string.IsNullOrWhiteSpace(SelectedImageFullPath);

        // The relative path saved to DB (e.g. "CarImages\ferrari.jpg")
        private string? _savedRelativePath;

        // ── Error message ─────────────────────────────────────────────
        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        // ── Events ────────────────────────────────────────────────────
        public event Action? CarAdded;
        public event Action? CancelRequested;

        // ── Commands ──────────────────────────────────────────────────
        public RelayCommand AddCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand SelectImageCommand { get; }
        public RelayCommand RemoveImageCommand { get; }

        public AddCarViewModel()
        {
            AddCommand = new RelayCommand(_ => TryAddCar());
            CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke());
            SelectImageCommand = new RelayCommand(_ => SelectImage());
            RemoveImageCommand = new RelayCommand(_ => RemoveImage(),
                                                  _ => HasSelectedImage);
        }

        // ── Language helper ───────────────────────────────────────────
        private static string T(string key)
            => Application.Current.Resources[key] as string ?? key;

        // ── Image selection — opens file dialog ───────────────────────
        private void SelectImage()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select Car Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.webp",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (dlg.ShowDialog() != true) return;

            // Show preview immediately — full path for binding
            SelectedImageFullPath = dlg.FileName;

            // Store the source path — will be copied to CarImages/ on save
            _savedRelativePath = dlg.FileName; // temp, replaced in TryAddCar
        }

        private void RemoveImage()
        {
            SelectedImageFullPath = "";
            _savedRelativePath = null;
        }

        // ── Add logic ─────────────────────────────────────────────────
        private void TryAddCar()
        {
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(ModelNumber) ||
                string.IsNullOrWhiteSpace(Brand) ||
                string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(YearText) ||
                string.IsNullOrWhiteSpace(PriceText) ||
                string.IsNullOrWhiteSpace(StockText))
            {
                ErrorMessage = T("AddCar_Error_Required");
                return;
            }

            if (!int.TryParse(YearText, out int year) || year < 1900 || year > 2100)
            {
                ErrorMessage = T("AddCar_Error_Year");
                return;
            }

            if (!decimal.TryParse(PriceText, out decimal price) || price <= 0)
            {
                ErrorMessage = T("AddCar_Error_Price");
                return;
            }

            if (!int.TryParse(StockText, out int stock) || stock < 0)
            {
                ErrorMessage = T("AddCar_Error_Stock");
                return;
            }

            // ── Copy image to CarImages folder ────────────────────────
            string? relativePath = null;
            if (!string.IsNullOrWhiteSpace(_savedRelativePath) &&
                File.Exists(_savedRelativePath))
            {
                try
                {
                    string folder = DatabaseHelper.EnsureCarImagesFolder();
                    string ext = Path.GetExtension(_savedRelativePath);
                    // Filename = ModelNumber_Brand_Name.ext  (safe for filesystem)
                    string safeName = $"{ModelNumber.Trim()}_{Brand.Trim()}_{Name.Trim()}{ext}"
                                        .Replace(" ", "_")
                                        .Replace("/", "_");
                    string destPath = Path.Combine(folder, safeName);

                    File.Copy(_savedRelativePath, destPath, overwrite: true);

                    // Store relative path in DB — portable across machines
                    relativePath = Path.Combine("CarImages", safeName);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Image save failed: {ex.Message}";
                    return;
                }
            }

            var car = new Car
            {
                ModelNumber = ModelNumber.Trim().ToUpper(),
                Brand = Brand.Trim(),
                Name = Name.Trim(),
                Year = year,
                Color = Color.Trim(),
                Price = price,
                Stock = stock,
                ImagePath = relativePath   // null if no image selected
            };

            try
            {
                bool success = DatabaseHelper.AddCar(car);
                if (success)
                {
                    MessageBox.Show(
                        $"✓ {car.Brand} {car.Name} added successfully!",
                        T("AddCar_Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    CarAdded?.Invoke();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}