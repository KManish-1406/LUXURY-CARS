using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using luxury_cars.Models;
using Microsoft.Reporting.NETCore;

namespace luxury_cars.Views
{
    public partial class ReportWindow : Window
    {
        private string _currentFilter = "daily";
        private List<SaleReportRow> _currentData = new();

        public ReportWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadReport("daily");
        }

        // ── Filter button click ───────────────────────────
        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            _currentFilter = btn.Tag?.ToString() ?? "daily";
            LoadReport(_currentFilter);
        }

        // ── Load data ─────────────────────────────────────
        private void LoadReport(string filter)
        {
            _currentData = DatabaseHelper.GetSalesReport(filter);
            ReportGrid.ItemsSource = _currentData;

            var (totalSales, totalRevenue, totalDue) =
                DatabaseHelper.GetReportSummary(filter);

            TotalSalesCount.Text = totalSales.ToString();
            TotalRevenue.Text = $"₹{totalRevenue:N0}";
            TotalDue.Text = $"₹{totalDue:N0}";
        }

        // ── Build LocalReport from embedded RDLC ──────────
        private LocalReport BuildReport()
        {
            var report = new LocalReport();

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(
                "luxury_cars.SalesReport.rdlc");

            if (stream == null)
                throw new Exception(
                    "SalesReport.rdlc not found.\n" +
                    "Set Build Action = Embedded Resource.");

            report.LoadReportDefinition(stream);

            var dt = new System.Data.DataTable();
            dt.Columns.Add("SaleId", typeof(int));
            dt.Columns.Add("SaleDate", typeof(string));
            dt.Columns.Add("CustomerName", typeof(string));
            dt.Columns.Add("CustomerPhone", typeof(string));
            dt.Columns.Add("Brand", typeof(string));
            dt.Columns.Add("CarName", typeof(string));
            dt.Columns.Add("ModelNumber", typeof(string));
            dt.Columns.Add("Color", typeof(string));
            dt.Columns.Add("PaymentType", typeof(string));
            dt.Columns.Add("EmiMonths", typeof(int));
            dt.Columns.Add("SalePrice", typeof(decimal));
            dt.Columns.Add("AmountPaid", typeof(decimal));
            dt.Columns.Add("DueAmount", typeof(decimal));
            dt.Columns.Add("DeliveryDate", typeof(string));

            foreach (var r in _currentData)
                dt.Rows.Add(r.SaleId, r.SaleDate, r.CustomerName,
                    r.CustomerPhone, r.Brand, r.CarName,
                    r.ModelNumber, r.Color, r.PaymentType,
                    r.EmiMonths, r.SalePrice, r.AmountPaid,
                    r.DueAmount, r.DeliveryDate);

            report.DataSources.Add(new ReportDataSource("SalesData", dt));
            return report;
        }

        // ── Export PDF ────────────────────────────────────
        private void ExportPdfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export PDF",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                var report = BuildReport();
                byte[] bytes = report.Render("PDF");

                string folder = Environment.GetFolderPath(
                                    Environment.SpecialFolder.Desktop);
                string fileName = $"SalesReport_{_currentFilter}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);
                File.WriteAllBytes(filePath, bytes);

                Process.Start(new ProcessStartInfo(filePath)
                { UseShellExecute = true });

                MessageBox.Show($"PDF saved to Desktop:\n{fileName}",
                    "Export Complete", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF export failed:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Export Excel ──────────────────────────────────
        private void ExportExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export Excel",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Sales Report");

                // Title
                ws.Cell(1, 1).Value = "Car Inventory System — Sales Report";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Font.FontColor =
                    XLColor.FromArgb(194, 24, 91);
                ws.Range(1, 1, 1, 13).Merge();

                ws.Cell(2, 1).Value = _currentFilter switch
                {
                    "daily" => $"Today — {DateTime.Now:dd MMM yyyy}",
                    "weekly" => $"This Week — {DateTime.Now:dd MMM yyyy}",
                    "yearly" => $"This Year — {DateTime.Now.Year}",
                    _ => ""
                };
                ws.Range(2, 1, 2, 13).Merge();

                // Summary
                var (ts, tr, td) = DatabaseHelper.GetReportSummary(_currentFilter);
                ws.Cell(3, 1).Value = $"Total Sales: {ts}";
                ws.Cell(3, 5).Value = $"Total Revenue: ₹{tr:N0}";
                ws.Cell(3, 5).Style.Font.FontColor = XLColor.Green;
                ws.Cell(3, 9).Value = $"Total Due: ₹{td:N0}";
                ws.Cell(3, 9).Style.Font.FontColor = XLColor.Red;

                // Headers
                string[] headers = { "Sale ID", "Date", "Customer", "Phone",
                                     "Brand", "Model", "Color", "Payment",
                                     "EMI", "Sale Price", "Amount Paid",
                                     "Due", "Delivery" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(5, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor =
                        XLColor.FromArgb(233, 30, 140);
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;
                }

                // Data rows
                int rowNum = 6;
                foreach (var row in _currentData)
                {
                    ws.Cell(rowNum, 1).Value = row.SaleIdDisplay;
                    ws.Cell(rowNum, 2).Value = row.SaleDate;
                    ws.Cell(rowNum, 3).Value = row.CustomerName;
                    ws.Cell(rowNum, 4).Value = row.CustomerPhone;
                    ws.Cell(rowNum, 5).Value = row.Brand;
                    ws.Cell(rowNum, 6).Value = row.ModelNumber;
                    ws.Cell(rowNum, 7).Value = row.Color;
                    ws.Cell(rowNum, 8).Value = row.PaymentType;
                    ws.Cell(rowNum, 9).Value = row.EmiMonths > 0
                                                    ? $"{row.EmiMonths} months" : "-";
                    ws.Cell(rowNum, 10).Value = row.SalePriceDisplay;
                    ws.Cell(rowNum, 11).Value = row.AmountPaidDisplay;
                    ws.Cell(rowNum, 12).Value = row.DueAmountDisplay;
                    ws.Cell(rowNum, 13).Value = row.DeliveryDate;

                    if (rowNum % 2 == 0)
                        ws.Range(rowNum, 1, rowNum, 13)
                          .Style.Fill.BackgroundColor =
                              XLColor.FromArgb(255, 240, 245);
                    rowNum++;
                }

                ws.Columns().AdjustToContents();

                string folder = Environment.GetFolderPath(
                                    Environment.SpecialFolder.Desktop);
                string fileName = $"SalesReport_{_currentFilter}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);
                wb.SaveAs(filePath);

                Process.Start(new ProcessStartInfo(filePath)
                { UseShellExecute = true });

                MessageBox.Show($"Excel saved to Desktop:\n{fileName}",
                    "Export Complete", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel export failed:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}