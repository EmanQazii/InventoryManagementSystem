using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;

namespace InventoryManagementSystem
{
    public partial class ReportsAndAnalytics : Window
    {
        private string username;
        private string role;
        private const string ConnectionString = "Data Source=C:\\Users\\noora\\source\\repos\\project ex\\project ex\\project ex\\InventoryManagement.db;Version=3;";

        public ReportsAndAnalytics(string username, string role)
        {
            InitializeComponent();
            this.username = username;
            this.role = role;
            LoadCharts();
        }

        private void LoadCharts()
        {
            LoadInventoryValuationChart();
            LoadSalesTrendsChart();
        }

        private void LoadInventoryValuationChart()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    var query = "SELECT CategoryID, SUM(UnitPrice * Quantity) AS Valuation FROM Products GROUP BY CategoryID";
                    var command = new SQLiteCommand(query, connection);

                    var seriesCollection = new SeriesCollection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seriesCollection.Add(new PieSeries
                            {
                                Title = reader["CategoryID"].ToString(),
                                Values = new ChartValues<double> { Convert.ToDouble(reader["Valuation"]) }
                            });
                        }
                    }

                    InventoryValuationPieChart.Series = seriesCollection;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Inventory Valuation chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the Dashboard
            Dashboard dashboardWindow = new Dashboard(username,"Admin");
            dashboardWindow.Show();
            this.Close(); // Close the current InventoryTracking window
        }
        private void LoadSalesTrendsChart()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    // Updated query to group by product category
                    var query = "SELECT CategoryID, SUM(TotalAmount) AS Sales FROM SaleOrders " +
                                "JOIN Products ON SaleOrders.ProductID = Products.ProductID " +
                                "GROUP BY CategoryID ORDER BY CategoryID";
                    var command = new SQLiteCommand(query, connection);

                    var salesSeries = new LineSeries
                    {
                        Title = "Sales by Category",
                        Values = new ChartValues<double>()
                    };

                    var labels = new List<string>();

                    using (var reader = command.ExecuteReader())
                    {
                        bool hasData = false;
                        while (reader.Read())
                        {
                            if (reader["Sales"] != DBNull.Value)
                            {
                                salesSeries.Values.Add(Convert.ToDouble(reader["Sales"]));
                                labels.Add(reader["CategoryID"].ToString());  // X-Axis labels as Category ID
                                hasData = true;
                            }
                        }

                        if (!hasData)
                        {
                            MessageBox.Show("No sales data available to display.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }

                    // Clear previous chart data and set new data
                    SalesTrendsLineChart.Series.Clear();
                    SalesTrendsLineChart.Series.Add(salesSeries);

                    // Set X-Axis labels as Product Categories
                    SalesTrendsLineChart.AxisX.Clear();
                    SalesTrendsLineChart.AxisX.Add(new Axis
                    {
                        Title = "Product Category ID",  // Change to appropriate label
                        Labels = labels
                    });

                    // Set Y-Axis for sales
                    SalesTrendsLineChart.AxisY.Clear();
                    SalesTrendsLineChart.AxisY.Add(new Axis
                    {
                        Title = "Sales (Rs Lakhs)",
                        LabelFormatter = value => value.ToString("C") // Format as currency
                    });
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



        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCharts();
        }
    }
}
