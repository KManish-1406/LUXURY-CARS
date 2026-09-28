using System;
using System.Windows;

namespace luxury_cars
{
    public class LanguageManager : DependencyObject
    {
        public static readonly LanguageManager Instance = new();

        public enum Language { English, Telugu }

        public static readonly DependencyProperty CurrentLanguageProperty =
            DependencyProperty.Register(
                "CurrentLanguage",
                typeof(Language),
                typeof(LanguageManager),
                new PropertyMetadata(
                    Language.English,
                    OnLanguageChanged
                )
            );

        public Language CurrentLanguage
        {
            get => (Language)GetValue(CurrentLanguageProperty);
            set => SetValue(CurrentLanguageProperty, value);
        }

        private static void OnLanguageChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lang = (Language)e.NewValue;

            string file = lang == Language.Telugu
                ? "Resources/Lang.te.xaml"
                : "Resources/Lang.en.xaml";

            var uri = new Uri(
                $"pack://application:,,,/luxury cars;component/{file}",
                UriKind.Absolute);

            var newDict = new ResourceDictionary { Source = uri };

            var merged = Application.Current.Resources.MergedDictionaries;

            for (int i = 0; i < merged.Count; i++)
            {
                var src = merged[i].Source?.ToString() ?? "";
                if (src.Contains("Lang.en.xaml") || src.Contains("Lang.te.xaml"))
                {
                    merged[i] = newDict;
                    return;
                }
            }

            merged.Add(newDict);
        }
    }
}