using System;
using System.Data.SQLite;
using System.Windows;
using InventoryManagementSystem;

namespace project_ex
{
    public partial class SignUpWindow : Window
    {
        public SignUpWindow()
        {
            InitializeComponent();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string username = SignUpUsernameTextBox.Text;
            string password = SignUpPasswordBox.Password;
            string role = "User";  // Role is constant as "User"

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill out all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "Data Source=C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db;Version=3;";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@Username, @Password, @Role)";
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);  // Note: You should hash the password before saving
                    command.Parameters.AddWithValue("@Role", role);

                    command.ExecuteNonQuery();
                    MessageBox.Show("Sign Up Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Close SignUp window after successful signup
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
