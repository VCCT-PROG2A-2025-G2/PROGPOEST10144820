using PROGPOE.Models;
using System.Collections.Generic;

namespace PROGPOE.Interfaces
{
    public interface IConversationService
    {
        List<ConversationMessage> ConversationHistory { get; }
        void AddMessage(string speaker, string message, SentimentType sentiment = SentimentType.Neutral);
        string GetHistoryString();
        void ClearHistory();
    }
}
