using Microsoft.Extensions.Configuration;
using Oqtane.Repository;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeChops.Module.AIVideoCoach.Services;
using CodeChops.Module.AIVideoCoach.Shared.Interfaces;
using CodeChops.Module.AIVideoCoach.Shared.Enums;


namespace CodeChops.Module.AIVideoCoach.Server.Services
{
    public class EmbeddingClientFactory : IChatClientFactory<IEmbeddingClient>
    {
        private readonly ISettingRepository _settingRepository;
        private readonly SiteState _siteState;

        public EmbeddingClientFactory(ISettingRepository settingRepository, SiteState siteState)
        {
            _settingRepository = settingRepository;
            _siteState = siteState;
        }

        public IEmbeddingClient Create()
        {
            var endpoint = _settingRepository.GetSetting(EntityNames.Site, _siteState.Alias.AliasId, AIVideoCoachCredentials.AZURE_OPENAI_ENDPOINT).SettingValue ?? throw new Exception("Missing Azure OpenAI endpoint.");
            var deploymentId = _settingRepository.GetSetting(EntityNames.Site, _siteState.Alias.AliasId, AIVideoCoachCredentials.AZURE_EMBEDDING_DEPLOYMENT).SettingValue ?? throw new Exception("Missing Azure OpenAI deployment ID.");
            var apiKey = _settingRepository.GetSetting(EntityNames.Site, _siteState.Alias.AliasId, AIVideoCoachCredentials.AZURE_OPENAI_API_KEY).SettingValue ?? throw new Exception("Missing Azure OpenAI API key.");

            return new AzureOpenAIEmbeddingClient(new Uri(endpoint), deploymentId, apiKey);
        }

        public Task<IEmbeddingClient> CreateAsync()
        {
            throw new NotImplementedException();
        }
    }

}
