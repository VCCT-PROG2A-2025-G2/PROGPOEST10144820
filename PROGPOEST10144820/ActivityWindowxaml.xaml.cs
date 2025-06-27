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

                // More robust null checking
                if (logger == null)
                {
                    Console.WriteLine("ERROR: ActivityLogger parameter is null!");
                    MessageBox.Show("Cannot create Activity Window: ActivityLogger is required.",
                                   "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new ArgumentNullException(nameof(logger), "ActivityLogger cannot be null");
                }

                activityLogger = logger;
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

                // Log the window opening - with null check
                SafeLogActivity("Activity Log Opened", "User accessed the activity log viewer");

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
                // Check if activityLogger is available
                if (activityLogger == null)
                {
                    Console.WriteLine("Cannot load activities - ActivityLogger is null");
                    ShowEmptyState();
                    ActivitySummaryTextBlock.Text = "Activity logger not available";
                    return;
                }

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
            if (allActivities == null) return;

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
                if (allActivities == null)
                {
                    displayedActivities.Clear();
                    ShowEmptyState();
                    return;
                }

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
            if (ActivityListBox != null) ActivityListBox.Visibility = Visibility.Collapsed;
            if (EmptyStatePanel != null) EmptyStatePanel.Visibility = Visibility.Visible;
        }

        private void HideEmptyState()
        {
            if (ActivityListBox != null) ActivityListBox.Visibility = Visibility.Visible;
            if (EmptyStatePanel != null) EmptyStatePanel.Visibility = Visibility.Collapsed;
        }

        private void UpdateActivitySummary()
        {
            try
            {
                if (allActivities == null || ActivitySummaryTextBlock == null)
                {
                    if (ActivitySummaryTextBlock != null)
                        ActivitySummaryTextBlock.Text = "Unable to load activity summary";
                    return;
                }

                int totalActivities = allActivities.Count;
                int todayActivities = allActivities.Count(a => a.Timestamp.Date == DateTime.Today);

                ActivitySummaryTextBlock.Text = $"Total: {totalActivities} activities | Today: {todayActivities} activities";
            }
            catch (Exception ex)
            {
                if (ActivitySummaryTextBlock != null)
                    ActivitySummaryTextBlock.Text = "Unable to load activity summary";
            }
        }

        private void UpdateActivityStats()
        {
            try
            {
                if (ActivityStatsTextBlock == null) return;

                if (allActivities == null || displayedActivities == null)
                {
                    ActivityStatsTextBlock.Text = "Statistics unavailable";
                    return;
                }

                int totalActivities = allActivities.Count;
                int displayedCount = displayedActivities.Count;

                ActivityStatsTextBlock.Text = $"Total Activities: {totalActivities} | Displayed: {displayedCount}";
            }
            catch (Exception ex)
            {
                if (ActivityStatsTextBlock != null)
                    ActivityStatsTextBlock.Text = "Statistics unavailable";
            }
        }

        // Helper method for safe logging
        private void SafeLogActivity(string action, string description)
        {
            try
            {
                if (activityLogger != null)
                {
                    activityLogger.LogActivity(action, description);
                }
                else
                {
                    Console.WriteLine($"Cannot log activity - ActivityLogger is null. Action: {action}, Description: {description}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging activity: {ex.Message}");
            }
        }

        // Event Handlers - Fixed with null checks
        private void ActivityFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Add null check to prevent NullReferenceException
            if (activityLogger == null)
            {
                Console.WriteLine("ActivityLogger is null in ActivityFilterComboBox_SelectionChanged");
                return;
            }

            if (ActivityFilterComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                currentFilter = selectedItem.Tag?.ToString() ?? "All";
                ApplyFiltersAndUpdate();

                SafeLogActivity("Activity Filter Changed", $"Filter set to: {currentFilter}");
            }
        }

        private void ActivityLimitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Add null check to prevent NullReferenceException
            if (activityLogger == null)
            {
                Console.WriteLine("ActivityLogger is null in ActivityLimitComboBox_SelectionChanged");
                return;
            }

            if (ActivityLimitComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag?.ToString(), out int limit))
                {
                    currentLimit = limit;
                    ApplyFiltersAndUpdate();

                    string limitText = limit == 0 ? "All" : limit.ToString();
                    SafeLogActivity("Activity Limit Changed", $"Display limit set to: {limitText} activities");
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadActivities();
                if (LastUpdatedTextBlock != null)
                    LastUpdatedTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");

                MessageBox.Show("Activity log refreshed successfully!", "Refresh Complete",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                SafeLogActivity("Activity Log Refreshed", "User manually refreshed the activity log");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing activities: {ex.Message}", "Refresh Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (activityLogger == null)
            {
                MessageBox.Show("Activity logger is not available.", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to clear all activity logs? This action cannot be undone.",
                "Confirm Clear Log",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int clearedCount = allActivities?.Count ?? 0;
                    activityLogger.ClearLog();

                    // Reload to reflect changes
                    LoadActivities();

                    MessageBox.Show($"Successfully cleared {clearedCount} activity entries.", "Log Cleared",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    SafeLogActivity("Activity Log Cleared", $"User cleared {clearedCount} activity entries");
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
            if (activityLogger == null)
            {
                MessageBox.Show("Activity logger is not available.", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (allActivities == null || allActivities.Count == 0)
            {
                MessageBox.Show("No activities to export.", "Export Error",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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

                    SafeLogActivity("Activity Log Exported",
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
            if (allActivities == null) return;

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
                        string description = activity.Description?.ToString() ?? "";
                        writer.WriteLine($"\"{activity.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{activity.Action}\",\"{description}\"");
                    }
                }
                else
                {
                    // Text format
                    foreach (var activity in allActivities.OrderByDescending(a => a.Timestamp))
                    {
                        writer.WriteLine($"[{activity.Timestamp:yyyy-MM-dd HH:mm:ss}] {activity.Action}");
                        if (!string.IsNullOrWhiteSpace(activity.Description?.ToString()))
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

            // Optional: Log the selection for analytics
            if (ActivityListBox?.SelectedItem is ActivityLogEntry selectedActivity)
            {
                SafeLogActivity("Activity Selected", $"User selected activity: {selectedActivity.Action}");
            }
        }

        private void BackToChatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SafeLogActivity("Activity Log Closed", "User returned to main chat interface");
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during window close: {ex.Message}");
                // Still close the window even if logging fails
                this.Close();
            }
        }

        // Window event handlers
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Safe logging with null check
                if (activityLogger != null && allActivities != null)
                {
                    SafeLogActivity("Activity Log Session Ended",
                        $"Activity log session completed. Total activities viewed: {allActivities.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging window close: {ex.Message}");
                // Ignore logging errors when closing
            }

            base.OnClosed(e);
        }

        // Helper method to get activity category for better filtering
        private string GetActivityCategory(ActivityLogEntry activity)
        {
            if (activity == null) return "Unknown";

            string action = activity.Action?.ToLower() ?? "";
            string description = activity.Description?.ToString().ToLower() ?? "";

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
            if (LastUpdatedTextBlock != null)
                LastUpdatedTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        // Method to focus on recent activities (can be called when new activities are added)
        public void ScrollToTop()
        {
            if (ActivityListBox?.Items.Count > 0)
            {
                ActivityListBox.ScrollIntoView(ActivityListBox.Items[0]);
            }
        }

        // Additional helper method to check if the window is properly initialized
        public bool IsProperlyInitialized()
        {
            return activityLogger != null &&
                   displayedActivities != null &&
                   allActivities != null;
        }

        // Method to safely get activity count
        public int GetActivityCount()
        {
            return allActivities?.Count ?? 0;
        }

        // Method to safely get displayed activity count
        public int GetDisplayedActivityCount()
        {
            return displayedActivities?.Count ?? 0;
        }
    }

    // Extension class to add ShowDate property to ActivityLogEntry if needed
    public static class ActivityLogEntryExtensions
    {
        public static bool GetShowDate(ActivityLogEntry entry, DateTime? previousDate)
        {
            if (entry == null) return false;
            return previousDate == null || entry.Timestamp.Date != previousDate.Value.Date;
        }
    }
}