using System;
using System.Data.SQLite;
using System.Windows;

namespace InventoryManagementSystem
{
    public partial class CustomerManagementWindow : Window
    {
        private string username;
        private string role;

        public CustomerManagementWindow(string username, string role)
        {
            InitializeComponent();
            this.username = username;
            this.role = role;
        }

        private void OpenDashboard()
        {
            // Open Dashboard with both username and role
            Dashboard dashboard = new Dashboard(username, role);
            dashboard.Show();
            this.Close();
        }
        private string connectionString = new SQLiteConnectionStringBuilder
        {
            DataSource = "C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db",
            Version = 3,
            DefaultTimeout = 30
        }.ToString();

        private void SearchCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            string customerName = CustomerNameTextBox.Text.Trim();
            Customer customer = GetCustomerInfo(customerName);
            if (customer != null)
            {
                // Display customer info
                CustomerName.Text = $"Name: {customer.Name}";
                CustomerPhone.Text = $"Phone: {customer.PhoneNumber}";
                CustomerEmail.Text = $"Email: {customer.EmailAddress}";
                CustomerAddress.Text = $"Address: {customer.Address}";

                // Display order history
                var orders = GetCustomerOrders(customer.CustomerId);
                OrderHistoryDataGrid.ItemsSource = orders;
            }
            else
            {
                MessageBox.Show("Customer not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateOrderStatusButton_Click(object sender, RoutedEventArgs e)
        {
            SaleOrder selectedOrder = (SaleOrder)OrderHistoryDataGrid.SelectedItem;

            if (selectedOrder != null)
            {
                string currentStatus = selectedOrder.Status.Trim();

                // Only allow updates for Pending or Shipped orders
                if (currentStatus == "Pending" || currentStatus == "Shipped")
                {
                    // Show the ComboBox and Update Button
                    StatusComboBoxPanel.Visibility = Visibility.Visible;

                    // Populate the ComboBox with available statuses (can be expanded later)
                    StatusComboBox.Items.Clear();
                    StatusComboBox.Items.Add("Pending");
                    StatusComboBox.Items.Add("Shipped");
                    StatusComboBox.Items.Add("Completed");
                    StatusComboBox.Items.Add("Cancelled");

                    // Set the current status as the selected item in the ComboBox
                    StatusComboBox.SelectedItem = currentStatus;
                }
                else
                {
                    MessageBox.Show("Only Pending or Shipped orders can be updated.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an order to update.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the Dashboard
            Dashboard dashboardWindow = new Dashboard(username, "Admin");
            dashboardWindow.Show();
            this.Close(); // Close the current InventoryTracking window
        }
        private void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            SaleOrder selectedOrder = (SaleOrder)OrderHistoryDataGrid.SelectedItem;

            // Ensure an order is selected and ComboBox has a valid selection
            if (selectedOrder != null && StatusComboBox.SelectedItem != null)
            {
                string newStatus = StatusComboBox.SelectedItem.ToString().Trim();

                // Update the order status in the database
                bool isUpdated = UpdateOrderStatus(selectedOrder.SaleOrderID, newStatus);

                if (isUpdated)
                {
                    MessageBox.Show("Order status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh the order list to show updated status
                    SearchCustomerButton_Click(sender, e);

                    // Hide the ComboBox after updating
                    StatusComboBoxPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Failed to update the order status. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a valid status to update.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Method to populate the ComboBox with available statuses from the database
        private void PopulateStatusComboBox()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT DISTINCT Status FROM SaleOrders";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            StatusComboBox.Items.Clear(); // Clear existing items

                            while (reader.Read())
                            {
                                StatusComboBox.Items.Add(reader["Status"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accessing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Customer GetCustomerInfo(string name)
        {
            Customer customer = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Customers WHERE Name LIKE @Name";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", "%" + name + "%");

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                customer = new Customer
                                {
                                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                                    Name = reader["Name"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString(),
                                    EmailAddress = reader["EmailAddress"].ToString(),
                                    Address = reader["Address"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accessing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return customer;
        }

        private System.Collections.Generic.List<SaleOrder> GetCustomerOrders(int customerId)
        {
            var orders = new System.Collections.Generic.List<SaleOrder>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM SaleOrders WHERE CustomerID = @CustomerID";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerID", customerId);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new SaleOrder
                                {
                                    SaleOrderID = Convert.ToInt32(reader["SaleOrderID"]),
                                    Status = reader["Status"].ToString(),
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    TotalAmount = Convert.ToDouble(reader["TotalAmount"]),
                                    OrderDate = Convert.ToDateTime(reader["OrderDate"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accessing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return orders;
        }

        private bool UpdateOrderStatus(int saleOrderId, string status)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE SaleOrders SET Status = @Status WHERE SaleOrderID = @SaleOrderID";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@SaleOrderID", saleOrderId);

                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0; // Returns true if one or more rows are updated
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating order status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; // Return false if there was an error
            }
        }

        public class SaleOrder
        {
            public int SaleOrderID { get; set; }
            public int Quantity { get; set; }
            public string Status { get; set; }
            public double TotalAmount { get; set; }
            public DateTime OrderDate { get; set; }
        }

        public class Customer
        {
            public int CustomerId { get; set; }
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string EmailAddress { get; set; }
            public string Address { get; set; }
        }
    }
}
