using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Npgsql;

namespace Whattodoifbored
{
    public partial class Buttonwindow : Window
    {
        // Флаг, отслеживающий, сохранена ли текущая активность
        private bool isActivitySaved = false;
        private bool isTaskDone = false;

        public Buttonwindow()
        {
            InitializeComponent();
        }

        // Класс для десериализации activities.json
        private class Activity
        {
            public string activity { get; set; }

            public string link { get; set; }
        }

        // Обработчик кнопки "Get Activity"
        private void button_get_Click(object sender, RoutedEventArgs e)
        {
            activityPanel.Visibility = Visibility.Visible;

            try
            {
                string json = File.ReadAllText("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\activities.json");
                List<Activity> activities = JsonSerializer.Deserialize<List<Activity>>(json);

                if (activities != null && activities.Count > 0)
                {
                    Random random = new Random();
                    string username = UserSession.Username;
                    if (string.IsNullOrWhiteSpace(username))
                    {
                        MessageBox.Show("User not found. Please try again.", "User Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string connectionString = "";
                    Activity randomActivity = null;
                    bool foundUncompleted = false;
                    bool isActivityLiked = false;

                    foreach (var activity in activities.OrderBy(x => random.Next()))
                    {
                        string taskName = activity.activity;

                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();

                            // Проверка, завершена ли активность
                            string checkCompletedQuery = "SELECT COUNT(*) FROM completed_tasks WHERE user_id = (SELECT user_id FROM users WHERE username = @username) AND task_name = @taskName";
                            using (var cmd = new NpgsqlCommand(checkCompletedQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@username", username);
                                cmd.Parameters.AddWithValue("@taskName", taskName);
                                var result = cmd.ExecuteScalar();
                                if (Convert.ToInt32(result) > 0)
                                    continue; // пропускаем завершённую активность
                            }

                            // Проверка, лайкнута ли активность
                            string checkLikedQuery = "SELECT COUNT(*) FROM liked_tasks WHERE user_id = (SELECT user_id FROM users WHERE username = @username) AND task_name = @taskName";
                            using (var cmd = new NpgsqlCommand(checkLikedQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@username", username);
                                cmd.Parameters.AddWithValue("@taskName", taskName);
                                var result = cmd.ExecuteScalar();
                                isActivityLiked = Convert.ToInt32(result) > 0;
                            }
                        }

                        // Если дошли до сюда — активность не завершена
                        randomActivity = activity;
                        foundUncompleted = true;
                        break;
                    }

                    if (foundUncompleted && randomActivity != null)
                    {
                        activityTextBlock.Text = randomActivity.activity;

                        // Загружаем изображение по ссылке, если она есть
                        if (!string.IsNullOrWhiteSpace(randomActivity.link))
                        {
                            ActivityImage.Source = new BitmapImage(new Uri(randomActivity.link, UriKind.Absolute));
                        }

                        // Устанавливаем иконку сердечка
                        LikeIcon.ApplyTemplate();
                        var heartImg = (Image)LikeIcon.Template.FindName("btnImage", LikeIcon);
                        if (heartImg != null)
                        {
                            string heartPath = isActivityLiked
                                ? "C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\redheart.png"
                                : "C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\heart.png";
                            heartImg.Source = new BitmapImage(new Uri(heartPath, UriKind.Absolute));
                        }

                        // Устанавливаем иконку выполнения
                        DoneIcon.ApplyTemplate();
                        var doneImg = (Image)DoneIcon.Template.FindName("btnImage", DoneIcon);
                        if (doneImg != null)
                        {
                            doneImg.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\check.png", UriKind.Absolute));
                        }

                        isActivitySaved = isActivityLiked;
                        isTaskDone = false;
                    }
                    else
                    {
                        activityTextBlock.Text = "You have no unfinished activities.";
                    }
                }
                else
                {
                    activityTextBlock.Text = "The activity list is currently empty.";
                }
            }
            catch (Exception ex)
            {
                activityTextBlock.Text = "An error occurred while loading activities.";
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Обработчик кнопки сердечка
        private void HeartButton_Click(object sender, RoutedEventArgs e)
        {
            string task = activityTextBlock.Text;
            string username = UserSession.Username;

            if (string.IsNullOrWhiteSpace(task) || string.IsNullOrWhiteSpace(username))
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
                            MessageBox.Show("User not found. Please try again.", "User Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        userId = Convert.ToInt32(result);
                    }

                    var btn = LikeIcon;
                    var img = (Image)btn.Template.FindName("btnImage", btn);

                    if (!isActivitySaved)
                    {
                        // Добавить в избранное
                        string insertQuery = "INSERT INTO liked_tasks (user_id, task_name) VALUES (@userId, @taskName)";
                        using (var cmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", task);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\redheart.png", UriKind.Absolute));
                        isActivitySaved = true;
                    }
                    else
                    {
                        // Удалить из избранного
                        string deleteQuery = "DELETE FROM liked_tasks WHERE user_id = @userId AND task_name = @taskName";
                        using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", task);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\heart.png", UriKind.Absolute));
                        isActivitySaved = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong while processing the like action:\n" + ex.Message, "Like Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            string activityText = activityTextBlock.Text;
            string username = UserSession.Username;

            if (string.IsNullOrWhiteSpace(activityText) || string.IsNullOrWhiteSpace(username))
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
                            MessageBox.Show("User not found. Please try again.", "User Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        userId = Convert.ToInt32(result);
                    }

                    var btn = DoneIcon;
                    var img = (Image)btn.Template.FindName("btnImage", btn);

                    if (!isTaskDone)
                    {
                        string insertQuery = "INSERT INTO completed_tasks (user_id, task_name) VALUES (@userId, @taskName)";
                        using (var cmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityText);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\greencheck.png", UriKind.Absolute));
                        isTaskDone = true;
                    }
                    else
                    {
                        string deleteQuery = "DELETE FROM completed_tasks WHERE user_id = @userId AND task_name = @taskName";
                        using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityText);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\check.png", UriKind.Absolute));
                        isTaskDone = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong while processing the completion action:\n" + ex.Message, "Completion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void filter_Click(object sender, EventArgs e)
        {
            Filterwin fiwi = new Filterwin();
            fiwi.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            About ab = new About();
            ab.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Profile profile = new Profile();
            profile.Show();
            this.Close();
        }
    }
}
