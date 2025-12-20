using System;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class UserDialog : Window
    {
        public string Username { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Role { get; private set; } = "User";
        public bool IsActive { get; private set; } = true;

        public UserDialog()
        {
            InitializeComponent();
        }

        public UserDialog(User user) : this()
        {
            Title = "Редактировать пользователя";
            UsernameTextBox.Text = user.Username;
            EmailTextBox.Text = user.Email;
            IsActiveCheckBox.IsChecked = user.IsActive;

            // Устанавливаем роль
            foreach (var item in RoleComboBox.Items)
            {
                if (item is System.Windows.Controls.ComboBoxItem comboBoxItem &&
                    comboBoxItem.Content.ToString() == user.Role)
                {
                    RoleComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Введите имя пользователя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return;
            }

            Username = UsernameTextBox.Text;
            Password = PasswordBox.Password;
            Email = EmailTextBox.Text;
            Role = (RoleComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "User";
            IsActive = IsActiveCheckBox.IsChecked ?? false;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}