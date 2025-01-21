    
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using Oqtane.Services;
using Oqtane.Shared;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifoia.Module.AIVideoCoach.Shared.Enums;
using Trifoia.Module.AIVideoCoach.Shared.Interfaces;


namespace Trifoia.Module.AIVideoCoach.Services
{
    public class ChatClientFactory : IChatClientFactory<IChatClient>
    {
        private readonly ISettingService _settingService;
        private readonly SiteState _siteState;

        public ChatClientFactory(ISettingService settingService, SiteState siteState)
        {
            _siteState = siteState;
            _settingService = settingService;
        }

        public IChatClient Create()
        {
            throw new NotImplementedException();
        }

        public async Task<IChatClient> CreateAsync()
        {
            var settings = await _settingService.GetSiteSettingsAsync(_siteState.Alias.AliasId);

            var client = (settings[AIVideoCoachCredentials.AIHost] ?? "local").ToLower();

            if (client == "local")
            {
                var endpoint = settings[AIVideoCoachCredentials.LOCAL_ENDPOINT]
                               ?? throw new Exception("Missing configuration LOCAL_ENDPOINT");
                var modelName = settings[AIVideoCoachCredentials.LOCAL_MODEL_NAME]
                                ?? throw new Exception("Missing configuration LOCAL_MODEL_NAME");
                return new OllamaChatClient(endpoint, modelName);
            }
            if (client == "openai")
            {
                var apiKey = settings[AIVideoCoachCredentials.OPENAI_KEY]
                             ?? throw new Exception("Missing configuration OPENAI_KEY");
                var modelId = settings[AIVideoCoachCredentials.REMOTE_MODEL_OR_DEPLOYMENT_ID]
                              ?? throw new Exception("Missing configuration REMOTE_MODEL_OR_DEPLOYMENT_ID");
                return new OpenAIClient(apiKey).AsChatClient(modelId);
            }
            else if (client == "azureopenai")
            {
                var endpoint = settings[AIVideoCoachCredentials.AZURE_OPENAI_ENDPOINT]
                               ?? throw new Exception("Missing configuration: AZURE_OPENAI_ENDPOINT");
                var deploymentId = settings[AIVideoCoachCredentials.AZURE_OPENAI_DEPLOYMENT]
                                   ?? throw new Exception("Missing configuration: AZURE_OPENAI_DEPLOYMENT");
                var apiKey = settings[AIVideoCoachCredentials.AZURE_OPENAI_API_KEY]
                             ?? throw new Exception("Missing configuration: AZURE_OPENAI_API_KEY");
                return new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey)).AsChatClient(deploymentId);
            }

            throw new Exception("Invalid AI host configuration.");
        }
    }
}
