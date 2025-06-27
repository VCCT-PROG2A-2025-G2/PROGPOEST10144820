using PROGPOE.Models;

namespace PROGPOE.Interfaces
{
    public interface ISentimentAnalysisService
    {
        SentimentType AnalyzeSentiment(string input);
        string GetSentimentResponse(SentimentType sentiment);
    }
}
