using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PROGPOE.Models;

namespace PROGPOE
{
    public partial class QuizWindow : Window
    {
        private QuizManager quizManager;
        private ActivityLogger activityLogger;
        private QuizSession currentSession;
        private DispatcherTimer timer;
        private DateTime quizStartTime;
        private bool questionAnswered = false;

        public QuizWindow(ActivityLogger logger)
        {
            InitializeComponent();
            activityLogger = logger;
            quizManager = new QuizManager();

            InitializeTimer();
            SetInitialState();
        }

        public QuizWindow(QuizQuestion quiz, ActivityLogger logger) : this(logger)
        {
            // This constructor maintains compatibility with your original code
            // but we'll use QuizManager instead of the single QuizQuestion
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentSession != null)
            {
                var elapsed = DateTime.Now - quizStartTime;
                TimerTextBlock.Text = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            }
        }

        private void SetInitialState()
        {
            // Disable answer buttons initially
            SetAnswerButtonsEnabled(false);
            NextQuestionButton.IsEnabled = false;
            RestartQuizButton.IsEnabled = false;
            ViewResultsButton.IsEnabled = false;

            // Hide explanation panel
            ExplanationPanel.Visibility = Visibility.Collapsed;
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Start new quiz session
                currentSession = quizManager.StartNewQuiz(10); // 10 questions
                quizStartTime = DateTime.Now;

                // Update UI state
                StartQuizButton.IsEnabled = false;
                RestartQuizButton.IsEnabled = true;
                SetAnswerButtonsEnabled(true);

                // Start timer
                timer.Start();

                // Display first question
                DisplayCurrentQuestion();

                // Log activity
                activityLogger?.LogActivity("Quiz started", $"New cybersecurity quiz session began with {currentSession.Questions.Count} questions");

                StatusTextBlock.Text = "Quiz in progress - Select your answer for each question";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting quiz: {ex.Message}", "Quiz Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayCurrentQuestion()
        {
            if (currentSession == null || currentSession.IsCompleted)
                return;

            try
            {
                var currentQuestion = currentSession.GetCurrentQuestion();
                if (currentQuestion == null)
                {
                    CompleteQuiz();
                    return;
                }

                // Update progress
                QuestionProgressTextBlock.Text = $"Question {currentSession.CurrentQuestionIndex + 1} of {currentSession.Questions.Count}";
                QuizProgressBar.Maximum = currentSession.Questions.Count;
                QuizProgressBar.Value = currentSession.CurrentQuestionIndex + 1;

                // Update score display
                ScoreTextBlock.Text = currentSession.Score.ToString();
                TotalQuestionsTextBlock.Text = currentSession.Questions.Count.ToString();

                // Display question text
                QuestionTextBlock.Text = currentQuestion.Question;

                // Update category
                CategoryTextBlock.Text = $"Category: {currentQuestion.Category}";

                // Update answer buttons
                if (currentQuestion.Options.Count >= 4)
                {
                    AnswerButtonA.Content = $"A) {currentQuestion.Options[0]}";
                    AnswerButtonB.Content = $"B) {currentQuestion.Options[1]}";
                    AnswerButtonC.Content = $"C) {currentQuestion.Options[2]}";
                    AnswerButtonD.Content = $"D) {currentQuestion.Options[3]}";

                    // Show all buttons for multiple choice
                    AnswerButtonA.Visibility = Visibility.Visible;
                    AnswerButtonB.Visibility = Visibility.Visible;
                    AnswerButtonC.Visibility = Visibility.Visible;
                    AnswerButtonD.Visibility = Visibility.Visible;
                }
                else if (currentQuestion.IsTrueFalse && currentQuestion.Options.Count >= 2)
                {
                    // True/False question
                    AnswerButtonA.Content = $"A) {currentQuestion.Options[0]}";
                    AnswerButtonB.Content = $"B) {currentQuestion.Options[1]}";
                    AnswerButtonC.Visibility = Visibility.Collapsed;
                    AnswerButtonD.Visibility = Visibility.Collapsed;
                }

                // Reset question state
                questionAnswered = false;
                SetAnswerButtonsEnabled(true);
                NextQuestionButton.IsEnabled = false;
                ExplanationPanel.Visibility = Visibility.Collapsed;
                ResetAnswerButtonStyles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying question: {ex.Message}", "Display Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (questionAnswered || currentSession == null || currentSession.IsCompleted)
                return;

            var button = sender as Button;
            if (button?.Tag == null) return;

            try
            {
                int selectedIndex = Convert.ToInt32(button.Tag);
                var currentQuestion = currentSession.GetCurrentQuestion();

                if (currentQuestion == null) return;

                // Process the answer
                bool isCorrect = quizManager.AnswerQuestion(selectedIndex);
                questionAnswered = true;

                // Update UI based on answer
                UpdateAnswerButtonStyles(selectedIndex, currentQuestion.CorrectAnswerIndex, isCorrect);
                SetAnswerButtonsEnabled(false);

                // Show explanation
                if (!string.IsNullOrEmpty(currentQuestion.Explanation))
                {
                    ExplanationTextBlock.Text = currentQuestion.Explanation;
                    ExplanationPanel.Visibility = Visibility.Visible;
                }

                // Log the activity
                string answerText = currentQuestion.Options[selectedIndex];
                activityLogger?.LogActivity("Quiz answer submitted",
                    $"Question: {currentQuestion.Question.Substring(0, Math.Min(50, currentQuestion.Question.Length))}... | " +
                    $"Answer: {answerText} | Result: {(isCorrect ? "Correct" : "Incorrect")}");

                // Update status
                StatusTextBlock.Text = isCorrect ? "✅ Correct! Well done." : "❌ Incorrect. Check the explanation below.";

                // Update score display
                ScoreTextBlock.Text = currentSession.Score.ToString();

                // Enable next question or complete quiz
                if (currentSession.IsCompleted)
                {
                    CompleteQuiz();
                }
                else
                {
                    NextQuestionButton.IsEnabled = true;
                    StatusTextBlock.Text += " Click 'Next Question' to continue.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing answer: {ex.Message}", "Answer Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSession != null && !currentSession.IsCompleted)
            {
                DisplayCurrentQuestion();
            }
        }

        private void RestartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to restart the quiz? Your current progress will be lost.",
                                       "Restart Quiz", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RestartQuiz();
            }
        }

        private void ViewResultsButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSession != null && currentSession.IsCompleted)
            {
                string results = quizManager.GetQuizResults();
                MessageBox.Show(results, "Quiz Results", MessageBoxButton.OK, MessageBoxImage.Information);

                activityLogger?.LogActivity("Quiz results viewed",
                    $"Final Score: {currentSession.Score}/{currentSession.Questions.Count} ({currentSession.GetPercentageScore():F1}%)");
            }
        }

