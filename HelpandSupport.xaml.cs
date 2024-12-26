using System;
using System.Windows;
using System.Windows.Controls;

namespace InventoryManagementSystem
{
    public partial class HelpAndSupportWindow : Window
    {
        private string userRole;
        private string username;
        public HelpAndSupportWindow(string username, string role)
        {
            InitializeComponent();
            this.username = username;
            userRole = role;
            ConfigureUIBasedOnRole();
        }

        private void ConfigureUIBasedOnRole()
        {
            if (userRole == "Admin")
            {
                LoadAdminContent();
            }
            else if (userRole == "User")
            {
                LoadUserContent();
            }
        }

        private void LoadAdminContent()
        {
            // Admin-specific manuals
            UserManualStackPanel.Children.Add(CreateManual("Manage Products",
                "This feature allows you to add, search, remove, and update products in the database. Navigate to 'Product Management' section to perform these actions."));

            UserManualStackPanel.Children.Add(CreateManual("Inventory Tracking",
                "This feature displays all inventory items and their details from the database. You can view and manage inventory quantities and product information."));

            UserManualStackPanel.Children.Add(CreateManual("Purchase Orders",
                "This section covers all supplier information, adding new suppliers, updating their statuses, and creating and tracking orders from the database."));

            UserManualStackPanel.Children.Add(CreateManual("Barcode",
                "This feature generates barcode images based on product numbers. It also displays the corresponding product details when the barcode is scanned."));

            UserManualStackPanel.Children.Add(CreateManual("Customer Management & Audit Log",
                "This feature contains customer information, order history, and audit logs for tracking customer activities."));

            UserManualStackPanel.Children.Add(CreateManual("Reports & Analytics",
                "Generates visual reports: a pie chart for inventory valuation and a line chart for sales trends across categories."));

            // Admin-specific FAQs
            FAQStackPanel.Children.Add(CreateFAQ("How to add a new product?",
                "Navigate to 'Manage Products', click 'Add New Product', fill in the details, and click 'Save'."));
            FAQStackPanel.Children.Add(CreateFAQ("How to view inventory?",
                "Go to 'Inventory Tracking' to view all products, their quantities, and other details."));
            FAQStackPanel.Children.Add(CreateFAQ("How to create a purchase order?",
                "Go to 'Purchase Orders', select 'Add Supplier', and enter the order details."));
            FAQStackPanel.Children.Add(CreateFAQ("How to generate reports?",
                "Navigate to 'Reports & Analytics', select the desired report, and view the results in graphical format."));
        }

        private void LoadUserContent()
        {
            // User-specific manuals
            UserManualStackPanel.Children.Add(CreateManual("Sales Order Management",
                "This feature allows you to input customer details, select products, and place orders for customers."));

            UserManualStackPanel.Children.Add(CreateManual("Profile & Order History",
                "This feature displays the user's profile information and shows past order history."));

            // User-specific FAQs
            FAQStackPanel.Children.Add(CreateFAQ("How to place a sales order?",
                "Navigate to 'Sales Order Management', enter customer details, select the products to buy, and place the order."));
            FAQStackPanel.Children.Add(CreateFAQ("How to view order history?",
                "Go to 'Profile & Order History' to view all past orders and their details."));
        }

        private StackPanel CreateManual(string title, string description)
        {
            StackPanel manualPanel = new StackPanel { Margin = new Thickness(5) };

            // Create the title TextBlock
            TextBlock titleTextBlock = new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.DarkBlue,
                Margin = new Thickness(0, 10, 0, 5)
            };

            // Create the description TextBlock
            TextBlock descriptionTextBlock = new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 5, 10, 10),
                Visibility = Visibility.Collapsed  // Change this to Visible
            };

            titleTextBlock.MouseLeftButtonUp += (sender, e) =>
            {
                descriptionTextBlock.Visibility = descriptionTextBlock.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            };

            // Add both the title and description to the manualPanel StackPanel
            manualPanel.Children.Add(titleTextBlock);
            manualPanel.Children.Add(descriptionTextBlock);

            return manualPanel;
        }

        private StackPanel CreateFAQ(string question, string answer)
        {
            StackPanel faqPanel = new StackPanel { Margin = new Thickness(5) };
            TextBlock questionTextBlock = new TextBlock
            {
                Text = question,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.DarkSlateGray
            };

            TextBlock answerTextBlock = new TextBlock
            {
                Text = answer,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 5, 10, 10),
                Visibility = Visibility.Collapsed  // Change this to Visible
            };

            // Toggle visibility of answer when question is clicked
            questionTextBlock.MouseLeftButtonUp += (s, e) =>
            {
                answerTextBlock.Visibility = answerTextBlock.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            };

            faqPanel.Children.Add(questionTextBlock);
            faqPanel.Children.Add(answerTextBlock);
            return faqPanel;
        }


        private void SubmitSupportForm_Click(object sender, RoutedEventArgs e)
        {
            string userName = UserNameTextBox.Text;
            string userEmail = UserEmailTextBox.Text;
            string message = SupportMessageTextBox.Text;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Please fill out all fields before submitting.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simulate sending a support request
            MessageBox.Show($"Thank you, {userName}. Your support request has been submitted.", "Support Submitted", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear form fields
            UserNameTextBox.Clear();
            UserEmailTextBox.Clear();
            SupportMessageTextBox.Clear();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard dashboardWindow = new Dashboard(username, userRole);
            dashboardWindow.Show();
            this.Close();
        }
    }
}
