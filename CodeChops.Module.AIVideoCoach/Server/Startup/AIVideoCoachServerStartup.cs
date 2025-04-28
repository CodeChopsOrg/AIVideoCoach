using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Oqtane.Modules;
using Oqtane.Repository;
using CodeChops.Module.AIVideoCoach.Extensions;
using CodeChops.Module.AIVideoCoach.Repository;
using CodeChops.Module.AIVideoCoach.Services;
using Oqtane.Services;
using OpenAI.Embeddings;
using Microsoft.Extensions.AI;
using CodeChops.Module.AIVideoCoach.Server.Services;
using CodeChops.Module.AIVideoCoach.Shared.Interfaces;

namespace CodeChops.Module.AIVideoCoach.Startup
{
    public class AIVideoCoachServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<AIVideoCoachContext>(opt => { }, ServiceLifetime.Transient);
            //Add ChatBot Configuration and Services
            services.AddTransient<IChatClientFactory<IEmbeddingClient>, EmbeddingClientFactory>();
            //Register ChatBot Services
            services.AddTransient<IAIVideoCoachServerService, AIVideoCoachServerService>();
        }
    }
}
