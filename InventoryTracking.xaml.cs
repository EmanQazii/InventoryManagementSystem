using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace InventoryManagementSystem
{
    public partial class InventoryTrackingWindow : Window
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
            this.Close(); // Close the current InventoryTracking window
        }

        public InventoryTrackingWindow(string username, string role)
        {
            InitializeComponent();
            this.username = username;
            this.role = role;
            LoadStockMovements();
        }

        // Method to load Stock Movements from the database
        private void LoadStockMovements()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT SM.BatchSerialID, 
                               P.Name AS Name, 
                               W.WarehouseName, 
                               SM.MovementDate, 
                               SM.MovementType, 
                               SM.Quantity, 
                               SM.Description
                        FROM StockMovement SM
                        INNER JOIN Products P ON SM.ProductID = P.ProductID
                        INNER JOIN Warehouses W ON SM.WarehouseID = W.WarehouseID";

                    var command = new SQLiteCommand(query, connection);
                    var adapter = new SQLiteDataAdapter(command);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Bind the data to the DataGrid
                    StockMovementsDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading stock movements: " + ex.Message);
            }
        }
    }
}
