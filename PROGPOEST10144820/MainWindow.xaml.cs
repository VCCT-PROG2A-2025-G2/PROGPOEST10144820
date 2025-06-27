using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PROGPOE.Models;
using static PROGPOE.Models.NLPProcessor;

namespace PROGPOE
{
    public partial class MainWindow : Window
    {
        private CyberBotEngine cyberBot;
        private bool isSetupComplete = false;

        // Part 3 Features
        private QuizQuestion quizEngine;
        private TaskItem taskEngine;
        private ActivityLogger activityLogger;
        private NLPProcessor nlpProcessor;

        // Windows for navigation
        private QuizWindow quizWindow;
        private TaskWindow taskWindow;
        private ActivityWindow activityWindow;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCyberBot();
            InitializePart3Features();
        }

        private void InitializePart3Features()
        {
            try
            {
                // Initialize Part 3 components  
                quizEngine = new QuizQuestion();
                taskEngine = new TaskItem();
                activityLogger = new ActivityLogger();
                nlpProcessor = new NLPProcessor();

                // Log initialization  
                activityLogger.LogActivity("System initialized with all features", "Initialization of Part 3 features completed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Part 3 features: {ex.Message}", "Initialization Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void InitializeCyberBot()
        {
            try
            {
                cyberBot = new CyberBotEngine();

                // Subscribe to events
                cyberBot.MessageReceived += OnMessageReceived;
                cyberBot.BotResponseReady += OnBotResponseReady;
                cyberBot.UserProfileUpdated += OnUserProfileUpdated;
                cyberBot.VoiceStatusChanged += OnVoiceStatusChanged;

                // Initialize the bot
                await cyberBot.InitializeAsync();

                StatusTextBlock.Text = "Ready! Please complete the setup to begin.";

                // Add welcome message to chat
                AddChatMessage("🤖 Welcome to CyberBot! I'm your cybersecurity assistant. I can help you with tasks, quizzes, and security advice. Please complete the setup to get started!", isUser: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing CyberBot: {ex.Message}", "Initialization Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                AddChatMessage(message, isUser: true);
            });
        }

        private void OnBotResponseReady(string response)
        {
            Dispatcher.Invoke(() =>
            {
                AddChatMessage(response, isUser: false);
            });
        }

        private void OnUserProfileUpdated(UserProfile profile)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUserProfileDisplay(profile);
            });
        }

        private void OnVoiceStatusChanged(bool isEnabled)
        {
            Dispatcher.Invoke(() =>
            {
                VoiceToggleButton.Content = isEnabled ? "🔊 Voice ON" : "🔇 Voice OFF";
                VoiceToggleButton.Background = isEnabled ?
                    new SolidColorBrush(Color.FromRgb(108, 92, 231)) :
                    new SolidColorBrush(Color.FromRgb(108, 117, 125));
            });
        }

