using System;
using System.Diagnostics;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using luxury_cars.Models;

namespace luxury_cars
{
    public class PdfGenerator
    {
        public static string GenerateSaleReport(SaleRecord sale, int saleId)
        {
          

            var doc = new PdfDocument();
            doc.Info.Title = $"Sale Receipt #{saleId}";

            var page = doc.AddPage();
            page.Width = XUnit.FromMillimeter(210);
            page.Height = XUnit.FromMillimeter(297);

            var gfx = XGraphics.FromPdfPage(page);

            // Fonts
            var fontTitle = new XFont("Segoe UI", 20, XFontStyleEx.Bold);
            var fontBold = new XFont("Segoe UI", 12, XFontStyleEx.Bold);
            var fontNormal = new XFont("Segoe UI", 11, XFontStyleEx.Regular);
            var fontSmall = new XFont("Segoe UI", 9, XFontStyleEx.Regular);
            var fontSub = new XFont("Segoe UI", 12, XFontStyleEx.Bold);

            // Colors
            var primaryColor = XColor.FromArgb(194, 24, 91);
            var darkColor = XColor.FromArgb(0, 0, 0);
            var grayColor = XColor.FromArgb(100, 116, 139);
            var greenColor = XColor.FromArgb(34, 197, 94);
            var redColor = XColor.FromArgb(239, 68, 68);
            var lightBg = XColor.FromArgb(255, 240, 245);

            double margin = 40;
            double y = 40;
            double w = page.Width.Point - margin * 2;

            // ── Header ───────────────────────────────────
            gfx.DrawRectangle(new XSolidBrush(primaryColor),
                              margin, y, w, 64);
            gfx.DrawString("Car Inventory System",
                           fontTitle, XBrushes.White,
                           new XRect(margin + 12, y, w, 36),
                           XStringFormats.CenterLeft);
            gfx.DrawString($"Sale Receipt  #SR-{saleId:D4}",
                           fontNormal, XBrushes.White,
                           new XRect(margin + 12, y + 36, w, 28),
                           XStringFormats.CenterLeft);
            gfx.DrawString($"Date: {DateTime.Now:dd MMM yyyy}",
                           fontNormal, XBrushes.White,
                           new XRect(margin, y, w - 12, 64),
                           XStringFormats.CenterRight);
            y += 80;

            // ── Car Details ───────────────────────────────
            DrawSection(gfx, fontSub, primaryColor, lightBg,
                        margin, y, w, "CAR DETAILS");
            y += 32;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Model Number", sale.ModelNumber);
            y += 22;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Brand & Name",
                    $"{sale.Brand} {sale.CarName} ({sale.Year})");
            y += 22;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Color", sale.Color);
            y += 30;

            // ── Customer Details ──────────────────────────
            DrawSection(gfx, fontSub, primaryColor, lightBg,
                        margin, y, w, "CUSTOMER DETAILS");
            y += 32;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Full Name", sale.CustomerName);
            y += 22;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Phone", sale.CustomerPhone);
            y += 22;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Email", sale.CustomerEmail);
            y += 22;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Address", sale.CustomerAddress);
            y += 22;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "ID Proof", sale.IdProof);
            y += 30;

            // ── Payment Details ───────────────────────────
            DrawSection(gfx, fontSub, primaryColor, lightBg,
                        margin, y, w, "PAYMENT DETAILS");
            y += 32;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Car Price", sale.PriceDisplay);
            y += 22;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Amount Paid", sale.PaidDisplay);
            y += 22;

            var dueColor = sale.DueAmount > 0 ? redColor : greenColor;
            DrawRow(gfx, fontNormal, fontBold, darkColor, dueColor,
                    margin, y, w, "Due Amount",
                    sale.DueAmount > 0 ? sale.DueDisplay : "FULLY PAID");
            y += 22;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Payment Type", sale.PaymentType);
            y += 22;

            if (sale.PaymentType == "EMI" && sale.EmiMonths > 0)
            {
                DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                        margin, y, w, "EMI Plan", sale.EmiDisplay);
                y += 22;
            }
            y += 30;

            // ── Delivery Details ──────────────────────────
            DrawSection(gfx, fontSub, primaryColor, lightBg,
                        margin, y, w, "DELIVERY DETAILS");
            y += 32;
            DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                    margin, y, w, "Delivery Date",
                    sale.DeliveryDate.ToString("dd MMM yyyy"));
            y += 22;
            if (!string.IsNullOrWhiteSpace(sale.Notes))
            {
                DrawRow(gfx, fontNormal, fontBold, darkColor, grayColor,
                        margin, y, w, "Notes", sale.Notes);
                y += 22;
            }
            y += 24;

            // ── Footer ────────────────────────────────────
            gfx.DrawRectangle(new XSolidBrush(primaryColor),
                              margin, y, w, 2);
            y += 12;
            gfx.DrawString(
                "Thank you for your purchase! " +
                "This is a computer generated receipt.",
                fontSmall, new XSolidBrush(grayColor),
                new XRect(margin, y, w, 20),
                XStringFormats.Center);

            // ── Save to Desktop ───────────────────────────
            string folder = Environment.GetFolderPath(
                              Environment.SpecialFolder.Desktop);
            string fileName = $"SaleReceipt_SR{saleId:D4}_" +
                              $"{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(folder, fileName);
            doc.Save(filePath);

            // Open PDF automatically
            Process.Start(new ProcessStartInfo(filePath)
            { UseShellExecute = true });

            return filePath;
        }

        private static void DrawSection(
            XGraphics gfx, XFont font,
            XColor primary, XColor bg,
            double x, double y, double w, string title)
        {
            gfx.DrawRectangle(new XSolidBrush(bg), x, y, w, 26);
            gfx.DrawString(title, font,
                           new XSolidBrush(primary),
                           new XRect(x + 10, y, w, 26),
                           XStringFormats.CenterLeft);
        }

        private static void DrawRow(
            XGraphics gfx,
            XFont labelFont, XFont valueFont,
            XColor labelColor, XColor valueColor,
            double x, double y, double w,
            string label, string value)
        {
            gfx.DrawString(label + ":",
                           labelFont,
                           new XSolidBrush(labelColor),
                           new XRect(x + 10, y, w * 0.35, 20),
                           XStringFormats.CenterLeft);
            gfx.DrawString(value,
                           valueFont,
                           new XSolidBrush(valueColor),
                           new XRect(x + 10 + w * 0.35, y, w * 0.65, 20),
                           XStringFormats.CenterLeft);
        }
    }
}