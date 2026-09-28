using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using luxury_cars.Models;

namespace luxury_cars.Views
{
    public partial class SaleWindow : Window
    {
        private readonly Car _car;
        public event Action? SaleCompleted;

        public SaleWindow(Car car)
        {
            InitializeComponent();
            _car = car;
            LoadCarDetails();
            DeliveryDate.SelectedDate = DateTime.Today;
            SalePrice.Text = car.Price.ToString("0");
        }

        // ── Helper: read from active language dictionary ──────────────
        private static string T(string key)
            => Application.Current.Resources[key] as string ?? key;

        // ── Load car info into top card ───────────────────────────────
        private void LoadCarDetails()
        {
            CarModel.Text = _car.ModelNumber;
            CarBrandName.Text = $"{_car.Brand} {_car.Name} ({_car.Year})";
            CarPrice.Text = _car.PriceDisplay;
        }

        // ── Recalculate due when price or paid changes ────────────────
        private void SalePrice_Changed(object sender, TextChangedEventArgs e)
            => RecalcDue();

        private void AmountPaid_Changed(object sender, TextChangedEventArgs e)
            => RecalcDue();

        private void RecalcDue()
        {
            decimal.TryParse(SalePrice.Text, out decimal price);
            decimal.TryParse(AmountPaid.Text, out decimal paid);
            decimal due = price - paid;
            DueDisplay.Text = due >= 0 ? $"₹{due:N0}" : "₹0";
            RecalcEmi();
        }

        // ── Show/hide EMI panel ───────────────────────────────────────
        private void PaymentType_Changed(object sender, RoutedEventArgs e)
        {
            if (EmiPanel == null) return;
            EmiPanel.Visibility = RadioEMI.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // ── Recalculate EMI per month ─────────────────────────────────
        private void EmiMonths_Changed(object sender, TextChangedEventArgs e)
            => RecalcEmi();

        private void RecalcEmi()
        {
            if (EmiDisplay == null) return;
            decimal.TryParse(SalePrice.Text, out decimal price);
            decimal.TryParse(AmountPaid.Text, out decimal paid);
            int.TryParse(EmiMonths.Text, out int months);

            decimal due = price - paid;
            EmiDisplay.Text = months > 0 && due > 0
                ? $"₹{due / months:N0} / month for {months} months"
                : "";
        }

        // ── Confirm sale ──────────────────────────────────────────────
        private void ConfirmSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            // All validation messages come from active language dictionary
            if (string.IsNullOrWhiteSpace(CustomerName.Text) ||
                string.IsNullOrWhiteSpace(CustomerPhone.Text))
            {
                ShowError(T("Sale_Error_NamePhone"));
                return;
            }

            if (!decimal.TryParse(SalePrice.Text, out decimal salePrice) || salePrice <= 0)
            {
                ShowError(T("Sale_Error_Price"));
                return;
            }

            if (!decimal.TryParse(AmountPaid.Text, out decimal amountPaid) || amountPaid < 0)
            {
                ShowError(T("Sale_Error_AmountPaid"));
                return;
            }

            if (amountPaid > salePrice)
            {
                ShowError(T("Sale_Error_Overpaid"));
                return;
            }

            if (DeliveryDate.SelectedDate == null)
            {
                ShowError(T("Sale_Error_DeliveryDate"));
                return;
            }

            string paymentType = RadioCash.IsChecked == true ? "Cash"
                               : RadioOnline.IsChecked == true ? "Online"
                               : "EMI";

            int emiMonths = 0;
            if (paymentType == "EMI")
            {
                if (!int.TryParse(EmiMonths.Text, out emiMonths) || emiMonths <= 0)
                {
                    ShowError(T("Sale_Error_EmiMonths"));
                    return;
                }
            }

            var sale = new SaleRecord
            {
                CarId = _car.Id,
                ModelNumber = _car.ModelNumber,
                Brand = _car.Brand,
                CarName = _car.Name,
                Year = _car.Year,
                Color = _car.Color,
                CustomerName = CustomerName.Text.Trim(),
                CustomerPhone = CustomerPhone.Text.Trim(),
                CustomerEmail = CustomerEmail.Text.Trim(),
                CustomerAddress = CustomerAddress.Text.Trim(),
                IdProof = CustomerIdProof.Text.Trim(),
                SalePrice = salePrice,
                AmountPaid = amountPaid,
                PaymentType = paymentType,
                EmiMonths = emiMonths,
                DeliveryDate = DeliveryDate.SelectedDate!.Value,
                Notes = Notes.Text.Trim()
            };

            var (success, saleId, message) = DatabaseHelper.ProcessSale(sale);

            if (!success)
            {
                ShowError(message);
                return;
            }

            try
            {
                string pdfPath = PdfGenerator.GenerateSaleReport(sale, saleId);

                MessageBox.Show(
                    $"✓ Sale completed successfully!\n\n" +
                    $"Sale ID: #{saleId}\n" +
                    $"PDF saved to Desktop:\n{System.IO.Path.GetFileName(pdfPath)}",
                    T("Sale_Success_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Sale saved but PDF failed: {ex.Message}",
                    T("Sale_PdfError_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            SaleCompleted?.Invoke();
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
            => Close();

        private void ShowError(string msg)
        {
            ErrorText.Text = msg;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}