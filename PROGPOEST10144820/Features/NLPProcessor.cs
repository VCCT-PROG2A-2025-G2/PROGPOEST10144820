using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ST10144820_PROG_POE
{
    public enum IntentType
    {
        AddTask,
        StartQuiz,
        ShowActivity,
        ShowTasks,
        SecurityAdvice,
        General,
        CompleteTask,
        DeleteTask,
        SetReminder,
        Help
    }

    public class NLPIntent
    {
        public IntentType Type { get; set; }
        public double Confidence { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string ReminderTime { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public object IntentType { get; internal set; }

        public NLPIntent()
        {
            Parameters = new Dictionary<string, object>();
            Confidence = 0.0;
        }
    }

    public class KeywordPattern
    {
        public string Pattern { get; set; }
        public IntentType Intent { get; set; }
        public double Weight { get; set; }
        public bool IsRegex { get; set; }

        public KeywordPattern(string pattern, IntentType intent, double weight = 1.0, bool isRegex = false)
        {
            Pattern = pattern;
            Intent = intent;
            Weight = weight;
            IsRegex = isRegex;
        }
    }

    public class NLPProcessor
    {
        private List<KeywordPattern> patterns;
        private Dictionary<string, string> taskKeywords;
        private Dictionary<string, string> timeKeywords;

        public NLPProcessor()
        {
            InitializePatterns();
            InitializeKeywords();
        }

        private void InitializePatterns()
        {
            patterns = new List<KeywordPattern>
            {
                // Add Task patterns
                new KeywordPattern(@"add\s+(task|reminder)", IntentType.AddTask, 2.0, true),
                new KeywordPattern(@"create\s+(task|reminder)", IntentType.AddTask, 2.0, true),
                new KeywordPattern(@"set\s+(task|reminder)", IntentType.AddTask, 2.0, true),
                new KeywordPattern("remind me", IntentType.AddTask, 1.5),
                new KeywordPattern("new task", IntentType.AddTask, 1.8),
                new KeywordPattern("add", IntentType.AddTask, 1.0),
                new KeywordPattern("create", IntentType.AddTask, 1.0),

                // Quiz patterns
                new KeywordPattern("start quiz", IntentType.StartQuiz, 2.0),
                new KeywordPattern("begin quiz", IntentType.StartQuiz, 2.0),
                new KeywordPattern("take quiz", IntentType.StartQuiz, 2.0),
                new KeywordPattern("quiz", IntentType.StartQuiz, 1.5),
                new KeywordPattern("test", IntentType.StartQuiz, 1.2),
                new KeywordPattern("game", IntentType.StartQuiz, 1.0),
                new KeywordPattern("play", IntentType.StartQuiz, 1.0),
                new KeywordPattern("challenge", IntentType.StartQuiz, 1.0),

                // Activity Log patterns
                new KeywordPattern("show activity", IntentType.ShowActivity, 2.0),
                new KeywordPattern("activity log", IntentType.ShowActivity, 2.0),
                new KeywordPattern("what have you done", IntentType.ShowActivity, 2.0),
                new KeywordPattern("recent actions", IntentType.ShowActivity, 1.8),
                new KeywordPattern("history", IntentType.ShowActivity, 1.5),
                new KeywordPattern("log", IntentType.ShowActivity, 1.2),
                new KeywordPattern("activity", IntentType.ShowActivity, 1.0),

                // Show Tasks patterns
                new KeywordPattern("show tasks", IntentType.ShowTasks, 2.0),
                new KeywordPattern("view tasks", IntentType.ShowTasks, 2.0),
                new KeywordPattern("list tasks", IntentType.ShowTasks, 2.0),
                new KeywordPattern("my tasks", IntentType.ShowTasks, 1.8),
                new KeywordPattern("tasks", IntentType.ShowTasks, 1.2),

                // Complete Task patterns
                new KeywordPattern(@"complete\s+task", IntentType.CompleteTask, 2.0, true),
                new KeywordPattern(@"finish\s+task", IntentType.CompleteTask, 2.0, true),
                new KeywordPattern(@"done\s+with", IntentType.CompleteTask, 1.8, true),
                new KeywordPattern("mark complete", IntentType.CompleteTask, 1.8),
                new KeywordPattern("completed", IntentType.CompleteTask, 1.5),
                new KeywordPattern("finished", IntentType.CompleteTask, 1.5),
                new KeywordPattern("done", IntentType.CompleteTask, 1.0),

                // Delete Task patterns
                new KeywordPattern(@"delete\s+task", IntentType.DeleteTask, 2.0, true),
                new KeywordPattern(@"remove\s+task", IntentType.DeleteTask, 2.0, true),
                new KeywordPattern("delete", IntentType.DeleteTask, 1.5),
                new KeywordPattern("remove", IntentType.DeleteTask, 1.5),
                new KeywordPattern("cancel", IntentType.DeleteTask, 1.2),

                // Set Reminder patterns
                new KeywordPattern(@"set\s+reminder", IntentType.SetReminder, 2.0, true),
                new KeywordPattern(@"add\s+reminder", IntentType.SetReminder, 2.0, true),
                new KeywordPattern("remind me", IntentType.SetReminder, 1.8),
                new KeywordPattern("reminder", IntentType.SetReminder, 1.5),

                // Security Advice patterns
                new KeywordPattern("security advice", IntentType.SecurityAdvice, 2.0),
                new KeywordPattern("security tip", IntentType.SecurityAdvice, 2.0),
                new KeywordPattern("cybersecurity", IntentType.SecurityAdvice, 1.8),
                new KeywordPattern("what should i do", IntentType.SecurityAdvice, 1.8),
                new KeywordPattern("how to", IntentType.SecurityAdvice, 1.5),
                new KeywordPattern("security", IntentType.SecurityAdvice, 1.2),
                new KeywordPattern("protect", IntentType.SecurityAdvice, 1.2),
                new KeywordPattern("safe", IntentType.SecurityAdvice, 1.0),

                // Help patterns
                new KeywordPattern("help", IntentType.Help, 2.0),
                new KeywordPattern("what can you do", IntentType.Help, 2.0),
                new KeywordPattern("commands", IntentType.Help, 1.8),
                new KeywordPattern("how to use", IntentType.Help, 1.8),
                new KeywordPattern("guide", IntentType.Help, 1.5),
                new KeywordPattern("instructions", IntentType.Help, 1.5)
            };
        }

        private void InitializeKeywords()
        {
            taskKeywords = new Dictionary<string, string>
            {
                { "2fa", "Enable Two-Factor Authentication" },
                { "two factor", "Enable Two-Factor Authentication" },
                { "two-factor", "Enable Two-Factor Authentication" },
                { "mfa", "Enable Multi-Factor Authentication" },
                { "password", "Update Password" },
                { "passwords", "Update Passwords" },
                { "privacy", "Review Privacy Settings" },
                { "privacy settings", "Review Privacy Settings" },
                { "vpn", "Set up VPN" },
                { "backup", "Create Data Backup" },
                { "backups", "Create Data Backups" },
                { "antivirus", "Update Antivirus Software" },
                { "firewall", "Configure Firewall" },
                { "phishing", "Learn About Phishing Protection" },
                { "social engineering", "Study Social Engineering Attacks" },
                { "malware", "Scan for Malware" },
                { "virus", "Scan for Viruses" },
                { "update", "Install Security Updates" },
                { "updates", "Install Security Updates" },
                { "wifi", "Secure WiFi Connection" },
                { "wi-fi", "Secure WiFi Connection" },
                { "email", "Configure Email Security" },
                { "browser", "Update Browser Security" },
                { "cookies", "Manage Browser Cookies" },
                { "encryption", "Enable Data Encryption" },
                { "ssl", "Verify SSL Certificates" },
                { "https", "Use HTTPS Connections" },
                { "patches", "Install Security Patches" }
            };

            timeKeywords = new Dictionary<string, string>
            {
                { "tomorrow", "In 1 day" },
                { "1 day", "In 1 day" },
                { "one day", "In 1 day" },
                { "next day", "In 1 day" },
                { "3 days", "In 3 days" },
                { "three days", "In 3 days" },
                { "7 days", "In 7 days" },
                { "week", "In 7 days" },
                { "one week", "In 7 days" },
                { "next week", "In 7 days" },
                { "2 weeks", "In 2 weeks" },
                { "two weeks", "In 2 weeks" },
                { "14 days", "In 2 weeks" },
                { "fortnight", "In 2 weeks" },
                { "month", "In 30 days" },
                { "1 month", "In 30 days" },
                { "30 days", "In 30 days" }
            };
        }

        public NLPIntent ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new NLPIntent { Type = IntentType.General, Confidence = 0.0 };
            }

            string normalizedInput = NormalizeInput(input);
            NLPIntent intent = AnalyzeIntent(normalizedInput);

            // Extract additional information based on intent type
            ExtractParameters(normalizedInput, intent);

            return intent;
        }

        private string NormalizeInput(string input)
        {
            // Convert to lowercase
            string normalized = input.ToLower().Trim();

            // Remove extra punctuation but keep hyphens and apostrophes
            normalized = Regex.Replace(normalized, @"[^\w\s\-']", " ");

            // Replace multiple spaces with single space
            normalized = Regex.Replace(normalized, @"\s+", " ");

            return normalized.Trim();
        }

        private NLPIntent AnalyzeIntent(string input)
        {
            var intentScores = new Dictionary<IntentType, double>();

            // Initialize scores for all intent types
            foreach (IntentType intentType in Enum.GetValues(typeof(IntentType)))
            {
                intentScores[intentType] = 0.0;
            }

            // Score each pattern
            foreach (var pattern in patterns)
            {
                double score = 0.0;

                if (pattern.IsRegex)
                {
                    if (Regex.IsMatch(input, pattern.Pattern, RegexOptions.IgnoreCase))
                    {
                        score = pattern.Weight;
                    }
                }
                else
                {
                    if (input.Contains(pattern.Pattern))
                    {
                        // Bonus for exact matches or word boundaries
                        if (input == pattern.Pattern)
                        {
                            score = pattern.Weight * 1.5;
                        }
                        else if (Regex.IsMatch(input, @"\b" + Regex.Escape(pattern.Pattern) + @"\b"))
                        {
                            score = pattern.Weight * 1.2;
                        }
                        else
                        {
                            score = pattern.Weight;
                        }
                    }
                }

                intentScores[pattern.Intent] += score;
            }

            // Apply contextual bonuses
            ApplyContextualBonuses(input, intentScores);

            // Find the best matching intent
            var bestIntent = intentScores.OrderByDescending(x => x.Value).First();

            return new NLPIntent
            {
                Type = bestIntent.Value > 0 ? bestIntent.Key : IntentType.General,
                Confidence = Math.Min(bestIntent.Value / 5.0, 1.0) // Normalize confidence to 0-1
            };
        }

        private void ApplyContextualBonuses(string input, Dictionary<IntentType, double> scores)
        {
            // Boost task-related intents if cybersecurity keywords are present
            if (ContainsCyberSecurityKeywords(input))
            {
                scores[IntentType.AddTask] += 0.5;
                scores[IntentType.SetReminder] += 0.3;
            }

            // Boost reminder intent if time keywords are present
            if (ContainsTimeKeywords(input))
            {
                scores[IntentType.AddTask] += 0.5;
                scores[IntentType.SetReminder] += 0.7;
            }

            // Boost question intents for question words
            if (input.Contains("what") || input.Contains("how") || input.Contains("why"))
            {
                scores[IntentType.SecurityAdvice] += 0.5;
                scores[IntentType.Help] += 0.3;
            }

            // Boost general intent for greetings
            if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey"))
            {
                scores[IntentType.General] += 1.0;
            }
        }

        private void ExtractParameters(string input, NLPIntent intent)
        {
            switch (intent.Type)
            {
                case IntentType.AddTask:
                case IntentType.SetReminder:
                    ExtractTaskInformation(input, intent);
                    break;

                case IntentType.CompleteTask:
                case IntentType.DeleteTask:
                    ExtractTaskReference(input, intent);
                    break;
            }

            // Always try to extract time information
            ExtractTimeInformation(input, intent);
        }

        private void ExtractTaskInformation(string input, NLPIntent intent)
        {
            // Extract task title from predefined keywords
            string taskTitle = ExtractPredefinedTask(input);

            if (!string.IsNullOrEmpty(taskTitle))
            {
                intent.TaskTitle = taskTitle;
                intent.TaskDescription = GenerateTaskDescription(taskTitle);
            }
            else
            {
                // Try to extract custom task title
                string customTitle = ExtractCustomTaskTitle(input);
                if (!string.IsNullOrEmpty(customTitle))
                {
                    intent.TaskTitle = CapitalizeWords(customTitle);
                    intent.TaskDescription = $"Complete: {CapitalizeWords(customTitle)}";
                }
            }

            // Store original input for context
            intent.Parameters["original_input"] = input;
        }

        private string ExtractPredefinedTask(string input)
        {
            // Sort by length (longest first) to match more specific terms first
            var sortedKeywords = taskKeywords.OrderByDescending(x => x.Key.Length);

            foreach (var keyword in sortedKeywords)
            {
                if (input.Contains(keyword.Key))
                {
                    return keyword.Value;
                }
            }

            return null;
        }

        private string ExtractCustomTaskTitle(string input)
        {
            // Patterns to extract custom task titles
            var patterns = new[]
            {
                @"add task (?:to )?(.+?)(?:\s+(?:in|tomorrow|next|for|by)|\s*$)",
                @"create task (?:to )?(.+?)(?:\s+(?:in|tomorrow|next|for|by)|\s*$)",
                @"remind me to (.+?)(?:\s+(?:in|tomorrow|next|for|by)|\s*$)",
                @"set (?:a )?reminder (?:to )?(.+?)(?:\s+(?:in|tomorrow|next|for|by)|\s*$)",
                @"new task (?:to )?(.+?)(?:\s+(?:in|tomorrow|next|for|by)|\s*$)",
                @"task (?:to )?(.+?)(?:\s+(?:in|tomorrow|next|for|by)|\s*$)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    string title = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrEmpty(title) && title.Length > 2)
                    {
                        return CleanTaskTitle(title);
                    }
                }
            }

            return null;
        }

        private string CleanTaskTitle(string title)
        {
            // Remove common stop words and clean up the title
            title = title.Trim();

            // Remove reminder-related words from the end
            var cleanupPatterns = new[]
            {
                @"\s+reminder\s*$",
                @"\s+today\s*$",
                @"\s+now\s*$"
            };

            foreach (var pattern in cleanupPatterns)
            {
                title = Regex.Replace(title, pattern, "", RegexOptions.IgnoreCase).Trim();
            }

            return title;
        }

        private void ExtractTaskReference(string input, NLPIntent intent)
        {
            // Try to extract task references for complete/delete operations
            var patterns = new[]
            {
                @"(?:complete|finish|done with|delete|remove) (?:task )?['""]?(.+?)['""]?$",
                @"(?:complete|finish|done with|delete|remove) (.+?)(?:\s+task)?$"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    intent.Parameters["task_reference"] = match.Groups[1].Value.Trim();
                    break;
                }
            }
        }

        private void ExtractTimeInformation(string input, NLPIntent intent)
        {
            foreach (var timeKeyword in timeKeywords)
            {
                if (input.Contains(timeKeyword.Key))
                {
                    intent.ReminderTime = timeKeyword.Value;
                    intent.Parameters["reminder_time"] = timeKeyword.Value;
                    break;
                }
            }

            // Extract specific dates or times using regex
            var timePatterns = new[]
            {
                @"in (\d+) days?",
                @"in (\d+) weeks?",
                @"in (\d+) hours?",
                @"(\d+) days? from now",
                @"(\d+) weeks? from now"
            };

            foreach (var pattern in timePatterns)
            {
                var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    int number = int.Parse(match.Groups[1].Value);
                    if (pattern.Contains("week"))
                    {
                        intent.ReminderTime = $"In {number} week{(number > 1 ? "s" : "")}";
                    }
                    else if (pattern.Contains("day"))
                    {
                        intent.ReminderTime = $"In {number} day{(number > 1 ? "s" : "")}";
                    }
                    else if (pattern.Contains("hour"))
                    {
                        intent.ReminderTime = $"In {number} hour{(number > 1 ? "s" : "")}";
                    }
                    break;
                }
            }
        }

        private string GenerateTaskDescription(string taskTitle)
        {
            var descriptions = new Dictionary<string, string>
            {
                { "Enable Two-Factor Authentication", "Set up 2FA on your important accounts to add an extra layer of security protection." },
                { "Enable Multi-Factor Authentication", "Configure MFA to enhance account security with multiple verification methods." },
                { "Update Password", "Change your password to a strong, unique combination using best security practices." },
                { "Update Passwords", "Review and update all your passwords to ensure they meet security standards." },
                { "Review Privacy Settings", "Check and update privacy settings on your accounts, social media, and applications." },
                { "Set up VPN", "Install and configure a VPN service for secure and private internet browsing." },
                { "Create Data Backup", "Back up your important files and data to protect against loss or ransomware." },
                { "Create Data Backups", "Establish a regular backup routine for all critical data and files." },
                { "Update Antivirus Software", "Ensure your antivirus software is current with the latest virus definitions." },
                { "Configure Firewall", "Set up and configure firewall settings to protect your network from threats." },
                { "Learn About Phishing Protection", "Study how to identify and avoid phishing attacks and social engineering." },
                { "Study Social Engineering Attacks", "Learn about social engineering tactics and develop defenses against them." },
                { "Scan for Malware", "Run a comprehensive system scan to detect and remove malicious software." },
                { "Scan for Viruses", "Perform a full antivirus scan to identify and eliminate virus threats." },
                { "Install Security Updates", "Install the latest security patches for your operating system and software." },
                { "Secure WiFi Connection", "Review and improve your WiFi security settings and encryption protocols." },
                { "Configure Email Security", "Set up email security features including spam filtering and encryption." },
                { "Update Browser Security", "Review and update your web browser's security and privacy settings." },
                { "Manage Browser Cookies", "Review and manage browser cookies to protect your online privacy." },
                { "Enable Data Encryption", "Set up encryption for sensitive files and communications." },
                { "Verify SSL Certificates", "Check and verify SSL certificates for secure website connections." },
                { "Use HTTPS Connections", "Ensure you're using secure HTTPS connections for sensitive activities." },
                { "Install Security Patches", "Apply the latest security patches to protect against vulnerabilities." }
            };

            return descriptions.TryGetValue(taskTitle, out string description)
                ? description
                : $"Complete the cybersecurity task: {taskTitle}";
        }

        private bool ContainsCyberSecurityKeywords(string input)
        {
            return taskKeywords.Keys.Any(keyword => input.Contains(keyword));
        }

        private bool ContainsTimeKeywords(string input)
        {
            return timeKeywords.Keys.Any(keyword => input.Contains(keyword));
        }

        private string CapitalizeWords(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Fixing the CS1503 error by using the correct overload of Split method  
            var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            return string.Join(" ", words);
        }

        // Replace the switch expression with a traditional switch statement to ensure compatibility with C# 7.3.
        public string GetIntentDescription(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.AddTask:
                    return "Add a new cybersecurity task";
                case IntentType.StartQuiz:
                    return "Start the cybersecurity quiz";
                case IntentType.ShowActivity:
                    return "Display activity log";
                case IntentType.ShowTasks:
                    return "Show current tasks";
                case IntentType.SecurityAdvice:
                    return "Provide security advice";
                case IntentType.CompleteTask:
                    return "Mark a task as completed";
                case IntentType.DeleteTask:
                    return "Delete a task";
                case IntentType.SetReminder:
                    return "Set a reminder";
                case IntentType.Help:
                    return "Show help information";
                case IntentType.General:
                    return "General conversation";
                default:
                    return "Unknown intent";
            }
        }
    }
}