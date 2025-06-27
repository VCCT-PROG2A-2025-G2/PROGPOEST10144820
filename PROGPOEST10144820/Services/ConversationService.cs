using System;
using System.Collections.Generic;
using System.Linq;
using PROGPOE.Interfaces;
using PROGPOE.Models;

namespace PROGPOE.Services
{
    public class ConversationService : IConversationService
    {
        private readonly List<ConversationMessage> conversationHistory;

        public ConversationService()
        {
            conversationHistory = new List<ConversationMessage>();
        }

        // Fix: Ensure the return type matches the interface definition  
        public List<ConversationMessage> ConversationHistory => conversationHistory;

        public void AddMessage(string speaker, string message, SentimentType sentiment = SentimentType.Neutral)
        {
            conversationHistory.Add(new ConversationMessage
            {
                Speaker = speaker,
                Message = message,
                Sentiment = sentiment,
                Timestamp = DateTime.Now
            });
        }

        public string GetHistoryString()
        {
            if (conversationHistory.Count == 0)
            {
                return "📝 No conversation history yet - this is our first exchange!";
            }

            string history = "📜 **CONVERSATION HISTORY** 📜\n\n";
            int showCount = Math.Min(10, conversationHistory.Count);

            for (int i = conversationHistory.Count - showCount; i < conversationHistory.Count; i++)
            {
                var msg = conversationHistory[i];
                history += $"[{msg.Timestamp:HH:mm:ss}] {msg.Speaker}: {msg.Message}\n";
            }

            if (conversationHistory.Count > 10)
            {
                history += $"\n... and {conversationHistory.Count - 10} earlier messages";
            }

            return history;
        }

        public void ClearHistory()
        {
            conversationHistory.Clear();
        }
    }
}
