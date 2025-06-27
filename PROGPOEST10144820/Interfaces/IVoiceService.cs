using System.Threading.Tasks;
using PROGPOE.Models;

namespace PROGPOE.Interfaces
{
    public interface IVoiceService
    {
        bool IsVoiceEnabled { get; }
        Task InitializeAsync();
        Task SpeakAsync(string text);
        Task ConfigureVoiceSettingsAsync();
        void ToggleVoice();
        void Dispose();
    }
}
