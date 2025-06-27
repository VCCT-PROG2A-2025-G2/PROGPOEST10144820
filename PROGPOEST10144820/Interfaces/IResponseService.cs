using PROGPOE.Models;
using System.Collections.Generic;

namespace PROGPOE.Interfaces
{
    public interface IResponseService
    {
        string ProcessInput(string input, UserProfile userProfile);
        string HandleSpecialCommand(string command, UserProfile userProfile);
        string GetSecurityTip();
        Dictionary<string, List<string>> GetKeywordResponses();
    }
}
