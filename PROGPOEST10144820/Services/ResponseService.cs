using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PROGPOE.Interfaces;
using PROGPOE.Models;

namespace PROGPOE.Services
{
    public class ResponseService : IResponseService
    {
        private readonly Dictionary<string, List<string>> keywordResponses;
        private readonly Dictionary<string, string> securityTips;
        private readonly Random random;

        public ResponseService()
        {
            random = new Random();
            keywordResponses = InitializeKeywordResponses();
            securityTips = InitializeSecurityTips();
        }

        public string ProcessInput(string input, UserProfile userProfile)
        {
            string lowerInput = input.ToLower();

            // Handle greetings
            if (Regex.IsMatch(lowerInput, @"\b(hello|hi|hey|greetings)\b"))
            {
                return HandleGreeting(userProfile);
            }

            // Handle personal questions
            if (Regex.IsMatch(lowerInput, @"\b(how are you|what are you|who are you)\b"))
            {
                return HandlePersonalQuestions(lowerInput);
            }

            // Find cybersecurity topic responses
            string response = FindTopicResponse(lowerInput, userProfile);
            if (!string.IsNullOrEmpty(response))
            {
                return response;
            }

            return GetIntelligentDefault();
        }

        public string HandleSpecialCommand(string command, UserProfile userProfile)
        {
            command = command.ToLower();
            switch (command)
            {
                case "help":
                    return GetHelpResponse();
                case "memory":
                    return GetMemoryResponse(userProfile);
                case "stats":
                    return GetStatsResponse(userProfile);
                case "tips":
                    return GetSecurityTip();
                case "history":
                    return "History is managed by the conversation service.";
                default:
                    return "Unknown command. Type 'help' for available commands.";
            }
        }

        public string GetSecurityTip()
        {
            var tipKeys = securityTips.Keys.ToList();
            string randomTip = securityTips[tipKeys[random.Next(tipKeys.Count)]];

            return $@"💡 **RANDOM SECURITY TIP** 💡

{randomTip}

🎲 Want another tip? Just ask for 'tips' again!
🎯 For specific advice, ask about: {string.Join(", ", keywordResponses.Keys)}";
        }

        public Dictionary<string, List<string>> GetKeywordResponses()
        {
            return keywordResponses;
        }

        private string HandleGreeting(UserProfile userProfile)
        {
            string[] greetings =
            {
                $"Hello again, {userProfile.Name}! Ready to boost your cybersecurity knowledge? 🚀",
                $"Hi {userProfile.Name}! I'm here and ready to help you stay safe online! 🛡️",
                $"Hey there, {userProfile.Name}! What cybersecurity topic shall we explore today? 🔍"
            };
            return greetings[random.Next(greetings.Length)];
        }

        private string HandlePersonalQuestions(string input)
        {
            if (input.Contains("how are you"))
            {
                return "🤖 I'm functioning perfectly and constantly updating my cybersecurity knowledge! How are you feeling about your online security today?";
            }
            else if (input.Contains("what are you"))
            {
                return "🧠 I'm an advanced AI cybersecurity assistant with sentiment analysis, memory capabilities, voice synthesis, and personalized learning features!";
            }
            else if (input.Contains("who are you"))
            {
                return "👋 I'm your personal cybersecurity mentor with AI voice capabilities! I can adapt to your learning style, remember our conversations, and provide tailored security advice.";
            }
            return "🤖 I'm here to help you with all your cybersecurity questions using advanced AI technology!";
        }

        private string GetHelpResponse()
        {
            return @"🆘 **HELP MENU** 🆘
            
📚 **Topics I can help with:**
• passwords, phishing, scams, privacy
• malware, wifi, social media, banking

🔧 **Special Commands:**
• 'memory' - See what I remember about you
• 'stats' - View your learning progress  
• 'tips' - Get random security tips
• 'history' - See our conversation history
• 'voice toggle' - Enable/disable voice responses
• 'exit' - End our conversation

🧠 **Advanced AI Features:**
• Voice synthesis for important responses
• Emotion detection and adaptive responses
• Persistent memory of your interests and preferences
• Progress tracking of explored topics
• Personalized security advice

💡 Just ask me anything about cybersecurity and I'll respond with both text and voice!";
        }

