using System;
using System.Data.SQLite;
using System.Configuration;
using System.Windows;
using InventoryManagementSystem;

namespace project_ex
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Event handler for Username TextBox GotFocus event
        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == "Enter Username")
            {
                UsernameTextBox.Clear();
            }
        }

        // Event handler for Username TextBox LostFocus event
        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernameTextBox.Text = "Enter Username";
            }
        }

        // Event handler for PasswordBox GotFocus event
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password == "Enter Password")
            {
                PasswordBox.Clear();
            }
        }

        // Event handler for PasswordBox LostFocus event
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordBox.Password = "Enter Password";
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string relativePath = "InventoryManagement.db";
            string connectionString = $"Data Source={System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath)};Version=3;";

            //string connectionString = "Data Source=C:\\\\Users\\\\noora\\\\source\\\\repos\\\\project ex\\\\project ex\\\\project ex\\\\InventoryManagement.db;Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Role FROM Users WHERE Username = @Username AND PasswordHash = @Password";
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    var role = command.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(role))
                    {
                        Dashboard dashboard = new Dashboard(username,role);
                        dashboard.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Invalid credentials.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NavigateToWindow(Window window)
        {
            window.Show();
            this.Close();
        }
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new SignUpWindow());
        }
    }
}
