using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Data.SQLite;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InventoryManagementSystem
{
    public partial class ProductWindow : Window
    {
        private string username;
        private string role;
        private string connectionString = new SQLiteConnectionStringBuilder
        {
            DataSource = "C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db",
            Version = 3,
            DefaultTimeout = 30
        }.ToString();

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the Dashboard
            Dashboard dashboardWindow = new Dashboard(username, "Admin");
            dashboardWindow.Show();
            this.Close(); // Close the current Product window
        }

        public ProductWindow(string username, string role)
        {
            InitializeComponent();
            this.username = username;
            this.role = role;
            LoadCategories();
            LoadProducts();
        }

        private void LoadCategories()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT CategoryID, CategoryName FROM Categories";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        var categories = new List<KeyValuePair<int, string>>();

                        while (reader.Read())
                        {
                            int categoryID = Convert.ToInt32(reader["CategoryID"]);
                            string categoryName = reader["CategoryName"].ToString();
                            categories.Add(new KeyValuePair<int, string>(categoryID, categoryName));
                        }

                        // Bind data to ComboBox
                        CategoryComboBox.ItemsSource = categories;
                        CategoryComboBox.DisplayMemberPath = "Value";
                        CategoryComboBox.SelectedValuePath = "Key";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT * FROM Products ORDER BY ProductID";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        var productList = new List<Product>();

                        while (reader.Read())
                        {
                            productList.Add(new Product
                            {
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                Name = reader["Name"].ToString(),
                                SKU = reader["SKU"].ToString(),
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                Barcode = reader["Barcode"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                            });
                        }

                        ProductListView.ItemsSource = productList;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string productName = ProductNameTextBox.Text.Trim();
                string sku = SKUTextBox.Text.Trim();
                string barcode = BarcodeTextBox.Text.Trim();
                int quantity = int.Parse(QuantityTextBox.Text.Trim());
                decimal unitPrice = decimal.Parse(UnitPriceTextBox.Text.Trim());

                if (CategoryComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Please select a valid category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(CategoryComboBox.SelectedValue.ToString(), out int categoryID))
                {
                    MessageBox.Show("Invalid category selection.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    // Insert product into Products table
                    string query = "INSERT INTO Products (Name, SKU, CategoryID, Quantity, UnitPrice, Barcode, CreatedAt, UpdatedAt) " +
                                   "VALUES (@Name, @SKU, @CategoryID, @Quantity, @UnitPrice, @Barcode, @CreatedAt, @UpdatedAt); " +
                                   "SELECT last_insert_rowid();";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", productName);
                        cmd.Parameters.AddWithValue("@SKU", sku);
                        cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                        cmd.Parameters.AddWithValue("@Barcode", barcode);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                        int productId = Convert.ToInt32(cmd.ExecuteScalar());

                        // Insert a new entry into the Warehouses table (hardcoded for now)
                        string warehouseQuery = "INSERT INTO Warehouses (WarehouseName, Location) VALUES (@WarehouseName, @Location); SELECT last_insert_rowid();";
                        int warehouseId;
                        Random random = new Random();
                        string[] WarehouseName  = { "Karachi Central Warehouse", "Lahore Storage Facility", "Islamabad North Hub", "Peshawar East Depot "};
                        string WName = WarehouseName[random.Next(WarehouseName.Length)];
                        using (SQLiteCommand warehouseCmd = new SQLiteCommand(warehouseQuery, conn))
                        {
                            warehouseCmd.Parameters.AddWithValue("@WarehouseName", WName = WarehouseName[random.Next(WarehouseName.Length)]);
                            warehouseCmd.Parameters.AddWithValue("@Location", "Main Location");
                            warehouseId = Convert.ToInt32(warehouseCmd.ExecuteScalar());
                        }

                        // Insert a new entry into BatchSerialTracking table (hardcoded values for now)
                        string batchQuery = "INSERT INTO BatchSerialTracking (ProductID, BatchNumber, SerialNumber, Quantity, ExpiryDate, WarehouseID) " +
                                            "VALUES (@ProductID, @BatchNumber, @SerialNumber, @Quantity, @ExpiryDate, @WarehouseID);";
                        using (SQLiteCommand batchCmd = new SQLiteCommand(batchQuery, conn))
                        {
                            batchCmd.Parameters.AddWithValue("@ProductID", productId);
                            batchCmd.Parameters.AddWithValue("@BatchNumber", "BATCH001");
                            batchCmd.Parameters.AddWithValue("@SerialNumber", Guid.NewGuid().ToString());
                            batchCmd.Parameters.AddWithValue("@Quantity", quantity);
                            batchCmd.Parameters.AddWithValue("@ExpiryDate", DateTime.Now.AddYears(1)); // 1-year expiry
                            batchCmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
                            batchCmd.ExecuteNonQuery();
                        }

                        // Insert a new entry into StockMovement table (hardcoded for now)
                        string stockMovementQuery = "INSERT INTO StockMovement (ProductID, MovementType, Quantity, MovementDate, Description, WarehouseID, BatchSerialID) " +
                                                    "VALUES (@ProductID, @MovementType, @Quantity, @MovementDate, @Description, @WarehouseID, (SELECT BatchSerialID FROM BatchSerialTracking WHERE ProductID = @ProductID));";
                        //default default = new Default();
                        string[] Description = { "Stock received in Karachi Central Warehouse", "Sale of product from Lahore Storage Facility", "Sale of product from Islamabad North Hub", "Stock received in Peshawar East Depot" };
                        string description = Description[random.Next(Description.Length)];
                        using (SQLiteCommand stockMovementCmd = new SQLiteCommand(stockMovementQuery, conn))
                        {
                            stockMovementCmd.Parameters.AddWithValue("@ProductID", productId);
                            stockMovementCmd.Parameters.AddWithValue("@MovementType", "IN");
                            stockMovementCmd.Parameters.AddWithValue("@Quantity", quantity);
                            stockMovementCmd.Parameters.AddWithValue("@MovementDate", DateTime.Now);
                            stockMovementCmd.Parameters.AddWithValue("@Description", description = Description[random.Next(Description.Length)]);
                            stockMovementCmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
                            stockMovementCmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Product and inventory details added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
                NotificationTextBlock.Text = $"New Product : {productName} added to the Inventory! Check Inventory for more details";
                NotificationPopup.IsOpen = true;

                await Task.Delay(5000); // Wait for 5 seconds to show the popup

                NotificationPopup.IsOpen = false; // Hide the popup after 5 seconds
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Method to handle RemoveProductButton click event
        private void RemoveProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductListView.SelectedItem == null)
                {
                    MessageBox.Show("Please select a product to remove.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Product selectedProduct = (Product)ProductListView.SelectedItem;

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    // Step 1: Delete the selected product
                    string deleteQuery = "DELETE FROM Products WHERE ProductID = @ProductID";
                    using (SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", selectedProduct.ProductID);
                        cmd.ExecuteNonQuery();
                    }

                    // Step 2: Create a temporary table
                    string tempTableQuery = @"
                CREATE TEMP TABLE ProductsTemp AS
                SELECT 
                    Name, 
                    SKU, 
                    CategoryID, 
                    Quantity, 
                    UnitPrice, 
                    Barcode, 
                    CreatedAt, 
                    UpdatedAt
                FROM Products;
            ";
                    using (SQLiteCommand cmd = new SQLiteCommand(tempTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Step 3: Clear the original table and reset the sequence
                    string clearTableQuery = "DELETE FROM Products; DELETE FROM sqlite_sequence WHERE name = 'Products';";
                    using (SQLiteCommand cmd = new SQLiteCommand(clearTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Step 4: Insert data back into the original table with new ProductID sequence
                    string resequenceQuery = @"
                INSERT INTO Products (ProductID, Name, SKU, CategoryID, Quantity, UnitPrice, Barcode, CreatedAt, UpdatedAt)
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY CreatedAt ASC) AS ProductID,
                    Name,
                    'SKU-' || ROW_NUMBER() OVER (ORDER BY CreatedAt ASC) AS SKU, -- Resequence SKU
                    CategoryID, 
                    Quantity, 
                    UnitPrice, 
                    '1234567890' || ROW_NUMBER() OVER (ORDER BY CreatedAt ASC) AS Barcode, -- Resequence Barcode
                    CreatedAt, 
                    UpdatedAt
                FROM ProductsTemp;
            ";
                    using (SQLiteCommand cmd = new SQLiteCommand(resequenceQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Step 5: Drop the temporary table
                    string dropTempTableQuery = "DROP TABLE ProductsTemp;";
                    using (SQLiteCommand cmd = new SQLiteCommand(dropTempTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Product removed successfully, and IDs resequenced!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Method to handle UpdateProductButton click event
        private void UpdateProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductListView.SelectedItem == null)
                {
                    MessageBox.Show("Please select a product to update.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Product selectedProduct = (Product)ProductListView.SelectedItem;
                string productName = ProductNameTextBox.Text.Trim();
                string sku = SKUTextBox.Text.Trim();
                string barcode = BarcodeTextBox.Text.Trim();
                int quantity = int.Parse(QuantityTextBox.Text.Trim());
                decimal unitPrice = decimal.Parse(UnitPriceTextBox.Text.Trim());

                if (CategoryComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Please select a valid category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(CategoryComboBox.SelectedValue.ToString(), out int categoryID))
                {
                    MessageBox.Show("Invalid category selection.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    // Check if SKU is unique
                    string checkSKUQuery = "SELECT COUNT(*) FROM Products WHERE SKU = @SKU AND ProductID != @ProductID";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkSKUQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@SKU", sku);
                        checkCmd.Parameters.AddWithValue("@ProductID", selectedProduct.ProductID);
                        long skuCount = (long)checkCmd.ExecuteScalar();
                        if (skuCount > 0)
                        {
                            MessageBox.Show("The SKU is already in use. Please use a unique SKU.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Update product
                    string updateQuery = "UPDATE Products SET Name = @Name, SKU = @SKU, CategoryID = @CategoryID, Quantity = @Quantity, UnitPrice = @UnitPrice, Barcode = @Barcode, UpdatedAt = @UpdatedAt WHERE ProductID = @ProductID";
                    using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", productName);
                        cmd.Parameters.AddWithValue("@SKU", sku);
                        cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                        cmd.Parameters.AddWithValue("@Barcode", barcode);
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ProductID", selectedProduct.ProductID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Method to handle SearchTextBox key-up event
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                string searchQuery = SearchTextBox.Text.Trim();

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT * FROM Products WHERE Name LIKE @Search OR SKU LIKE @Search";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchQuery + "%");

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            var productList = new List<Product>();
                            while (reader.Read())
                            {
                                productList.Add(new Product
                                {
                                    ProductID = Convert.ToInt32(reader["ProductID"]),
                                    Name = reader["Name"].ToString(),
                                    SKU = reader["SKU"].ToString(),
                                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                    Barcode = reader["Barcode"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                                });
                            }

                            ProductListView.ItemsSource = productList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public int CategoryID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Barcode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
