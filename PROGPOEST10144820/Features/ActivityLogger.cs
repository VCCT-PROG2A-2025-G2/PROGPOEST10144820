using System;
using System.Collections.Generic;
using System.Linq;

namespace ST10144820_PROG_POE
{
    public class ActivityEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string Category { get; set; }
        public string UserInput { get; set; }

        public ActivityEntry()
        {
            Timestamp = DateTime.Now;
        }

        public ActivityEntry(string action, string details, string category = "General")
        {
            Timestamp = DateTime.Now;
            Action = action;
            Details = details;
            Category = category;
        }
    }

    public class ActivityLogger
    {
        private List<ActivityEntry> activities;
        private int nextId;
        private readonly int maxLogEntries;

        public event EventHandler<ActivityEntry> ActivityLogged;

        public ActivityLogger(int maxEntries = 100)
        {
            activities = new List<ActivityEntry>();
            nextId = 1;
            maxLogEntries = maxEntries;
            LogStartupActivity();
        }

        private void LogStartupActivity()
        {
            LogActivity("System Started", "Cybersecurity Assistant initialized", "System");
        }

        public void LogActivity(string action, string details, string category = "General", string userInput = null)
        {
            var entry = new ActivityEntry
            {
                Id = nextId++,
                Action = action,
                Details = details,
                Category = category,
                UserInput = userInput,
                Timestamp = DateTime.Now
            };

            activities.Add(entry);

            // Keep only the most recent entries
            if (activities.Count > maxLogEntries)
            {
                activities.RemoveAt(0);
            }

            ActivityLogged?.Invoke(this, entry);
        }

        public List<ActivityEntry> GetAllActivities()
        {
            return activities.OrderByDescending(a => a.Timestamp).ToList();
        }

        public List<ActivityEntry> GetRecentActivities(int count = 10)
        {
            return activities.OrderByDescending(a => a.Timestamp).Take(count).ToList();
        }

        public List<ActivityEntry> GetActivitiesByCategory(string category)
        {
            return activities.Where(a => a.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                           .OrderByDescending(a => a.Timestamp)
                           .ToList();
        }

        public List<ActivityEntry> GetActivitiesByDateRange(DateTime startDate, DateTime endDate)
        {
            return activities.Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                           .OrderByDescending(a => a.Timestamp)
                           .ToList();
        }

        public List<ActivityEntry> GetTodaysActivities()
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            return GetActivitiesByDateRange(today, tomorrow);
        }

        public void ClearLog()
        {
            activities.Clear();
            LogActivity("Log Cleared", "Activity log was cleared by user", "System");
        }

        public void LogTaskActivity(string action, CyberTask task)
        {
            string details = $"Task: '{task.Title}'";
            if (!string.IsNullOrEmpty(task.Description))
                details += $" - {task.Description}";

            LogActivity(action, details, "Tasks");
        }

        public void LogQuizActivity(string action, string details = "")
        {
            LogActivity(action, details, "Quiz");
        }

        public void LogChatActivity(string userMessage, string botResponse = "")
        {
            string details = $"User message: {userMessage}";
            if (!string.IsNullOrEmpty(botResponse))
                details += $" | Bot response: {botResponse.Substring(0, Math.Min(botResponse.Length, 100))}...";

            LogActivity("Chat Interaction", details, "Chat", userMessage);
        }

        public void LogNLPActivity(string intent, string originalInput)
        {
            LogActivity("NLP Processing", $"Intent detected: {intent} from input: '{originalInput}'", "NLP", originalInput);
        }

        public void LogSystemActivity(string action, string details = "")
        {
            LogActivity(action, details, "System");
        }

        public Dictionary<string, int> GetActivitySummary()
        {
            return activities.GroupBy(a => a.Category)
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<string, int> GetDailyActivityCount(int days = 7)
        {
            var result = new Dictionary<string, int>();
            var startDate = DateTime.Now.Date.AddDays(-days + 1);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dateString = date.ToString("MM/dd");
                var count = activities.Count(a => a.Timestamp.Date == date);
                result[dateString] = count;
            }

            return result;
        }

        public List<string> GetFrequentActions(int topCount = 5)
        {
            return activities.GroupBy(a => a.Action)
                           .OrderByDescending(g => g.Count())
                           .Take(topCount)
                           .Select(g => $"{g.Key} ({g.Count()})")
                           .ToList();
        }

        public int GetTotalActivityCount()
        {
            return activities.Count;
        }

        public ActivityEntry GetLastActivity()
        {
            return activities.OrderByDescending(a => a.Timestamp).FirstOrDefault();
        }

        public List<ActivityEntry> SearchActivities(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllActivities();

            var term = searchTerm.ToLower();
            return activities.Where(a =>
                a.Action.ToLower().Contains(term) ||
                a.Details.ToLower().Contains(term) ||
                a.Category.ToLower().Contains(term))
                .OrderByDescending(a => a.Timestamp)
                .ToList();
        }

        public void ExportActivities(string filePath)
        {
            try
            {
                var lines = new List<string>
                {
                    "Timestamp,Action,Details,Category"
                };

                foreach (var activity in GetAllActivities())
                {
                    var line = $"\"{activity.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{activity.Action}\",\"{activity.Details}\",\"{activity.Category}\"";
                    lines.Add(line);
                }

                System.IO.File.WriteAllLines(filePath, lines);
                LogActivity("Export Completed", $"Activities exported to {filePath}", "System");
            }
            catch (Exception ex)
            {
                LogActivity("Export Failed", $"Failed to export activities: {ex.Message}", "System");
            }
        }

        public string GetActivitySummaryText()
        {
            var summary = GetActivitySummary();
            var totalActivities = GetTotalActivityCount();
            var todayCount = GetTodaysActivities().Count;

            var text = $"Activity Summary:\n";
            text += $"Total Activities: {totalActivities}\n";
            text += $"Today's Activities: {todayCount}\n\n";
            text += "By Category:\n";

            foreach (var category in summary.OrderByDescending(kvp => kvp.Value))
            {
                text += $"• {category.Key}: {category.Value}\n";
            }

            var recentActions = GetFrequentActions(3);
            if (recentActions.Any())
            {
                text += "\nMost Frequent Actions:\n";
                foreach (var action in recentActions)
                {
                    text += $"• {action}\n";
                }
            }

            return text;
        }

        public void LogMultipleActivities(List<(string action, string details, string category)> activities)
        {
            foreach (var (action, details, category) in activities)
            {
                LogActivity(action, details, category);
            }
        }

        public bool HasActivitiesToday()
        {
            return GetTodaysActivities().Any();
        }

        public TimeSpan GetSessionDuration()
        {
            var firstActivity = activities.OrderBy(a => a.Timestamp).FirstOrDefault();
            var lastActivity = activities.OrderByDescending(a => a.Timestamp).FirstOrDefault();

            if (firstActivity != null && lastActivity != null)
            {
                return lastActivity.Timestamp - firstActivity.Timestamp;
            }

            return TimeSpan.Zero;
        }
    }
}