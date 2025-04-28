using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Oqtane.Services;
using Oqtane.Shared;
using System.Net.Http;
using CodeChops.Module.AIVideoCoach.Services;
using CodeChops.Module.AIVideoCoach.Shared.Interfaces;

namespace CodeChops.Module.AIVideoCoach.Client.Services
{
    public class Startup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<AIVideoCoachService, AIVideoCoachService>();
            services.AddTransient<IChatClientFactory<IChatClient>, ChatClientFactory>();
            services.AddMudServices();
        }


    }
}