        private string GetMemoryResponse(UserProfile userProfile)
        {
            var memory = $@"🧠 **MY MEMORY OF YOU** 🧠

👤 **Name:** {userProfile.Name}
📅 **Last Interaction:** {userProfile.LastInteraction:yyyy-MM-dd HH:mm:ss}
😊 **Current Mood:** {userProfile.LastSentiment}
🔊 **Voice Enabled:** {(userProfile.VoiceEnabled ? "Yes" : "No")}
🎯 **Interests:** {(userProfile.Interests.Count > 0 ? string.Join(", ", userProfile.Interests) : "General cybersecurity")}

📊 **Topic Exploration:**";

            if (userProfile.TopicCounts.Count > 0)
            {
                foreach (var topic in userProfile.TopicCounts.OrderByDescending(x => x.Value))
                {
                    memory += $"\n• {topic.Key}: {topic.Value} questions";
                }
            }
            else
            {
                memory += "\n• No topics explored yet - let's start learning!";
            }

            return memory;
        }

        private string GetStatsResponse(UserProfile userProfile)
        {
            return $@"📈 **YOUR LEARNING STATISTICS** 📈

🔢 **Total Questions Asked:** {userProfile.TotalQuestions}
🏆 **Most Explored Topic:** {userProfile.MostInterested}
📚 **Topics Covered:** {userProfile.TopicCounts.Count} out of {keywordResponses.Count}
🔊 **Voice Responses:** {(userProfile.VoiceEnabled ? "Enabled" : "Disabled")}

🎯 **Progress:** {((double)userProfile.TopicCounts.Count / keywordResponses.Count * 100):F1}% of topics explored!";
        }

        private string FindTopicResponse(string input, UserProfile userProfile)
        {
            foreach (var topic in keywordResponses.Keys)
            {
                if (input.Contains(topic))
                {
                    if (userProfile.TopicCounts.ContainsKey(topic))
                        userProfile.TopicCounts[topic]++;
                    else
                        userProfile.TopicCounts[topic] = 1;

                    string response = GetRandomFromList(keywordResponses[topic]);

                    if (userProfile.Interests.Contains(topic))
                    {
                        response += $"\n\n💡 Since you're particularly interested in {topic}, here's an advanced tip: {GetAdvancedTip(topic)}";
                    }

                    return response;
                }
            }
            return string.Empty;
        }

        private string GetAdvancedTip(string topic)
        {
            var advancedTips = new Dictionary<string, string>
            {
                ["password"] = "Use passphrases instead of passwords - 'Coffee!Morning@Beach2024' is stronger than 'C0ff33!'",
                ["phishing"] = "Check email headers to see the actual sender path - many phishing emails show suspicious routing",
                ["scam"] = "Research unknown phone numbers on scammer databases before engaging with callers",
                ["privacy"] = "Use different email addresses for different purposes to limit data correlation",
                ["malware"] = "Run executables in a sandbox environment first if you're unsure about their safety",
                ["wifi"] = "Use MAC address randomization to prevent tracking across different networks",
                ["social"] = "Regularly search for yourself online to see what information is publicly available",
                ["banking"] = "Set up account notifications for logins from new devices or locations"
            };

            return advancedTips.ContainsKey(topic) ? advancedTips[topic] : "Stay vigilant and keep learning!";
        }

        private string GetIntelligentDefault()
        {
            string[] smartDefaults =
            {
                "🤔 I'm not sure about that specific topic. Could you ask about passwords, phishing, scams, privacy, malware, wifi, social media, or banking?",
                "💭 That's an interesting question! Try asking about cybersecurity topics like authentication, data protection, or online safety.",
                "🎯 I specialize in cybersecurity topics. What would you like to know about staying safe online?",
                "🔍 I didn't recognize that topic. Type 'help' to see what I can assist you with!"
            };

            return smartDefaults[random.Next(smartDefaults.Length)];
        }

        private string GetRandomFromList(List<string> list)
        {
            return list[random.Next(list.Count)];
        }

