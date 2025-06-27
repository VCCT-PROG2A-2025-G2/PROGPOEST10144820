using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PROGPOE.Models
{
    public class NLPProcessor
    {
        private Dictionary<string, List<string>> commandKeywords;
        private Dictionary<string, List<string>> taskKeywords;

        public NLPProcessor()
        {
            InitializeKeywords();
        }

        private void InitializeKeywords()
        {
            commandKeywords = new Dictionary<string, List<string>>
            {
                ["add_task"] = new List<string> { "add task", "create task", "new task", "task add", "add reminder", "remind me", "set reminder" },
                ["list_tasks"] = new List<string> { "list tasks", "show tasks", "view tasks", "my tasks", "task list", "tasks" },
                ["complete_task"] = new List<string> { "complete task", "finish task", "done task", "mark complete" },
                ["delete_task"] = new List<string> { "delete task", "remove task", "cancel task" },
                ["start_quiz"] = new List<string> { "start quiz", "begin quiz", "quiz", "test me", "cybersecurity quiz" },
                ["activity_log"] = new List<string> { "activity log", "show log", "what have you done", "recent activities", "show activities" },
                ["help"] = new List<string> { "help", "commands", "what can you do" }
            };

            taskKeywords = new Dictionary<string, List<string>>
            {
                ["security"] = new List<string> { "password", "2fa", "two factor", "authentication", "security", "privacy", "phishing", "malware" },
                ["time"] = new List<string> { "tomorrow", "today", "next week", "in 3 days", "in a week", "monday", "tuesday", "wednesday", "thursday", "friday" }
            };
        }

        public string ProcessCommand(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "unknown";

            string input = userInput.ToLower().Trim();

            // Check for each command type
            foreach (var commandType in commandKeywords.Keys)
            {
                if (commandKeywords[commandType].Any(keyword => input.Contains(keyword)))
                {
                    return commandType;
                }
            }

            return "unknown";
        }

        public string ExtractTaskTitle(string userInput)
        {
            string input = userInput.ToLower().Trim();

            // Remove command phrases
            var taskTriggers = new[] { "add task", "create task", "remind me to", "set reminder", "task add" };

            foreach (var trigger in taskTriggers)
            {
                if (input.Contains(trigger))
                {
                    input = input.Replace(trigger, "").Trim();
                    break;
                }
            }

            // Clean up common words
            input = Regex.Replace(input, @"\b(to|for|about|regarding)\b", "", RegexOptions.IgnoreCase).Trim();

            // Capitalize first letter
            if (input.Length > 0)
            {
                input = char.ToUpper(input[0]) + input.Substring(1);
            }

            return string.IsNullOrWhiteSpace(input) ? "Cybersecurity task" : input;
        }

        public DateTime? ExtractReminderDate(string userInput)
        {
            string input = userInput.ToLower();
            DateTime baseDate = DateTime.Now;

            // Check for specific time patterns
            if (input.Contains("tomorrow"))
                return baseDate.AddDays(1);

            if (input.Contains("today"))
                return baseDate;

            if (input.Contains("next week"))
                return baseDate.AddDays(7);

            // Check for "in X days" pattern
            var daysMatch = Regex.Match(input, @"in (\d+) days?");
            if (daysMatch.Success)
            {
                if (int.TryParse(daysMatch.Groups[1].Value, out int days))
                    return baseDate.AddDays(days);
            }

            // Check for "in X weeks" pattern
            var weeksMatch = Regex.Match(input, @"in (\d+) weeks?");
            if (weeksMatch.Success)
            {
                if (int.TryParse(weeksMatch.Groups[1].Value, out int weeks))
                    return baseDate.AddDays(weeks * 7);
            }

            return null; // No date found
        }

        public int ExtractTaskId(string userInput)
        {
            // Look for task ID patterns like "task 1", "task #1", "id 1", etc.
            var patterns = new[]
            {
                @"task (\d+)",
                @"task #(\d+)",
                @"id (\d+)",
                @"#(\d+)",
                @"\b(\d+)\b"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(userInput.ToLower(), pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
                {
                    return id;
                }
            }

            return -1; // No ID found
        }

        public bool ContainsCybersecurityTerms(string input)
        {
            return taskKeywords["security"].Any(term => input.ToLower().Contains(term));
        }

        public string GetSuggestedTaskDescription(string title)
        {
            string lowerTitle = title.ToLower();

            if (lowerTitle.Contains("password"))
                return "Review and update your passwords to ensure they are strong and unique.";

            if (lowerTitle.Contains("2fa") || lowerTitle.Contains("two factor"))
                return "Enable two-factor authentication on your important accounts for added security.";

            if (lowerTitle.Contains("privacy"))
                return "Review your privacy settings across social media and online accounts.";

            if (lowerTitle.Contains("phishing"))
                return "Learn about phishing attacks and how to identify suspicious emails.";

            if (lowerTitle.Contains("malware") || lowerTitle.Contains("antivirus"))
                return "Run a full system scan and update your antivirus software.";

            return "Complete this important cybersecurity task to improve your online safety.";
        }

        internal object ProcessInput(string message)
        {
            throw new NotImplementedException();
        }

        internal class NLPIntent
        {
            public object IntentType { get; internal set; }
        }
    }
}