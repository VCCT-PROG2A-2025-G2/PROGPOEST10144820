using System;
using System.Collections.Generic;
using System.Linq;

namespace ST10144820_PROG_POE
{
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public string ReminderText { get; set; }
        public string Status { get; set; } // "Pending", "Completed", "Overdue"

        public CyberTask()
        {
            Status = "Pending";
            ReminderText = "None";
        }
    }

    public class TaskEngen
    {
        private List<CyberTask> tasks;
        private int nextId;

        public event EventHandler<CyberTask> TaskAdded;
        public event EventHandler<CyberTask> TaskCompleted;
        public event EventHandler<CyberTask> TaskDeleted;

        public TaskEngen()
        {
            tasks = new List<CyberTask>();
            nextId = 1;
            InitializeDefaultTasks();
        }

        private void InitializeDefaultTasks()
        {
            // Add some default cybersecurity tasks
            var defaultTasks = new[]
            {
                new CyberTask
                {
                    Id = nextId++,
                    Title = "Enable Two-Factor Authentication",
                    Description = "Set up 2FA on all important accounts (email, banking, social media)",
                    CreatedDate = DateTime.Now.AddDays(-2),
                    ReminderText = "In 1 day",
                    ReminderDate = DateTime.Now.AddDays(1)
                },
                new CyberTask
                {
                    Id = nextId++,
                    Title = "Update Passwords",
                    Description = "Change passwords for accounts that haven't been updated in 6+ months",
                    CreatedDate = DateTime.Now.AddDays(-1),
                    ReminderText = "In 3 days",
                    ReminderDate = DateTime.Now.AddDays(3)
                },
                new CyberTask
                {
                    Id = nextId++,
                    Title = "Review Privacy Settings",
                    Description = "Check and update privacy settings on social media platforms",
                    CreatedDate = DateTime.Now,
                    ReminderText = "None"
                }
            };

            tasks.AddRange(defaultTasks);
        }

        public void AddTask(CyberTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            task.Id = nextId++;
            task.CreatedDate = DateTime.Now;

            if (string.IsNullOrEmpty(task.Status))
                task.Status = "Pending";

            tasks.Add(task);
            TaskAdded?.Invoke(this, task);
        }

        public void AddTask(string title, string description = "", string reminderText = "", DateTime? reminderDate = null)
        {
            var task = new CyberTask
            {
                Title = title,
                Description = description,
                ReminderText = reminderText ?? "None",
                ReminderDate = reminderDate
            };

            AddTask(task);
        }

        public List<CyberTask> GetAllTasks()
        {
            UpdateTaskStatuses();
            return tasks.OrderByDescending(t => t.CreatedDate).ToList();
        }

        public List<CyberTask> GetPendingTasks()
        {
            UpdateTaskStatuses();
            return tasks.Where(t => t.Status == "Pending").OrderByDescending(t => t.CreatedDate).ToList();
        }

        public List<CyberTask> GetCompletedTasks()
        {
            return tasks.Where(t => t.Status == "Completed").OrderByDescending(t => t.CreatedDate).ToList();
        }

        public List<CyberTask> GetOverdueTasks()
        {
            UpdateTaskStatuses();
            return tasks.Where(t => t.Status == "Overdue").OrderByDescending(t => t.CreatedDate).ToList();
        }

        public void CompleteTask(int taskId)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.Status = "Completed";
                TaskCompleted?.Invoke(this, task);
            }
        }

        public void DeleteTask(int taskId)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                tasks.Remove(task);
                TaskDeleted?.Invoke(this, task);
            }
        }

        public CyberTask GetTask(int taskId)
        {
            return tasks.FirstOrDefault(t => t.Id == taskId);
        }

        public List<CyberTask> GetTasksDueToday()
        {
            var today = DateTime.Now.Date;
            return tasks.Where(t => t.ReminderDate?.Date == today && t.Status == "Pending").ToList();
        }

        public List<CyberTask> GetTasksDueSoon(int days = 3)
        {
            var cutoffDate = DateTime.Now.AddDays(days).Date;
            return tasks.Where(t => t.ReminderDate.HasValue &&
                                  t.ReminderDate.Value.Date <= cutoffDate &&
                                  t.Status == "Pending").ToList();
        }

        public int GetTaskCount()
        {
            return tasks.Count;
        }

        public int GetPendingTaskCount()
        {
            UpdateTaskStatuses();
            return tasks.Count(t => t.Status == "Pending");
        }

        public int GetCompletedTaskCount()
        {
            return tasks.Count(t => t.Status == "Completed");
        }

        private void UpdateTaskStatuses()
        {
            var now = DateTime.Now;
            foreach (var task in tasks.Where(t => t.Status == "Pending"))
            {
                if (task.ReminderDate.HasValue && task.ReminderDate.Value < now)
                {
                    task.Status = "Overdue";
                }
            }
        }

        public List<CyberTask> SearchTasks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllTasks();

            var term = searchTerm.ToLower();
            return tasks.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term))
                .OrderByDescending(t => t.CreatedDate)
                .ToList();
        }

        public void UpdateTask(int taskId, string title = null, string description = null,
                              string reminderText = null, DateTime? reminderDate = null)
        {
            var task = GetTask(taskId);
            if (task != null)
            {
                if (!string.IsNullOrEmpty(title))
                    task.Title = title;

                if (description != null)
                    task.Description = description;

                if (!string.IsNullOrEmpty(reminderText))
                    task.ReminderText = reminderText;

                if (reminderDate.HasValue)
                    task.ReminderDate = reminderDate;
            }
        }

        public List<string> GetCommonCyberSecurityTasks()
        {
            return new List<string>
            {
                "Enable two-factor authentication",
                "Update passwords",
                "Review privacy settings",
                "Install security updates",
                "Backup important data",
                "Check bank statements",
                "Review social media privacy",
                "Update antivirus software",
                "Secure home Wi-Fi network",
                "Enable device screen locks",
                "Review app permissions",
                "Set up VPN",
                "Create emergency contact list",
                "Review email security settings",
                "Check credit reports"
            };
        }

        public CyberTask CreateTaskFromTemplate(string template)
        {
            var templates = new Dictionary<string, (string title, string description)>
            {
                ["2fa"] = ("Enable Two-Factor Authentication", "Set up 2FA on important accounts for extra security"),
                ["password"] = ("Update Passwords", "Change weak or old passwords using strong, unique combinations"),
                ["privacy"] = ("Review Privacy Settings", "Check and update privacy settings on social media and online accounts"),
                ["backup"] = ("Backup Data", "Create secure backups of important files and documents"),
                ["update"] = ("Install Updates", "Install latest security updates for operating system and software"),
                ["wifi"] = ("Secure Wi-Fi", "Review and strengthen home Wi-Fi network security settings"),
                ["antivirus"] = ("Update Antivirus", "Ensure antivirus software is current and running scans"),
                ["banking"] = ("Check Financial Accounts", "Review bank and credit card statements for suspicious activity")
            };

            if (templates.ContainsKey(template.ToLower()))
            {
                var (title, description) = templates[template.ToLower()];
                return new CyberTask
                {
                    Title = title,
                    Description = description,
                    CreatedDate = DateTime.Now,
                    Status = "Pending"
                };
            }

            return null;
        }
    }
}