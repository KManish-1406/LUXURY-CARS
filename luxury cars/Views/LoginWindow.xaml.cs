using System.Windows;
using System.Windows.Controls;

namespace luxury_cars.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // ── Language switcher ─────────────────────────────
        private void LanguagePicker_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            if (LanguagePicker.SelectedItem is not ComboBoxItem item) return;

            LanguageManager.Instance.CurrentLanguage = item.Name switch
            {
                "LangTelugu" => LanguageManager.Language.Telugu,
                _ => LanguageManager.Language.English
            };
        }

        // ── Login button ──────────────────────────────────
        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            Wiintest objjj=new Wiintest();
            objjj.ShowDialog();

            UsernameError.Visibility = Visibility.Collapsed;
            PasswordError.Visibility = Visibility.Collapsed;
            ErrorBox.Visibility = Visibility.Collapsed;

            string username = UsernameInput.Text.Trim();
            string password = PasswordInput.Password.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                UsernameError.Text = "Username is required.";
                UsernameError.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordError.Text = "Password is required.";
                PasswordError.Visibility = Visibility.Visible;
                return;
            }

            var result = DatabaseHelper.ValidateUserLogin(username, password);

            if (result == null)
            {
                ErrorText.Text = "Invalid username or password.";
                ErrorBox.Visibility = Visibility.Visible;
                return;
            }

            SessionManager.UserId = result.Value.userId;
            SessionManager.Username = username;
            SessionManager.RoleId = result.Value.roleId;
            SessionManager.RoleName = result.Value.roleName;
            SessionManager.Permissions = DatabaseHelper.GetRolePermissions(
                                            result.Value.roleId);

            var dashboard = new DashboardWindow(username);
            dashboard.Show();
            Close();
        }
    }
}