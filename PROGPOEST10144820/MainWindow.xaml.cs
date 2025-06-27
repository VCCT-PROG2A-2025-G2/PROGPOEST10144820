using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PROGPOE.Models;
using PROGPOEST10144820.Features;
using PROGPOEST10144820.QuizEngene;
using PROGPOEST10144820.TaskEngen;
using PROGPOEST10144820.Features; // Add this for your feature classes

namespace PROGPOE
{
    public partial class MainWindow : Window
    {
        private CyberBotEngine cyberBot;
        private bool isSetupComplete = false;

        // Part 3 Features
        private QuizEngen quizEngine;
        private TaskEngen taskEngine;
        private ActivityLogger activityLogger;
        private NLPProcessor nlpProcessor;

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
                quizEngine = new QuizEngen();
                taskEngine = new TaskEngen();
                activityLogger = new ActivityLogger();
                nlpProcessor = new NLPProcessor();

                // Log initialization
                activityLogger.LogActivity("System initialized with all features");
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
                activityLogger.LogActivity($"User setup completed: {UserNameTextBox.Text}");

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

                // Process with NLP first
                var intent = nlpProcessor.ProcessUserInput(message);

                // Handle different intents based on NLP processing
                await HandleUserIntent(message, intent);

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

        private async Task HandleUserIntent(string originalMessage, string intent)
        {
            string response = "";

            switch (intent.ToLower())
            {
                case "start_quiz":
                case "quiz":
                    response = "🎯 Starting Cybersecurity Quiz!\n\n" + quizEngine.StartQuiz();
                    activityLogger.LogActivity("Quiz started");
                    break;

                case "add_task":
                case "task":
                    response = await HandleTaskCreation(originalMessage);
                    break;

                case "view_tasks":
                    response = "📋 Your Current Tasks:\n\n" + taskEngine.GetAllTasks();
                    activityLogger.LogActivity("Viewed task list");
                    break;

                case "activity_log":
                case "log":
                    response = "📊 Recent Activity:\n\n" + activityLogger.GetRecentActivity();
                    break;

                case "help":
                    response = GetHelpMessage();
                    break;

                default:
                    // Process with original cyberBot if no specific intent matched
                    if (cyberBot != null)
                    {
                        await cyberBot.ProcessUserInputAsync(originalMessage);
                        return;
                    }
                    else
                    {
                        response = "I'm here to help with cybersecurity tasks, quizzes, and reminders. Try 'help' for more options!";
                    }
                    break;
            }

            // Add the response to chat
            AddChatMessage(response, isUser: false);
        }

        private async Task<string> HandleTaskCreation(string message)
        {
            try
            {
                // Extract task details from message using NLP
                var taskDetails = nlpProcessor.ExtractTaskDetails(message);

                if (string.IsNullOrEmpty(taskDetails.Title))
                {
                    return "❌ Please specify what task you'd like to add. For example: 'Add task to enable two-factor authentication'";
                }

                var task = taskEngine.AddTask(taskDetails.Title, taskDetails.Description, taskDetails.ReminderDate);
                activityLogger.LogActivity($"Task added: {taskDetails.Title}");

                string response = $"✅ Task added successfully!\n\n📝 Title: {task.Title}\n📄 Description: {task.Description}";

                if (task.ReminderDate.HasValue)
                {
                    response += $"\n⏰ Reminder: {task.ReminderDate:yyyy-MM-dd HH:mm}";
                }

                return response;
            }
            catch (Exception ex)
            {
                return $"❌ Error adding task: {ex.Message}";
            }
        }

        private string GetHelpMessage()
        {
            return @"🤖 CyberBot Commands Help:

📝 TASK MANAGEMENT:
• 'add task [description]' - Add a new cybersecurity task
• 'view tasks' - See all your tasks
• 'remind me to [task]' - Add task with reminder

🎯 QUIZ FEATURES:
• 'start quiz' - Begin cybersecurity quiz
• 'quiz' - Start a quick quiz session

📊 ACTIVITY & LOGS:
• 'activity log' - View recent actions
• 'what have you done' - Show activity summary

💡 GENERAL:
• 'help' - Show this help menu
• 'tips' - Get cybersecurity tips
• 'stats' - View your progress

Type naturally! I understand variations like:
'Can you remind me to update my password tomorrow?'
'Set a task for enabling 2FA'";
        }

        private async void QuickActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSetupComplete) return;

            var button = sender as Button;
            string command = "help";

            if (button != null)
            {
                switch (button.Name)
                {
                    case "HelpButton":
                        command = "help";
                        break;
                    case "TipsButton":
                        command = "tips";
                        break;
                    case "StatsButton":
                        command = "stats";
                        break;
                    case "MemoryButton":
                        command = "memory";
                        break;
                    case "QuizButton":
                        command = "start quiz";
                        break;
                    case "TasksButton":
                        command = "view tasks";
                        break;
                    case "ActivityButton":
                        command = "activity log";
                        break;
                }
            }

            await HandleUserIntent(command, command);
        }

        private async void VoiceToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSetupComplete || cyberBot == null) return;
            await cyberBot.ProcessUserInputAsync("voice toggle");
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings panel will be implemented in future tabs.", "Settings",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear the chat history?",
                                       "Clear Chat", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear all chat messages except the welcome message
                ChatMessagesPanel.Children.Clear();

                // Add back welcome message
                var welcomeBorder = new Border
                {
                    Style = (Style)FindResource("ChatBubbleStyle"),
                    Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                var welcomeText = new TextBlock
                {
                    Text = "🤖 Chat cleared! I'm ready to help you with cybersecurity questions, tasks, and quizzes.",
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14
                };

                welcomeBorder.Child = welcomeText;
                ChatMessagesPanel.Children.Add(welcomeBorder);

                activityLogger.LogActivity("Chat history cleared");
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
}