        private Dictionary<string, List<string>> InitializeKeywordResponses()
        {
            return new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
                {
                    "🔐 Strong passwords are your first line of defense! Use at least 12 characters with a mix of uppercase, lowercase, numbers, and symbols.",
                    "🔑 Never reuse passwords across multiple accounts. Each account should have a unique, strong password.",
                    "📱 Consider using a reputable password manager like Bitwarden or LastPass to generate and store unique passwords.",
                    "⚡ Enable two-factor authentication (2FA) wherever possible - it adds an extra security layer even if your password is compromised.",
                    "🚫 Avoid using personal information like birthdays, pet names, or family names in your passwords."
                },

                ["phishing"] = new List<string>
                {
                    "🎣 Phishing emails often create urgency - take your time to verify before clicking any links or attachments.",
                    "📧 Check the sender's email address carefully. Phishers often use domains that look similar to legitimate ones.",
                    "🔍 Hover over links before clicking to see the actual destination URL. If it looks suspicious, don't click!",
                    "📞 When in doubt, contact the organization directly through their official website or verified phone number.",
                    "🚨 Be especially wary of emails asking for passwords, personal info, or urgent financial actions."
                },

                ["scam"] = new List<string>
                {
                    "⚠️ If something sounds too good to be true, it probably is! Be skeptical of get-rich-quick schemes.",
                    "🕒 Scammers create artificial urgency. Legitimate organizations won't pressure you to act immediately.",
                    "💳 Never share personal information like ID numbers, banking details, or PINs over phone or email.",
                    "🔍 Research unfamiliar companies or offers online before engaging with them.",
                    "👥 Ask family or friends for advice if you're unsure about an offer or request."
                },

                ["privacy"] = new List<string>
                {
                    "🔒 Review your social media privacy settings regularly - you'd be surprised how much is public by default!",
                    "📍 Be cautious about sharing location data, especially in real-time on social platforms.",
                    "🍪 Use privacy-focused browsers like Firefox or Brave, and consider VPN services for additional protection.",
                    "📖 Actually read privacy policies (or at least the summary) before agreeing to new services.",
                    "🗑️ Regularly audit and delete old accounts you no longer use - they're potential security risks."
                },

                ["malware"] = new List<string>
                {
                    "🛡️ Keep your antivirus software updated and run regular system scans to catch threats early.",
                    "⬇️ Only download software from official sources like app stores or verified developer websites.",
                    "🚫 Avoid clicking on pop-up ads claiming your computer is infected - they're often malware themselves!",
                    "🔄 Keep your operating system and all software updated to patch security vulnerabilities.",
                    "📎 Be extremely cautious with email attachments, especially from unknown senders or unexpected sources."
                },

                ["wifi"] = new List<string>
                {
                    "📶 Avoid public Wi-Fi for sensitive activities like banking - use your mobile data instead when possible.",
                    "🔐 Use a VPN when connecting to public networks to encrypt your internet traffic.",
                    "🏠 Secure your home Wi-Fi with WPA3 encryption and a strong, unique password.",
                    "📱 Turn off automatic Wi-Fi connection to prevent your device from joining unknown networks.",
                    "👀 Be wary of Wi-Fi networks with names like 'Free WiFi' or similar generic names - they could be malicious."
                },

                ["social"] = new List<string>
                {
                    "📱 Think before you post - once it's online, it's often there forever, even if you delete it.",
                    "👤 Be selective about friend/follower requests - fake profiles are common and used for data harvesting.",
                    "📸 Avoid posting photos that reveal personal information like addresses, license plates, or documents.",
                    "🎂 Consider not posting your real birthday publicly - it's often used for identity verification.",
                    "🔍 Regularly review what others can see on your profile and adjust settings accordingly."
                },

                ["banking"] = new List<string>
                {
                    "🏦 Always type your bank's URL directly into the browser rather than clicking email links.",
                    "📱 Set up account alerts for transactions so you're notified of any suspicious activity immediately.",
                    "💳 Use credit cards instead of debit cards for online purchases - they offer better fraud protection.",
                    "🔒 Never do banking on public Wi-Fi or shared computers.",
                    "📞 If you receive suspicious calls about your accounts, hang up and call your bank directly."
                }
            };
        }

        private Dictionary<string, string> InitializeSecurityTips()
        {
            return new Dictionary<string, string>
            {
                ["daily"] = "💡 Daily Tip: Use different passwords for different accounts. A password manager can help!",
                ["weekly"] = "📅 Weekly Reminder: Check your bank and credit card statements for unauthorized transactions.",
                ["monthly"] = "🗓️ Monthly Task: Review and update your privacy settings on social media platforms.",
                ["backup"] = "💾 Backup Tip: Follow the 3-2-1 rule: 3 copies of important data, 2 different media types, 1 offsite.",
                ["update"] = "🔄 Update Reminder: Keep your software, apps, and operating system up to date!"
            };
        }
    }
}
