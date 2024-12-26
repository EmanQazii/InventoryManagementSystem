using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InventoryManagement;
using project_ex;
using SalesOrderManagementApp;

namespace InventoryManagementSystem
{
    public partial class Dashboard : Window
    {
        private string userRole;
        private string username;
        public Dashboard(string username , string role)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Error: Role not specified.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            userRole = role;
            this.username = username;  // Store username
            userRole = role;
            ConfigureUIBasedOnRole();
            LoadDashboardData();
            LoadNotifications();

            NotificationManager.ShowNotifications = ShowNotifications;
        }

        private void ConfigureUIBasedOnRole()
        {
            if (userRole == "Admin")
            {
                ManageProductsButton.Visibility = Visibility.Visible;
                InventoryTrackingButton.Visibility = Visibility.Visible;
                PurchaseOrderManagementButton.Visibility = Visibility.Visible;
                ReportsButton.Visibility = Visibility.Visible;
                SalesOrderManagementButton.Visibility = Visibility.Collapsed;
                BarCodeButton.Visibility = Visibility.Visible; 
                CustomerAuditButton.Visibility = Visibility.Visible;
                ProfileHistoryButton.Visibility = Visibility.Collapsed;

            }
            else if (userRole == "User")
            {
                ManageProductsButton.Visibility = Visibility.Collapsed;
                ReportsButton.Visibility = Visibility.Collapsed;
                InventoryTrackingButton.Visibility = Visibility.Collapsed;
                PurchaseOrderManagementButton.Visibility = Visibility.Collapsed;
                SalesOrderManagementButton.Visibility = Visibility.Visible;
                BarCodeButton.Visibility = Visibility.Collapsed;
                CustomerAuditButton.Visibility = Visibility.Collapsed;  
                ProfileHistoryButton.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Invalid role detected. Access restricted.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                string connectionString = "Data Source=C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db;Version=3;";

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    TotalProductsTextBlock.Text = ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM Products") ?? "0";
                    TotalSuppliersTextBlock.Text = ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM Suppliers") ?? "0";
                    LowStockTextBlock.Text = ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM Products WHERE Quantity < 10") ?? "0";
                    SalesSummaryTextBlock.Text = ExecuteScalarQuery(conn, "SELECT SUM(TotalAmount) FROM SaleOrders WHERE OrderDate >= date('now', '-7 days')") ?? "0";
                }
            }
            catch (SQLiteException sqlEx)
            {
                MessageBox.Show($"Database Error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"General Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNotifications()
        {
            try
            {
                string connectionString = "Data Source=C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db;Version=3;";

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT Name, Quantity FROM Products WHERE Quantity < 50";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader["Name"].ToString();
                            int quantity = Convert.ToInt32(reader["Quantity"]);
                            ShowNotifications($"Low stock alert: {productName} has only {quantity} units left.");
                        }
                    }
                  
                    ShowNotifications("ReStock Suggestion!! Stock is below minimum level. ");

                    string orderQuery = "SELECT COUNT(*) FROM SaleOrders WHERE OrderDate >= date('now', '-1 day')";
                    using (SQLiteCommand cmd = new SQLiteCommand(orderQuery, conn))
                    {
                        int ordersLastDay = Convert.ToInt32(cmd.ExecuteScalar());
                        ShowNotifications($"Number of orders placed in the last 24 hours: {ordersLastDay}");
                    }
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading notifications: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ShowNotifications(string message)
        {
            NotificationTextBlock.Text = message;
            NotificationPopup.IsOpen = true;

            await Task.Delay(8000); // Display notification for 5 seconds

            NotificationPopup.IsOpen = false;
        }

        private string ExecuteScalarQuery(SQLiteConnection conn, string query)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
            {
                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }

        private void NavigateToWindow(Window window)
        {
            window.Show();
            this.Close();
        }

        private void ManageProductsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new ProductWindow(username, "Admin"));
            NotificationManager.Notify("Opened!");
        }

        private void InventoryTrackingButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new InventoryTrackingWindow(username, "Admin"));
        }

        private void PurchaseOrderManagementButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new PurchaseOrderManagementWindow(username, "Admin"));
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new ReportsAndAnalytics(username, "Admin"));
        }

        private void SalesOrderManagementButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new SalesOrderManagement(username, "User"));
        }

        private void BarCodeButtonButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new BarcodeWindow(username,"Admin"));
        }

        private void CustomerAuditButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new CustomerManagementWindow(username,"Admin"));
        }

        private void ProfileHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new ProfileandHistoryWindow(username));
        }

        private void HelpandSupportButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWindow(new HelpAndSupportWindow(username, userRole));
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            // Close the current Dashboard window
            this.Close();
        }

    }
}
