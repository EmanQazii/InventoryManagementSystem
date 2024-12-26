using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.ServiceModel.Security;
using System.Windows;
using System.Windows.Controls;
using InventoryManagementSystem;
using project_ex;

namespace SalesOrderManagementApp
{
    public partial class SalesOrderManagement : Window
    {
        private string username;
        private string role;
        public SalesOrderManagement(string username, string role)
        {
            InitializeComponent();
            this.username = username;
            this.role = role;
            LoadProducts(); // Load products into combo box when the window is loaded
        }

        private string connectionString = new SQLiteConnectionStringBuilder
        {
            DataSource = "C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db",
            Version = 3,
            DefaultTimeout = 30
        }.ToString(); // Connection string for SQLite
        private long customerId = -1;
        private List<OrderItem> OrderItems = new List<OrderItem>(); // To hold the order items

        

        // OrderItem class to hold product details
        private class OrderItem
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice; // Calculate total price per item

        }


        // Load product details into the ComboBox (cmbProduct)
        private void LoadProducts()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Name FROM Products"; // Assuming 'Name' is correct
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    SQLiteDataReader reader = command.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        Console.WriteLine("No products found in the database."); // Replace MessageBox
                        return;
                    }

                    cmbProduct.Items.Clear(); // Clear existing items
                    while (reader.Read())
                    {
                        cmbProduct.Items.Add(reader["Name"].ToString()); // Add product names
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading product details: " + ex.Message); // Replace MessageBox
            }
        }



        // Event handler for Add Product button click
        // Event handler for Add Product button click
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            

            // Product selection and order item addition logic
            string Name;
            if (cmbProduct.SelectedItem is Product selectedProduct)
            {
                Name = selectedProduct.Name; // Retrieve the Name property from the selected object
            }
            else
            {
                Name = cmbProduct.SelectedItem?.ToString(); // Fallback for string items
            }

            if (string.IsNullOrEmpty(Name))
            {
                MessageBox.Show("Please select a product.");
                return;
            }

            decimal UnitPrice = GetProductPrice(Name);
            if (UnitPrice == 0)
            {
                MessageBox.Show("Error retrieving product price.");
                return;
            }

            string quantityInput = Microsoft.VisualBasic.Interaction.InputBox("Enter quantity:", "Product Quantity", "1");
            if (int.TryParse(quantityInput, out int quantity) && quantity > 0)
            {
                AddOrderItem(Name, quantity, UnitPrice);
                CalculateGrandTotal();
            }
            else
            {
                MessageBox.Show("Please enter a valid quantity.");
            }
        }


        // New event handler for TextChanged event
        private void txtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateGrandTotal(); // Call CalculateGrandTotal when text changes
        }


        // Method to get product price from the database
        private decimal GetProductPrice(string Name)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT UnitPrice FROM Products WHERE Name = @ProductName";
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@ProductName", Name);
                    var result = command.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving product price: " + ex.Message);
                return 0;
            }
        }

        // Add order item to the list and update the DataGrid
        private void AddOrderItem(string pName, int quantity, decimal price)
        {
            OrderItem newItem = new OrderItem
            {
                Name = pName,
                Quantity = quantity,
                UnitPrice = price
            };

            OrderItems.Add(newItem);
            dgOrderDetails.ItemsSource = null;
            dgOrderDetails.ItemsSource = OrderItems;  // Rebind to update the DataGrid
        }

        // Calculate the grand total of the order
        private void CalculateGrandTotal()
        {
            decimal subtotal = 0;
            foreach (var item in OrderItems)
            {
                subtotal += item.TotalPrice;
            }

            decimal discount = string.IsNullOrEmpty(txtDiscount.Text) ? 0 : Convert.ToDecimal(txtDiscount.Text);
            decimal salesTax = 1; // Logic for sales tax can be added as per your requirement

            txtSubtotal.Text = subtotal.ToString("F2");
            txtSalesTax.Text = salesTax.ToString("F2");
            txtTotalAmount.Text = (subtotal - discount + salesTax).ToString("F2");
        }

        // Event handler for Remove Item button click
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrderDetails.SelectedItem is OrderItem selectedItem)
            {
                OrderItems.Remove(selectedItem);
                dgOrderDetails.ItemsSource = null;
                dgOrderDetails.ItemsSource = OrderItems;  // Rebind to update the DataGrid
                CalculateGrandTotal();
            }
            else
            {
                MessageBox.Show("Please select an item to remove.");
            }
        }

        // Event handler for Place Order button click
        // Event handler for Place Order button click
        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            // Ensure customer information is provided
            if (string.IsNullOrEmpty(txtCustomerName.Text) ||
                string.IsNullOrEmpty(txtPhoneNumber.Text) ||
                string.IsNullOrEmpty(txtEmailAddress.Text) ||
                string.IsNullOrEmpty(txtAddress.Text))
            {
                MessageBox.Show("Please enter complete customer details before adding a product.");
                return;
            }

            // Save customer details only if not already saved
            if (customerId == -1)
            {
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();

                        string insertCustomerQuery = @"
                    INSERT INTO Customers (Name, PhoneNumber, EmailAddress, Address) 
                    VALUES (@Name, @PhoneNumber, @EmailAddress, @Address); 
                    SELECT last_insert_rowid();";

                        SQLiteCommand command = new SQLiteCommand(insertCustomerQuery, connection);
                        command.Parameters.AddWithValue("@Name", txtCustomerName.Text);
                        command.Parameters.AddWithValue("@PhoneNumber", txtPhoneNumber.Text);
                        command.Parameters.AddWithValue("@EmailAddress", txtEmailAddress.Text);
                        command.Parameters.AddWithValue("@Address", txtAddress.Text);

                        customerId = (long)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving customer details: " + ex.Message);
                    return;
                }
            }
            if (OrderItems.Count == 0)
            {
                MessageBox.Show("No items in the order.");
                return;
            }

            if (customerId == -1)
            {
                MessageBox.Show("Customer details are missing. Please add a product to save customer information first.");
                return;
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert each order item
                            foreach (var item in OrderItems)
                            {
                                string getProductIDQuery = "SELECT ProductID FROM Products WHERE Name = @ProductName";
                                SQLiteCommand productCommand = new SQLiteCommand(getProductIDQuery, connection);
                                productCommand.Parameters.AddWithValue("@ProductName", item.Name);

                                object productIdResult = productCommand.ExecuteScalar();
                                if (productIdResult == null)
                                {
                                    throw new Exception($"Product '{item.Name}' not found in the database.");
                                }

                                long productId = (long)productIdResult;

                                string insertOrderQuery = @"
                            INSERT INTO SaleOrders 
                            (Quantity, OrderDate, Status, TotalAmount, Discount, ProductID, CustomerID) 
                            VALUES 
                            (@Quantity, @OrderDate, @Status, @TotalAmount, @Discount, @ProductID, @CustomerID);";

                                SQLiteCommand orderCommand = new SQLiteCommand(insertOrderQuery, connection);
                                orderCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                                orderCommand.Parameters.AddWithValue("@Status", "Pending");
                                orderCommand.Parameters.AddWithValue("@TotalAmount", item.TotalPrice);
                                orderCommand.Parameters.AddWithValue("@Discount", string.IsNullOrEmpty(txtDiscount.Text) ? 0 : Convert.ToDecimal(txtDiscount.Text));
                                orderCommand.Parameters.AddWithValue("@ProductID", productId);
                                orderCommand.Parameters.AddWithValue("@CustomerID", customerId);

                                orderCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            MessageBox.Show("Order placed successfully.");
                            ClearOrder();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Error placing order: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }






        // Event handler for Cancel button click
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ClearOrder(); // Clear all order data when cancelling
            NotificationManager.Notify("Alert! The order is cancelled !");
        }

        // Clear the order details
        private void ClearOrder()
        {
            OrderItems.Clear();
            dgOrderDetails.ItemsSource = null;
            txtCustomerName.Clear();
            txtPhoneNumber.Clear();
            txtEmailAddress.Clear();
            txtAddress.Clear();
            txtDiscount.Clear();
            txtSalesTax.Clear();
            txtSubtotal.Clear();
            txtTotalAmount.Clear();
        }

        // Event handler for Back Button click
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            // Create a new instance of the Dashboard window
            Dashboard dashboardWindow = new Dashboard(username, "User"); // Pass the appropriate argument for the role

            // Show the Dashboard window
            dashboardWindow.Show();
        }

    }

}
