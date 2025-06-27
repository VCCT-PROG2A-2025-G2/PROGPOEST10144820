using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PROGPOE.Models;

namespace PROGPOE
{
    public partial class ActivityWindow : Window
    {
        private ActivityLogger activityLogger;

        public ActivityWindow(ActivityLogger logger)
        {
            InitializeComponent(); // Ensure this method is correctly defined in the generated partial class  
            activityLogger = logger;

            InitializeActivity();
        }

        // Remove this duplicate definition of InitializeComponent()  
        // The method is already defined in the auto-generated partial class  
        // associated with the XAML file.  
        // private void InitializeComponent()  
        // {  
        //     throw new NotImplementedException();  
        // }  

        private void InitializeActivity()
        {
            try
            {
                DisplayActivities();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing activity log: {ex.Message}", "Activity Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayActivities()
        {
            try
            {
                var activities = activityLogger.GetRecentActivities();
                // Update UI with activities - you'll need a ListBox or similar in ActivityWindow.xaml  

                string activityText = string.Join("\n", activities.Take(20)); // Show last 20 activities  
                                                                              // Display in your UI element  
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying activities: {ex.Message}", "Display Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayActivities();
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear the activity log?",
                                         "Clear Activity Log", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear activities if your ActivityLogger has such method  
                // activityLogger.ClearActivities();  
                DisplayActivities();
            }
        }
    }
}
