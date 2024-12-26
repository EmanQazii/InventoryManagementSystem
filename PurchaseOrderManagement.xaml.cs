using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using InventoryManagementSystem;

namespace InventoryManagement
{
    public partial class PurchaseOrderManagementWindow : Window
    {
        private string username;
        private string role;

        public PurchaseOrderManagementWindow(string username, string role)
        {
            InitializeComponent();
            this.username = username;
            this.role = role;

            LoadSuppliers();
            LoadProducts();
            LoadOrders();
        }

        // Helper method to get the database connection
        private SQLiteConnection GetDatabaseConnection()
        {
            string connectionString = @"Data Source=C:\Users\noora\source\repos\project ex\project ex\project ex\InventoryManagement.db;Version=3;";
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        // Class representing an Order
        private class Order
        {
            public int OrderID { get; set; }
            public string ProductName { get; set; }
            public string SupplierName { get; set; }
            public int Quantity { get; set; }
        }

        // Class representing a Supplier
        private class Supplier
        {
            public string SupplierName { get; set; }
            public string ContactName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
        }

        // Load suppliers into the supplierComboBox and DataGrid
        private List<Supplier> suppliersList = new List<Supplier>();  // Store suppliers in a list

        // Load suppliers into the supplierComboBox and DataGrid
        private void LoadSuppliers()
        {
            suppliersList.Clear(); // Clear the list before loading new data
            supplierComboBox.Items.Clear(); // Clear the combo box before adding new data

            using (var connection = GetDatabaseConnection())
            {
                string query = "SELECT SupplierName, ContactName, Phone, Email, Address FROM Suppliers";
                var command = new SQLiteCommand(query, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var supplier = new Supplier
                    {
                        SupplierName = reader["SupplierName"].ToString(),
                        ContactName = reader["ContactName"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Email = reader["Email"].ToString(),
                        Address = reader["Address"].ToString()
                    };
                    suppliersList.Add(supplier);

                    // Add supplier names to the combo box
                    supplierComboBox.Items.Add(supplier.SupplierName);
                }

                // Bind the list to the DataGrid
                suppliersDataGrid.ItemsSource = null;
                suppliersDataGrid.ItemsSource = suppliersList;
            }
        }



        // Load products into the statusComboBox
        private void LoadProducts()
        {
            statusComboBox.Items.Clear();

            using (var connection = GetDatabaseConnection())
            {
                string query = "SELECT Name FROM Products";
                var command = new SQLiteCommand(query, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    statusComboBox.Items.Add(reader["Name"].ToString());
                }
            }
        }

        // Load orders into the DataGrid
        private void LoadOrders()
        {
            using (var connection = GetDatabaseConnection())
            {
                string query = "SELECT PODetailID, ProductName, SupplierName, Quantity FROM PurchaseOrderDetail";
                var command = new SQLiteCommand(query, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                var orders = new List<Order>();
                while (reader.Read())
                {
                    orders.Add(new Order
                    {
                        OrderID = Convert.ToInt32(reader["PODetailID"]),
                        ProductName = reader["ProductName"].ToString(),
                        SupplierName = reader["SupplierName"].ToString(),
                        Quantity = Convert.ToInt32(reader["Quantity"]),
                    });
                }
                orderHistoryDataGrid.ItemsSource = orders;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard dashboardWindow = new Dashboard(username, "Admin");
            dashboardWindow.Show();
            this.Close();
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedProduct = statusComboBox.SelectedItem?.ToString();
            string selectedSupplier = supplierComboBox.SelectedItem?.ToString();
            string quantityText = orderIDTextBox.Text;
            int quantity;

            if (string.IsNullOrEmpty(selectedProduct) || string.IsNullOrEmpty(selectedSupplier) || !int.TryParse(quantityText, out quantity))
            {
                MessageBox.Show("Please fill all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = GetDatabaseConnection())
            {
                string query = "INSERT INTO PurchaseOrderDetail (ProductName, SupplierName, Quantity) VALUES (@ProductName, @SupplierName, @Quantity)";
                var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@ProductName", selectedProduct);
                command.Parameters.AddWithValue("@SupplierName", selectedSupplier);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Order Created Successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadOrders(); // Reload orders to reflect the new order
        }

        private void TrackOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (orderHistoryDataGrid.SelectedItem is Order selectedOrder)
            {
                // Generate a random status
                string[] statuses = { "Pending", "Shipped", "Delivered", "Cancelled" };
                Random random = new Random();
                string randomStatus = statuses[random.Next(statuses.Length)];

                // Display order details along with the random status
                MessageBox.Show($"Order Details:\n" +
                                $"Order ID: {selectedOrder.OrderID}\n" +
                                $"Product: {selectedOrder.ProductName}\n" +
                                $"Supplier: {selectedOrder.SupplierName}\n" +
                                $"Quantity: {selectedOrder.Quantity}\n" +
                                $"Status: {randomStatus}",
                                "Order Status", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select an order from the list.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (orderHistoryDataGrid.SelectedItem is Order selectedOrder)
            {
                statusComboBox.SelectedItem = selectedOrder.ProductName;
                supplierComboBox.SelectedItem = selectedOrder.SupplierName;
                orderIDTextBox.Text = selectedOrder.Quantity.ToString();
            }
        }

        // Add Supplier
        private void AddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            string supplierName = supplierNameTextBox.Text;
            string contactName = contactNameTextBox.Text;
            string phone = phoneTextBox.Text;
            string email = emailTextBox.Text;
            string address = addressTextBox.Text;

            if (string.IsNullOrEmpty(supplierName) || string.IsNullOrEmpty(contactName))
            {
                MessageBox.Show("Please fill in all details for the supplier.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = GetDatabaseConnection())
            {
                string query = "INSERT INTO Suppliers (SupplierName, ContactName, Phone, Email, Address) VALUES (@SupplierName, @ContactName, @Phone, @Email, @Address)";
                var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@SupplierName", supplierName);
                command.Parameters.AddWithValue("@ContactName", contactName);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Address", address);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Supplier added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearSupplierFields();
            LoadSuppliers(); // Reload suppliers in the combobox and DataGrid
        }

        // Remove Supplier by selecting from DataGrid
        private void RemoveSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if a supplier is selected from the DataGrid
            if (suppliersDataGrid.SelectedItem is Supplier selectedSupplier)
            {
                string supplierName = selectedSupplier.SupplierName;

                // Confirm the removal
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to remove the supplier '{supplierName}'?", "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = GetDatabaseConnection())
                    {
                        string query = "DELETE FROM Suppliers WHERE SupplierName = @SupplierName";
                        var command = new SQLiteCommand(query, connection);
                        command.Parameters.AddWithValue("@SupplierName", supplierName);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Supplier removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Remove supplier from the list and refresh the DataGrid
                            suppliersList.Remove(selectedSupplier);
                            suppliersDataGrid.ItemsSource = null;
                            suppliersDataGrid.ItemsSource = suppliersList;  // Rebind the updated list to DataGrid
                        }
                        else
                        {
                            MessageBox.Show("Supplier not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a supplier from the DataGrid to remove.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Update Supplier
        private void UpdateSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            string supplierName = supplierNameTextBox.Text;
            string contactName = contactNameTextBox.Text;
            string phone = phoneTextBox.Text;
            string email = emailTextBox.Text;
            string address = addressTextBox.Text;

            if (string.IsNullOrEmpty(supplierName))
            {
                MessageBox.Show("Please enter the supplier name to update.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = GetDatabaseConnection())
            {
                string query = "UPDATE Suppliers SET ContactName = @ContactName, Phone = @Phone, Email = @Email, Address = @Address WHERE SupplierName = @SupplierName";
                var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@SupplierName", supplierName);
                command.Parameters.AddWithValue("@ContactName", contactName);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Address", address);
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Supplier updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSuppliers(); // Reload suppliers in the combobox and DataGrid
                }
                else
                {
                    MessageBox.Show("Supplier not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Clear Supplier Fields
        private void ClearSupplierFields()
        {
            supplierNameTextBox.Clear();
            contactNameTextBox.Clear();
            phoneTextBox.Clear();
            emailTextBox.Clear();
            addressTextBox.Clear();
        }
    }
}
