using System;
using System.Data.SQLite;
using System.Windows;
using System.Data;

namespace InventoryManagementSystem
{
    public partial class ProfileandHistoryWindow : Window
    {
        private string username;
        private string connectionString = new SQLiteConnectionStringBuilder
        {
            DataSource = "C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db", // Update path to the correct DB path
            Version = 3,
            DefaultTimeout = 30
        }.ToString(); // Connection string for SQLite

        public ProfileandHistoryWindow(string username)
        {
            InitializeComponent();
            this.username = username; // Save the username passed from the MainWindow
            LoadUserProfile();
            LoadOrderHistory();
        }

        // Method to load the user profile
        private void LoadUserProfile()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT c.Name, c.PhoneNumber, c.EmailAddress, c.Address FROM Customers c " +
                                   "JOIN Users u ON u.Username = c.Name WHERE u.Username = @Username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserNameTextBlock.Text = $"{reader["Name"].ToString()}";
                                UserEmailTextBlock.Text = $"{reader["EmailAddress"].ToString()}";
                                UserPhoneTextBlock.Text = $"{reader["PhoneNumber"].ToString()}";
                                UserAddressTextBlock.Text =$"{reader["Address"].ToString()}";
                            }
                            else
                            {
                                MessageBox.Show("User not found.");
                            }
                        }
                    }
                }
            }
            catch (SQLiteException sqlEx)
            {
                MessageBox.Show($"Database Error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the current window and show the Dashboard window
            this.Hide();
            Dashboard dashboardWindow = new Dashboard(username, "User");
            dashboardWindow.Show();
        }
        // Method to load the order history for the user
        private void LoadOrderHistory()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT o.SaleOrderID,o.Quantity, o.OrderDate, o.Status, o.TotalAmount, o.Discount " +
                                   "FROM SaleOrders o JOIN Customers c ON o.CustomerID = c.CustomerID " +
                                   "JOIN Users u ON u.Username = c.Name WHERE u.Username = @Username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);
                            OrderHistoryDataGrid.ItemsSource = dataTable.DefaultView; // Bind the order history to the DataGrid
                        }
                    }
                }
            }
            catch (SQLiteException sqlEx)
            {
                MessageBox.Show($"Database Error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
