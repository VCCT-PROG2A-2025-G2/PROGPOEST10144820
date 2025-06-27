using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PROGPOE.Interfaces;
using PROGPOE.Models;
using PROGPOE.Services;

namespace PROGPOE
{
    public class CyberBotEngine : IDisposable
    {
        private readonly IVoiceService voiceService;
        private readonly ISentimentAnalysisService sentimentService;
        private readonly IResponseService responseService;
        private readonly IConversationService conversationService;
        private UserProfile userProfile;
        private bool disposed = false;

        // Events for UI communication
        public event Action<string> MessageReceived;
        public event Action<string> BotResponseReady;
        public event Action<UserProfile> UserProfileUpdated;
        public event Action<bool> VoiceStatusChanged;

        public CyberBotEngine()
        {
            voiceService = new VoiceService();
            sentimentService = new SentimentAnalysisService();
            responseService = new ResponseService();
            conversationService = new ConversationService();
            userProfile = new UserProfile();
        }

        public async Task InitializeAsync()
        {
            await voiceService.InitializeAsync();
            VoiceStatusChanged?.Invoke(voiceService.IsVoiceEnabled);
        }

        public async Task SetupUserProfileAsync(string userName, string interests)
        {
            userProfile.Name = userName;
            userProfile.LastInteraction = DateTime.Now;

            // Parse interests
            if (!string.IsNullOrEmpty(interests))
            {
                var keywordResponses = responseService.GetKeywordResponses();
                string lowerInterests = interests.ToLower();
                
                foreach (var keyword in keywordResponses.Keys)
                {
                    if (lowerInterests.Contains(keyword))
                    {
                        userProfile.Interests.Add(keyword);
                    }
                }
            }

            UserProfileUpdated?.Invoke(userProfile);

            // Welcome message
            string welcomeMessage = $"Hello {userName}! I'm your cybersecurity assistant. I'm ready to help you stay safe online!";
            
            if (voiceService.IsVoiceEnabled && userProfile.VoiceEnabled)
            {
                await voiceService.SpeakAsync(welcomeMessage);
            }

            BotResponseReady?.Invoke(welcomeMessage);
        }

        public async Task ProcessUserInputAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            // Add user message to conversation
            var sentiment = sentimentService.AnalyzeSentiment(input);
            userProfile.LastSentiment = sentiment;
            userProfile.LastInteraction = DateTime.Now;
            
            conversationService.AddMessage(userProfile.Name, input, sentiment);
            MessageReceived?.Invoke($"{userProfile.Name}: {input}");

            // Process the input
            string response = await ProcessInputInternalAsync(input, sentiment);

            // Add bot response to conversation
            conversationService.AddMessage("CyberBot", response);
            
            // Handle voice response for important messages
            if (ShouldUseVoice(response) && voiceService.IsVoiceEnabled && userProfile.VoiceEnabled)
            {
                string voiceSummary = GetVoiceSummary(response);
                await voiceService.SpeakAsync(voiceSummary);
            }

            BotResponseReady?.Invoke($"🤖 CyberBot: {response}");
            UserProfileUpdated?.Invoke(userProfile);
        }

        private async Task<string> ProcessInputInternalAsync(string input, SentimentType sentiment)
        {
            string lowerInput = input.ToLower().Trim();

            // Handle special commands
            if (IsSpecialCommand(lowerInput))
            {
                return HandleSpecialCommand(lowerInput);
            }

            // Get sentiment response if applicable
            string sentimentResponse = sentimentService.GetSentimentResponse(sentiment);
            
            // Get topic response
            string topicResponse = responseService.ProcessInput(input, userProfile);

            // Combine responses
            if (!string.IsNullOrEmpty(sentimentResponse) && !string.IsNullOrEmpty(topicResponse))
            {
                return $"{sentimentResponse} {topicResponse}";
            }

            return !string.IsNullOrEmpty(topicResponse) ? topicResponse : sentimentResponse;
        }

        private bool IsSpecialCommand(string input)
        {
            string[] commands = { "help", "memory", "stats", "tips", "history", "voice toggle", "exit" };
            return Array.Exists(commands, cmd => input == cmd);
        }

        private string HandleSpecialCommand(string command)
        {
            switch (command)
            {
                case "help":
                    return responseService.HandleSpecialCommand("help", userProfile);
                case "memory":
                    return responseService.HandleSpecialCommand("memory", userProfile);
                case "stats":
                    return responseService.HandleSpecialCommand("stats", userProfile);
                case "tips":
                    return responseService.HandleSpecialCommand("tips", userProfile);
                case "history":
                    return conversationService.GetHistoryString();
                case "voice toggle":
                    return ToggleVoice();
                case "exit":
                    return GetGoodbyeMessage();
                default:
                    return "Unknown command. Type 'help' for available commands.";
            }
        }

        private string ToggleVoice()
        {
            userProfile.VoiceEnabled = !userProfile.VoiceEnabled;
            VoiceStatusChanged?.Invoke(userProfile.VoiceEnabled && voiceService.IsVoiceEnabled);
            
            return userProfile.VoiceEnabled ?
                "🔊 Voice responses enabled! I'll speak important messages." :
                "🔇 Voice responses disabled. Text-only mode activated.";
        }

        private string GetGoodbyeMessage()
        {
            return $@"👋 **GOODBYE, {userProfile.Name.ToUpper()}!** 👋

📊 **Session Summary:**
• Questions asked: {userProfile.TotalQuestions}
• Topics explored: {userProfile.TopicCounts.Count}
• Most interested in: {userProfile.MostInterested}

🛡️ **Remember:** Stay vigilant, keep learning, and always verify before you trust!
🔐 **Stay safe online!** 🔐";
        }

        private bool ShouldUseVoice(string response)
        {
            return response.Contains("🔐") || response.Contains("⚠️") || 
                   response.Contains("💡") || response.Length > 200;
        }

        private string GetVoiceSummary(string response)
        {
            if (response.Length <= 100)
                return response;
            
            return response.Substring(0, Math.Min(100, response.Length)) + "... Please read the full response for more details.";
        }

        public UserProfile GetUserProfile() => userProfile;
        
        public bool IsVoiceEnabled => voiceService.IsVoiceEnabled && userProfile.VoiceEnabled;
        
        public List<ConversationMessage> GetConversationHistory() => conversationService.ConversationHistory;

        public void Dispose()
        {
            if (!disposed)
            {
                voiceService?.Dispose();
                disposed = true;
            }
        }
    }
}
