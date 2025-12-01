using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

namespace Whattodoifbored
{
    /// <summary>
    /// Окно регистрации
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
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
            // Показываем или скрываем плейсхолдер
            loginPlaceholder.Visibility = string.IsNullOrWhiteSpace(loginbox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Ограничиваем длину логина
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
            emailPlaceholder.Visibility = string.IsNullOrWhiteSpace(emailbox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (emailbox.Text.Length > 30)
            {
                int caretIndex = emailbox.CaretIndex;
                emailbox.Text = emailbox.Text.Substring(0, 30);
                emailbox.CaretIndex = caretIndex > 30 ? 30 : caretIndex;
            }
        }

        private bool isPasswordVisible = false;
        private bool isSyncing = false; // Флаг синхронизации полей

        // Обработка изменения пароля в скрытом поле
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isSyncing) return;

            // Ограничение длины пароля
            if (passwordBox.Password.Length > 20)
            {
                passwordBox.Password = passwordBox.Password.Substring(0, 20);
            }

            passwordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Синхронизация со вторым полем (видимым)
            if (isPasswordVisible)
            {
                isSyncing = true;
                visiblePasswordBox.Text = passwordBox.Password;
                visiblePasswordBox.CaretIndex = visiblePasswordBox.Text.Length;
                isSyncing = false;
            }
        }

        // Обработка изменения текста в видимом поле пароля
        private void VisiblePasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isSyncing) return;

            // Ограничение длины
            if (visiblePasswordBox.Text.Length > 20)
            {
                visiblePasswordBox.Text = visiblePasswordBox.Text.Substring(0, 20);
                visiblePasswordBox.CaretIndex = visiblePasswordBox.Text.Length;
                return;
            }

            // Синхронизация со скрытым полем
            isSyncing = true;
            passwordBox.Password = visiblePasswordBox.Text;
            isSyncing = false;
        }

        private void TogglePasswordVisibilityBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isPasswordVisible)
            {
                // Показываем скрытое поле
                isSyncing = true;
                passwordBox.Password = visiblePasswordBox.Text;
                visiblePasswordBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                eyeIcon.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\eye.png"));
                isPasswordVisible = false;
                isSyncing = false;
            }
            else
            {
                // Показываем видимое поле
                isSyncing = true;
                visiblePasswordBox.Text = passwordBox.Password;
                visiblePasswordBox.CaretIndex = visiblePasswordBox.Text.Length;
                visiblePasswordBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                eyeIcon.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\close-eye.png"));
                isPasswordVisible = true;
                isSyncing = false;
            }
        }

        private void registerbutton_Click(object sender, RoutedEventArgs e)
        {
            string username = loginbox.Text.Trim();
            string email = emailbox.Text.Trim();
            string password = passwordBox.Password;

            bool isValid = true;
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            Regex emailRegex = new Regex(emailPattern);

            if (string.IsNullOrWhiteSpace(username) &&
                string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(password))
            {
                loginbox1.BorderBrush = Brushes.Red;
                emailBoxBorder.BorderBrush = Brushes.Red;
                passwordBoxBorder.BorderBrush = Brushes.Red;
                MessageBox.Show("Please fill in all fields to register.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(username) || username.Length < 4)
            {
                loginbox1.BorderBrush = Brushes.Red;
                MessageBox.Show("Username must be at least 4 characters long.", "Invalid Username", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(email) || !emailRegex.IsMatch(email))
            {
                emailBoxBorder.BorderBrush = Brushes.Red;
                MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                passwordBoxBorder.BorderBrush = Brushes.Red;
                MessageBox.Show("Password must be at least 4 characters long.", "Invalid Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (!isValid) return;

            string connectionString = "";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Проверка, существует ли уже пользователь с таким логином
                string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("username", username);
                    int userExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (userExists > 0)
                    {
                        loginbox1.BorderBrush = Brushes.Red;
                        MessageBox.Show("A user with this username already exists. Please choose a different one.", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Добавление нового пользователя
                string insertQuery = "INSERT INTO users (username, email, password) VALUES (@username, @email, @password)";
                using (var insertCommand = new NpgsqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("username", username);
                    insertCommand.Parameters.AddWithValue("email", email);
                    insertCommand.Parameters.AddWithValue("password", password);

                    int result = insertCommand.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Buttonwindow but = new Buttonwindow();
                        but.Show();
                        this.Close();
                        // Сохраняем данные в сессию
                        UserSession.Username = username;
                        UserSession.Email = email;
                    }
                    else
                    {
                        MessageBox.Show("An error occurred during registration. Please try again later.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Autorization auto = new Autorization();
            auto.Show();
            this.Close();
        }
    }
}