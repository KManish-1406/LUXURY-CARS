using System;
using System.Windows;
using System.Windows.Controls;

namespace luxury_cars.Views
{
    public partial class AddUserWindow : Window
    {
        public event Action? UserAdded;

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            string username = UsernameInput.Text.Trim();
            string password = PasswordInput.Password.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                ErrorText.Text = "Username is required.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Password is required.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            // Get selected role id
            int roleId = RoleCombo.SelectedItem is ComboBoxItem item
                ? item.Content.ToString() switch
                {
                    "Admin" => 1,
                    "Manager" => 2,
                    "Salesperson" => 3,
                    "Viewer" => 4,
                    _ => 2
                }
                : 2;

            var (success, message, userId) = DatabaseHelper.AddUser(username, password, roleId);

            if (!success)
            {
                ErrorText.Text = message;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            // ── Check if trigger fired ──────────────────────
            var (triggered, trigUsername, triggeredAt) =
                DatabaseHelper.CheckUserTrigger(userId);

            if (triggered)
            {
                MessageBox.Show(
                    $"✅ Trigger Fired Successfully!\n\n" +
                    $"Trigger Name  : after_user_insert\n" +
                    $"Table         : users\n" +
                    $"Action        : NEW USER ADDED\n" +
                    $"Username      : {trigUsername}\n" +
                    $"Triggered At  : {triggeredAt:dd MMM yyyy HH:mm:ss}\n\n" +
                    $"Audit log updated in user_audit_log table.",
                    "🔔 Trigger Successfully Triggered!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"User '{username}' added successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            UserAdded?.Invoke();
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}