        private void CompleteQuiz()
        {
            timer.Stop();
            SetAnswerButtonsEnabled(false);
            NextQuestionButton.IsEnabled = false;
            ViewResultsButton.IsEnabled = true;
            StartQuizButton.IsEnabled = false;

            StatusTextBlock.Text = "🎉 Quiz completed! Click 'Results' to see your performance.";

            // Auto-show results
            ViewResultsButton_Click(null, null);

            activityLogger?.LogActivity("Quiz completed",
                $"Quiz finished - Score: {currentSession.Score}/{currentSession.Questions.Count} ({currentSession.GetPercentageScore():F1}%)");
        }

        private void RestartQuiz()
        {
            timer.Stop();
            currentSession = null;

            // Reset UI
            StartQuizButton.IsEnabled = true;
            RestartQuizButton.IsEnabled = false;
            ViewResultsButton.IsEnabled = false;
            NextQuestionButton.IsEnabled = false;

            SetAnswerButtonsEnabled(false);
            ResetAnswerButtonStyles();
            ExplanationPanel.Visibility = Visibility.Collapsed;

            // Reset displays
            QuestionTextBlock.Text = "Click 'Start Quiz' to begin your cybersecurity challenge!";
            CategoryTextBlock.Text = "Test Your Security Knowledge";
            QuestionProgressTextBlock.Text = "Question 1 of 10";
            QuizProgressBar.Value = 0;
            ScoreTextBlock.Text = "0";
            TimerTextBlock.Text = "00:00";
            StatusTextBlock.Text = "Ready to test your cybersecurity knowledge? Click 'Start Quiz' to begin!";

            activityLogger?.LogActivity("Quiz restarted", "User chose to restart the quiz");
        }

        private void SetAnswerButtonsEnabled(bool enabled)
        {
            AnswerButtonA.IsEnabled = enabled;
            AnswerButtonB.IsEnabled = enabled;
            AnswerButtonC.IsEnabled = enabled;
            AnswerButtonD.IsEnabled = enabled;
        }

        private void UpdateAnswerButtonStyles(int selectedIndex, int correctIndex, bool isCorrect)
        {
            // Reset all buttons
            ResetAnswerButtonStyles();

            var buttons = new[] { AnswerButtonA, AnswerButtonB, AnswerButtonC, AnswerButtonD };

            // Highlight correct answer in green
            if (correctIndex < buttons.Length)
            {
                buttons[correctIndex].Background = System.Windows.Media.Brushes.LightGreen;
            }

            // If selected answer was wrong, highlight it in red
            if (!isCorrect && selectedIndex < buttons.Length)
            {
                buttons[selectedIndex].Background = System.Windows.Media.Brushes.LightCoral;
            }
        }

        private void ResetAnswerButtonStyles()
        {
            var buttons = new[] { AnswerButtonA, AnswerButtonB, AnswerButtonC, AnswerButtonD };
            foreach (var button in buttons)
            {
                button.ClearValue(BackgroundProperty);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();
            base.OnClosed(e);
        }
    }
}