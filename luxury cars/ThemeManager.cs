using System;
using System.Windows;

namespace luxury_cars
{
    /// <summary>
    /// THEME MANAGER — exactly mirrors LanguageManager architecture.
    ///
    /// Uses a Dependency Property to track the current theme.
    /// When CurrentTheme changes → OnThemeChanged callback fires →
    /// swaps the theme ResourceDictionary in App.Current.Resources →
    /// every DynamicResource binding in every open window updates instantly.
    /// </summary>
    public class ThemeManager : DependencyObject
    {
        // ── Singleton ─────────────────────────────────────────────────
        public static readonly ThemeManager Instance = new();

        // ── Theme enum — one entry per theme file ─────────────────────
        public enum AppTheme
        {
            Classic,
            LunaBlue,
            LunaOlive,
            LunaSilver,
            Royale,
            Aero,
            AeroLite,
            Aero2,
            Generic
        }

        // ── DEPENDENCY PROPERTY ───────────────────────────────────────
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(
                "CurrentTheme",
                typeof(AppTheme),
                typeof(ThemeManager),
                new PropertyMetadata(AppTheme.Aero2, OnThemeChanged)
            );

        // ── CLR wrapper ───────────────────────────────────────────────
        public AppTheme CurrentTheme
        {
            get => (AppTheme)GetValue(CurrentThemeProperty);
            set => SetValue(CurrentThemeProperty, value);
        }

        // ── Callback: fires every time CurrentTheme is set ────────────
        private static void OnThemeChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var theme = (AppTheme)e.NewValue;

            // Map enum to resource file name
            string file = theme switch
            {
                AppTheme.Classic => "Theme.Classic.xaml",
                AppTheme.LunaBlue => "Theme.Luna.Blue.xaml",
                AppTheme.LunaOlive => "Theme.Luna.Olive.xaml",
                AppTheme.LunaSilver => "Theme.Luna.Silver.xaml",
                AppTheme.Royale => "Theme.Royale.xaml",
                AppTheme.Aero => "Theme.Aero.xaml",
                AppTheme.AeroLite => "Theme.AeroLite.xaml",
                AppTheme.Aero2 => "Theme.Aero2.xaml",
                AppTheme.Generic => "Theme.Generic.xaml",
                _ => "Theme.Aero2.xaml"
            };

            // Build pack URI — matches your assembly name
            var uri = new Uri(
                $"pack://application:,,,/luxury cars;component/Resources/Themes/{file}",
                UriKind.Absolute);

            var newDict = new ResourceDictionary { Source = uri };

            // ── Hot-swap in App.Current.Resources ────────────────────
            // Find existing theme dict and replace it.
            // All DynamicResource bindings across ALL open windows
            // refresh automatically on this single line.
            var merged = Application.Current.Resources.MergedDictionaries;

            for (int i = 0; i < merged.Count; i++)
            {
                var src = merged[i].Source?.ToString() ?? "";
                if (src.Contains("/Resources/Themes/"))
                {
                    merged[i] = newDict;
                    return;
                }
            }

            // First time — just add it
            merged.Add(newDict);
        }
    }
}