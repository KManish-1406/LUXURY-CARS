using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace luxury_cars.ViewModels
{
    /// <summary>
    /// Every ViewModel inherits this.
    /// SetProperty() → changes field → fires PropertyChanged → WPF binding updates UI.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Fire the event — [CallerMemberName] fills propertyName automatically
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Set field, fire event only if value actually changed
        protected bool SetProperty<T>(ref T field, T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}