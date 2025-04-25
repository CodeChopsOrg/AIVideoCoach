using MudBlazor;
using Oqtane.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifoia.Module.AIVideoCoach.Shared.Enums;

namespace Trifoia.Module.AIVideoCoach
{
    public class SettingsViewModel
    {
        // Dictionary to hold key-value pairs for settings
        public Dictionary<string, string> Settings { get; private set; } = new Dictionary<string, string>();

        // Constructor to load settings
        public SettingsViewModel(ISettingService settingService, Dictionary<string, string> settings)
        {
            var keys = new[]
            {
            AIVideoCoachCredentials.AIHost,
            AIVideoCoachCredentials.OPENAI_KEY,
            AIVideoCoachCredentials.AZURE_OPENAI_ENDPOINT,
            AIVideoCoachCredentials.AZURE_OPENAI_DEPLOYMENT,
            AIVideoCoachCredentials.AZURE_OPENAI_API_KEY,
            AIVideoCoachCredentials.AZURE_EMBEDDING_DEPLOYMENT,
            AIVideoCoachCredentials.GITHUB_TOKEN,
            AIVideoCoachCredentials.AZURE_INFERENCE_KEY,
            AIVideoCoachCredentials.REMOTE_MODEL_OR_DEPLOYMENT_ID,
            AIVideoCoachCredentials.REMOTE_ENDPOINT,
            AIVideoCoachCredentials.LOCAL_MODEL_NAME,
            AIVideoCoachCredentials.LOCAL_ENDPOINT
        };

            foreach (var key in keys)
            {
                var value = settingService.GetSetting(settings, key, string.Empty);
                Settings[key] = value;
            }
        }

        // Method to save settings
        public void SetSettings(ISettingService settingService, Dictionary<string, string> settings)
        {
            foreach (var kvp in Settings)
            {
                settingService.SetSetting(settings, kvp.Key, kvp.Value);
            }
        }
    }
}
