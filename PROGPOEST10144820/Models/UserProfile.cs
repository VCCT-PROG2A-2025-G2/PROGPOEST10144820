using System;
using System.Collections.Generic;
using System.Linq;
using PROGPOE.Models;

namespace PROGPOE.Models
{
    public class UserProfile
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Interests { get; set; } = new List<string>();
        public Dictionary<string, int> TopicCounts { get; set; } = new Dictionary<string, int>();
        public DateTime LastInteraction { get; set; } = DateTime.Now;
        public SentimentType LastSentiment { get; set; } = SentimentType.Neutral;
        public bool VoiceEnabled { get; set; } = true;
        
        public int TotalQuestions => TopicCounts.Values.Sum();
        public string MostInterested => TopicCounts.Count > 0 ? 
            TopicCounts.OrderByDescending(x => x.Value).First().Key : "None yet";
    }
}
