using System;
using System.Collections.Generic;
using System.Linq;

namespace PROGPOE.Models
{
    public class ActivityLogger
    {
        private List<ActivityLogEntry> activities;
        private const int MAX_LOG_ENTRIES = 50; // Keep only the last 50 entries

        public ActivityLogger()
        {
            activities = new List<ActivityLogEntry>();
        }

        public void LogActivity(string action, string details = "")
        {
            var entry = new ActivityLogEntry
            {
                Id = Guid.NewGuid(),
                Action = action,
                Details = details,
                Timestamp = DateTime.Now
            };

            activities.Add(entry);

            // Keep only the most recent entries
            if (activities.Count > MAX_LOG_ENTRIES)
            {
                activities.RemoveAt(0);
            }
        }

        public List<ActivityLogEntry> GetRecentActivities(int count = 10)
        {
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public List<ActivityLogEntry> GetAllActivities()
        {
            return activities
                .OrderByDescending(a => a.Timestamp)
                .ToList();
        }

        public string GetActivitySummary(int count = 5)
        {
            var recentActivities = GetRecentActivities(count);

            if (!recentActivities.Any())
            {
                return "No recent activity logged.";
            }

            var summary = "Recent Activity:\n";
            for (int i = 0; i < recentActivities.Count; i++)
            {
                var activity = recentActivities[i];
                summary += $"{i + 1}. {activity.Action}";

                if (!string.IsNullOrEmpty(activity.Details))
                {
                    summary += $" - {activity.Details}";
                }

                summary += $" ({activity.Timestamp:HH:mm:ss})\n";
            }

            return summary;
        }

        public void ClearLog()
        {
            activities.Clear();
            LogActivity("Activity Log Cleared", "All previous log entries were removed");
        }

        public int GetLogCount()
        {
            return activities.Count;
        }
    }

    public class ActivityLogEntry
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }

        public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

        public string ShortDescription
        {
            get
            {
                if (string.IsNullOrEmpty(Details))
                    return Action;

                return $"{Action} - {Details}";
            }
        }
    }
}