using System;
using System.Data.SQLite; // Ensure System.Data.SQLite NuGet package is installed
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using System.Windows.Media.Imaging;


namespace InventoryManagementSystem
{
    public partial class BarcodeWindow : Window
    {
        private string username;
        private string role;

        public BarcodeWindow(string username, string role)
        {
            InitializeComponent();
            this.username = username;
            this.role = role;
        }
        private string connectionString = new SQLiteConnectionStringBuilder
        {
            DataSource = "C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db",
            Version = 3,
            DefaultTimeout = 30
        }.ToString();
        private BitmapImage GenerateBarcodeImage(string barcodeText)
        {
            try
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 100,
                        Width = 300,
                        Margin = 10
                    },
                    Renderer = new ZXing.Rendering.BitmapRenderer()
                };

                using (var bitmap = writer.Write(barcodeText))
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate barcode image. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredBarcode = BarcodeTextBox.Text.Trim();

            // Fetch product from the database
            Product product = GetProductByBarcode(enteredBarcode);

            if (product != null)
            {
                // Display product info
                ProductName.Text = $"Product Name: {product.Name}";
                ProductCategory.Text = $"Category: {product.CategoryID}";
                ProductPrice.Text = $"Price: {product.UnitPrice:C}";
                ProductStock.Text = $"Stock: {product.Quantity}";
                ProductCreatedAt.Text = $"SKU: {product.SKU}";
                ProductUpdatedAt.Text = $"Updated At: {product.UpdatedAt}";

                // Generate and display barcode image dynamically
                var barcodeImage = GenerateBarcodeImage(product.Barcode);

                if (barcodeImage != null)
                {
                    ProductImage.Source = barcodeImage;
                }
                else
                {
                    MessageBox.Show("Failed to generate barcode. Displaying default image.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    ProductImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/DefaultProductImage.png"));
                }
            }
            else
            {
                MessageBox.Show("Barcode not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }


        private Product GetProductByBarcode(string barcode)
        {
            Product product = null;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Name, CategoryID, UnitPrice, Quantity, CreatedAt, UpdatedAt, SKU FROM Products WHERE Barcode = @Barcode";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Barcode", barcode);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                product = new Product
                                {
                                    Barcode = barcode,
                                    Name = reader["Name"].ToString(),
                                    CategoryID = reader["CategoryID"].ToString(),
                                    UnitPrice = Convert.ToDouble(reader["UnitPrice"]),
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    CreatedAt = reader["CreatedAt"].ToString(),
                                    UpdatedAt = reader["UpdatedAt"].ToString(),
                                    SKU = reader["SKU"].ToString() // Assuming SKU is a string
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

            return product;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the Dashboard
            Dashboard dashboardWindow = new Dashboard(username, "Admin");
            dashboardWindow.Show();
            this.Close(); // Close the current InventoryTracking window
        }
        public class Product
        {
            public string Barcode { get; set; }
            public string Name { get; set; }
            public string CategoryID { get; set; }
            public double UnitPrice { get; set; }
            public int Quantity { get; set; }
            public string ImagePath { get; set; }
            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
            public string SKU { get; set; }
        }
    }
}

