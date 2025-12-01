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
using System.Xml.Linq;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using Npgsql;
using System.Windows.Media.Effects;

namespace Whattodoifbored
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        public Profile()
        {
            InitializeComponent();

            try
            {
                using (var connection = new Npgsql.NpgsqlConnection(""))
                {
                    connection.Open();

                    string getUserDataQuery = "SELECT username, email FROM users WHERE username = @username";
                    using (var cmd = new Npgsql.NpgsqlCommand(getUserDataQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", UserSession.Username);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string loginFromDb = reader["username"].ToString();
                                string emailFromDb = reader["email"].ToString();

                                // Теперь устанавливаем данные в интерфейс
                                HelloLabel.Content = $"Hello, {loginFromDb}!";
                                loginbox.Text = loginFromDb;
                                emailbox.Text = emailFromDb;

                                // Заодно обновляем UserSession
                                UserSession.Username = loginFromDb;
                                UserSession.Email = emailFromDb;
                            }
                            else
                            {
                                MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading data: " + ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadFavoriteActivities();
            LoadCompletedActivities();
            LoadLikedAndCompletedActivities();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Buttonwindow buttonwindow = new Buttonwindow();
            buttonwindow.Show();
            this.Close();
        }

        private void filter_Click(object sender, RoutedEventArgs e)
        {
            Filterwin filterwin = new Filterwin();
            filterwin.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
            this.Close();        
        }

        private void loginbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9._]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void loginbox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
            if (emailbox.Text.Length > 30)
            {
                int caretIndex = emailbox.CaretIndex;
                emailbox.Text = emailbox.Text.Substring(0, 30);
                emailbox.CaretIndex = caretIndex > 30 ? 30 : caretIndex;
            }
        }

        // Нажатие кнопки сохранения
        private void autorizbutton_Click(object sender, RoutedEventArgs e)
        {
            string username = UserSession.Username;
            string email = UserSession.Email;

            string newLogin = loginbox.Text.Trim();
            string newEmail = emailbox.Text.Trim();

            loginbox1.BorderBrush = Brushes.Gray;
            loginbox2.BorderBrush = Brushes.Gray;

            bool hasError = false;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(newLogin))
            {
                loginbox1.BorderBrush = Brushes.Red;
                hasError = true;
            }

            if (string.IsNullOrEmpty(newEmail))
            {
                loginbox2.BorderBrush = Brushes.Red;
                hasError = true;
            }

            if (hasError)
            {
                MessageBox.Show("Please fill in all required fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newLogin.Length <= 3)
            {
                loginbox1.BorderBrush = Brushes.Red;
                MessageBox.Show("Username must be longer than 3 characters.", "Invalid Username", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка email через регулярку
            Regex emailRegex = new Regex(@"^[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (!emailRegex.IsMatch(newEmail))
            {
                loginbox2.BorderBrush = Brushes.Red;
                MessageBox.Show("Invalid email format. Please check and try again.", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newLogin == username && newEmail == email)
            {
                MessageBox.Show("Please modify at least one field before saving.", "No Changes Detected", MessageBoxButton.OK, MessageBoxImage.Information);
                loginbox1.BorderBrush = Brushes.Red;
                loginbox2.BorderBrush = Brushes.Red;
                return;
            }

            try
            {
                using (var connection = new Npgsql.NpgsqlConnection(""))
                {
                    connection.Open();

                    int userId;

                    // Получаем user_id по текущему логину
                    string getUserIdQuery = "SELECT user_id FROM users WHERE username = @username";
                    using (var getUserIdCmd = new Npgsql.NpgsqlCommand(getUserIdQuery, connection))
                    {
                        getUserIdCmd.Parameters.AddWithValue("@username", username);
                        var result = getUserIdCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        userId = Convert.ToInt32(result);
                    }

                    // Проверяем, занят ли новый логин другим пользователем
                    string checkLoginQuery = "SELECT COUNT(*) FROM users WHERE username = @newLogin AND user_id != @userId";
                    using (var checkCmd = new Npgsql.NpgsqlCommand(checkLoginQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@newLogin", newLogin);
                        checkCmd.Parameters.AddWithValue("@userId", userId);

                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            loginbox1.BorderBrush = Brushes.Red;
                            MessageBox.Show("This username is already taken by another user.", "Username Unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Обновляем данные
                    using (var updateCmd = new Npgsql.NpgsqlCommand())
                    {
                        updateCmd.Connection = connection;
                        updateCmd.CommandText = "UPDATE users SET username = @newLogin, email = @newEmail WHERE user_id = @userId";
                        updateCmd.Parameters.AddWithValue("@newLogin", newLogin);
                        updateCmd.Parameters.AddWithValue("@newEmail", newEmail);
                        updateCmd.Parameters.AddWithValue("@userId", userId);

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Your profile has been successfully updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Обновляем UserSession и интерфейс
                            UserSession.Username = newLogin;
                            UserSession.Email = newEmail;
                            HelloLabel.Content = $"Hello, {newLogin}!";
                        }
                        else
                        {
                            MessageBox.Show("An error occurred while saving your changes: ", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update info: " + ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class ActivityJsonModel
        {
            public string activity { get; set; }
            public string link { get; set; }
        }

        private void LoadFavoriteActivities()
        {
            try
            {
                using (var connection = new NpgsqlConnection(""))
                {
                    connection.Open();

                    // Получаем user_id по текущему логину
                    int userId;
                    string getUserIdQuery = "SELECT user_id FROM users WHERE username = @username";
                    using (var getUserIdCmd = new NpgsqlCommand(getUserIdQuery, connection))
                    {
                        getUserIdCmd.Parameters.AddWithValue("@username", UserSession.Username);
                        var result = getUserIdCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        userId = Convert.ToInt32(result);
                    }

                    // Получаем список любимых активностей по user_id
                    var activityNames = new List<string>();
                    string getFavoritesQuery = "SELECT task_name FROM liked_tasks WHERE user_id = @userId";
                    using (var getFavoritesCmd = new NpgsqlCommand(getFavoritesQuery, connection))
                    {
                        getFavoritesCmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = getFavoritesCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                activityNames.Add(reader["task_name"].ToString());
                            }
                        }
                    }

                    // Получаем список выполненных активностей
                    var completedActivities = new HashSet<string>();
                    string getCompletedQuery = "SELECT task_name FROM completed_tasks WHERE user_id = @userId";
                    using (var getCompletedCmd = new NpgsqlCommand(getCompletedQuery, connection))
                    {
                        getCompletedCmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = getCompletedCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                completedActivities.Add(reader["task_name"].ToString());
                            }
                        }
                    }

                    // Читаем JSON-файл
                    string json = File.ReadAllText(@"C:\Users\Lenovo\OneDrive\Документы\3 курс\КУРСОВАЯ\Whattodoifbored\activities.json");
                    var jsonActivities = JsonConvert.DeserializeObject<List<ActivityJsonModel>>(json);

                    // Собираем карточки
                    var likedActivities = new List<(string activityName, string link)>();
                    foreach (var name in activityNames)
                    {
                        // Проверяем, не выполнена ли активность
                        if (completedActivities.Contains(name))
                            continue; // Пропускаем, если активность уже выполнена

                        var match = jsonActivities.FirstOrDefault(a => a.activity == name);
                        string link = match?.link ?? "";
                        likedActivities.Add((name, link));
                    }

                    // Отображаем карточки
                    activityWrapPanel.Children.Clear();
                    foreach (var activity in likedActivities)
                    {
                        var card = CreateActivityCard(activity.activityName, activity.link, liked: true, completed: false);
                        activityWrapPanel.Children.Add(card);
                    }

                    // Показываем или скрываем надпись о пустом списке
                    bool hasFavorites = likedActivities.Count > 0;
                    smallTextLabel.Visibility = hasFavorites ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load your favorite activities: " + ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCompletedActivities()
        {
            try
            {
                using (var connection = new NpgsqlConnection(""))
                {
                    connection.Open();

                    // Получаем user_id по текущему логину
                    int userId;
                    string getUserIdQuery = "SELECT user_id FROM users WHERE username = @username";
                    using (var getUserIdCmd = new NpgsqlCommand(getUserIdQuery, connection))
                    {
                        getUserIdCmd.Parameters.AddWithValue("@username", UserSession.Username);
                        var result = getUserIdCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        userId = Convert.ToInt32(result);
                    }

                    // Получаем список выполненных активностей
                    var completedActivities = new List<string>();
                    string getCompletedQuery = "SELECT task_name FROM completed_tasks WHERE user_id = @userId";
                    using (var getCompletedCmd = new NpgsqlCommand(getCompletedQuery, connection))
                    {
                        getCompletedCmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = getCompletedCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                completedActivities.Add(reader["task_name"].ToString());
                            }
                        }
                    }

                    // Получаем список любимых активностей (чтобы исключить их)
                    var favoriteActivities = new HashSet<string>();
                    string getFavoritesQuery = "SELECT task_name FROM liked_tasks WHERE user_id = @userId";
                    using (var getFavoritesCmd = new NpgsqlCommand(getFavoritesQuery, connection))
                    {
                        getFavoritesCmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = getFavoritesCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                favoriteActivities.Add(reader["task_name"].ToString());
                            }
                        }
                    }

                    // Читаем JSON-файл
                    string json = File.ReadAllText(@"C:\Users\Lenovo\OneDrive\Документы\3 курс\КУРСОВАЯ\Whattodoifbored\activities.json");
                    var jsonActivities = JsonConvert.DeserializeObject<List<ActivityJsonModel>>(json);

                    // Собираем карточки
                    var completedList = new List<(string activityName, string link)>();
                    foreach (var name in completedActivities)
                    {
                        if (favoriteActivities.Contains(name))
                            continue; // Пропускаем, если есть в любимых

                        var match = jsonActivities.FirstOrDefault(a => a.activity == name);
                        string link = match?.link ?? "";
                        completedList.Add((name, link));
                    }

                    // Отображаем карточки
                    completedActivityWrapPanel.Children.Clear();
                    foreach (var activity in completedList)
                    {
                        var card = CreateActivityCard(activity.activityName, activity.link, liked: false, completed: true);
                        completedActivityWrapPanel.Children.Add(card);
                    }

                    // Показываем или скрываем надпись о пустом списке
                    bool hasCompleted = completedList.Count > 0;
                    completedSmallTextLabel.Visibility = hasCompleted ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load your completed activities: " + ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLikedAndCompletedActivities()
        {
            try
            {
                using (var connection = new NpgsqlConnection(""))
                {
                    connection.Open();

                    // Получаем user_id по текущему логину
                    int userId;
                    string getUserIdQuery = "SELECT user_id FROM users WHERE username = @username";
                    using (var getUserIdCmd = new NpgsqlCommand(getUserIdQuery, connection))
                    {
                        getUserIdCmd.Parameters.AddWithValue("@username", UserSession.Username);
                        var result = getUserIdCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        userId = Convert.ToInt32(result);
                    }

                    // Получаем активности, которые и лайкнуты, и выполнены
                    var likedAndCompletedActivities = new List<string>();
                    string intersectionQuery = @"
                SELECT task_name 
                FROM liked_tasks 
                WHERE user_id = @userId AND task_name IN (
                    SELECT task_name 
                    FROM completed_tasks 
                    WHERE user_id = @userId
                )";
                    using (var cmd = new NpgsqlCommand(intersectionQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                likedAndCompletedActivities.Add(reader["task_name"].ToString());
                            }
                        }
                    }

                    // Читаем JSON-файл
                    string json = File.ReadAllText(@"C:\Users\Lenovo\OneDrive\Документы\3 курс\КУРСОВАЯ\Whattodoifbored\activities.json");
                    var jsonActivities = JsonConvert.DeserializeObject<List<ActivityJsonModel>>(json);

                    // Собираем карточки
                    var resultList = new List<(string activityName, string link)>();
                    foreach (var name in likedAndCompletedActivities)
                    {
                        var match = jsonActivities.FirstOrDefault(a => a.activity == name);
                        string link = match?.link ?? "";
                        resultList.Add((name, link));
                    }

                    // Отображаем карточки
                    likedAndCompletedWrapPanel.Children.Clear();
                    foreach (var activity in resultList)
                    {
                        var card = CreateActivityCard(activity.activityName, activity.link, liked: true, completed: true);
                        likedAndCompletedWrapPanel.Children.Add(card);
                    }

                    // Показываем или скрываем надпись о пустом списке
                    bool hasAny = resultList.Count > 0;
                    likedCompletedSmallTextLabel.Visibility = hasAny ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading liked and completed activities: " + ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Grid CreateActivityCard(string activityName, string link, bool liked, bool completed)
        {
            Grid cardGrid = new Grid
            {
                Width = 406,
                Height = 418,
                Margin = new Thickness(10)
            };


            Border border = new Border
            {
                Width = 406,
                Height = 418,
                Background = Brushes.White,
                CornerRadius = new CornerRadius(30),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 40,
                    Direction = 1,
                    ShadowDepth = 1,
                    Opacity = 0.05
                }
            };
            cardGrid.Children.Add(border);

            Button likeButton = new Button
            {
                Width = 49,
                Height = 41,
                Margin = new Thickness(26, 348, 332, 13),
                Cursor = Cursors.Hand,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Tag = activityName,
                Focusable = false
            };

            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetBinding(
                Border.BackgroundProperty,
                new Binding(nameof(Button.Background))
                { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );
            borderFactory.SetBinding(
                Border.BorderBrushProperty,
                new Binding(nameof(Button.BorderBrush))
                { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );
            borderFactory.SetBinding(
                Border.BorderThicknessProperty,
                new Binding(nameof(Button.BorderThickness))
                { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );

            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenterFactory.SetBinding(
                ContentPresenter.ContentProperty,
                new Binding(nameof(Button.Content))
                { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );

            borderFactory.AppendChild(contentPresenterFactory);

            var template = new ControlTemplate(typeof(Button))
            {
                VisualTree = borderFactory
            };

            likeButton.Template = template;

            Image likeImage = new Image
            {
                Source = new BitmapImage(new Uri(liked ? "C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\redheart.png" : "C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\heart.png")),
                Stretch = Stretch.Uniform
            };
            likeButton.Content = likeImage;
            cardGrid.Children.Add(likeButton);

            likeButton.Content = likeImage;
            likeButton.Click += LikeButton_Click;  // Добавляем обработчик нажатия

            Button doneButton = new Button
            {
                Width = 49,
                Height = 41,
                Margin = new Thickness(332, 349, 26, 12),
                Cursor = Cursors.Hand,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Tag = activityName
            };

            doneButton.Template = template;

            Image doneImage = new Image
            {
                Source = new BitmapImage(new Uri(completed ? "C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\greencheck.png" : "C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\check.png")),
                Stretch = Stretch.Uniform
            };
            doneButton.Content = doneImage;
            cardGrid.Children.Add(doneButton);

            doneButton.Content = doneImage;
            doneButton.Click += DoneButton_Click;  // Добавляем обработчик нажатия

            Image activityImage = new Image
            {
                Width = 200,
                Height = 200,
                Margin = new Thickness(10, 50, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri(link))
            };
            cardGrid.Children.Add(activityImage);

            TextBlock activityText = new TextBlock
            {
                Text = activityName,
                FontFamily = new FontFamily("Inter"),
                FontSize = 23,
                FontWeight = FontWeights.Medium,
                Width = 286,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(62, 38, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            cardGrid.Children.Add(activityText);

            return cardGrid;
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            string activityName = btn.Tag as string;
            string username = UserSession.Username;

            if (string.IsNullOrWhiteSpace(activityName) || string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("No activity text found or user is not logged in.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "";

            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    // Получаем user_id
                    string getUserIdQuery = "SELECT user_id FROM users WHERE username = @username";
                    int userId;
                    using (var cmd = new NpgsqlCommand(getUserIdQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        userId = Convert.ToInt32(result);
                    }

                    // Проверяем, сохранена ли уже эта активность
                    bool isSaved;
                    string checkQuery = "SELECT COUNT(*) FROM liked_tasks WHERE user_id = @userId AND task_name = @taskName";
                    using (var cmd = new NpgsqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@taskName", activityName);
                        isSaved = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    // Получаем изображение в кнопке
                    var img = btn.Content as Image;
                    if (img == null) return;

                    if (!isSaved)
                    {
                        // Добавляем в избранное
                        string insertQuery = "INSERT INTO liked_tasks (user_id, task_name) VALUES (@userId, @taskName)";
                        using (var cmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityName);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\redheart.png", UriKind.Absolute));
                    }
                    else
                    {
                        // Удаляем из избранного
                        string deleteQuery = "DELETE FROM liked_tasks WHERE user_id = @userId AND task_name = @taskName";
                        using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityName);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\heart.png", UriKind.Absolute));
                    }

                    // Обновляем UI (и обязательно очищай панели внутри методов Load...())
                    LoadFavoriteActivities();
                    LoadLikedAndCompletedActivities();
                    LoadCompletedActivities();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while updating the like status: " + ex.Message, "Like Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            string activityName = btn.Tag as string;
            string username = UserSession.Username;

            if (string.IsNullOrWhiteSpace(activityName) || string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("No activity text found or user is not logged in.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "";

            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    // Получаем user_id
                    string getUserIdQuery = "SELECT user_id FROM users WHERE username = @username";
                    int userId;
                    using (var cmd = new NpgsqlCommand(getUserIdQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        userId = Convert.ToInt32(result);
                    }

                    // Проверяем, завершена ли уже эта активность
                    bool isDone;
                    string checkQuery = "SELECT COUNT(*) FROM completed_tasks WHERE user_id = @userId AND task_name = @taskName";
                    using (var cmd = new NpgsqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@taskName", activityName);
                        isDone = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    // Достаём Image из кнопки
                    var img = btn.Content as Image;
                    if (img == null) return;

                    if (!isDone)
                    {
                        // Добавляем в завершённые
                        string insertQuery = "INSERT INTO completed_tasks (user_id, task_name) VALUES (@userId, @taskName)";
                        using (var cmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityName);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\greencheck.png", UriKind.Absolute));
                    }
                    else
                    {
                        // Удаляем из завершённых
                        string deleteQuery = "DELETE FROM completed_tasks WHERE user_id = @userId AND task_name = @taskName";
                        using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityName);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\check.png", UriKind.Absolute));
                    }

                    // Обновляем UI
                    LoadLikedAndCompletedActivities();
                    LoadCompletedActivities();
                    LoadFavoriteActivities();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while updating the done status: " + ex.Message, "Done Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
