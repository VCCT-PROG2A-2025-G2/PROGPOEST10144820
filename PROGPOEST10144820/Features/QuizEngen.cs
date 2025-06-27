using System;
using System.Collections.Generic;
using System.Linq;

namespace ST10144820_PROG_POE
{
    public class QuizQuestion
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
        public string Category { get; set; }
        public bool IsAnswered { get; set; }
        public bool AnsweredCorrectly { get; set; }

        public QuizQuestion()
        {
            Options = new List<string>();
        }
    }

    public class QuizScore
    {
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int QuestionsAnswered { get; set; }
        public double Percentage => TotalQuestions > 0 ? (double)CorrectAnswers / TotalQuestions * 100 : 0;
        public DateTime CompletedDate { get; set; }
    }

    public class QuizEngen
    {
        private List<QuizQuestion> questions;
        private List<QuizQuestion> currentQuizQuestions;
        private int currentQuestionIndex;
        private int correctAnswers;
        private bool isQuizActive;

        public event EventHandler QuizStarted;
        public event EventHandler<QuizScore> QuizCompleted;
        public event EventHandler<QuizQuestion> QuestionAnswered;

        public int CurrentQuestionIndex => currentQuestionIndex;
        public int TotalQuestions => currentQuizQuestions?.Count ?? 0;
        public bool IsQuizActive => isQuizActive;

        public QuizEngen()
        {
            InitializeQuestions();
            ResetQuiz();
        }

        private void InitializeQuestions()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    QuestionText = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Always report phishing emails to help prevent scams and protect others.",
                    Category = "Phishing"
                },
                new QuizQuestion
                {
                    QuestionText = "Which of the following makes a password strong?",
                    Options = new List<string> { "Using your birthday", "A mix of letters, numbers, and symbols", "Your pet's name", "A common word" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Strong passwords combine uppercase/lowercase letters, numbers, and special characters.",
                    Category = "Password Security"
                },
                new QuizQuestion
                {
                    QuestionText = "What is two-factor authentication (2FA)?",
                    Options = new List<string> { "Using two passwords", "An extra security step beyond passwords", "Having two accounts", "Logging in twice" },
                    CorrectAnswerIndex = 1,
                    Explanation = "2FA adds an extra layer of security by requiring a second form of verification.",
                    Category = "Authentication"
                },
                new QuizQuestion
                {
                    QuestionText = "Is it safe to use public Wi-Fi for online banking?",
                    Options = new List<string> { "Yes, always", "No, never without proper protection", "Only on weekends", "Only for small amounts" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Public Wi-Fi can be intercepted. Use a VPN or mobile data for sensitive activities.",
                    Category = "Network Security"
                },
                new QuizQuestion
                {
                    QuestionText = "What is social engineering in cybersecurity?",
                    Options = new List<string> { "Building social networks", "Manipulating people to reveal information", "Programming social media", "Creating online communities" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Social engineering tricks people into giving away confidential information or access.",
                    Category = "Social Engineering"
                },
                new QuizQuestion
                {
                    QuestionText = "How often should you update your software?",
                    Options = new List<string> { "Once a year", "Only when it breaks", "As soon as updates are available", "Never" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Regular updates patch security vulnerabilities and keep your system protected.",
                    Category = "Software Security"
                },
                new QuizQuestion
                {
                    QuestionText = "What is ransomware?",
                    Options = new List<string> { "Free software", "Malware that encrypts your files for money", "A type of antivirus", "A backup system" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Ransomware locks your files and demands payment for the decryption key.",
                    Category = "Malware"
                },
                new QuizQuestion
                {
                    QuestionText = "Which is the safest way to share sensitive information?",
                    Options = new List<string> { "Email", "Text message", "Encrypted messaging app", "Social media" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Encrypted messaging apps protect your conversations from interception.",
                    Category = "Communication Security"
                },
                new QuizQuestion
                {
                    QuestionText = "What should you do before disposing of an old computer?",
                    Options = new List<string> { "Just throw it away", "Sell it immediately", "Wipe the hard drive completely", "Remove the monitor" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Completely wiping the hard drive prevents data recovery by unauthorized persons.",
                    Category = "Data Protection"
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: It's safe to click on links in emails from unknown senders.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Never click links from unknown senders - they may lead to malicious websites or downloads.",
                    Category = "Email Security"
                },
                new QuizQuestion
                {
                    QuestionText = "What is the best practice for password management?",
                    Options = new List<string> { "Use the same password everywhere", "Write passwords on paper", "Use a reputable password manager", "Share passwords with friends" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Password managers generate and store unique, strong passwords securely.",
                    Category = "Password Security"
                },
                new QuizQuestion
                {
                    QuestionText = "How can you identify a secure website?",
                    Options = new List<string> { "It has lots of colors", "The URL starts with https://", "It loads quickly", "It has many ads" },
                    CorrectAnswerIndex = 1,
                    Explanation = "HTTPS encrypts data between your browser and the website, indicated by https:// and often a lock icon.",
                    Category = "Web Security"
                },
                new QuizQuestion
                {
                    QuestionText = "What is the purpose of a firewall?",
                    Options = new List<string> { "To start fires", "To block unauthorized network access", "To speed up internet", "To store passwords" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Firewalls monitor and control incoming and outgoing network traffic based on security rules.",
                    Category = "Network Security"
                },
                new QuizQuestion
                {
                    QuestionText = "What should you do if you suspect your account has been compromised?",
                    Options = new List<string> { "Wait and see", "Change password immediately", "Delete the account", "Tell everyone" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Immediately changing your password can prevent further unauthorized access.",
                    Category = "Incident Response"
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: Antivirus software provides 100% protection against all threats.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "No security solution is 100% effective. Multiple layers of security and safe practices are needed.",
                    Category = "Antivirus"
                }
            };
        }

        public void StartQuiz(int numberOfQuestions = 10)
        {
            if (numberOfQuestions > questions.Count)
                numberOfQuestions = questions.Count;

            // Randomly select questions
            var random = new Random();
            currentQuizQuestions = questions.OrderBy(x => random.Next()).Take(numberOfQuestions).ToList();

            // Reset quiz state
            currentQuestionIndex = 0;
            correctAnswers = 0;
            isQuizActive = true;

            // Reset question states
            foreach (var question in currentQuizQuestions)
            {
                question.IsAnswered = false;
                question.AnsweredCorrectly = false;
            }

            QuizStarted?.Invoke(this, EventArgs.Empty);
        }

        public QuizQuestion GetCurrentQuestion()
        {
            if (!isQuizActive || currentQuestionIndex >= currentQuizQuestions.Count)
                return null;

            return currentQuizQuestions[currentQuestionIndex];
        }

        public bool SubmitAnswer(int selectedAnswerIndex)
        {
            if (!isQuizActive || currentQuestionIndex >= currentQuizQuestions.Count)
                return false;

            var currentQuestion = currentQuizQuestions[currentQuestionIndex];

            if (currentQuestion.IsAnswered)
                return currentQuestion.AnsweredCorrectly;

            bool isCorrect = selectedAnswerIndex == currentQuestion.CorrectAnswerIndex;

            currentQuestion.IsAnswered = true;
            currentQuestion.AnsweredCorrectly = isCorrect;

            if (isCorrect)
                correctAnswers++;

            QuestionAnswered?.Invoke(this, currentQuestion);

            return isCorrect;
        }

        public bool HasNextQuestion()
        {
            return isQuizActive && currentQuestionIndex < currentQuizQuestions.Count - 1;
        }

        public void NextQuestion()
        {
            if (HasNextQuestion())
            {
                currentQuestionIndex++;
            }
            else if (isQuizActive)
            {
                EndQuiz();
            }
        }

        public void EndQuiz()
        {
            if (!isQuizActive) return;

            isQuizActive = false;
            var finalScore = GetFinalScore();
            finalScore.CompletedDate = DateTime.Now;

            QuizCompleted?.Invoke(this, finalScore);
        }

        public QuizScore GetCurrentScore()
        {
            int questionsAnswered = currentQuizQuestions?.Count(q => q.IsAnswered) ?? 0;

            return new QuizScore
            {
                CorrectAnswers = correctAnswers,
                TotalQuestions = TotalQuestions,
                QuestionsAnswered = questionsAnswered
            };
        }

        public QuizScore GetFinalScore()
        {
            return new QuizScore
            {
                CorrectAnswers = correctAnswers,
                TotalQuestions = TotalQuestions,
                QuestionsAnswered = TotalQuestions,
                CompletedDate = DateTime.Now
            };
        }

        public void ResetQuiz()
        {
            isQuizActive = false;
            currentQuestionIndex = 0;
            correctAnswers = 0;
            currentQuizQuestions = null;
        }

        public List<QuizQuestion> GetAllQuestions()
        {
            return questions.ToList();
        }

        public List<QuizQuestion> GetQuestionsByCategory(string category)
        {
            return questions.Where(q => q.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<string> GetCategories()
        {
            return questions.Select(q => q.Category).Distinct().OrderBy(c => c).ToList();
        }

        public void AddQuestion(QuizQuestion question)
        {
            if (question != null && !string.IsNullOrWhiteSpace(question.QuestionText))
            {
                questions.Add(question);
            }
        }

        public QuizQuestion CreateCustomQuestion(string questionText, List<string> options,
                                               int correctAnswerIndex, string explanation, string category = "Custom")
        {
            return new QuizQuestion
            {
                QuestionText = questionText,
                Options = new List<string>(options),
                CorrectAnswerIndex = correctAnswerIndex,
                Explanation = explanation,
                Category = category
            };
        }

        public Dictionary<string, int> GetCategoryStats()
        {
            return questions.GroupBy(q => q.Category)
                          .ToDictionary(g => g.Key, g => g.Count());
        }

        public double GetAverageScore()
        {
            // This would typically be stored in a database or file
            // For now, returning a placeholder
            return 75.0; // 75% average
        }
    }
}