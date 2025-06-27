using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PROGPOE.Interfaces;
using PROGPOE.Models;

namespace PROGPOE.Services
{
    public class SentimentAnalysisService : ISentimentAnalysisService
    {
        private readonly Dictionary<string, List<string>> sentimentResponses;
        private readonly Random random;

        public SentimentAnalysisService()
        {
            random = new Random();
            sentimentResponses = InitializeSentimentResponses();
        }

        public SentimentType AnalyzeSentiment(string input)
        {
            string lowerInput = input.ToLower();

            if (Regex.IsMatch(lowerInput, @"\b(worried|scared|afraid|anxious|nervous|concerned)\b"))
                return SentimentType.Worried;
            if (Regex.IsMatch(lowerInput, @"\b(frustrated|annoyed|angry|mad|irritated)\b"))
                return SentimentType.Frustrated;
            if (Regex.IsMatch(lowerInput, @"\b(curious|interested|want to learn|tell me more|fascinating)\b"))
                return SentimentType.Curious;
            if (Regex.IsMatch(lowerInput, @"\b(confused|don't understand|unclear|complicated|difficult)\b"))
                return SentimentType.Confused;
            if (Regex.IsMatch(lowerInput, @"\b(happy|great|awesome|excellent|love|amazing)\b"))
                return SentimentType.Happy;
            if (Regex.IsMatch(lowerInput, @"\b(hate|terrible|awful|worst|stupid)\b"))
                return SentimentType.Angry;

            return SentimentType.Neutral;
        }

        public string GetSentimentResponse(SentimentType sentiment)
        {
            string sentimentKey = sentiment.ToString().ToLower();
            if (sentimentResponses.ContainsKey(sentimentKey))
            {
                var responses = sentimentResponses[sentimentKey];
                return responses[random.Next(responses.Count)];
            }
            return string.Empty;
        }

        private Dictionary<string, List<string>> InitializeSentimentResponses()
        {
            return new Dictionary<string, List<string>>
            {
                ["worried"] = new List<string>
                {
                    "I completely understand your concerns about online security. It's actually smart to be cautious!",
                    "Your worry shows you're taking cybersecurity seriously, which is great! Let me help ease your concerns.",
                    "It's natural to feel overwhelmed by cybersecurity threats. Let's break it down into manageable steps."
                },
                ["frustrated"] = new List<string>
                {
                    "I can sense your frustration, and that's completely valid. Cybersecurity can be complex!",
                    "I understand this can be annoying. Let's find a simpler way to approach this together.",
                    "Your frustration is understandable. Let me try to explain this more clearly."
                },
                ["curious"] = new List<string>
                {
                    "I love your curiosity! That's exactly the right attitude for learning about cybersecurity.",
                    "Great question! Your eagerness to learn will help keep you safe online.",
                    "Excellent! Curiosity is the best defense against cyber threats."
                },
                ["confused"] = new List<string>
                {
                    "No worries at all! Let me break this down into simpler terms.",
                    "That's a complex topic - let me explain it step by step.",
                    "I'll clarify that for you. Feel free to ask if anything is still unclear!"
                },
                ["happy"] = new List<string>
                {
                    "I'm glad you're feeling positive about cybersecurity! That's the right attitude.",
                    "Your enthusiasm is contagious! Let's keep building your security knowledge.",
                    "It's wonderful that you're taking a positive approach to online safety!"
                },
                ["angry"] = new List<string>
                {
                    "I understand your frustration. Let's work together to find solutions.",
                    "Your concerns are valid. Let me help you feel more secure online.",
                    "I hear your frustration. Let's focus on practical steps to improve your security."
                }
            };
        }
    }
}
