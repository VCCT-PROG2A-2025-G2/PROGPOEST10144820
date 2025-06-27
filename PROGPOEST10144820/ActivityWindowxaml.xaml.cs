using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using PROGPOE.Models;

namespace PROGPOE
{
    public partial class ActivityWindow : Window
    {
        private ActivityLogger activityLogger;
        private ObservableCollection<ActivityLogEntry> displayedActivities;
        private List<ActivityLogEntry> allActivities;
        private string currentFilter = "All";
        private int currentLimit = 10;

        public ActivityWindow(ActivityLogger logger)
        {
            try
            {
                Console.WriteLine("ActivityWindow constructor starting...");
                InitializeComponent();
                Console.WriteLine("InitializeComponent completed");

                activityLogger = logger ?? throw new ArgumentNullException(nameof(logger));
                Console.WriteLine("ActivityLogger assigned");

                displayedActivities = new ObservableCollection<ActivityLogEntry>();
                allActivities = new List<ActivityLogEntry>();
                Console.WriteLine("Collections initialized");

                InitializeWindow();
                Console.WriteLine("InitializeWindow completed");

                LoadActivities();
                Console.WriteLine("LoadActivities completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ActivityWindow constructor failed: {ex}");
                MessageBox.Show($"Failed to create Activity Window: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                               "Window Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Re-throw to prevent partially constructed window
            }
        }

        private void InitializeWindow()
        {
            try
            {
                // Set up data binding
                ActivityListBox.ItemsSource = displayedActivities;

                // Update last updated time
                LastUpdatedTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");

                // Log the window opening
                activityLogger.LogActivity("Activity Log Opened", "User accessed the activity log viewer");

                // Set up value converter for boolean to visibility (if not available in XAML)
                // This ensures the ShowDate property works correctly

                UpdateActivitySummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing activity window: {ex.Message}", "Initialization Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadActivities()
        {
            try
            {
                // Get all activities from the logger
                allActivities = activityLogger.GetAllActivities().ToList();

                // Process activities to add date information
                ProcessActivitiesForDisplay();

                // Apply current filters
                ApplyFiltersAndUpdate();

                UpdateActivitySummary();
                UpdateActivityStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading activities: {ex.Message}", "Load Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                ShowEmptyState();
            }
        }

        private void ProcessActivitiesForDisplay()
        {
            DateTime? lastDate = null;

            foreach (var activity in allActivities.OrderByDescending(a => a.Timestamp))
            {
                // Determine if we should show the date for this entry
                activity.ShowDate = lastDate == null ||
                                   activity.Timestamp.Date != lastDate.Value.Date;

                lastDate = activity.Timestamp;
            }
        }

        private void ApplyFiltersAndUpdate()
        {
            try
            {
                // Start with all activities  
                var filteredActivities = allActivities.AsEnumerable();

                // Apply category filter  
                if (currentFilter != "All")
                {
                    filteredActivities = filteredActivities.Where(a =>
                        a.Action.ToLower().Contains(currentFilter.ToLower()) ||
                        (a.Description is string description && description.ToLower().Contains(currentFilter.ToLower())));
                }

                // Order by timestamp (most recent first)  
                filteredActivities = filteredActivities.OrderByDescending(a => a.Timestamp);

                // Apply limit  
                if (currentLimit > 0)
                {
                    filteredActivities = filteredActivities.Take(currentLimit);
                }

                // Update displayed collection  
                displayedActivities.Clear();
                foreach (var activity in filteredActivities)
                {
                    displayedActivities.Add(activity);
                }

                // Show/hide empty state  
                if (displayedActivities.Count == 0)
                {
                    ShowEmptyState();
                }
                else
                {
                    HideEmptyState();
                }

                UpdateActivityStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Filter Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowEmptyState()
        {
            ActivityListBox.Visibility = Visibility.Collapsed;
            EmptyStatePanel.Visibility = Visibility.Visible;
        }

        private void HideEmptyState()
        {
            ActivityListBox.Visibility = Visibility.Visible;
            EmptyStatePanel.Visibility = Visibility.Collapsed;
        }

        private void UpdateActivitySummary()
        {
            try
            {
                int totalActivities = allActivities.Count;
                int todayActivities = allActivities.Count(a => a.Timestamp.Date == DateTime.Today);

                ActivitySummaryTextBlock.Text = $"Total: {totalActivities} activities | Today: {todayActivities} activities";
            }
            catch (Exception ex)
            {
                ActivitySummaryTextBlock.Text = "Unable to load activity summary";
            }
        }

        private void UpdateActivityStats()
        {
            try
            {
                int totalActivities = allActivities.Count;
                int displayedCount = displayedActivities.Count;

                ActivityStatsTextBlock.Text = $"Total Activities: {totalActivities} | Displayed: {displayedCount}";
            }
            catch (Exception ex)
            {
                ActivityStatsTextBlock.Text = "Statistics unavailable";
            }
        }

        // Event Handlers
        private void ActivityFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActivityFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                currentFilter = selectedItem.Tag?.ToString() ?? "All";
                ApplyFiltersAndUpdate();

                activityLogger.LogActivity("Activity Filter Changed",
                    $"Filter set to: {currentFilter}");
            }
        }

        private void ActivityLimitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActivityLimitComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag?.ToString(), out int limit))
                {
                    currentLimit = limit;
                    ApplyFiltersAndUpdate();

                    string limitText = limit == 0 ? "All" : limit.ToString();
                    activityLogger.LogActivity("Activity Limit Changed",
                        $"Display limit set to: {limitText} activities");
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadActivities();
                LastUpdatedTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");

                MessageBox.Show("Activity log refreshed successfully!", "Refresh Complete",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                activityLogger.LogActivity("Activity Log Refreshed", "User manually refreshed the activity log");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing activities: {ex.Message}", "Refresh Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all activity logs? This action cannot be undone.",
                "Confirm Clear Log",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int clearedCount = allActivities.Count;
                    activityLogger.ClearLog();

                    // Reload to reflect changes
                    LoadActivities();

                    MessageBox.Show($"Successfully cleared {clearedCount} activity entries.", "Log Cleared",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    activityLogger.LogActivity("Activity Log Cleared",
                        $"User cleared {clearedCount} activity entries");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing activity log: {ex.Message}", "Clear Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Activity Log",
                    Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"ActivityLog_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportActivitiesToFile(saveFileDialog.FileName);

                    MessageBox.Show($"Activity log exported successfully to:\n{saveFileDialog.FileName}",
                                  "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                    activityLogger.LogActivity("Activity Log Exported",
                        $"User exported activity log to: {Path.GetFileName(saveFileDialog.FileName)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting activity log: {ex.Message}", "Export Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportActivitiesToFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write header
                writer.WriteLine("=== CYBERSECURITY ASSISTANT ACTIVITY LOG ===");
                writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Total Activities: {allActivities.Count}");
                writer.WriteLine(new string('=', 50));
                writer.WriteLine();

                if (extension == ".csv")
                {
                    // CSV format
                    writer.WriteLine("Timestamp,Action,Description");
                    foreach (var activity in allActivities.OrderByDescending(a => a.Timestamp))
                    {
                        writer.WriteLine($"\"{activity.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{activity.Action}\",\"{activity.Description}\"");
                    }
                }
                else
                {
                    // Text format
                    foreach (var activity in allActivities.OrderByDescending(a => a.Timestamp))
                    {
                        writer.WriteLine($"[{activity.Timestamp:yyyy-MM-dd HH:mm:ss}] {activity.Action}");
                        if (!string.IsNullOrWhiteSpace((string)activity.Description))
                        {
                            writer.WriteLine($"  Description: {activity.Description}");
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        private void ActivityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // You can add functionality here to show more details about selected activity
            // For example, in a details panel or popup
        }

        private void BackToChatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                activityLogger.LogActivity("Activity Log Closed", "User returned to main chat interface");
                this.Close();
            }
            catch (Exception ex)
            {
                // Still close the window even if logging fails
                this.Close();
            }
        }

        // Window event handlers
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                activityLogger.LogActivity("Activity Log Session Ended",
                    $"Activity log session completed. Total activities viewed: {allActivities.Count}");
            }
            catch
            {
                // Ignore logging errors when closing
            }

            base.OnClosed(e);
        }

        // Helper method to get activity category for better filtering
        private string GetActivityCategory(ActivityLogEntry activity)
        {
            string action = activity.Action.ToLower();
            string description = activity.Description?.ToString().ToLower() ?? string.Empty;

            if (action.Contains("task") || description.Contains("task"))
                return "Task";
            if (action.Contains("quiz") || description.Contains("quiz"))
                return "Quiz";
            if (action.Contains("chat") || description.Contains("message") || description.Contains("response"))
                return "Chat";

            return "System";
        }

        // Method that can be called from the main window to refresh data
        public void RefreshActivityData()
        {
            LoadActivities();
            LastUpdatedTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        // Method to focus on recent activities (can be called when new activities are added)
        public void ScrollToTop()
        {
            if (ActivityListBox.Items.Count > 0)
            {
                ActivityListBox.ScrollIntoView(ActivityListBox.Items[0]);
            }
        }
    }

    // Extension class to add ShowDate property to ActivityLogEntry if needed
    public static class ActivityLogEntryExtensions
    {
        public static bool GetShowDate(ActivityLogEntry entry, DateTime? previousDate)
        {
            return previousDate == null || entry.Timestamp.Date != previousDate.Value.Date;
        }
    }
}