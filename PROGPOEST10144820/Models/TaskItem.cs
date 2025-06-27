// TaskItem.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PROGPOE.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        // Static list to store all tasks in memory
        private static List<TaskItem> _allTasks = new List<TaskItem>();

        private string _title;
        private string _description;
        private string _status;
        private DateTime? _reminderDate;
        private DateTime _createdDate;

        public Guid Id { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public DateTime? ReminderDate
        {
            get => _reminderDate;
            set
            {
                _reminderDate = value;
                OnPropertyChanged(nameof(ReminderDate));
                OnPropertyChanged(nameof(ReminderDateDisplay));
            }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged(nameof(CreatedDate));
            }
        }

        // Display property for reminder date
        public string ReminderDateDisplay
        {
            get
            {
                if (!ReminderDate.HasValue)
                    return "No reminder";
                if (ReminderDate.Value.Date == DateTime.Today)
                    return "Today";
                if (ReminderDate.Value.Date == DateTime.Today.AddDays(1))
                    return "Tomorrow";
                return ReminderDate.Value.ToShortDateString();
            }
        }

        public TaskItem()
        {
            Id = Guid.NewGuid();
            Status = "Pending";
            CreatedDate = DateTime.Now;
        }

        public TaskItem(string title, string description, DateTime? reminderDate = null) : this()
        {
            Title = title;
            Description = description;
            ReminderDate = reminderDate;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Static methods implementation
        internal static IEnumerable<TaskItem> GetAllTaskItems()
        {
            return _allTasks.ToList(); // Return a copy to prevent external modification
        }

        internal static void AddTask(string title, string description, DateTime? reminderDate)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title cannot be empty", nameof(title));

            var newTask = new TaskItem(title, description, reminderDate);
            _allTasks.Add(newTask);
        }

        internal static void UpdateTask(Guid id, string title, string description, DateTime? reminderDate)
        {
            var task = _allTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                throw new InvalidOperationException($"Task with ID {id} not found");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title cannot be empty", nameof(title));

            task.Title = title;
            task.Description = description;
            task.ReminderDate = reminderDate;
        }

        internal static void CompleteTask(Guid id)
        {
            var task = _allTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                throw new InvalidOperationException($"Task with ID {id} not found");

            task.Status = "Completed";
        }

        internal static void DeleteTask(Guid id)
        {
            var task = _allTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                throw new InvalidOperationException($"Task with ID {id} not found");

            _allTasks.Remove(task);
        }

        // Additional utility methods
        public static int GetTaskCount()
        {
            return _allTasks.Count;
        }

        public static int GetCompletedTaskCount()
        {
            return _allTasks.Count(t => t.Status == "Completed");
        }

        public static int GetPendingTaskCount()
        {
            return _allTasks.Count(t => t.Status == "Pending");
        }

        public static IEnumerable<TaskItem> GetTasksDueToday()
        {
            return _allTasks.Where(t =>
                t.ReminderDate.HasValue &&
                t.ReminderDate.Value.Date == DateTime.Today &&
                t.Status != "Completed");
        }

        // Override ToString for better display in ListBox
        public override string ToString()
        {
            string statusIcon = Status == "Completed" ? "✅" : "⏳";
            string reminderInfo = ReminderDate.HasValue ? $" (📅 {ReminderDateDisplay})" : "";
            return $"{statusIcon} {Title}{reminderInfo}";
        }
    }
}