using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PROGPOE.Models;

namespace PROGPOE
{
    public partial class MainWindow : Window
    {
        private CyberBotEngine cyberBot;
        private bool isSetupComplete = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCyberBot();
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
            if (!isSetupComplete || cyberBot == null) return;

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

                // Process the message
                await cyberBot.ProcessUserInputAsync(message);

                // Handle exit command
                if (message.ToLower() == "exit")
                {
                    await Task.Delay(2000); // Give time to read goodbye message
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

        private async void QuickActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSetupComplete || cyberBot == null) return;

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
                }
            }

            await cyberBot.ProcessUserInputAsync(command);
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
                    Text = "🤖 Chat cleared! I'm ready to help you with cybersecurity questions.",
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14
                };

                welcomeBorder.Child = welcomeText;
                ChatMessagesPanel.Children.Add(welcomeBorder);
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
