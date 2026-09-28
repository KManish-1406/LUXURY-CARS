using System.Windows;
using PdfSharp.Fonts;

namespace luxury_cars
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Register font resolver for PdfSharp
            GlobalFontSettings.FontResolver = new FontResolver();
        }
    }
}