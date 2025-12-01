using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Text.Json;
using Npgsql;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Windows.Media.Effects;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Whattodoifbored
{
    /// <summary>
    /// Логика взаимодействия для Filterwin.xaml
    /// </summary>
    public partial class Filterwin : Window
    {
        public Filterwin()
        {
            InitializeComponent();
        }

        private void but_Click(object sender, RoutedEventArgs e)
        {
            Buttonwindow buttonwindow = new Buttonwindow();
            buttonwindow.Show();
            this.Close();
        }

        private void DropdownButton_Click(object sender, RoutedEventArgs e)
        {
            PopupList.IsOpen = true;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PopupList.IsOpen)
            {
                if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem item)
                {
                    SelectedText.Text = item.Content.ToString();
                    PopupList.IsOpen = false;
                }
            }
        }

        private void ParticipantsDropdownButton_Click(object sender, RoutedEventArgs e)
        {
            ParticipantsPopupList.IsOpen = true;
        }

        private void ParticipantsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParticipantsPopupList.IsOpen)
            {
                if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem item)
                {
                    SelectedParticipantsText.Text = item.Content.ToString();
                    ParticipantsPopupList.IsOpen = false;
                }
            }
        }

        private void DurationDropdownButton_Click(object sender, RoutedEventArgs e)
        {
            DurationPopupList.IsOpen = true;
        }

        private void DurationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DurationPopupList.IsOpen)
            {
                if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem item)
                {
                    SelectedDurationText.Text = item.Content.ToString();
                    DurationPopupList.IsOpen = false;
                }
            }
        }

        public class Activity
        {
            public string activity { get; set; }
            public string type { get; set; }
            public int participants { get; set; }
            public string duration { get; set; }
            public string link { get; set; }
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            // 1. Считываем выбранные фильтры
            string selectedType = SelectedText.Text;
            string selectedParticipants = SelectedParticipantsText.Text;
            string selectedDuration = SelectedDurationText.Text;

            if (string.IsNullOrWhiteSpace(selectedType) &&
                string.IsNullOrWhiteSpace(selectedParticipants) &&
                string.IsNullOrWhiteSpace(selectedDuration))
            {
                MessageBox.Show("Please select at least one filter.", "Filter Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Читаем JSON-файл
            List<Activity> allActivities;
            try
            {
                string json = File.ReadAllText(@"C:\Users\Lenovo\OneDrive\Документы\3 курс\КУРСОВАЯ\Whattodoifbored\activities.json");
                allActivities = JsonSerializer.Deserialize<List<Activity>>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while reading the JSON file:\n" + ex.Message, "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3. Фильтруем по выбранным критериям
            var filtered = allActivities.Where(a =>
                (string.IsNullOrWhiteSpace(selectedType) || a.type == selectedType)
                && (string.IsNullOrWhiteSpace(selectedParticipants) || a.participants.ToString() == selectedParticipants)
                && (string.IsNullOrWhiteSpace(selectedDuration) || a.duration == selectedDuration)
            ).ToList();

            if (!filtered.Any())
            {
                MessageBox.Show("No activities match the selected filters.", "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
                activityWrapPanel.Children.Clear();
                ResetFilters();
                return;
            }

            // 4. Подготовка списков результатов
            var likedActivities = new List<(string activityName, string link)>();
            var normalActivities = new List<(string activityName, string link)>();

            string connString = "";
            string username = UserSession.Username;
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("User is not logged in.", "Authorization Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                foreach (var act in filtered)
                {
                    // 4.1 Пропускаем выполненные
                    if (IsTaskCompleted(conn, username, act.activity))
                        continue;

                    // 4.2 Сортируем по лайку
                    bool isLiked = IsTaskLiked(conn, username, act.activity);
                    if (isLiked)
                        likedActivities.Add((act.activity, act.link));
                    else
                        normalActivities.Add((act.activity, act.link));
                }
            }

            // 5. Проверяем итог
            if (!likedActivities.Any() && !normalActivities.Any())
            {
                MessageBox.Show("No unfinished activities match the selected filters.", "No Matches", MessageBoxButton.OK, MessageBoxImage.Information);
                activityWrapPanel.Children.Clear();
                ResetFilters();
                return;
            }

            // 6. Дальнейшая отрисовка/логика
            Console.WriteLine("=== Лайкнутые активности ===");
            foreach (var item in likedActivities)
                Console.WriteLine($"{item.activityName} — {item.link}");

            Console.WriteLine("=== Обычные активности ===");
            foreach (var item in normalActivities)
                Console.WriteLine($"{item.activityName} — {item.link}");

            activityWrapPanel.Children.Clear(); // очищаем старые карточки

            foreach (var activity in likedActivities)
            {
                var card = CreateActivityCard(activity.activityName, activity.link, liked: true);
                activityWrapPanel.Children.Add(card);
            }

            foreach (var activity in normalActivities)
            {
                var card = CreateActivityCard(activity.activityName, activity.link, liked: false);
                activityWrapPanel.Children.Add(card);
            }
        }

        private bool IsTaskCompleted(NpgsqlConnection conn, string username, string taskName)
        {
            string checkCompletedQuery = "SELECT COUNT(*) FROM completed_tasks WHERE user_id = (SELECT user_id FROM users WHERE username = @username) AND task_name = @taskName";
            using (var cmd = new NpgsqlCommand(checkCompletedQuery, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@taskName", taskName);
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
        }

        private bool IsTaskLiked(NpgsqlConnection conn, string username, string taskName)
        {
            string checkLikedQuery = "SELECT COUNT(*) FROM liked_tasks WHERE user_id = (SELECT user_id FROM users WHERE username = @username) AND task_name = @taskName";
            using (var cmd = new NpgsqlCommand(checkLikedQuery, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@taskName", taskName);
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
        }


        private Grid CreateActivityCard(string activityName, string link, bool liked)
        {
            // Создаем корневой контейнер карточки — Grid (табличная разметка, позволяет накладывать элементы друг на друга)
            Grid cardGrid = new Grid
            {
                Width = 406,     
                Height = 418,    
                Margin = new Thickness(10)  // Отступы со всех сторон от соседних элементов
            };

            Border border = new Border
            {
                Width = 406,
                Height = 418,
                Background = Brushes.White,  // Цвет фона — белый
                CornerRadius = new CornerRadius(30), 
                Effect = new DropShadowEffect         // Добавляем эффект тени
                {
                    Color = Colors.Black,             
                    BlurRadius = 40,                 
                    Direction = 1,                    
                    ShadowDepth = 1,                  
                    Opacity = 0.1                    
                }
            };
            cardGrid.Children.Add(border);  // Добавляем фон в карточку (на самый задний план)

            // Создаем кнопку "лайк" (сердце), которая будет менять иконку и записывать/удалять активность в таблице liked_tasks
            Button likeButton = new Button
            {
                Width = 49,
                Height = 41,
                Margin = new Thickness(26, 348, 332, 13),  
                Cursor = Cursors.Hand,                     
                Background = Brushes.Transparent,          
                BorderBrush = Brushes.Transparent,        
                BorderThickness = new Thickness(0),      
                Padding = new Thickness(0),                // Нет внутренних отступов
                Tag = activityName,                        // Сохраняем название активности внутри кнопки, чтобы использовать позже
                Focusable = false                         
            };

            // Создаем шаблон (ControlTemplate) для кнопки — определяем, как она визуально строится
            // Используется для обеих кнопок (лайк и сделано)
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding(nameof(Button.Background))
            { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding(nameof(Button.BorderBrush))
            { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding(nameof(Button.BorderThickness))
            { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });

            // Добавляем внутрь кнопки контент (изображение)
            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenterFactory.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(Button.Content))
            { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });

            borderFactory.AppendChild(contentPresenterFactory); // Добавляем ContentPresenter внутрь Border

            var template = new ControlTemplate(typeof(Button)) // Создаем шаблон кнопки
            {
                VisualTree = borderFactory // Определяем, как визуально будет выглядеть кнопка
            };

            likeButton.Template = template;  // Применяем шаблон к кнопке "лайк"

            // В зависимости от того, лайкнута ли активность, отображается соответствующее изображение
            Image likeImage = new Image
            {
                Source = new BitmapImage(new Uri(
                    liked
                        ? "C:\\...\\redheart.png" // Если уже лайкнуто — красное сердце
                        : "C:\\...\\heart.png"     // Если нет — обычное сердце
                )),
                Stretch = Stretch.Uniform // Масштабируем изображение без искажений
            };
            likeButton.Content = likeImage; // Устанавливаем изображение как содержимое кнопки

            cardGrid.Children.Add(likeButton); // Добавляем кнопку лайка на карточку
            likeButton.Click += LikeButton_Click;  // Назначаем обработчик события нажатия

            // КНОПКА "СДЕЛАНО"
            // Создаем кнопку "сделано" (галочка)
            Button doneButton = new Button
            {
                Width = 49,
                Height = 41,
                Margin = new Thickness(332, 349, 26, 12),  // Располагаем в нижнем правом углу
                Cursor = Cursors.Hand,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Tag = activityName // Тоже сохраняем имя активности
            };

            doneButton.Template = template; // Используем тот же шаблон
            Image doneImage = new Image
            {
                Source = new BitmapImage(new Uri("C:\\...\\check.png")), // Иконка галочки
                Stretch = Stretch.Uniform
            };
            doneButton.Content = doneImage;
            cardGrid.Children.Add(doneButton);
            doneButton.Click += DoneButton_Click; // Обработчик клика по "сделано"

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
            cardGrid.Children.Add(activityText); // Добавляем текст поверх

            return cardGrid; // Возвращаем готовую карточку
        }

        private bool isActivitySaved = false;

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

                    string getUserIdQuery = "SELECT user_id FROM users WHERE username = @username";
                    int userId;
                    using (var cmd = new NpgsqlCommand(getUserIdQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("User is not logged in.", "Authorization Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        userId = Convert.ToInt32(result);
                    }

                    var img = btn.Content as Image;
                    if (img == null) return;

                    if (!isActivitySaved)
                    {
                        string insertQuery = "INSERT INTO liked_tasks (user_id, task_name) VALUES (@userId, @taskName)";
                        using (var cmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityName);
                            cmd.ExecuteNonQuery();
                        }

                        img.Source = new BitmapImage(new Uri("C:\\Users\\Lenovo\\OneDrive\\Документы\\3 курс\\КУРСОВАЯ\\Whattodoifbored\\pic\\redheart.png", UriKind.Absolute));
                        isActivitySaved = true;
                    }
                    else
                    {
                        string deleteQuery = "DELETE FROM liked_tasks WHERE user_id = @userId AND task_name = @taskName";
                        using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityName);
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

        private bool isTaskDone = false;

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

                    string getUserIdQuery = "SELECT user_id FROM users WHERE username = @username";
                    int userId;
                    using (var cmd = new NpgsqlCommand(getUserIdQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("User is not logged in.", "Authorization Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        userId = Convert.ToInt32(result);
                    }

                    var img = btn.Content as Image;
                    if (img == null) return;

                    if (!isTaskDone)
                    {
                        string insertQuery = "INSERT INTO completed_tasks (user_id, task_name) VALUES (@userId, @taskName)";
                        using (var cmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@taskName", activityName);
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
                            cmd.Parameters.AddWithValue("@taskName", activityName);
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

        private void ab_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.Show();
            this.Close();
        }

        private void pro_Click(object sender, EventArgs e)
        {
            Profile profile = new Profile();
            profile.Show();
            this.Close();
        }

        private void ResetFilters()
        {
            Dispatcher.InvokeAsync(() =>
            {
                tupetext.SelectedIndex = -1;
                partext.SelectedIndex = -1;
                durtext.SelectedIndex = -1;

                SelectedText.Text = "";
                SelectedParticipantsText.Text = "";
                SelectedDurationText.Text = "";
            });
        }

        private void clear_Click(object sender, EventArgs e)
        {
            // Проверяем: есть ли что-то выбранное хотя бы в одном фильтре
            if (!string.IsNullOrEmpty(SelectedText.Text) ||
                !string.IsNullOrEmpty(SelectedParticipantsText.Text) ||
                !string.IsNullOrEmpty(SelectedDurationText.Text))
            {
                ResetFilters();
            }
            else
            {
                MessageBox.Show("There are no filters to reset.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}