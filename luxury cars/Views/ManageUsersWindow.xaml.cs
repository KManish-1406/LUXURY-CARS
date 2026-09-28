using System;
using System.Windows;
using System.Windows.Controls;

namespace luxury_cars.Views
{
    public class UserRow
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string RoleName { get; set; } = "";
        public int RoleId { get; set; }
    }

    public partial class ManageUsersWindow : Window
    {
        private bool _loading = false;

        public ManageUsersWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        // ── Load users into grid ──────────────────────────
        private void LoadUsers()
        {
            _loading = true;
            var rawList = DatabaseHelper.GetAllUsers();
            var list = new System.Collections.Generic.List<UserRow>();
            foreach (var u in rawList)
                list.Add(new UserRow
                {
                    Id = u.id,
                    Username = u.username,
                    RoleName = u.roleName,
                    RoleId = u.roleId
                });
            UsersGrid.ItemsSource = list;
            _loading = false;
        }

        // ── Add user button ───────────────────────────────
        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var addWin = new AddUserWindow();
            addWin.Owner = this;
            addWin.UserAdded += () => LoadUsers();
            addWin.ShowDialog();
        }

        // ── Role combo changed ────────────────────────────
        private void RoleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            if (sender is not ComboBox combo) return;
            if (combo.Tag is not int userId) return;
            if (combo.SelectedItem is not ComboBoxItem item) return;

            // Map role name to role id
            int roleId = item.Content.ToString() switch
            {
                "Admin" => 1,
                "Manager" => 2,
                "Salesperson" => 3,
                "Viewer" => 4,
                _ => 1
            };

            // Prevent changing own role
            if (userId == SessionManager.UserId)
            {
                MessageBox.Show("You cannot change your own role.",
                    "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadUsers();
                return;
            }

            bool success = DatabaseHelper.UpdateUserRole(userId, roleId);
            StatusText.Text = success
                ? $"✓ Role updated successfully."
                : "✗ Failed to update role.";

            LoadUsers();
        }

        // ── Delete user ───────────────────────────────────
        private void DeleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not int userId) return;

            // Prevent deleting own account
            if (userId == SessionManager.UserId)
            {
                MessageBox.Show("You cannot delete your own account.",
                    "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "Delete this user? This cannot be undone.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            var (success, message) = DatabaseHelper.DeleteUser(userId);
            StatusText.Text = success ? $"✓ {message}" : $"✗ {message}";
            LoadUsers();
        }
    }
}