        private void AddChatMessage(string message, bool isUser)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ChatBubbleStyle"),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Background = isUser ?
                    new SolidColorBrush(Color.FromRgb(108, 92, 231)) :
                    new SolidColorBrush(Color.FromRgb(15, 52, 96))
            };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                LineHeight = 20
            };

            border.Child = textBlock;
            ChatMessagesPanel.Children.Add(border);

            // Auto-scroll to bottom
            ChatScrollViewer.ScrollToEnd();
        }

        private void UpdateUserProfileDisplay(UserProfile profile)
        {
            UserNameTextBlock.Text = $"Name: {profile.Name}";
            LastInteractionTextBlock.Text = $"Last active: {profile.LastInteraction:HH:mm:ss}";
            CurrentMoodTextBlock.Text = $"Mood: {profile.LastSentiment}";
            QuestionsAskedTextBlock.Text = $"Questions: {profile.TotalQuestions}";
            TopicsExploredTextBlock.Text = $"Topics: {profile.TopicCounts.Count}/8";

            double progress = profile.TopicCounts.Count > 0 ? (profile.TopicCounts.Count / 8.0) * 100 : 0;
            ProgressTextBlock.Text = $"Progress: {progress:F1}%";
        }

        private async void SetupCompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
            {
                MessageBox.Show("Please enter your name to continue.", "Setup Required",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                UserNameTextBox.Focus();
                return;
            }

            try
            {
                SetupCompleteButton.IsEnabled = false;
                SetupCompleteButton.Content = "⏳ Setting up...";

                if (cyberBot != null)
                {
                    await cyberBot.SetupUserProfileAsync(
                        UserNameTextBox.Text.Trim(),
                        UserInterestsTextBox.Text.Trim());
                }

                // Hide setup panel and show chat input
                SetupPanel.Visibility = Visibility.Collapsed;
                ChatInputPanel.Visibility = Visibility.Visible;
                isSetupComplete = true;

                StatusTextBlock.Text = $"Chatting with {UserNameTextBox.Text} - AI Assistant Ready";

                // Log setup completion
                activityLogger.LogActivity($"User setup completed: {UserNameTextBox.Text}", "Setup process completed successfully.");

                // Add welcome message
                AddChatMessage($"🎉 Welcome {UserNameTextBox.Text}! I'm ready to help you with cybersecurity tasks, quizzes, and advice. Type 'help' to see what I can do!", isUser: false);

                ChatInputTextBox.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during setup: {ex.Message}", "Setup Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                SetupCompleteButton.IsEnabled = true;
                SetupCompleteButton.Content = "🎯 Start Chat";
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void ChatInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (!isSetupComplete) return;

            string message = ChatInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            try
            {
                // Disable input while processing  
                ChatInputTextBox.IsEnabled = false;
                SendButton.IsEnabled = false;
                SendButton.Content = "⏳ Sending...";

                // Clear input  
                ChatInputTextBox.Text = string.Empty;

                // Add user message to chat
                AddChatMessage(message, isUser: true);

                // Process with NLP first  
                var intent = nlpProcessor.ProcessInput(message);

                // Handle the processed intent with navigation
                await HandleUserIntentWithNavigation(message, intent);

                // Handle exit command  
                if (message.ToLower() == "exit")
                {
                    await Task.Delay(2000);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing message: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable input  
                ChatInputTextBox.IsEnabled = true;
                SendButton.IsEnabled = true;
                SendButton.Content = "📤 Send";
                ChatInputTextBox.Focus();
            }
        }

        private async Task HandleUserIntentWithNavigation(string message, object intent)
        {
            throw new NotImplementedException();
        }

        private async Task HandleUserIntentWithNavigation(string message, NLPIntent intent)
        {
            string response = "";

            try
            {
                switch (intent.IntentType.ToString().ToLower())
                {
                    case "start_quiz":
                    case "quiz":
                        NavigateToQuiz();
                        response = "🎯 Opening Quiz Window...";
                        activityLogger.LogActivity("Quiz window opened", "User navigated to quiz section.");
                        break;

                    case "quiz_answer":
                        if (quizEngine.IsQuizActive)
                        {
                            response = quizEngine.ProcessAnswer(message);
                            if (quizEngine.IsQuizActive)
                            {
                                response += "\n\n" + quizEngine.GetCurrentQuestion();
                            }
                            activityLogger.LogActivity("Quiz answer submitted", $"User answered: {message}");
                        }
                        else
                        {
                            NavigateToQuiz();
                            response = "🎯 Opening Quiz Window...";
                        }
                        break;

                    case "add_task":
                    case "task":
                        NavigateToTasks();
                        response = "📝 Opening Task Manager...";
                        activityLogger.LogActivity("Task window opened", "User navigated to task section.");
                        break;

                    case "view_tasks":
                        NavigateToTasks();
                        response = "📝 Opening Task Manager...";
                        activityLogger.LogActivity("Task window opened", "User requested to view all tasks.");
                        break;

                    case "activity_log":
                        NavigateToActivity();
                        response = "📊 Opening Activity Log...";
                        activityLogger.LogActivity("Activity window opened", "User navigated to activity section.");
                        break;

                    case "help":
                        response = GetHelpMessage();
                        activityLogger.LogActivity("Help requested", "User requested help menu.");
                        break;

                    case "tips":
                        response = GetTipsMessage();
                        activityLogger.LogActivity("Tips requested", "User requested cybersecurity tips.");
                        break;

                    case "stats":
                        response = GetStatsMessage(GetTaskEngine());
                        activityLogger.LogActivity("Stats requested", "User requested progress statistics.");
                        break;

                    default:
                        // Check if it's a quiz answer when quiz is active  
                        if (quizEngine.IsQuizActive && IsQuizAnswer(message))
                        {
                            response = quizEngine.ProcessAnswer(message);
                            if (quizEngine.IsQuizActive)
                            {
                                response += "\n\n" + quizEngine.GetCurrentQuestion();
                            }
                            activityLogger.LogActivity("Quiz answer submitted", $"User answered: {message}");
                        }
                        else
                        {
                            // Process with original cyberBot if no specific intent matched  
                            if (cyberBot != null)
                            {
                                await cyberBot.ProcessUserInputAsync(message);
                                return; // Exit early since cyberBot will handle the response
                            }
                            else
                            {
                                response = "I'm here to help with cybersecurity tasks, quizzes, and reminders. Try 'help' for more options!";
                                activityLogger.LogActivity("Unhandled intent", $"User input: {message}");
                            }
                        }
                        break;
                }

                // Add the response to chat (only if we didn't delegate to cyberBot)
                if (!string.IsNullOrEmpty(response))
                {
                    AddChatMessage(response, isUser: false);
                }
            }
            catch (Exception ex)
            {
                response = $"❌ Error processing your request: {ex.Message}";
                AddChatMessage(response, isUser: false);
            }
        }

        // Navigation methods to open specific windows
        private void NavigateToQuiz()
        {
            try
            {
                if (quizWindow == null || !quizWindow.IsLoaded)
                {
                    quizWindow = new QuizWindow(quizEngine, activityLogger);
                    quizWindow.Owner = this;
                }
                quizWindow.Show();
                quizWindow.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Quiz window: {ex.Message}", "Navigation Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToTasks()
        {
            try
            {
                if (taskWindow == null || !taskWindow.IsLoaded)
                {
                    // Pass the activityLogger to the TaskWindow constructor as required
                    taskWindow = new TaskWindow(activityLogger);
                    taskWindow.Owner = this;
                }
                taskWindow.Show();
                taskWindow.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Task window: {ex.Message}", "Navigation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToActivity()
        {
            try
            {
                Console.WriteLine("Starting NavigateToActivity...");

                if (activityWindow == null || !activityWindow.IsLoaded)
                {
                    Console.WriteLine("Creating new ActivityWindow...");

                    if (activityLogger == null)
                    {
                        throw new InvalidOperationException("ActivityLogger is null");
                    }

                    activityWindow = new ActivityWindow(activityLogger);
                    activityWindow.Owner = this;

                    // Add closed event handler to clean up reference
                    activityWindow.Closed += (s, e) => {
                        Console.WriteLine("ActivityWindow closed, cleaning up reference");
                        activityWindow = null;
                    };
                }

                Console.WriteLine("Showing ActivityWindow...");
                activityWindow.Show();
                activityWindow.Activate();

                Console.WriteLine("NavigateToActivity completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NavigateToActivity failed: {ex}");
                MessageBox.Show($"Error opening Activity window: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                               "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Clean up failed window reference
                activityWindow = null;
            }
        }

        private bool IsQuizAnswer(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;
            char firstChar = char.ToUpper(message.Trim()[0]);
            return firstChar >= 'A' && firstChar <= 'D';
        }

        private string GetHelpMessage()
        {
            return @"🤖 CyberBot Commands Help:

📝 TASK MANAGEMENT:
• 'add task [description]' - Open Task Manager
• 'view tasks' - Open Task Manager
• 'remind me to [task]' - Open Task Manager

🎯 QUIZ FEATURES:
• 'start quiz' - Open Quiz Window
• 'quiz' - Start a quick quiz session

📊 ACTIVITY & LOGS:
• 'activity log' - Open Activity Window

💡 GENERAL:
• 'help' - Show this help menu
• 'tips' - Get cybersecurity tips
• 'stats' - View your progress

Click the buttons on the right to navigate directly to different sections!";
        }

        private string GetTipsMessage()
        {
            return @"🔐 Cybersecurity Tips:

1. 🔑 Use strong, unique passwords for each account
2. 🛡️ Enable two-factor authentication wherever possible
3. 🔄 Keep your software and systems updated
4. 📧 Be cautious with email attachments and links
5. 🌐 Use secure networks and avoid public Wi-Fi for sensitive tasks
6. 💾 Regularly backup your important data
7. 🔍 Monitor your accounts for suspicious activity
8. 🚫 Don't share personal information on social media
9. 💻 Use reputable antivirus software
10. 🎓 Stay educated about current cyber threats";
        }

        private TaskItem GetTaskEngine()
        {
            return taskEngine;
        }

        private string GetStatsMessage(TaskItem taskEngine)
        {
            try
            {
                int totalTasks = TaskItem.GetTaskCount(); // Fixed by qualifying with the type name  
                int completedTasks = TaskItem.GetCompletedTaskCount(); // Fixed by qualifying with the type name  
                var recentActivities = activityLogger.GetRecentActivities();
                int activitiesCount = recentActivities?.Count ?? 0; // Fixed by removing parentheses from 'Count'  

                int quizzesTaken = QuizQuestion.GetQuizzesTakenCount(); // Fixed by qualifying with the type name  

                return $@"📊 Your CyberBot Statistics:  

                      📝 Tasks: {totalTasks} total ({completedTasks} completed)  
                      🎯 Quizzes taken: {quizzesTaken}  
                      📈 Recent activities: {activitiesCount}  
                      ⏰ Last active: {DateTime.Now:HH:mm:ss}  

                      Keep up the great work on your cybersecurity journey! 🚀";
            }
            catch (Exception)
            {
                return "📊 Statistics are currently being calculated. Please try again in a moment.";
            }
        }

        private async void QuickActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSetupComplete) return;

            var button = sender as Button;

            if (button != null)
            {
                switch (button.Name)
                {
                    case "HelpButton":
                        AddChatMessage(GetHelpMessage(), isUser: false);
                        break;
                    case "TipsButton":
                        AddChatMessage(GetTipsMessage(), isUser: false);
                        break;
                    case "StatsButton":
                        AddChatMessage(GetStatsMessage(GetTaskEngine()), isUser: false);
                        break;
                    case "QuizButton":
                        NavigateToQuiz();
                        AddChatMessage("🎯 Opening Quiz Window...", isUser: false);
                        break;
                    case "TasksButton":
                        NavigateToTasks();
                        AddChatMessage("📝 Opening Task Manager...", isUser: false);
                        break;
                    case "ActivityButton":
                        NavigateToActivity();
                        AddChatMessage("📊 Opening Activity Log...", isUser: false);
                        break;
                }
            }
        }

        private async void VoiceToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSetupComplete || cyberBot == null) return;
            await cyberBot.ProcessUserInputAsync("voice toggle");
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings panel will be implemented in future updates.", "Settings",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear the chat history?",
                                         "Clear Chat", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear all chat messages
                ChatMessagesPanel.Children.Clear();

                // Add back welcome message  
                AddChatMessage("🤖 Chat cleared! I'm ready to help you with cybersecurity questions, tasks, and quizzes.", isUser: false);

                // Log the activity
                activityLogger.LogActivity("Chat history cleared", "User cleared the chat history.");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up resources
            if (cyberBot != null)
            {
                cyberBot.MessageReceived -= OnMessageReceived;
                cyberBot.BotResponseReady -= OnBotResponseReady;
                cyberBot.UserProfileUpdated -= OnUserProfileUpdated;
                cyberBot.VoiceStatusChanged -= OnVoiceStatusChanged;
                cyberBot.Dispose();
            }

            // Close child windows
            quizWindow?.Close();
            taskWindow?.Close();
            activityWindow?.Close();

            base.OnClosed(e);
        }

        // Window dragging functionality
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }

    public class TaskDetails
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
    }
}