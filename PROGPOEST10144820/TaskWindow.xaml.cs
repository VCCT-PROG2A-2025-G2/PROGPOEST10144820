using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PROGPOE.Models;

namespace PROGPOE
{
    public partial class TaskWindow : Window
    {
        private List<TaskItem> allTasks;
        private ActivityLogger activityLogger;
        private ObservableCollection<TaskItem> taskItems;
        private TaskItem selectedTask;
        private bool isEditMode = false;

        public TaskWindow(ActivityLogger logger)
        {
            InitializeComponent();
            activityLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            allTasks = new List<TaskItem>();
            taskItems = new ObservableCollection<TaskItem>();
            InitializeTasks();
        }

        public TaskWindow()
        {
        }

        private void InitializeTasks()
        {
            try
            {
                // Initialize the task list with existing tasks
                RefreshTaskList();

                // Set up data binding
                TaskListBox.ItemsSource = taskItems;

                // Update task count
                UpdateTaskCount();

                // Set default reminder date to tomorrow
                ReminderDatePicker.SelectedDate = DateTime.Today.AddDays(1);

                // Log the initialization
                activityLogger.LogActivity("Task Manager Opened", "User accessed the task management interface");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing tasks: {ex.Message}", "Task Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTaskList()
        {
            taskItems.Clear();
            var tasks = TaskItem.GetAllTaskItems();
            foreach (var task in tasks)
            {
                taskItems.Add(task);
            }
            UpdateTaskCount();
        }

        private void UpdateTaskCount()
        {
            int totalTasks = taskItems.Count;
            int completedTasks = taskItems.Count(t => t.Status == "Completed");
            TaskCountLabel.Text = $"Tasks: {totalTasks} Total, {completedTasks} Completed";
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string taskTitle = TaskTitleTextBox.Text.Trim();
                string taskDescription = TaskDescriptionTextBox.Text.Trim();

                // Validate input
                if (string.IsNullOrEmpty(taskTitle))
                {
                    MessageBox.Show("Please enter a task title.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    TaskTitleTextBox.Focus();
                    return;
                }

                // Create the task
                DateTime? reminderDate = null;
                if (SetReminderCheckBox.IsChecked == true && ReminderDatePicker.SelectedDate.HasValue)
                {
                    reminderDate = ReminderDatePicker.SelectedDate.Value;

                    // Validate reminder date is not in the past
                    if (reminderDate.Value.Date < DateTime.Today)
                    {
                        MessageBox.Show("Reminder date cannot be in the past.", "Invalid Date",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (isEditMode && selectedTask != null)
                {
                    // Update existing task
                    UpdateTask(selectedTask, taskTitle, taskDescription, reminderDate);
                }
                else
                {
                    // Add new task
                    AddNewTask(taskTitle, taskDescription, reminderDate);
                }

                // Clear the form and refresh
                ClearForm();
                RefreshTaskList();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing task: {ex.Message}", "Task Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewTask(string title, string description, DateTime? reminderDate)
        {
            // Add cybersecurity context if not present
            if (string.IsNullOrEmpty(description))
            {
                description = GetCybersecurityDescription(title);
            }

            TaskItem.AddTask(title, description, reminderDate);

            string reminderInfo = reminderDate.HasValue ?
                $" with reminder set for {reminderDate.Value.ToShortDateString()}" : "";

            activityLogger.LogActivity($"Task Added: {title}",
                $"Description: {description}{reminderInfo}");

            MessageBox.Show($"Task '{title}' added successfully!{reminderInfo}", "Success",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateTask(TaskItem task, string title, string description, DateTime? reminderDate)
        {
            string oldTitle = task.Title;
            TaskItem.UpdateTask(task.Id, title, description, reminderDate);

            string reminderInfo = reminderDate.HasValue ?
                $" with reminder updated to {reminderDate.Value.ToShortDateString()}" : "";

            activityLogger.LogActivity($"Task Updated: {oldTitle} → {title}",
                $"Description updated{reminderInfo}");

            MessageBox.Show($"Task '{title}' updated successfully!{reminderInfo}", "Success",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GetCybersecurityDescription(string title)
        {
            string lowerTitle = title.ToLower();

            if (lowerTitle.Contains("password"))
                return "Update and strengthen your passwords to enhance account security.";
            if (lowerTitle.Contains("2fa") || lowerTitle.Contains("two-factor") || lowerTitle.Contains("authentication"))
                return "Enable two-factor authentication to add an extra layer of security to your accounts.";
            if (lowerTitle.Contains("privacy"))
                return "Review and update your privacy settings to protect your personal information.";
            if (lowerTitle.Contains("backup"))
                return "Create secure backups of your important data to prevent data loss.";
            if (lowerTitle.Contains("update") || lowerTitle.Contains("patch"))
                return "Keep your software updated to protect against security vulnerabilities.";
            if (lowerTitle.Contains("antivirus") || lowerTitle.Contains("malware"))
                return "Ensure your antivirus software is active and up-to-date.";
            if (lowerTitle.Contains("wifi") || lowerTitle.Contains("network"))
                return "Secure your network connections and avoid unsecured public Wi-Fi for sensitive activities.";

            return "Important cybersecurity task to maintain your digital safety and privacy.";
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask == null)
            {
                MessageBox.Show("Please select a task to mark as completed.", "No Task Selected",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                TaskItem.CompleteTask(selectedTask.Id);
                activityLogger.LogActivity($"Task Completed: {selectedTask.Title}",
                    "Task marked as completed by user");

                MessageBox.Show($"Task '{selectedTask.Title}' marked as completed!", "Task Completed",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                RefreshTaskList();
                ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing task: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask == null)
            {
                MessageBox.Show("Please select a task to delete.", "No Task Selected",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the task '{selectedTask.Title}'?",
                                       "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string taskTitle = selectedTask.Title;
                    TaskItem.DeleteTask(selectedTask.Id);
                    activityLogger.LogActivity($"Task Deleted: {taskTitle}", "Task removed by user");

                    MessageBox.Show($"Task '{taskTitle}' deleted successfully!", "Task Deleted",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    RefreshTaskList();
                    ClearSelection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting task: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask == null)
            {
                MessageBox.Show("Please select a task to edit.", "No Task Selected",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Populate form with selected task data
            TaskTitleTextBox.Text = selectedTask.Title;
            TaskDescriptionTextBox.Text = selectedTask.Description;

            if (selectedTask.ReminderDate.HasValue)
            {
                SetReminderCheckBox.IsChecked = true;
                ReminderDatePicker.SelectedDate = selectedTask.ReminderDate.Value;
                ReminderPanel.Visibility = Visibility.Visible;
            }
            else
            {
                SetReminderCheckBox.IsChecked = false;
                ReminderPanel.Visibility = Visibility.Collapsed;
            }

            // Switch to edit mode
            isEditMode = true;
            AddTaskButton.Content = "Update Task";

            activityLogger.LogActivity($"Task Edit Started: {selectedTask.Title}", "User began editing task");
        }

        private void TaskListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedTask = TaskListBox.SelectedItem as TaskItem;

            bool hasSelection = selectedTask != null;
            CompleteTaskButton.IsEnabled = hasSelection && selectedTask?.Status != "Completed";
            DeleteTaskButton.IsEnabled = hasSelection;
            EditTaskButton.IsEnabled = hasSelection;
        }

        private void SetReminderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ReminderPanel.Visibility = Visibility.Visible;
        }

        private void SetReminderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ReminderPanel.Visibility = Visibility.Collapsed;
        }

        private void RemindDayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int days))
            {
                ReminderDatePicker.SelectedDate = DateTime.Today.AddDays(days);
                SetReminderCheckBox.IsChecked = true;
                ReminderPanel.Visibility = Visibility.Visible;
            }
        }

        private void ClearFormButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            TaskTitleTextBox.Clear();
            TaskDescriptionTextBox.Clear();
            SetReminderCheckBox.IsChecked = false;
            ReminderPanel.Visibility = Visibility.Collapsed;
            ReminderDatePicker.SelectedDate = DateTime.Today.AddDays(1);

            // Reset edit mode
            isEditMode = false;
            AddTaskButton.Content = "Add Task";
            selectedTask = null;

            ClearSelection();
        }

        private void ClearSelection()
        {
            TaskListBox.SelectedItem = null;
            selectedTask = null;
            CompleteTaskButton.IsEnabled = false;
            DeleteTaskButton.IsEnabled = false;
            EditTaskButton.IsEnabled = false;
        }

        private void RefreshTasksButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshTaskList();
            activityLogger.LogActivity("Task List Refreshed", "User manually refreshed the task list");

            MessageBox.Show("Task list refreshed!", "Refresh Complete",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackToChatButton_Click(object sender, RoutedEventArgs e)
        {
            activityLogger.LogActivity("Task Manager Closed", "User returned to main chat interface");
            this.Close();
        }

        // Window closing event
        protected override void OnClosed(EventArgs e)
        {
            activityLogger.LogActivity("Task Manager Session Ended",
                $"Task management session completed. Final count: {taskItems.Count} tasks");
            base.OnClosed(e);
        }

        // Method to handle reminder notifications (can be called from main app)
        public void CheckAndShowReminders()
        {
            try
            {
                var todayReminders = taskItems.Where(t =>
                    t.ReminderDate.HasValue &&
                    t.ReminderDate.Value.Date == DateTime.Today &&
                    t.Status != "Completed").ToList();

                if (todayReminders.Any())
                {
                    string reminderMessage = "🔔 Reminder: You have tasks due today!\n\n";
                    foreach (var task in todayReminders)
                    {
                        reminderMessage += $"• {task.Title}\n";
                    }

                    MessageBox.Show(reminderMessage, "Task Reminders",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    activityLogger.LogActivity("Reminders Shown",
                        $"Displayed {todayReminders.Count} task reminders to user");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking reminders: {ex.Message}", "Reminder Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        internal void InitializeActivityLogger(ActivityLogger activityLogger)
        {
            throw new NotImplementedException();
        }

        internal void InitializeTaskEngine(TaskItem taskEngine)
        {
            throw new NotImplementedException();
        }
    }
}