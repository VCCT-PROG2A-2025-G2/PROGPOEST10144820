using System;

namespace PROGPOE.Models
{
    public class ConversationMessage
    {
        public string Speaker { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public SentimentType Sentiment { get; set; } = SentimentType.Neutral;
        public bool IsVoiceResponse { get; set; } = false;
    }
}
