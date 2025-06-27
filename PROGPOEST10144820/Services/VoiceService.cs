using System;
using System.Linq;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PROGPOE.Interfaces;
using PROGPOE.Models;

namespace PROGPOE.Services
{
    public class VoiceService : IVoiceService, IDisposable
    {
        private SpeechSynthesizer speechSynthesizer;
        private bool isVoiceEnabled = false;
        private bool disposed = false;

        public bool IsVoiceEnabled => isVoiceEnabled;

        public async Task InitializeAsync()
        {
            try
            {
                speechSynthesizer = new SpeechSynthesizer();

                // Configure voice settings for better AI-like experience
                var voices = speechSynthesizer.GetInstalledVoices();

                // Try to find a female voice first (often sounds more friendly for assistants)
                var preferredVoice = voices.FirstOrDefault(v =>
                    v.VoiceInfo.Gender == VoiceGender.Female &&
                    v.VoiceInfo.Culture.Name.StartsWith("en")) ??
                    voices.FirstOrDefault(v => v.VoiceInfo.Culture.Name.StartsWith("en"));

                if (preferredVoice != null)
                {
                    speechSynthesizer.SelectVoice(preferredVoice.VoiceInfo.Name);
                }

                // Set voice characteristics for AI-like experience
                speechSynthesizer.Rate = 0;    // Normal speed
                speechSynthesizer.Volume = 85;  // Slightly lower for comfort

                isVoiceEnabled = true;
            }
            catch (Exception)
            {
                isVoiceEnabled = false;
            }
        }

        public async Task SpeakAsync(string text)
        {
            if (!isVoiceEnabled || speechSynthesizer == null) return;

            try
            {
                // Clean text for speech (remove emojis and special characters)
                string cleanText = Regex.Replace(text, @"[^\w\s\.\,\!\?]", "");

                // Add SSML for better voice control
                string ssmlText = $@"
                <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                    <voice name='{speechSynthesizer.Voice.Name}'>
                        <prosody rate='medium' pitch='medium' volume='medium'>
                            {cleanText}
                        </prosody>
                    </voice>
                </speak>";

                // Speak asynchronously
                await Task.Run(() =>
                {
                    try
                    {
                        speechSynthesizer.SpeakSsml(ssmlText);
                    }
                    catch
                    {
                        // Fallback to simple speak
                        speechSynthesizer.Speak(cleanText);
                    }
                });
            }
            catch (Exception)
            {
                // Fail silently for voice errors
            }
        }

        public async Task ConfigureVoiceSettingsAsync()
        {
            // This would be handled by the UI now
            await Task.CompletedTask;
        }

        public void ToggleVoice()
        {
            if (speechSynthesizer != null)
            {
                isVoiceEnabled = !isVoiceEnabled;
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                speechSynthesizer?.Dispose();
                disposed = true;
            }
        }
    }
}
