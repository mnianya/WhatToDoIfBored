using Npgsql;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Whattodoifbored
{
    /// <summary>
    /// Окно авторизации пользователя
    /// </summary>
    public partial class Autorization : Window
    {
        public Autorization()
        {
            InitializeComponent();
        }

        private void loginbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9._]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void loginbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            loginPlaceholder.Visibility = string.IsNullOrWhiteSpace(loginbox.Text) ? Visibility.Visible : Visibility.Collapsed;

            if (loginbox.Text.Length > 20)
            {
                int caretIndex = loginbox.CaretIndex;
                loginbox.Text = loginbox.Text.Substring(0, 20);
                loginbox.CaretIndex = caretIndex > 20 ? 20 : caretIndex;
            }
        }

        private void emailbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9@._-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void emailbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            emailPlaceholder.Visibility = string.IsNullOrWhiteSpace(emailbox.Text) ? Visibility.Visible : Visibility.Collapsed;

            if (emailbox.Text.Length > 30)
            {
                int caretIndex = emailbox.CaretIndex;
                emailbox.Text = emailbox.Text.Substring(0, 30);
                emailbox.CaretIndex = caretIndex > 30 ? 30 : caretIndex;
            }
        }

        private bool isPasswordVisible = false;
        private bool isSyncing = false; // флаг синхронизации полей

        // Синхронизация между скрытым и видимым паролем
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isSyncing) return;

            if (passwordBox.Password.Length > 20)
            {
                passwordBox.Password = passwordBox.Password.Substring(0, 20);
            }

            passwordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (isPasswordVisible)
            {
                isSyncing = true;
                visiblePasswordBox.Text = passwordBox.Password;
                visiblePasswordBox.CaretIndex = visiblePasswordBox.Text.Length;
                isSyncing = false;
            }
        }

        // Синхронизация из видимого TextBox обратно в PasswordBox
        private void VisiblePasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isSyncing) return;

            if (visiblePasswordBox.Text.Length > 20)
            {
                visiblePasswordBox.Text = visiblePasswordBox.Text.Substring(0, 20);
                visiblePasswordBox.CaretIndex = visiblePasswordBox.Text.Length;
                return;
            }

            isSyncing = true;
            passwordBox.Password = visiblePasswordBox.Text;
            isSyncing = false;
        }

        // Переключение отображения пароля
        private void TogglePasswordVisibilityBtn_Click(object sender, RoutedEventArgs e)
        {
            isSyncing = true;

            if (isPasswordVisible)
            {
                passwordBox.Password = visiblePasswordBox.Text;
                visiblePasswordBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                eyeIcon.Source = new BitmapImage(new Uri("путь_к_иконке_глаз.png"));
                isPasswordVisible = false;
            }
            else
            {
                visiblePasswordBox.Text = passwordBox.Password;
                visiblePasswordBox.CaretIndex = visiblePasswordBox.Text.Length;
                visiblePasswordBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                eyeIcon.Source = new BitmapImage(new Uri("путь_к_иконке_закрытого_глаза.png"));
                isPasswordVisible = true;
            }

            isSyncing = false;
        }

        // Обработка кнопки входа
        private void autorizbutton_Click(object sender, RoutedEventArgs e)
        {
            string username = loginbox.Text.Trim();
            string email = emailbox.Text.Trim();
            string password = passwordBox.Password;

            bool isValid = true;

            // Сброс цвета рамок
            loginbox1.BorderBrush = Brushes.Gray;
            emailBoxBorder.BorderBrush = Brushes.Gray;
            passwordBoxBorder.BorderBrush = Brushes.Gray;

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
            {
                loginbox1.BorderBrush = Brushes.Red;
                emailBoxBorder.BorderBrush = Brushes.Red;
                passwordBoxBorder.BorderBrush = Brushes.Red;
                MessageBox.Show("Please fill in all fields.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Отдельные проверки
            if (string.IsNullOrWhiteSpace(username))
            {
                loginbox1.BorderBrush = Brushes.Red;
                MessageBox.Show("Please enter your username.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                emailBoxBorder.BorderBrush = Brushes.Red;
                MessageBox.Show("Please enter your email.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                passwordBoxBorder.BorderBrush = Brushes.Red;
                MessageBox.Show("Please enter your password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (!isValid) return;

            // Подключение к базе данных PostgreSQL
            string connectionString = "";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Проверка наличия пользователя
                string checkUserQuery = "SELECT email, password FROM users WHERE username = @username";
                using (var command = new NpgsqlCommand(checkUserQuery, connection))
                {
                    command.Parameters.AddWithValue("username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            loginbox1.BorderBrush = Brushes.Red;
                            MessageBox.Show("User with this username was not found.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string storedEmail = reader.GetString(0);
                        string storedPassword = reader.GetString(1);

                        bool correctEmail = storedEmail == email;
                        bool correctPassword = storedPassword == password;

                        if (!correctEmail)
                        {
                            emailBoxBorder.BorderBrush = Brushes.Red;
                            MessageBox.Show("Email does not match.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (!correctPassword)
                        {
                            passwordBoxBorder.BorderBrush = Brushes.Red;
                            MessageBox.Show("Incorrect password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Успешный вход
                        MessageBox.Show("Welcome!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        Buttonwindow but = new Buttonwindow();
                        but.Show();
                        this.Close();

                        // Сохранение данных сессии
                        UserSession.Username = username;
                        UserSession.Email = email;
                    }
                }
            }
        }

        // Переход к регистрации
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Registration regis = new Registration();
            regis.Show();
            this.Close();
        }
    }
}